using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text;
using System.Linq;
using Data_Manager;

namespace DonkeyDataManager
{
    // Trainer 화면입니다.
    // tub 데이터 폴더를 로드해 catalog를 확인하고,
    // WSL의 mycar/train.py를 실행해 모델(.h5)을 생성/등록/삭제/이름변경합니다.
    public partial class frmNewtrainer : Form
    {
        // =====================================================
        // Windows API P/Invoke 선언
        // =====================================================

        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;
        private const int FLASHW_ALL = 3;
        private const uint FLASHW_TIMERNOFG = 4;

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        // =====================================================
        // 화면 상태와 WSL/학습 실행 상태
        // =====================================================

        // 사용자가 학습 대상으로 선택한 tub 폴더의 Windows 경로입니다.
        private string selectedDataPath = "";
        private List<string> selectedTubFolders =
            new List<string>();

        // 선택한 tub의 catalog 내용을 UI 목록과 프레임 재생에 쓰기 위해 메모리에 보관합니다.
        private List<CatalogRecord> integratedCatalogList =
            new List<CatalogRecord>();
        private List<CatalogListRow> catalogDisplayRows =
            new List<CatalogListRow>();
        private HashSet<int> deletedCatalogIndexes =
            new HashSet<int>();
        private string currentPreviewImagePath = "";
        private Size currentPreviewRenderSize = Size.Empty;

        private System.Windows.Forms.Timer playbackTimer =
            new System.Windows.Forms.Timer();

        private Process wslProcess = null;
        private Process browserProcess = null;
        private int trainingProcessWslPid;

        private System.Windows.Forms.Timer browserMonitorTimer =
            new System.Windows.Forms.Timer();

        // WSL 배포판, WSL 사용자, mycar 경로는 실행 환경마다 다르므로 시작 시 자동 탐색합니다.
        private string wslDistroName = "Ubuntu-22.04";
        private string wslUsername = "";
        private string wslBasePath = "";
        private string wslMycarPath = "";
        private const string ModelDirectoryName = "models";
        private const string TrainingCondaEnvironment = "e2e_env";
        private readonly string modelRegistryPath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "DonkeyDataManager",
                "models.json");
        private readonly object trainingLogLock = new object();
        private readonly Dictionary<Control, Rectangle> originalControlBounds =
            new Dictionary<Control, Rectangle>();
        private readonly Dictionary<Control, float> originalControlFontSizes =
            new Dictionary<Control, float>();
        private Size responsiveBaseClientSize =
            new Size(1600, 900);
        private bool isApplyingResponsiveLayout;

        // =====================================================
        // 모델 목록 동기화 상태
        // =====================================================

        private System.Windows.Forms.Timer modelRefreshTimer =
            new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer responsiveResizeTimer =
            new System.Windows.Forms.Timer();
        private bool isLoadingModels;
        private string lastModelListSignature = "";

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete && lstModels.ContainsFocus)
            {
                BtnModelDlt_Click(this, EventArgs.Empty);
                return true;
            }

            if (keyData == Keys.Space && !cmbSpeed.ContainsFocus)
            {
                ToggleCatalogPlayback();
                return true;
            }

            if (keyData == Keys.Left && !cmbSpeed.ContainsFocus)
            {
                MoveCatalogSelection(-1);
                return true;
            }

            if (keyData == Keys.Right && !cmbSpeed.ContainsFocus)
            {
                MoveCatalogSelection(1);
                return true;
            }

            if (keyData == Keys.Enter && lstTubFolders.ContainsFocus)
            {
                BtnTrain_Click(this, EventArgs.Empty);
                return true;
            }

            if (keyData == (Keys.Control | Keys.N) && lstModels.ContainsFocus)
            {
                BtnNameCh_Click(this, EventArgs.Empty);
                return true;
            }

            if (keyData == (Keys.Control | Keys.D) && lstCatalogRows.ContainsFocus)
            {
                BtnCleanData_Click(this, EventArgs.Empty);
                return true;
            }

            if (keyData == (Keys.Control | Keys.R) && lstCatalogRows.ContainsFocus)
            {
                BtnRestoreData_Click(this, EventArgs.Empty);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // =====================================================
        // 데이터 구조
        // =====================================================

        public class CatalogRecord
        {
            // catalog 원본 한 줄과 화면 표시용으로 파싱한 주요 값입니다.
            // IsDeleted는 UI에서 프레임 제외/복원 기능을 구현할 때 사용합니다.
            public string OriginalLine { get; set; }

            public string SourceFilePath { get; set; }

            public int LineIndex { get; set; }

            public string ImageFileName { get; set; }

            public string Angle { get; set; }

            public string Throttle { get; set; }

            public string Index { get; set; }

            public bool IsDeleted { get; set; }

        }

        private class CatalogListRow
        {
            public bool IsHeader { get; set; }

            public string Text { get; set; } = "";

            public CatalogRecord Record { get; set; }
        }

        public class ModelRegistryEntry
        {
            // 학습된 모델 파일과 그 모델이 어떤 tub에서 만들어졌는지 기록하는 항목입니다.
            // frmNewtrainer와 Pliot 화면이 같은 registry를 통해 모델 목록을 공유합니다.
            public string Name { get; set; } = "";

            public string WindowsPath { get; set; } = "";

            public string WslPath { get; set; } = "";

            public string SourceTubWindowsPath { get; set; } = "";

            public string SourceTubWslPath { get; set; } = "";

            public DateTime CreatedAt { get; set; }

            public bool IsDeleted { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        // =====================================================
        // 생성자
        // =====================================================

        public frmNewtrainer()
        {
            InitializeComponent();
            ClientSize = new Size(1600, 900);

            WireUiEvents();
            InitializeCatalogListDrawing();
            EnableDoubleBuffering(lstModels);
            EnableDoubleBuffering(lstCatalogRows);
            EnableDoubleBuffering(lstTubFolders);

            InitializePlaybackTimer();

            InitializeBrowserMonitor();

            InitializeWSLPaths();

            InitializeTrainerButtonStyles();
            InitializeResponsiveLayout();

            // 모델 폴더와 registry를 주기적으로 동기화합니다.
            InitializeModelRefreshTimer();

            // 시작 시 mycar/models 내부 모델만 표시합니다.
            LoadModelsToList();

            SharedModelRegistry.ModelsChanged +=
                SharedModelRegistry_ModelsChanged;

            FormClosed += FrmNewtrainer_FormClosed;
        }

        private void SharedModelRegistry_ModelsChanged(
            object sender,
            EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            BeginInvoke(
                new Action(
                    () =>
                    {
                        if (!IsDisposed)
                        {
                            LoadModelsToList();
                        }
                    }));
        }

        private void FrmNewtrainer_FormClosed(
            object sender,
            FormClosedEventArgs e)
        {
            SharedModelRegistry.ModelsChanged -=
                SharedModelRegistry_ModelsChanged;
            modelRefreshTimer.Stop();
            modelRefreshTimer.Dispose();
            responsiveResizeTimer.Stop();
            responsiveResizeTimer.Dispose();
            playbackTimer.Stop();
            playbackTimer.Dispose();
            browserMonitorTimer.Stop();
            browserMonitorTimer.Dispose();
        }

        // =====================================================
        // 모델 목록 로드와 registry 동기화
        // =====================================================

        private void InitializeModelRefreshTimer()
        {
            modelRefreshTimer.Interval = 30000;

            modelRefreshTimer.Tick += (s, e) =>
            {
                if (!Visible || WindowState == FormWindowState.Minimized)
                {
                    return;
                }

                LoadModelsToList();
            };

            modelRefreshTimer.Start();
        }

        private void LoadModelsToList()
        {
            try
            {
                if (lstModels == null || isLoadingModels)
                    return;

                isLoadingModels = true;

                string modelFolder =
                    Path.Combine(
                        wslBasePath,
                        ModelDirectoryName);

                bool registryChanged = false;

                List<ModelRegistryEntry> entries =
                    LoadModelRegistry()
                        .Where(IsValidModelRegistryEntry)
                        .Where(entry =>
                            ModelImportService.IsPathInsideDirectory(
                                entry.WindowsPath,
                                modelFolder))
                        .ToList();

                if (Directory.Exists(modelFolder))
                {
                    string[] modelFiles =
                        Directory.GetFiles(
                            modelFolder,
                            "*.h5");

                    Array.Sort(modelFiles);

                    foreach (string file in modelFiles)
                    {
                        string name =
                            Path.GetFileName(file);

                        if (
                            entries.Exists(
                                entry =>
                                    string.Equals(
                                        entry.WindowsPath,
                                        file,
                                        StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        entries.Add(
                            new ModelRegistryEntry()
                            {
                                Name = name,
                                WindowsPath = file,
                                WslPath = GetModelWslPath(name),
                                CreatedAt =
                                    File.GetCreationTime(file),
                                IsDeleted = false
                            });

                        registryChanged = true;
                    }
                }

                List<ModelRegistryEntry> displayEntries =
                    entries
                        .OrderBy(entry => entry.IsDeleted ? 1 : 0)
                        .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                string newSignature =
                    BuildModelListSignature(entries);

                if (string.Equals(
                    lastModelListSignature,
                    newSignature,
                    StringComparison.Ordinal))
                {
                    return;
                }

                lastModelListSignature = newSignature;

                lstModels.BeginUpdate();
                try
                {
                    lstModels.Items.Clear();

                    for (int i = 0; i < displayEntries.Count; i++)
                    {
                        lstModels.Items.Add(
                            CreateModelListItem(
                                displayEntries[i],
                                i + 1));
                    }
                }
                finally
                {
                    lstModels.EndUpdate();
                }

                if (registryChanged)
                {
                    SaveModelRegistry(entries);
                }
            }
            catch
            {

            }
            finally
            {
                isLoadingModels = false;
            }
        }

        private string BuildModelListSignature(List<ModelRegistryEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return "";
            }

            return string.Join(
                "|",
                entries
                    .OrderBy(entry => entry.WindowsPath, StringComparer.OrdinalIgnoreCase)
                    .Select(entry =>
                        entry.Name +
                        ":" +
                        entry.WindowsPath +
                        ":" +
                        entry.IsDeleted));
        }

        private void LoadModelTrashToList(List<ModelRegistryEntry> allEntries)
        {
            if (lstModelTrash == null)
            {
                return;
            }

            List<ModelRegistryEntry> deletedEntries =
                (allEntries ?? new List<ModelRegistryEntry>())
                    .Where(entry => entry.IsDeleted && IsValidModelRegistryEntry(entry))
                    .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

            lstModelTrash.BeginUpdate();
            try
            {
                lstModelTrash.Items.Clear();

                for (int i = 0; i < deletedEntries.Count; i++)
                {
                    lstModelTrash.Items.Add(
                        CreateModelListItem(
                            deletedEntries[i],
                            i + 1));
                }
            }
            finally
            {
                lstModelTrash.EndUpdate();
            }
        }

        private bool IsValidModelRegistryEntry(ModelRegistryEntry entry)
        {
            if (entry == null)
            {
                return false;
            }

            if (
                string.IsNullOrWhiteSpace(entry.Name) ||
                !entry.Name.EndsWith(
                    ".h5",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (
                string.IsNullOrWhiteSpace(entry.WindowsPath) ||
                !File.Exists(entry.WindowsPath))
            {
                return false;
            }

            return true;
        }

        // =====================================================
        // UI 이벤트와 반응형 배치 초기화
        // =====================================================

        private void InitializePlaybackTimer()
        {
            playbackTimer.Interval = 100;

            playbackTimer.Tick += PlaybackTimer_Tick;
        }

        private void EnableDoubleBuffering(Control control)
        {
            try
            {
                typeof(Control)
                    .GetProperty(
                        "DoubleBuffered",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(control, true, null);
            }
            catch
            {
            }
        }

        private void WireUiEvents()
        {
            btnPlay.Click -= BtnPlay_Click;
            btnPlay.Click += BtnPlay_Click;

            btnLeft.Click -= BtnLeft_Click;
            btnLeft.Click += BtnLeft_Click;

            btnRight.Click -= BtnRight_Click;
            btnRight.Click += BtnRight_Click;

            lstCatalogRows.SelectedIndexChanged -=
                LstCatalogRows_SelectedIndexChanged;
            lstCatalogRows.SelectedIndexChanged +=
                LstCatalogRows_SelectedIndexChanged;

            lstTubFolders.SelectedIndexChanged -=
                LstTubFolders_SelectedIndexChanged;
            lstTubFolders.SelectedIndexChanged +=
                LstTubFolders_SelectedIndexChanged;
            lstTubFolders.ItemCheck -=
                LstTubFolders_ItemCheck;
            lstTubFolders.ItemCheck +=
                LstTubFolders_ItemCheck;

            btnAddTubFolder.Click -= BtnAddTubFolder_Click;
            btnAddTubFolder.Click += BtnAddTubFolder_Click;

            btnRemoveTubFolder.Click -= BtnRemoveTubFolder_Click;
            btnRemoveTubFolder.Click += BtnRemoveTubFolder_Click;

            cmbSpeed.SelectedIndexChanged -=
                CmbSpeed_SelectedIndexChanged;
            cmbSpeed.SelectedIndexChanged +=
                CmbSpeed_SelectedIndexChanged;

            btnCleanData.Click -= BtnCleanData_Click;
            btnCleanData.Click += BtnCleanData_Click;

            btnRestoreData.Click -= BtnRestoreData_Click;
            btnRestoreData.Click += BtnRestoreData_Click;

            btnModelDlt.Click += BtnModelDlt_Click;

            btnNameCh.Click += BtnNameCh_Click;

            btnModelRestore.Click += BtnModelRestore_Click;

            btnImportModel.Click += BtnImportModel_Click;

            lstModels.SelectedIndexChanged += LstModels_SelectedIndexChanged;
            lstModelTrash.SelectedIndexChanged += LstModelTrash_SelectedIndexChanged;

            picDriveImage.Resize += (s, e) =>
            {
                currentPreviewRenderSize = Size.Empty;
            };

            cmbSpeed.SelectedIndex = 1;
        }

        private void InitializeCatalogListDrawing()
        {
            lstCatalogRows.SelectionMode = SelectionMode.MultiExtended;
            lstCatalogRows.DrawMode = DrawMode.OwnerDrawFixed;
            lstCatalogRows.DrawItem += LstCatalogRows_DrawItem;
        }

        private void InitializeResponsiveLayout()
        {
            CaptureResponsiveSnapshot();

            MinimumSize = new Size(1000, 650);
            responsiveResizeTimer.Interval = 80;
            responsiveResizeTimer.Tick += (s, e) =>
            {
                responsiveResizeTimer.Stop();
                ApplyResponsiveLayout();
            };
            Resize -= FrmNewtrainer_Resize;
            Resize += FrmNewtrainer_Resize;
        }

        private void CaptureResponsiveSnapshot()
        {
            responsiveBaseClientSize =
                ClientSize.Width > 0 && ClientSize.Height > 0
                    ? ClientSize
                    : new Size(1600, 900);

            originalControlBounds.Clear();
            originalControlFontSizes.Clear();
            CaptureResponsiveControl(this);
        }

        private void CaptureResponsiveControl(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                originalControlBounds[control] = control.Bounds;
                originalControlFontSizes[control] = control.Font.Size;

                if (control.HasChildren)
                {
                    CaptureResponsiveControl(control);
                }
            }
        }

        private void FrmNewtrainer_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                return;
            }

            responsiveResizeTimer.Stop();
            responsiveResizeTimer.Start();
        }

        private void ApplyResponsiveLayout()
        {
            if (isApplyingResponsiveLayout ||
                WindowState == FormWindowState.Minimized ||
                responsiveBaseClientSize.Width <= 0 ||
                responsiveBaseClientSize.Height <= 0)
            {
                return;
            }

            try
            {
                isApplyingResponsiveLayout = true;
                SuspendResponsiveLayout(this);

                float scaleX = ClientSize.Width / (float)responsiveBaseClientSize.Width;
                float scaleY = ClientSize.Height / (float)responsiveBaseClientSize.Height;
                float fontScale = Math.Max(0.75f, Math.Min(1.35f, Math.Min(scaleX, scaleY)));

                ApplyResponsiveControl(this, scaleX, scaleY, fontScale);
                UpdatePlaybackButtonIcons();
                ResizeModelColumns();
            }
            finally
            {
                ResumeResponsiveLayout(this);
                isApplyingResponsiveLayout = false;
            }
        }

        private void SuspendResponsiveLayout(Control parent)
        {
            parent.SuspendLayout();

            foreach (Control control in parent.Controls)
            {
                if (control.HasChildren)
                {
                    SuspendResponsiveLayout(control);
                }
            }
        }

        private void ResumeResponsiveLayout(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control.HasChildren)
                {
                    ResumeResponsiveLayout(control);
                }
            }

            parent.ResumeLayout(false);
        }

        private void ApplyResponsiveControl(Control parent, float scaleX, float scaleY, float fontScale)
        {
            foreach (Control control in parent.Controls)
            {
                if (originalControlBounds.TryGetValue(control, out Rectangle bounds))
                {
                    Rectangle scaledBounds = new Rectangle(
                        (int)Math.Round(bounds.X * scaleX),
                        (int)Math.Round(bounds.Y * scaleY),
                        Math.Max(1, (int)Math.Round(bounds.Width * scaleX)),
                        Math.Max(1, (int)Math.Round(bounds.Height * scaleY)));

                    if (control.Dock == DockStyle.Top || control.Dock == DockStyle.Bottom)
                    {
                        control.Height = scaledBounds.Height;
                    }
                    else if (control.Dock == DockStyle.Left || control.Dock == DockStyle.Right)
                    {
                        control.Width = scaledBounds.Width;
                    }
                    else if (control.Dock == DockStyle.None)
                    {
                        control.Bounds = scaledBounds;
                    }
                }

                if (originalControlFontSizes.TryGetValue(control, out float fontSize))
                {
                    float scaledFont = Math.Max(6f, fontSize * fontScale);
                    if (Math.Abs(control.Font.Size - scaledFont) > 0.1f)
                    {
                        control.Font = new Font(control.Font.FontFamily, scaledFont, control.Font.Style);
                    }
                }

                if (control.HasChildren)
                {
                    ApplyResponsiveControl(control, scaleX, scaleY, fontScale);
                }
            }
        }

        private void ResizeModelColumns()
        {
            if (lstModels == null || lstModels.Columns.Count < 3)
            {
                return;
            }

            int width = Math.Max(320, lstModels.ClientSize.Width);
            colModelNo.Width = Math.Max(45, (int)Math.Round(width * 0.11));
            colModelName.Width = Math.Max(120, (int)Math.Round(width * 0.34));
            colModelPath.Width = Math.Max(150, width - colModelNo.Width - colModelName.Width - 8);

            if (lstModelTrash != null && lstModelTrash.Columns.Count >= 3)
            {
                int trashWidth = Math.Max(320, lstModelTrash.ClientSize.Width);
                colTrashNo.Width = Math.Max(45, (int)Math.Round(trashWidth * 0.11));
                colTrashName.Width = Math.Max(120, (int)Math.Round(trashWidth * 0.34));
                colTrashPath.Width = Math.Max(150, trashWidth - colTrashNo.Width - colTrashName.Width - 8);
            }
        }

        // =====================================================
        // 브라우저 감시
        // =====================================================

        private void InitializeBrowserMonitor()
        {
            browserMonitorTimer.Interval = 5000;

            browserMonitorTimer.Tick += (s, e) =>
            {
                if (
                    browserProcess != null &&
                    browserProcess.HasExited)
                {
                    TryRestartBrowser();
                }
            };
        }

        // =====================================================
        // WSL/mycar 경로 탐색
        // =====================================================

        private void InitializeWSLPaths()
        {
            // 사용 가능한 Ubuntu 배포판을 찾고, 그 안의 mycar 프로젝트 위치를 기준 경로로 잡습니다.
            // 실패 시 기본 Ubuntu-22.04와 /home/<user>/mycar 후보를 사용합니다.
            try
            {
                var distros = GetWSLDistros();

                wslDistroName =
                    ChooseWslDistro(distros);

                RefreshWslPaths();
            }
            catch
            {
                wslUsername =
                    string.IsNullOrWhiteSpace(wslUsername)
                        ? ""
                        : wslUsername;

                wslMycarPath =
                    string.IsNullOrWhiteSpace(wslUsername)
                        ? "/home/*/mycar"
                        : "/home/" + wslUsername + "/mycar";

                wslBasePath =
                    BuildWslSharePath(
                        "Ubuntu-22.04",
                        wslMycarPath);
            }
        }

        private void RefreshWslPaths()
        {
            wslMycarPath = ResolveMycarWslPath();

            string pathUser =
                ExtractWslUserFromMycarPath(wslMycarPath);

            wslUsername =
                string.IsNullOrWhiteSpace(pathUser)
                    ? GetWSLUserName()
                    : pathUser;

            wslBasePath =
                BuildWslSharePath(
                    wslDistroName,
                    wslMycarPath);
        }

        private string BuildWslSharePath(
            string distroName,
            string mycarWslPath)
        {
            string relativePath =
                mycarWslPath
                    .Trim()
                    .TrimStart('/')
                    .Replace('/', '\\');

            string wslDollarPath =
                $@"\\wsl$\{distroName}\{relativePath}";

            string wslLocalhostPath =
                $@"\\wsl.localhost\{distroName}\{relativePath}";

            if (Directory.Exists(wslDollarPath))
            {
                return wslDollarPath;
            }

            if (Directory.Exists(wslLocalhostPath))
            {
                return wslLocalhostPath;
            }

            return wslDollarPath;
        }

        private string ChooseWslDistro(
            List<string> distros)
        {
            List<string> orderedDistros =
                OrderWslDistrosByPreference(distros);

            foreach (string distro in orderedDistros)
            {
                if (
                    TryResolveMycarFromWslShare(
                        distro,
                        out _,
                        out _))
                {
                    return distro;
                }
            }

            foreach (string distro in orderedDistros)
            {
                if (
                    TryRunWslCommand(
                        distro,
                        BuildMycarProbeCommand("test -f \"$MYCAR_PATH/train.py\"")))
                {
                    return distro;
                }
            }

            foreach (string distro in orderedDistros)
            {
                if (
                    TryRunWslCommand(
                        distro,
                        BuildMycarProbeCommand("test -d \"$MYCAR_PATH\"")))
                {
                    return distro;
                }
            }

            if (orderedDistros.Count > 0)
            {
                return orderedDistros[0];
            }

            return "Ubuntu-22.04";
        }

        private List<string> OrderWslDistrosByPreference(
            List<string> distros)
        {
            List<string> ordered =
                new List<string>();

            string[] preferredNames =
                new string[]
                {
                    "Ubuntu-22.04",
                    "Ubuntu22.04",
                    "Ubuntu 22.04",
                    "Ubuntu"
                };

            foreach (string preferredName in preferredNames)
            {
                foreach (string distro in distros)
                {
                    if (
                        string.Equals(
                            distro,
                            preferredName,
                            StringComparison.OrdinalIgnoreCase) &&
                        !ordered.Contains(distro))
                    {
                        ordered.Add(distro);
                    }
                }
            }

            foreach (string distro in distros)
            {
                if (!ordered.Contains(distro))
                {
                    ordered.Add(distro);
                }
            }

            return ordered;
        }

        private string BuildMycarProbeCommand(string testCommand)
        {
            return
                BuildMycarResolverScript(false) +
                testCommand;
        }

        private string ResolveMycarWslPath()
        {
            if (
                TryResolveMycarFromWslShare(
                    wslDistroName,
                    out string shareWslPath,
                    out _))
            {
                return shareWslPath;
            }

            return
                RunWslCommandForOutput(
                    BuildMycarResolverScript(true) +
                    "cd \"$MYCAR_PATH\" && pwd");
        }

        private bool TryResolveMycarFromWslShare(
            string distroName,
            out string mycarWslPath,
            out string mycarWindowsPath)
        {
            mycarWslPath = "";
            mycarWindowsPath = "";

            string[] shareRoots =
                new string[]
                {
                    $@"\\wsl$\{distroName}\home",
                    $@"\\wsl.localhost\{distroName}\home"
                };

            foreach (string shareRoot in shareRoots)
            {
                try
                {
                    if (!Directory.Exists(shareRoot))
                    {
                        continue;
                    }

                    foreach (
                        string userDirectory in
                        Directory.GetDirectories(shareRoot))
                    {
                        string candidate =
                            Path.Combine(
                                userDirectory,
                                "mycar");

                        if (
                            File.Exists(
                                Path.Combine(
                                    candidate,
                                    "train.py")))
                        {
                            string userName =
                                Path.GetFileName(userDirectory);

                            mycarWindowsPath = candidate;
                            mycarWslPath =
                                "/home/" +
                                userName +
                                "/mycar";

                            return true;
                        }
                    }
                }
                catch
                {

                }
            }

            return false;
        }

        private string ExtractWslUserFromMycarPath(string mycarWslPath)
        {
            if (string.IsNullOrWhiteSpace(mycarWslPath))
            {
                return "";
            }

            string normalized =
                mycarWslPath
                    .Replace("\\", "/")
                    .Trim('/');

            string[] parts =
                normalized.Split(
                    new char[] { '/' },
                    StringSplitOptions.RemoveEmptyEntries);

            if (
                parts.Length >= 3 &&
                string.Equals(
                    parts[0],
                    "home",
                    StringComparison.OrdinalIgnoreCase))
            {
                return parts[1];
            }

            return "";
        }

        private string BuildMycarResolverScript(bool requireTrainPy)
        {
            string requiredFile =
                requireTrainPy
                    ? "train.py"
                    : "";

            string requirementCheck =
                string.IsNullOrWhiteSpace(requiredFile)
                    ? "if [ -d \"$candidate\" ]; then MYCAR_PATH=\"$candidate\"; break; fi; "
                    : "if [ -f \"$candidate/" + requiredFile + "\" ]; then MYCAR_PATH=\"$candidate\"; break; fi; ";

            string fallbackFind =
                string.IsNullOrWhiteSpace(requiredFile)
                    ? "MYCAR_PATH=$(find /home -maxdepth 4 -type d -name mycar 2>/dev/null | head -n 1 || true); "
                    : "MYCAR_TRAIN=$(find /home -maxdepth 5 -path '*/mycar/" + requiredFile + "' 2>/dev/null | head -n 1 || true); " +
                      "if [ -n \"$MYCAR_TRAIN\" ]; then MYCAR_PATH=$(dirname \"$MYCAR_TRAIN\"); fi; ";

            string missingMessage =
                string.IsNullOrWhiteSpace(requiredFile)
                    ? "mycar folder was not found in WSL."
                    : "mycar folder with train.py was not found in WSL.";

            return
                "MYCAR_PATH=''; " +
                "WSL_USER=$(id -un 2>/dev/null || whoami 2>/dev/null || true); " +
                "USER_HOME=$(getent passwd \"$WSL_USER\" 2>/dev/null | cut -d: -f6); " +
                "if [ -z \"$USER_HOME\" ]; then USER_HOME=\"$HOME\"; fi; " +
                "for candidate in \"$USER_HOME/mycar\" \"$HOME/mycar\" \"/home/$WSL_USER/mycar\" /home/*/mycar; do " +
                "[ -e \"$candidate\" ] || continue; " +
                requirementCheck +
                "done; " +
                "if [ -z \"$MYCAR_PATH\" ]; then " +
                fallbackFind +
                "fi; " +
                "if [ -z \"$MYCAR_PATH\" ]; then " +
                "echo '" + missingMessage + "' >&2; " +
                "echo 'WSL user: '\"$WSL_USER\" >&2; " +
                "echo 'WSL home: '\"$USER_HOME\" >&2; " +
                "echo 'Checked: $USER_HOME/mycar, $HOME/mycar, /home/$WSL_USER/mycar, /home/*/mycar' >&2; " +
                "exit 20; " +
                "fi; ";
        }

        private string GetModelWslPath(string modelName)
        {
            string mycarPath =
                string.IsNullOrWhiteSpace(wslMycarPath)
                    ? "/home/" + wslUsername + "/mycar"
                    : wslMycarPath;

            return
                mycarPath.TrimEnd('/') +
                "/" +
                ModelDirectoryName +
                "/" +
                modelName;
        }

        private string GetRenamedModelWslPath(
            ModelRegistryEntry? oldEntry,
            string newName)
        {
            if (
                oldEntry != null &&
                !string.IsNullOrWhiteSpace(oldEntry.WslPath))
            {
                string normalized =
                    oldEntry.WslPath
                        .Replace("\\", "/")
                        .TrimEnd('/');

                int lastSlash =
                    normalized.LastIndexOf('/');

                if (lastSlash >= 0)
                {
                    return
                        normalized.Substring(0, lastSlash + 1) +
                        newName;
                }
            }

            return GetModelWslPath(newName);
        }

        private bool TryRunWslCommand(
            string distroName,
            string command)
        {
            try
            {
                ProcessStartInfo psi =
                    new ProcessStartInfo
                    {
                        FileName = "wsl.exe",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                AddWslArguments(
                    psi,
                    command,
                    distroName);

                using (Process proc =
                    Process.Start(psi))
                {
                    proc.WaitForExit();

                    return proc.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private List<string> GetWSLDistros()
        {
            List<string> distros =
                new List<string>();

            try
            {
                ProcessStartInfo psi =
                    new ProcessStartInfo
                    {
                        FileName = "wsl.exe",

                        Arguments =
                            "--list --quiet",

                        RedirectStandardOutput = true,

                        UseShellExecute = false,

                        CreateNoWindow = true
                    };

                using (Process proc =
                    Process.Start(psi))
                {
                    using (StreamReader reader =
                        proc.StandardOutput)
                    {
                        string line;

                        while (
                            (line = reader.ReadLine())
                            != null)
                        {
                            line =
                                line
                                    .Replace("\0", "")
                                    .Trim();

                            if (
                                !string.IsNullOrEmpty(
                                    line) &&
                                !line.StartsWith(
                                    "Windows",
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                distros.Add(line);
                            }
                        }
                    }

                    proc.WaitForExit();
                }
            }
            catch
            {
                distros.Add("Ubuntu");
            }

            return distros;
        }

        private string GetWSLUserName()
        {
            try
            {
                ProcessStartInfo psi =
                    new ProcessStartInfo
                    {
                        FileName = "wsl.exe",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                AddWslArguments(
                    psi,
                    "WSL_USER=$(whoami 2>/dev/null || true); " +
                    "if [ -z \"$WSL_USER\" ]; then WSL_USER=\"$USER\"; fi; " +
                    "if [ -z \"$WSL_USER\" ]; then WSL_USER=$(basename \"$HOME\"); fi; " +
                    "printf '%s' \"$WSL_USER\"");

                using (Process proc =
                    Process.Start(psi))
                {
                    string user =
                        proc.StandardOutput
                            .ReadToEnd()
                            .Replace("\0", "")
                            .Trim();

                    proc.WaitForExit();

                    if (
                        proc.ExitCode == 0 &&
                        !string.IsNullOrWhiteSpace(user))
                    {
                        return user;
                    }
                }
            }
            catch
            {

            }

            return "";
        }

        private void AddWslArguments(
            ProcessStartInfo psi,
            string command)
        {
            AddWslArguments(
                psi,
                command,
                wslDistroName);
        }

        private void AddWslArguments(
            ProcessStartInfo psi,
            string command,
            string distroName)
        {
            if (!string.IsNullOrWhiteSpace(distroName))
            {
                psi.ArgumentList.Add("-d");
                psi.ArgumentList.Add(distroName);

                if (!string.IsNullOrWhiteSpace(wslUsername))
                {
                    psi.ArgumentList.Add("-u");
                    psi.ArgumentList.Add(wslUsername);
                }

                psi.ArgumentList.Add("--");
            }

            psi.ArgumentList.Add("bash");
            psi.ArgumentList.Add("-lc");
            psi.ArgumentList.Add(command);
        }

        private string QuoteForBash(string value)
        {
            return
                "'" +
                value.Replace("'", "'\"'\"'") +
                "'";
        }

        private string RunWslCommandForOutput(string command)
        {
            ProcessStartInfo psi =
                new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

            AddWslArguments(psi, command);

            using (Process proc = Process.Start(psi))
            {
                string output =
                    proc.StandardOutput
                        .ReadToEnd()
                        .Replace("\0", "")
                        .Trim();

                string error =
                    proc.StandardError
                        .ReadToEnd()
                        .Replace("\0", "")
                        .Trim();

                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        string.IsNullOrWhiteSpace(error)
                            ? "WSL 명령 실행에 실패했습니다."
                            : error);
                }

                return output;
            }
        }

        private bool IsWslAvailable(out string message)
        {
            try
            {
                RunWslCommandForOutput("printf ready");
                message = "";
                return true;
            }
            catch (Exception ex)
            {
                message =
                    "WSL을 실행할 수 없습니다.\n\n" +
                    "Windows에 WSL과 Ubuntu 배포판이 설치되어 있고 " +
                    "초기 설정이 완료되었는지 확인하세요.\n\n" +
                    ex.Message;

                return false;
            }
        }

        private string ConvertWindowsPathToWslPath(string windowsPath)
        {
            if (string.IsNullOrWhiteSpace(windowsPath))
            {
                throw new ArgumentException(
                    "경로가 비어 있습니다.",
                    nameof(windowsPath));
            }

            string normalizedPath =
                Path.GetFullPath(windowsPath);

            string[] uncPrefixes =
                new string[]
                {
                    @"\\wsl$\",
                    @"\\wsl.localhost\"
                };

            foreach (string uncPrefix in uncPrefixes)
            {
                if (
                    !normalizedPath.StartsWith(
                        uncPrefix,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string remaining =
                    normalizedPath.Substring(
                        uncPrefix.Length);

                int separatorIndex =
                    remaining.IndexOf('\\');

                if (separatorIndex >= 0)
                {
                    return
                        "/" +
                        remaining
                            .Substring(separatorIndex + 1)
                            .Replace('\\', '/');
                }
            }

            string command =
                "wslpath -a " +
                QuoteForBash(normalizedPath);

            return RunWslCommandForOutput(command);
        }

        private string ResolveTubWslPath(
            string tubWindowsPath,
            string mycarWslPath)
        {
            // train.py는 WSL 내부에서 실행되므로 Windows 폴더 경로를 WSL 경로로 변환합니다.
            // tub가 WSL 공유 경로(mycar) 아래에 있으면 mycar 기준 상대 경로를 보존합니다.
            string fullTubPath =
                Path.GetFullPath(tubWindowsPath);

            string fullBasePath =
                Path.GetFullPath(wslBasePath);

            if (
                fullTubPath.StartsWith(
                    fullBasePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                string relativePath =
                    Path.GetRelativePath(
                        fullBasePath,
                        fullTubPath);

                if (
                    relativePath == "." ||
                    string.IsNullOrWhiteSpace(relativePath))
                {
                    return mycarWslPath;
                }

                if (!relativePath.StartsWith(".."))
                {
                    return
                        mycarWslPath.TrimEnd('/') +
                        "/" +
                        relativePath.Replace('\\', '/');
                }
            }

            string convertedPath =
                ConvertWindowsPathToWslPath(fullTubPath);

            string expectedPrefix =
                mycarWslPath.TrimEnd('/') + "/";

            if (
                convertedPath.StartsWith(
                    "/home/mycar/",
                    StringComparison.OrdinalIgnoreCase))
            {
                convertedPath =
                    expectedPrefix +
                    convertedPath.Substring(
                        "/home/mycar/".Length);
            }

            return convertedPath;
        }

        private string GetSelectedModelName()
        {
            ModelRegistryEntry entry =
                GetSelectedModelEntry();

            if (entry != null)
            {
                return entry.Name;
            }

            return
                lstModels.SelectedItems.Count == 0
                    ? ""
                    : (
                        lstModels.SelectedItems[0].SubItems.Count > 1
                            ? lstModels.SelectedItems[0].SubItems[1].Text
                            : lstModels.SelectedItems[0].Text)
                        .Replace("\0", "")
                        .Trim();
        }

        private ModelRegistryEntry GetSelectedModelEntry()
        {
            if (
                lstModels.SelectedItems.Count > 0 &&
                lstModels.SelectedItems[0].Tag is ModelRegistryEntry entry)
            {
                return entry;
            }

            string selectedModel =
                GetSelectedModelName();

            if (string.IsNullOrWhiteSpace(selectedModel))
            {
                return null;
            }

            return
                LoadModelRegistry()
                    .FirstOrDefault(
                        entry =>
                            !entry.IsDeleted &&
                            (string.Equals(
                                    entry.Name,
                                    selectedModel,
                                    StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(
                                    entry.Name,
                            selectedModel + ".h5",
                            StringComparison.OrdinalIgnoreCase)));
        }

        private List<ModelRegistryEntry> GetSelectedModelEntries()
        {
            if (lstModels.SelectedItems.Count == 0)
            {
                return new List<ModelRegistryEntry>();
            }

            return lstModels.SelectedItems
                .Cast<ListViewItem>()
                .Select(item => item.Tag as ModelRegistryEntry)
                .Where(entry => entry != null)
                .Cast<ModelRegistryEntry>()
                .ToList();
        }

        private void InitializeTrainerButtonStyles()
        {
            StyleTrainerButton(btnAddTubFolder, Color.FromArgb(56, 118, 198), Color.White);
            StyleTrainerButton(btnRemoveTubFolder, Color.FromArgb(83, 105, 136), Color.White);
            StyleTrainerButton(btnCleanData, Color.FromArgb(204, 91, 84), Color.White);
            StyleTrainerButton(btnRestoreData, Color.FromArgb(75, 143, 112), Color.White);
            StyleTrainerButton(btnTrain, Color.FromArgb(56, 118, 198), Color.White);
            StyleTrainerButton(btnDrive, Color.FromArgb(75, 143, 112), Color.White);
            StyleTrainerButton(btnImportModel, Color.FromArgb(56, 118, 198), Color.White);
            StyleTrainerButton(btnNameCh, Color.FromArgb(75, 143, 112), Color.White);
            StyleTrainerButton(btnModelDlt, Color.FromArgb(204, 91, 84), Color.White);
            StyleTrainerButton(btnModelRestore, Color.FromArgb(56, 118, 198), Color.White);

            StylePlaybackButton(btnLeft);
            StylePlaybackButton(btnPlay);
            StylePlaybackButton(btnRight);

            btnLeft.Text = "";
            btnPlay.Text = "";
            btnRight.Text = "";

            btnLeft.Resize += (s, e) => UpdatePlaybackButtonIcons();
            btnPlay.Resize += (s, e) => UpdatePlaybackButtonIcons();
            btnRight.Resize += (s, e) => UpdatePlaybackButtonIcons();

            UpdatePlaybackButtonIcons();
        }

        private void StyleTrainerButton(Button button, Color backColor, Color foreColor)
        {
            if (button == null)
            {
                return;
            }

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Cursor = Cursors.Hand;
            button.Font = new Font(button.Font.FontFamily, button.Font.Size, FontStyle.Bold);
        }

        private void StylePlaybackButton(Button button)
        {
            if (button == null)
            {
                return;
            }

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = Color.FromArgb(232, 238, 247);
            button.ForeColor = Color.FromArgb(26, 54, 93);
            button.Cursor = Cursors.Hand;
            button.ImageAlign = ContentAlignment.MiddleCenter;
        }

        private void UpdatePlaybackButtonIcons()
        {
            if (btnLeft == null || btnPlay == null || btnRight == null)
            {
                return;
            }

            SetPlaybackButtonImage(
                btnLeft,
                Data_Manager.Properties.Resources.arrow1_left);

            SetPlaybackButtonImage(
                btnPlay,
                playbackTimer != null && playbackTimer.Enabled
                    ? Data_Manager.Properties.Resources.pause
                    : Data_Manager.Properties.Resources.PlaySlide4655096);

            SetPlaybackButtonImage(
                btnRight,
                Data_Manager.Properties.Resources.arrow1_right);
        }

        private void SetPlaybackButtonImage(Button button, Image image)
        {
            if (button == null || image == null)
            {
                return;
            }

            int imageSize =
                Math.Max(
                    12,
                    Math.Min(button.Width - 18, button.Height - 12));

            Image oldImage = button.Image;
            button.Image =
                AD_AI_LearningData_Editor.IconProperty.ResizeImage(
                    image,
                    imageSize,
                    imageSize);

            if (oldImage != null &&
                !ReferenceEquals(oldImage, button.Image))
            {
                oldImage.Dispose();
            }

            button.ImageAlign = ContentAlignment.MiddleCenter;
        }

        private ModelRegistryEntry GetSelectedTrashModelEntry()
        {
            if (
                lstModelTrash.SelectedItems.Count > 0 &&
                lstModelTrash.SelectedItems[0].Tag is ModelRegistryEntry entry)
            {
                return entry;
            }

            return null;
        }

        private List<ModelRegistryEntry> GetSelectedTrashModelEntries()
        {
            if (lstModelTrash.SelectedItems.Count == 0)
            {
                return new List<ModelRegistryEntry>();
            }

            return lstModelTrash.SelectedItems
                .Cast<ListViewItem>()
                .Select(item => item.Tag as ModelRegistryEntry)
                .Where(entry => entry != null)
                .Cast<ModelRegistryEntry>()
                .ToList();
        }

        private void LstModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstModels.SelectedItems.Count > 0 && lstModelTrash != null)
            {
                lstModelTrash.SelectedItems.Clear();
            }
        }

        private void LstModelTrash_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstModelTrash.SelectedItems.Count > 0 && lstModels != null)
            {
                lstModels.SelectedItems.Clear();
            }
        }

        private ListViewItem CreateModelListItem(
            ModelRegistryEntry entry,
            int number)
        {
            ListViewItem item =
                new ListViewItem(number.ToString());

            item.SubItems.Add(
                Path.GetFileNameWithoutExtension(entry.Name));
            item.SubItems.Add(entry.WindowsPath);
            item.Tag = entry;

            if (entry.IsDeleted)
            {
                item.ForeColor = Color.FromArgb(190, 52, 52);
                item.BackColor = Color.FromArgb(255, 244, 244);
            }

            return item;
        }

        private List<ModelRegistryEntry> LoadModelRegistry()
        {
            try
            {
                if (!File.Exists(modelRegistryPath))
                {
                    return new List<ModelRegistryEntry>();
                }

                string json =
                    File.ReadAllText(modelRegistryPath);

                return
                    JsonSerializer.Deserialize
                        <List<ModelRegistryEntry>>(json) ??
                    new List<ModelRegistryEntry>();
            }
            catch
            {
                return new List<ModelRegistryEntry>();
            }
        }

        private void SaveModelRegistry(
            List<ModelRegistryEntry> entries)
        {
            string directory =
                Path.GetDirectoryName(modelRegistryPath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            JsonSerializerOptions options =
                new JsonSerializerOptions()
                {
                    WriteIndented = true
                };

            string json =
                JsonSerializer.Serialize(entries, options);

            string existingJson =
                File.Exists(modelRegistryPath)
                    ? File.ReadAllText(modelRegistryPath)
                    : "";

            File.WriteAllText(
                modelRegistryPath,
                json);

            if (
                !string.Equals(
                    existingJson,
                    json,
                    StringComparison.Ordinal))
            {
                SharedModelRegistry.NotifyChanged();
            }
        }

        private void UpsertModelRegistry(
            ModelRegistryEntry model)
        {
            model.IsDeleted = false;

            List<ModelRegistryEntry> entries =
                LoadModelRegistry();

            entries.RemoveAll(
                entry =>
                    !string.IsNullOrWhiteSpace(model.WindowsPath)
                        ? string.Equals(
                            entry.WindowsPath,
                            model.WindowsPath,
                            StringComparison.OrdinalIgnoreCase)
                        : string.Equals(
                            entry.Name,
                            model.Name,
                            StringComparison.OrdinalIgnoreCase));

            entries.Add(model);

            SaveModelRegistry(entries);
        }

        private void AddOrUpdateModelList(
            ModelRegistryEntry model)
        {
            for (int i = 0; i < lstModels.Items.Count; i++)
            {
                string itemName =
                    lstModels.Items[i].Tag is ModelRegistryEntry entry
                        ? entry.Name
                        : lstModels.Items[i].SubItems.Count > 1
                            ? lstModels.Items[i].SubItems[1].Text
                            : lstModels.Items[i].Text;

                if (
                    string.Equals(
                        itemName,
                        model.Name,
                        StringComparison.OrdinalIgnoreCase))
                {
                    ListViewItem item =
                        CreateModelListItem(
                            model,
                            i + 1);

                    lstModels.Items.RemoveAt(i);
                    lstModels.Items.Insert(i, item);
                    item.Selected = true;
                    item.EnsureVisible();
                    return;
                }
            }

            ListViewItem newItem =
                CreateModelListItem(
                    model,
                    lstModels.Items.Count + 1);

            lstModels.Items.Add(newItem);
            newItem.Selected = true;
            newItem.EnsureVisible();
        }

        // =====================================================
        // WINDOW EFFECT
        // =====================================================

        private void FlashWindow()
        {
            try
            {
                FLASHWINFO fwi =
                    new FLASHWINFO();

                fwi.cbSize =
                    Convert.ToUInt32(
                        Marshal.SizeOf(fwi));

                fwi.hwnd = this.Handle;

                fwi.dwFlags =
                    FLASHW_ALL |
                    FLASHW_TIMERNOFG;

                fwi.uCount = 5;

                fwi.dwTimeout = 500;

                FlashWindowEx(ref fwi);
            }
            catch
            {

            }
        }

        private void ActivateWindow()
        {
            try
            {
                if (IsIconic(this.Handle))
                {
                    ShowWindow(
                        this.Handle,
                        SW_RESTORE);
                }

                SetForegroundWindow(
                    this.Handle);
            }
            catch
            {

            }
        }

        private void AttentionWindow()
        {
            try
            {
                FlashWindow();

                ActivateWindow();
            }
            catch
            {

            }
        }

        // =====================================================
        // tub 폴더 선택과 catalog 로드
        // =====================================================

        private void BtnLoadData_Click(
            object sender,
            EventArgs e)
        {
            using (FolderBrowserDialog fbd =
                new FolderBrowserDialog())
            {
                fbd.Description =
                    "mycar/data 폴더 선택";

                if (
                    fbd.ShowDialog() ==
                    DialogResult.OK)
                {
                    LoadTubFolderSelection(fbd.SelectedPath, replaceExisting: true);
                }
            }
        }

        private void BtnAddTubFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "추가할 tub 폴더 또는 tub들이 들어 있는 폴더 선택";

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    LoadTubFolderSelection(fbd.SelectedPath, replaceExisting: false);
                }
            }
        }

        private void BtnRemoveTubFolder_Click(object sender, EventArgs e)
        {
            if (lstTubFolders.SelectedIndex < 0 || lstTubFolders.SelectedIndex >= selectedTubFolders.Count)
            {
                return;
            }

            int removeIndex = lstTubFolders.SelectedIndex;
            selectedTubFolders.RemoveAt(removeIndex);
            RefreshTubFolderList();

            if (selectedTubFolders.Count == 0)
            {
                selectedDataPath = "";
                integratedCatalogList.Clear();
                catalogDisplayRows.Clear();
                deletedCatalogIndexes.Clear();
                lstCatalogRows.Items.Clear();
                ReleasePreviewImage();
                return;
            }

            lstTubFolders.SelectedIndex = Math.Min(removeIndex, selectedTubFolders.Count - 1);
            LoadCheckedTubCatalogRows(showMessage: false);
        }

        private void LstTubFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstTubFolders.SelectedIndex < 0 || lstTubFolders.SelectedIndex >= selectedTubFolders.Count)
            {
                return;
            }

            selectedDataPath = selectedTubFolders[lstTubFolders.SelectedIndex];
            SelectFirstCatalogRowForTub(selectedDataPath);
        }

        private void LstTubFolders_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            BeginInvoke(
                new Action(
                    () => LoadCheckedTubCatalogRows(showMessage: false)));
        }

        private void LoadTubFolderSelection(string folderPath, bool replaceExisting)
        {
            List<string> tubFolders = FindTubFolders(folderPath);

            if (tubFolders.Count == 0)
            {
                MessageBox.Show("선택한 폴더에서 tub 데이터를 찾지 못했습니다.");
                return;
            }

            if (replaceExisting)
            {
                selectedTubFolders.Clear();
            }

            foreach (string tubFolder in tubFolders)
            {
                if (!selectedTubFolders.Any(path => string.Equals(path, tubFolder, StringComparison.OrdinalIgnoreCase)))
                {
                    selectedTubFolders.Add(tubFolder);
                }
            }

            selectedTubFolders = selectedTubFolders
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            RefreshTubFolderList();

            if (selectedTubFolders.Count > 0)
            {
                lstTubFolders.SelectedIndex = 0;
            }

            LoadCheckedTubCatalogRows(showMessage: false);
        }

        private List<string> FindTubFolders(string folderPath)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                return result;
            }

            void Search(string path)
            {
                if (IsTubDataFolder(path))
                {
                    result.Add(Path.GetFullPath(path));
                    return;
                }

                foreach (string child in Directory.GetDirectories(path))
                {
                    Search(child);
                }
            }

            Search(folderPath);

            return result
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private bool IsTubDataFolder(string folderPath)
        {
            return !string.IsNullOrWhiteSpace(folderPath) &&
                Directory.Exists(folderPath) &&
                !IsUploadedFileDataFolder(folderPath) &&
                Directory.GetFiles(folderPath, "catalog_*.catalog", SearchOption.TopDirectoryOnly).Length > 0 &&
                File.Exists(Path.Combine(folderPath, "manifest.json"));
        }

        private bool IsUploadedFileDataFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return false;
            }

            try
            {
                string uploadedDataPath = Path.GetFullPath(
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "UploadedFile",
                        "data")).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                string candidatePath = Path.GetFullPath(folderPath)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                return string.Equals(candidatePath, uploadedDataPath, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private void RefreshTubFolderList()
        {
            lstTubFolders.BeginUpdate();
            try
            {
                lstTubFolders.Items.Clear();

                foreach (string tubFolder in selectedTubFolders)
                {
                    lstTubFolders.Items.Add(tubFolder, true);
                }
            }
            finally
            {
                lstTubFolders.EndUpdate();
            }
        }

        private List<string> GetCheckedTubFolders()
        {
            return lstTubFolders.CheckedItems
                .Cast<object>()
                .Select(item => item?.ToString() ?? "")
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Where(IsTubDataFolder)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public void LoadDataFolder(string folderPath)
        {
            string fullFolderPath = Path.GetFullPath(folderPath);

            if (IsTubDataFolder(folderPath) &&
                !selectedTubFolders.Any(path => string.Equals(path, fullFolderPath, StringComparison.OrdinalIgnoreCase)))
            {
                selectedTubFolders.Add(fullFolderPath);
                selectedTubFolders = selectedTubFolders
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                RefreshTubFolderList();
            }

            int listIndex = selectedTubFolders.FindIndex(path => string.Equals(path, fullFolderPath, StringComparison.OrdinalIgnoreCase));
            if (listIndex >= 0 && lstTubFolders.SelectedIndex != listIndex)
            {
                lstTubFolders.SelectedIndex = listIndex;
            }

            LoadCheckedTubCatalogRows(showMessage: false);
        }

        private void LoadDataFolder(string folderPath, bool showMessage)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                if (showMessage)
                {
                    MessageBox.Show("데이터 폴더를 찾을 수 없습니다.");
                }

                return;
            }

            selectedDataPath = Path.GetFullPath(folderPath);
            LoadSingleTubCatalogRows(selectedDataPath, showMessage);
        }

        private void LoadCheckedTubCatalogRows(bool showMessage)
        {
            List<string> checkedTubFolders = GetCheckedTubFolders();

            if (checkedTubFolders.Count == 0)
            {
                integratedCatalogList.Clear();
                catalogDisplayRows.Clear();
                lstCatalogRows.BeginUpdate();
                try
                {
                    lstCatalogRows.Items.Clear();
                }
                finally
                {
                    lstCatalogRows.EndUpdate();
                }
                ReleasePreviewImage();
                return;
            }

            int tubNumber = 1;
            int loadedCount = 0;

            lstCatalogRows.BeginUpdate();
            try
            {
                integratedCatalogList.Clear();
                catalogDisplayRows.Clear();
                lstCatalogRows.Items.Clear();

                foreach (string tubFolder in checkedTubFolders)
                {
                    AddTubHeaderRow(tubNumber, checkedTubFolders.Count, tubFolder);
                    loadedCount += LoadTubCatalogRecords(tubFolder);
                    tubNumber++;
                }
            }
            finally
            {
                lstCatalogRows.EndUpdate();
            }

            if (integratedCatalogList.Count > 0)
            {
                SelectFirstCatalogRecordRow();
            }

            if (showMessage)
            {
                MessageBox.Show(
                    $"총 {checkedTubFolders.Count}개 tub, {loadedCount}개 프레임 로드 완료");
            }
        }

        private void LoadSingleTubCatalogRows(string folderPath, bool showMessage)
        {
            int loadedCount = 0;

            lstCatalogRows.BeginUpdate();
            try
            {
                integratedCatalogList.Clear();
                catalogDisplayRows.Clear();
                lstCatalogRows.Items.Clear();

                AddTubHeaderRow(1, 1, folderPath);
                loadedCount = LoadTubCatalogRecords(folderPath);
            }
            finally
            {
                lstCatalogRows.EndUpdate();
            }

            if (integratedCatalogList.Count > 0)
            {
                SelectFirstCatalogRecordRow();
            }

            if (showMessage)
            {
                MessageBox.Show(
                    $"총 {loadedCount}개 프레임 로드 완료");
            }
        }

        private void AddTubHeaderRow(int tubNumber, int totalTubCount, string tubFolder)
        {
            string text = $"[{tubNumber}/{totalTubCount}] TUB: {tubFolder}";

            catalogDisplayRows.Add(
                new CatalogListRow
                {
                    IsHeader = true,
                    Text = text
                });

            lstCatalogRows.Items.Add(text);
        }

        private int LoadTubCatalogRecords(string tubFolder)
        {
            HashSet<int> tubDeletedIndexes = ReadDeletedIndexesFromTubFolder(tubFolder);
            int loadedCount = 0;

            string[] catalogFiles =
                Directory.GetFiles(
                    tubFolder,
                    "catalog_*.catalog",
                    SearchOption.TopDirectoryOnly);

            Array.Sort(catalogFiles, StringComparer.OrdinalIgnoreCase);

            foreach (string catalogPath in catalogFiles)
            {
                int i = 0;
                foreach (string line in File.ReadLines(catalogPath))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        i++;
                        continue;
                    }

                    CatalogRecord record =
                        new CatalogRecord()
                        {
                            OriginalLine = line,
                            SourceFilePath = catalogPath,
                            LineIndex = i,
                            ImageFileName =
                                ExtractJsonValue(
                                    line,
                                    "cam/image_array"),
                            Angle =
                                ExtractJsonValue(
                                    line,
                                    "user/angle"),
                            Throttle =
                                ExtractJsonValue(
                                    line,
                                    "user/throttle"),
                            Index =
                                ExtractJsonValue(
                                    line,
                                    "_index")
                        };

                    int recordIndex = GetRecordIndex(record, i);
                    record.IsDeleted = tubDeletedIndexes.Contains(recordIndex);

                    integratedCatalogList.Add(record);
                    UpdateListBoxItem(record);
                    loadedCount++;
                    i++;
                }
            }

            return loadedCount;
        }

        // =====================================================
        // LIST UPDATE
        // =====================================================

        private HashSet<int> ReadDeletedIndexesFromTubFolder(string folderPath)
        {
            HashSet<int> deletedIndexes = new HashSet<int>();

            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                return deletedIndexes;
            }

            string[] manifestCandidates =
            {
                Path.Combine(folderPath, "manifest.json"),
                Path.Combine(folderPath, "catalog_manifest.json")
            };

            foreach (string manifestPath in manifestCandidates)
            {
                if (!File.Exists(manifestPath))
                {
                    continue;
                }

                try
                {
                    foreach (string line in File.ReadLines(manifestPath))
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        using JsonDocument document = JsonDocument.Parse(line);

                        if (document.RootElement.ValueKind != JsonValueKind.Object ||
                            !TryGetDeletedIndexesElement(document.RootElement, out JsonElement deletedElement) ||
                            deletedElement.ValueKind != JsonValueKind.Array)
                        {
                            continue;
                        }

                        foreach (JsonElement item in deletedElement.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.Number && item.TryGetInt32(out int number))
                            {
                                deletedIndexes.Add(number);
                            }
                            else if (int.TryParse(item.ToString(), out number))
                            {
                                deletedIndexes.Add(number);
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            return deletedIndexes;
        }

        private bool TryGetDeletedIndexesElement(JsonElement root, out JsonElement deletedElement)
        {
            if (root.TryGetProperty("deleted_index", out deletedElement))
            {
                return true;
            }

            deletedElement = default;
            return false;
        }

        private bool IsDeletedIndexesProperty(string propertyName)
        {
            return string.Equals(propertyName, "deleted_indexes", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(propertyName, "delete_index", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(propertyName, "delete_indexes", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(propertyName, "deleted_index", StringComparison.OrdinalIgnoreCase);
        }

        private int GetRecordIndex(CatalogRecord record, int fallbackIndex)
        {
            if (record != null && int.TryParse(record.Index, out int index))
            {
                return index;
            }

            return fallbackIndex;
        }

        private void UpdateListBoxItem(
            CatalogRecord record)
        {
            string fileName =
                Path.GetFileName(
                    record.SourceFilePath);

            string text =
                $"{fileName} | " +
                $"F_{record.Index} | " +
                $"A:{record.Angle} | " +
                $"T:{record.Throttle}";

            catalogDisplayRows.Add(
                new CatalogListRow
                {
                    IsHeader = false,
                    Text = text,
                    Record = record
                });

            lstCatalogRows.Items.Add(text);
        }

        private CatalogListRow GetCatalogListRow(int listIndex)
        {
            if (listIndex < 0 || listIndex >= catalogDisplayRows.Count)
            {
                return null;
            }

            return catalogDisplayRows[listIndex];
        }

        private bool IsCatalogHeaderRow(int listIndex)
        {
            CatalogListRow row = GetCatalogListRow(listIndex);
            return row != null && row.IsHeader;
        }

        private CatalogRecord GetCatalogRecordAtListIndex(int listIndex)
        {
            CatalogListRow row = GetCatalogListRow(listIndex);
            return row == null || row.IsHeader ? null : row.Record;
        }

        private void SelectFirstCatalogRecordRow()
        {
            for (int i = 0; i < catalogDisplayRows.Count; i++)
            {
                if (!catalogDisplayRows[i].IsHeader)
                {
                    lstCatalogRows.SelectedIndex = i;
                    return;
                }
            }
        }

        private void SelectFirstCatalogRowForTub(string tubFolder)
        {
            if (string.IsNullOrWhiteSpace(tubFolder))
            {
                return;
            }

            string fullTubFolder = Path.GetFullPath(tubFolder);

            for (int i = 0; i < catalogDisplayRows.Count; i++)
            {
                CatalogRecord record = GetCatalogRecordAtListIndex(i);
                if (record == null)
                {
                    continue;
                }

                string recordFolder = Path.GetDirectoryName(record.SourceFilePath) ?? "";
                if (string.Equals(Path.GetFullPath(recordFolder), fullTubFolder, StringComparison.OrdinalIgnoreCase))
                {
                    lstCatalogRows.SelectedIndex = i;
                    return;
                }
            }
        }

        private void LstCatalogRows_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lstCatalogRows.Items.Count)
            {
                return;
            }

            e.DrawBackground();

            CatalogListRow row = GetCatalogListRow(e.Index);

            bool isHeader =
                row != null &&
                row.IsHeader;

            bool isDeleted =
                row != null &&
                row.Record != null &&
                row.Record.IsDeleted;

            Color textColor;

            if (isHeader)
            {
                textColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                    ? Color.White
                    : Color.Purple;
            }
            else if (isDeleted)
            {
                textColor = Color.Red;
            }
            else
            {
                textColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                    ? SystemColors.HighlightText
                    : lstCatalogRows.ForeColor;
            }

            TextRenderer.DrawText(
                e.Graphics,
                lstCatalogRows.Items[e.Index]?.ToString() ?? string.Empty,
                e.Font,
                e.Bounds,
                textColor,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPrefix);

            e.DrawFocusRectangle();
        }

        // =====================================================
        // PLAYBACK
        // =====================================================

        private void PlaybackTimer_Tick(
            object sender,
            EventArgs e)
        {
            if (!MoveCatalogSelection(1))
            {
                StopCatalogPlayback();
            }
        }

        private void BtnPlay_Click(
            object sender,
            EventArgs e)
        {
            ToggleCatalogPlayback();
        }

        private void BtnLeft_Click(
            object sender,
            EventArgs e)
        {
            StopCatalogPlayback();
            MoveCatalogSelection(-1);
        }

        private void BtnRight_Click(
            object sender,
            EventArgs e)
        {
            StopCatalogPlayback();
            MoveCatalogSelection(1);
        }

        private void ToggleCatalogPlayback()
        {
            if (playbackTimer.Enabled)
            {
                StopCatalogPlayback();
                return;
            }

            if (lstCatalogRows.Items.Count == 0)
            {
                return;
            }

            playbackTimer.Interval = GetTrainerPlaybackInterval();
            playbackTimer.Start();
            UpdatePlaybackButtonIcons();
        }

        private void StopCatalogPlayback()
        {
            playbackTimer.Stop();
            UpdatePlaybackButtonIcons();
        }

        private bool MoveCatalogSelection(int delta)
        {
            if (lstCatalogRows.Items.Count == 0 || delta == 0)
            {
                return false;
            }

            int next = GetCurrentCatalogSelectionIndex(delta);

            if (next < 0)
            {
                next = delta > 0 ? -1 : lstCatalogRows.Items.Count;
            }

            do
            {
                next += delta;

                if (next < 0 || next >= lstCatalogRows.Items.Count)
                {
                    return false;
                }
            }
            while (IsCatalogHeaderRow(next));

            SelectCatalogRow(next);
            return true;
        }

        private int GetCurrentCatalogSelectionIndex(int delta)
        {
            if (lstCatalogRows.SelectedIndices.Count == 0)
            {
                return lstCatalogRows.SelectedIndex;
            }

            return delta >= 0
                ? lstCatalogRows.SelectedIndices
                    .Cast<int>()
                    .DefaultIfEmpty(lstCatalogRows.SelectedIndex)
                    .Max()
                : lstCatalogRows.SelectedIndices
                    .Cast<int>()
                    .DefaultIfEmpty(lstCatalogRows.SelectedIndex)
                    .Min();
        }

        private void SelectCatalogRow(int index)
        {
            if (index < 0 || index >= lstCatalogRows.Items.Count)
            {
                return;
            }

            lstCatalogRows.BeginUpdate();
            try
            {
                lstCatalogRows.ClearSelected();
                lstCatalogRows.SelectedIndex = index;

                int visiblePadding = 4;
                int topIndex = Math.Max(0, index - visiblePadding);
                if (lstCatalogRows.TopIndex != topIndex)
                {
                    lstCatalogRows.TopIndex = topIndex;
                }
            }
            finally
            {
                lstCatalogRows.EndUpdate();
            }
        }

        private void CmbSpeed_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            playbackTimer.Interval = GetTrainerPlaybackInterval();
        }

        private int GetTrainerPlaybackInterval()
        {
            string selected = cmbSpeed.SelectedItem?.ToString() ?? "1.0x";
            string numeric = selected.Replace("x", string.Empty);

            if (!double.TryParse(numeric, out double speed))
            {
                speed = 1.0;
            }

            return AD_AI_LearningData_Editor.frmMain.GetPlaybackIntervalForSpeed(speed);
        }

        // =====================================================
        // IMAGE VIEW
        // =====================================================

        private void LstCatalogRows_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            int idx =
                lstCatalogRows.SelectedIndex;

            if (
                idx < 0 ||
                idx >= catalogDisplayRows.Count ||
                IsCatalogHeaderRow(idx))
            {
                return;
            }

            CatalogRecord record =
                catalogDisplayRows[idx].Record;

            string imgPath =
                ResolveCatalogImagePath(record);

            if (string.Equals(currentPreviewImagePath, imgPath, StringComparison.OrdinalIgnoreCase) &&
                currentPreviewRenderSize != Size.Empty &&
                picDriveImage.Image != null)
            {
                return;
            }

            if (!File.Exists(imgPath))
            {
                ReleasePreviewImage();

                return;
            }

            try
            {
                using (
                    FileStream fs =
                    new FileStream(
                        imgPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read))
                {
                    using (
                        Image temp =
                        Image.FromStream(fs))
                    {
                        Size targetSize =
                            GetPreviewImageRenderSize(temp.Size);
                        Bitmap bitmap =
                            targetSize.Width > 0 && targetSize.Height > 0
                                ? new Bitmap(temp, targetSize)
                                : new Bitmap(temp);

                        ReleasePreviewImage();
                        picDriveImage.Image = bitmap;
                        currentPreviewImagePath = imgPath;
                        currentPreviewRenderSize = targetSize;
                    }
                }
            }
            catch
            {
                ReleasePreviewImage();
            }
        }

        private void ReleasePreviewImage()
        {
            if (picDriveImage.Image != null)
            {
                picDriveImage.Image.Dispose();
                picDriveImage.Image = null;
            }

            currentPreviewImagePath = "";
            currentPreviewRenderSize = Size.Empty;
        }

        private Size GetPreviewImageRenderSize(Size sourceSize)
        {
            if (sourceSize.Width <= 0 || sourceSize.Height <= 0)
            {
                return Size.Empty;
            }

            int maxWidth = Math.Max(1, picDriveImage.ClientSize.Width);
            int maxHeight = Math.Max(1, picDriveImage.ClientSize.Height);
            double scale = Math.Min(
                maxWidth / (double)sourceSize.Width,
                maxHeight / (double)sourceSize.Height);

            if (scale >= 1.0)
            {
                return sourceSize;
            }

            return new Size(
                Math.Max(1, (int)Math.Round(sourceSize.Width * scale)),
                Math.Max(1, (int)Math.Round(sourceSize.Height * scale)));
        }

        private string ResolveCatalogImagePath(CatalogRecord record)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.ImageFileName))
            {
                return "";
            }

            string imageName =
                record.ImageFileName
                    .Replace('\\', Path.DirectorySeparatorChar)
                    .Replace('/', Path.DirectorySeparatorChar);

            if (Path.IsPathRooted(imageName))
            {
                return imageName;
            }

            string catalogFolder =
                Path.GetDirectoryName(record.SourceFilePath) ??
                selectedDataPath;

            string directPath =
                Path.Combine(catalogFolder, imageName);

            if (File.Exists(directPath))
            {
                return directPath;
            }

            string imagesFolderPath =
                Path.Combine(catalogFolder, "images", Path.GetFileName(imageName));

            if (File.Exists(imagesFolderPath))
            {
                return imagesFolderPath;
            }

            return directPath;
        }

        // =====================================================
        // CLEAN
        // =====================================================

        private void BtnCleanData_Click(
            object sender,
            EventArgs e)
        {
            UpdateSelectedDeletedIndexes(markDeleted: true);
        }

        // =====================================================
        // RESTORE
        // =====================================================

        private void BtnRestoreData_Click(
            object sender,
            EventArgs e)
        {
            UpdateSelectedDeletedIndexes(markDeleted: false);
        }

        private bool UpdateSelectedDeletedIndexes(bool markDeleted)
        {
            if (lstCatalogRows.SelectedIndices.Count == 0)
            {
                MessageBox.Show("프레임을 선택하세요.");
                return false;
            }

            Dictionary<string, HashSet<int>> selectedIndexesByTub =
                new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);

            foreach (int selectedIndex in lstCatalogRows.SelectedIndices)
            {
                CatalogRecord record = GetCatalogRecordAtListIndex(selectedIndex);
                if (record == null)
                {
                    continue;
                }

                string tubFolder = Path.GetDirectoryName(record.SourceFilePath) ?? "";
                if (string.IsNullOrWhiteSpace(tubFolder))
                {
                    continue;
                }

                if (!selectedIndexesByTub.TryGetValue(tubFolder, out HashSet<int> indexes))
                {
                    indexes = new HashSet<int>();
                    selectedIndexesByTub[tubFolder] = indexes;
                }

                indexes.Add(GetRecordIndex(record, record.LineIndex));
            }

            if (selectedIndexesByTub.Count == 0)
            {
                MessageBox.Show("선택한 프레임의 인덱스를 확인하지 못했습니다.");
                return false;
            }

            foreach (var item in selectedIndexesByTub)
            {
                HashSet<int> tubDeletedIndexes = ReadDeletedIndexesFromTubFolder(item.Key);

                if (markDeleted)
                {
                    tubDeletedIndexes.UnionWith(item.Value);
                }
                else
                {
                    tubDeletedIndexes.ExceptWith(item.Value);
                }

                if (!WriteDeletedIndexesToTubManifest(item.Key, tubDeletedIndexes))
                {
                    MessageBox.Show("deleted_index 저장 중 오류가 발생했습니다.");
                    return false;
                }
            }

            foreach (CatalogRecord record in integratedCatalogList)
            {
                string tubFolder = Path.GetDirectoryName(record.SourceFilePath) ?? "";
                HashSet<int> tubDeletedIndexes = ReadDeletedIndexesFromTubFolder(tubFolder);
                int recordIndex = GetRecordIndex(record, record.LineIndex);
                record.IsDeleted = tubDeletedIndexes.Contains(recordIndex);
            }

            lstCatalogRows.Invalidate();
            return true;
        }

        private bool WriteDeletedIndexesToTubManifest(string folderPath, HashSet<int> deletedIndexes)
        {
            try
            {
                UpdateTubManifestDeletedIndexOnly(folderPath, deletedIndexes, refreshCatalogState: false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool EnsureTubV2ManifestForTraining(string folderPath, out string errorMessage)
        {
            errorMessage = "";

            try
            {
                if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                {
                    errorMessage = "데이터 폴더를 찾을 수 없습니다.";
                    return false;
                }

                if (Directory.GetFiles(folderPath, "catalog_*.catalog", SearchOption.TopDirectoryOnly).Length == 0)
                {
                    errorMessage = "catalog_*.catalog 파일을 찾을 수 없습니다.";
                    return false;
                }

                UpdateTubManifestDeletedIndexOnly(
                    folderPath,
                    ReadDeletedIndexesFromTubFolder(folderPath),
                    refreshCatalogState: false);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private void UpdateTubManifestDeletedIndexOnly(
            string folderPath,
            HashSet<int> deletedIndexes,
            bool refreshCatalogState)
        {
            string manifestPath = Path.Combine(folderPath, "manifest.json");
            List<string> lines = File.Exists(manifestPath)
                ? File.ReadAllLines(manifestPath).ToList()
                : new List<string>();

            bool hasTubV2Header =
                lines.Count >= 2 &&
                IsJsonArrayLine(lines[0]) &&
                IsJsonArrayLine(lines[1]);

            if (!hasTubV2Header)
            {
                lines = CreateDefaultTubV2ManifestLines();
                refreshCatalogState = true;
            }

            while (lines.Count < 5)
            {
                lines.Add("{}");
            }

            int stateLineIndex = FindTubManifestStateLineIndex(lines);
            if (stateLineIndex < 0)
            {
                stateLineIndex = 4;
            }

            JsonObject state = TryParseJsonObject(lines[stateLineIndex]) ?? new JsonObject();
            RemoveDeletedIndexProperties(state);

            JsonArray deletedArray = new JsonArray();
            foreach (int index in deletedIndexes.OrderBy(index => index))
            {
                deletedArray.Add(index);
            }

            if (refreshCatalogState)
            {
                JsonArray paths = new JsonArray();
                foreach (string catalogFile in Directory.GetFiles(folderPath, "catalog_*.catalog", SearchOption.TopDirectoryOnly)
                    .OrderBy(ExtractCatalogNumberFromPath)
                    .ThenBy(path => path, StringComparer.OrdinalIgnoreCase))
                {
                    paths.Add(Path.GetFileName(catalogFile));
                }

                state["paths"] = paths;
                state["current_index"] = GetNextCatalogIndex(folderPath);
                state["max_len"] = ReadCatalogMaxLen(folderPath);
            }

            state["deleted_index"] = deletedArray;
            lines[stateLineIndex] = state.ToJsonString(new JsonSerializerOptions { WriteIndented = false });

            File.WriteAllText(
                manifestPath,
                string.Join("\n", lines) + "\n",
                new UTF8Encoding(false));
        }

        private List<string> CreateDefaultTubV2ManifestLines()
        {
            return new List<string>
            {
                "[\"cam/image_array\", \"user/angle\", \"user/throttle\", \"user/mode\"]",
                "[\"image_array\", \"float\", \"float\", \"str\"]",
                "{}",
                CreateTubSessionLine(),
                "{}"
            };
        }

        private int FindTubManifestStateLineIndex(List<string> lines)
        {
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                JsonObject obj = TryParseJsonObject(lines[i]);
                if (obj == null)
                {
                    continue;
                }

                if (obj.ContainsKey("paths") ||
                    obj.ContainsKey("current_index") ||
                    obj.ContainsKey("max_len") ||
                    obj.Any(property => IsDeletedIndexesProperty(property.Key)))
                {
                    return i;
                }
            }

            return lines.Count > 4 && IsJsonObjectLine(lines[4])
                ? 4
                : -1;
        }

        private JsonObject TryParseJsonObject(string line)
        {
            try
            {
                return JsonNode.Parse(line) as JsonObject;
            }
            catch
            {
                return null;
            }
        }

        private void NormalizeCatalogFilesForTraining(string folderPath)
        {
            UTF8Encoding utf8NoBom = new UTF8Encoding(false);
            int maxLen = ReadCatalogMaxLen(folderPath);

            foreach (string catalogFile in Directory.GetFiles(folderPath, "catalog_*.catalog", SearchOption.TopDirectoryOnly)
                .OrderBy(ExtractCatalogNumberFromPath)
                .ThenBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                List<string> lines = File.ReadAllLines(catalogFile)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToList();

                File.WriteAllText(
                    catalogFile,
                    string.Join("\n", lines) + "\n",
                    utf8NoBom);

                int catalogNumber = ExtractCatalogNumberFromPath(catalogFile);
                int startIndex = ReadCatalogManifestStartIndex(catalogFile + "_manifest");
                if (startIndex < 0 && catalogNumber != int.MaxValue)
                {
                    startIndex = catalogNumber * maxLen;
                }

                JsonObject catalogManifest = new JsonObject
                {
                    ["created_at"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0,
                    ["line_lengths"] = new JsonArray(lines.Select(line => JsonValue.Create(utf8NoBom.GetByteCount(line) + 1)).ToArray<JsonNode>()),
                    ["path"] = Path.GetFileName(catalogFile) + "_manifest",
                    ["start_index"] = Math.Max(0, startIndex)
                };

                File.WriteAllText(
                    catalogFile + "_manifest",
                    catalogManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = false }) + "\n",
                    utf8NoBom);
            }
        }

        private string CreateFilteredTrainingTubFolder(string sourceFolder, HashSet<int> excludedIndexes)
        {
            string tempRoot = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "TrainingTemp");
            string tempFolder = Path.Combine(
                tempRoot,
                "tub_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "_" + Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(tempFolder);
            CopyDirectoryContents(sourceFolder, tempFolder);

            int remainingRows = FilterCatalogRowsForTraining(tempFolder, excludedIndexes);
            if (remainingRows <= 0)
            {
                throw new InvalidOperationException("제외되지 않은 학습 데이터가 없습니다.");
            }

            NormalizeCatalogFilesForTraining(tempFolder);
            UpdateTrainingTubManifestDeletedIndexes(tempFolder, new HashSet<int>(), refreshCatalogState: true);
            return tempFolder;
        }

        private void UpdateTrainingTubManifestDeletedIndexes(
            string folderPath,
            HashSet<int> deletedIndexes,
            bool refreshCatalogState)
        {
            string manifestPath = Path.Combine(folderPath, "manifest.json");
            List<string> lines = File.Exists(manifestPath)
                ? File.ReadAllLines(manifestPath).ToList()
                : new List<string>();

            bool hasTubV2Header =
                lines.Count >= 2 &&
                IsJsonArrayLine(lines[0]) &&
                IsJsonArrayLine(lines[1]);

            if (!hasTubV2Header)
            {
                lines = CreateDefaultTubV2ManifestLines();
                refreshCatalogState = true;
            }

            while (lines.Count < 5)
            {
                lines.Add("{}");
            }

            int stateLineIndex = FindTubManifestStateLineIndex(lines);
            if (stateLineIndex < 0)
            {
                stateLineIndex = 4;
            }

            JsonObject state = TryParseJsonObject(lines[stateLineIndex]) ?? new JsonObject();
            RemoveDeletedIndexProperties(state);

            JsonArray deletedArray = new JsonArray();
            foreach (int index in deletedIndexes.OrderBy(index => index))
            {
                deletedArray.Add(index);
            }

            if (refreshCatalogState)
            {
                JsonArray paths = new JsonArray();
                foreach (string catalogFile in Directory.GetFiles(folderPath, "catalog_*.catalog", SearchOption.TopDirectoryOnly)
                    .OrderBy(ExtractCatalogNumberFromPath)
                    .ThenBy(path => path, StringComparer.OrdinalIgnoreCase))
                {
                    paths.Add(Path.GetFileName(catalogFile));
                }

                state["paths"] = paths;
                state["current_index"] = GetNextCatalogIndex(folderPath);
                state["max_len"] = ReadCatalogMaxLen(folderPath);
            }

            state["deleted_indexes"] = deletedArray;
            lines[stateLineIndex] = state.ToJsonString(new JsonSerializerOptions { WriteIndented = false });

            File.WriteAllText(
                manifestPath,
                string.Join("\n", lines) + "\n",
                new UTF8Encoding(false));
        }

        private int FilterCatalogRowsForTraining(string tubFolder, HashSet<int> excludedIndexes)
        {
            int remainingRows = 0;

            foreach (string catalogFile in Directory.GetFiles(tubFolder, "catalog_*.catalog", SearchOption.TopDirectoryOnly))
            {
                List<string> filteredLines = new List<string>();
                int fallbackIndex = 0;

                foreach (string line in File.ReadLines(catalogFile))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    int recordIndex = TryReadCatalogIndex(line);
                    if (recordIndex < 0)
                    {
                        recordIndex = fallbackIndex;
                    }

                    fallbackIndex++;

                    if (excludedIndexes.Contains(recordIndex))
                    {
                        continue;
                    }

                    filteredLines.Add(line);
                }

                remainingRows += filteredLines.Count;
                File.WriteAllText(
                    catalogFile,
                    string.Join("\n", filteredLines) + (filteredLines.Count > 0 ? "\n" : ""),
                    new UTF8Encoding(false));
            }

            return remainingRows;
        }

        private void CopyDirectoryContents(string sourceFolder, string destinationFolder)
        {
            Directory.CreateDirectory(destinationFolder);

            foreach (string directory in Directory.GetDirectories(sourceFolder, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(sourceFolder, directory);
                Directory.CreateDirectory(Path.Combine(destinationFolder, relative));
            }

            foreach (string file in Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(sourceFolder, file);
                string destination = Path.Combine(destinationFolder, relative);
                string destinationDirectory = Path.GetDirectoryName(destination);

                if (!string.IsNullOrWhiteSpace(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                File.Copy(file, destination, overwrite: true);
            }
        }

        private void DeleteDirectorySafe(string folder)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
                {
                    Directory.Delete(folder, recursive: true);
                }
            }
            catch
            {
            }
        }

        private string TryGetJsonArrayLine(List<string> lines, int preferredIndex)
        {
            if (preferredIndex >= 0 && preferredIndex < lines.Count && IsJsonArrayLine(lines[preferredIndex]))
            {
                return lines[preferredIndex].Trim();
            }

            foreach (string line in lines)
            {
                if (IsJsonArrayLine(line))
                {
                    return line.Trim();
                }
            }

            return null;
        }

        private string TryGetJsonObjectLine(List<string> lines, int preferredIndex)
        {
            if (preferredIndex >= 0 && preferredIndex < lines.Count && IsJsonObjectLine(lines[preferredIndex]))
            {
                return lines[preferredIndex].Trim();
            }

            return null;
        }

        private bool IsJsonArrayLine(string line)
        {
            try
            {
                return JsonNode.Parse(line) is JsonArray;
            }
            catch
            {
                return false;
            }
        }

        private bool IsJsonObjectLine(string line)
        {
            try
            {
                return JsonNode.Parse(line) is JsonObject;
            }
            catch
            {
                return false;
            }
        }

        private JsonObject FindTubManifestStateObject(List<string> lines)
        {
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                try
                {
                    JsonNode node = JsonNode.Parse(lines[i]);
                    if (node is JsonObject obj &&
                        (obj.ContainsKey("paths") ||
                         obj.ContainsKey("current_index") ||
                         obj.ContainsKey("max_len") ||
                         obj.Any(property => IsDeletedIndexesProperty(property.Key))))
                    {
                        return obj;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private string CreateTubSessionLine()
        {
            double createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
            return "{\"created_at\":" + createdAt.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",\"sessions\":{\"all_full_ids\":[],\"last_id\":0,\"last_full_id\":\"\"}}";
        }

        private int GetNextCatalogIndex(string folderPath)
        {
            int maxIndex = -1;

            foreach (string catalogFile in Directory.GetFiles(folderPath, "catalog_*.catalog", SearchOption.TopDirectoryOnly))
            {
                foreach (string line in File.ReadLines(catalogFile))
                {
                    int index = TryReadCatalogIndex(line);
                    if (index > maxIndex)
                    {
                        maxIndex = index;
                    }
                }
            }

            return maxIndex + 1;
        }

        private int TryReadCatalogIndex(string catalogLine)
        {
            try
            {
                JsonNode node = JsonNode.Parse(catalogLine);
                if (node is JsonObject obj &&
                    obj.TryGetPropertyValue("_index", out JsonNode value) &&
                    value != null &&
                    int.TryParse(value.ToString(), out int index))
                {
                    return index;
                }
            }
            catch
            {
            }

            return -1;
        }

        private int ReadCatalogMaxLen(string folderPath)
        {
            int[] starts = Directory.GetFiles(folderPath, "catalog_*.catalog_manifest", SearchOption.TopDirectoryOnly)
                .Select(ReadCatalogManifestStartIndex)
                .Where(index => index >= 0)
                .OrderBy(index => index)
                .ToArray();

            if (starts.Length >= 2)
            {
                return Math.Max(1, starts[1] - starts[0]);
            }

            string manifestPath = Path.Combine(folderPath, "manifest.json");
            if (File.Exists(manifestPath))
            {
                foreach (string line in File.ReadLines(manifestPath))
                {
                    try
                    {
                        JsonNode node = JsonNode.Parse(line);
                        if (node is JsonObject obj &&
                            obj.TryGetPropertyValue("max_len", out JsonNode value) &&
                            value != null &&
                            int.TryParse(value.ToString(), out int maxLen) &&
                            maxLen > 0)
                        {
                            return maxLen;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return 1000;
        }

        private int ReadCatalogManifestStartIndex(string manifestPath)
        {
            try
            {
                JsonNode node = JsonNode.Parse(File.ReadAllText(manifestPath));
                if (node is JsonObject obj &&
                    obj.TryGetPropertyValue("start_index", out JsonNode value) &&
                    value != null &&
                    int.TryParse(value.ToString(), out int index))
                {
                    return index;
                }
            }
            catch
            {
            }

            return -1;
        }

        private int ExtractCatalogNumberFromPath(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            int underscoreIndex = fileName.LastIndexOf('_');

            if (underscoreIndex >= 0 &&
                int.TryParse(fileName.Substring(underscoreIndex + 1), out int number))
            {
                return number;
            }

            return int.MaxValue;
        }

        private void RemoveDeletedIndexProperties(JsonObject manifestObject)
        {
            if (manifestObject == null)
            {
                return;
            }

            string[] names = manifestObject
                .Select(property => property.Key)
                .Where(IsDeletedIndexesProperty)
                .ToArray();

            foreach (string name in names)
            {
                manifestObject.Remove(name);
            }
        }

        // =====================================================
        // 학습 실행과 중단/취소 처리
        // =====================================================

        private async void BtnTrain_Click(
            object sender,
            EventArgs e)
        {
            // 학습 버튼의 전체 흐름:
            // 1. 선택된 tub와 catalog가 있는지 확인
            // 2. WSL/mycar/train.py 실행 가능 여부 확인
            // 3. train.py --tubs <선택 tub> --model <models/파일명> 실행
            // 4. 로그를 파일과 상태창에 동시에 남기고, 성공 시 모델 registry에 등록
            TrainerStatus statusForm = null;
            bool trainingCancelled = false;
            bool trainingStopRequested = false;
            string modelName = "";
            string modelWindowsPath = "";
            List<string> filteredTrainingTubPaths = new List<string>();

            try
            {
                List<string> checkedTubFolders = lstTubFolders.CheckedItems
                    .Cast<object>()
                    .Select(item => item?.ToString() ?? "")
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .ToList();

                List<string> trainingSourceFolders = checkedTubFolders
                    .Where(path => IsTubDataFolder(path))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (trainingSourceFolders.Count == 0)
                {
                    MessageBox.Show(
                        "먼저 학습할 tub 데이터 폴더를 체크하세요.",
                        "데이터 없음",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                if (integratedCatalogList.Count == 0)
                {
                    MessageBox.Show(
                        "로드된 catalog 데이터가 없습니다.",
                        "데이터 없음",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                foreach (string tubFolder in trainingSourceFolders)
                {
                    if (!EnsureTubV2ManifestForTraining(tubFolder, out string manifestError))
                    {
                        MessageBox.Show(
                            "학습용 manifest.json을 확인하지 못했습니다.\n" + tubFolder + "\n" + manifestError,
                            "manifest 오류",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }
                }

                if (!IsWslAvailable(out string wslMessage))
                {
                    MessageBox.Show(
                        wslMessage,
                        "WSL 실행 실패",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                RefreshWslPaths();

                string mycarWslPath =
                    wslMycarPath;

                List<string> sourceTubWslPaths = new List<string>();
                List<string> trainingTubWslPaths = new List<string>();

                foreach (string tubFolder in trainingSourceFolders)
                {
                    sourceTubWslPaths.Add(
                        ResolveTubWslPath(
                            tubFolder,
                            mycarWslPath));

                    HashSet<int> indexesExcludedFromTraining =
                        ReadDeletedIndexesFromTubFolder(tubFolder);

                    string filteredTubPath =
                        CreateFilteredTrainingTubFolder(
                            tubFolder,
                            indexesExcludedFromTraining);

                    filteredTrainingTubPaths.Add(filteredTubPath);
                    trainingTubWslPaths.Add(
                        ResolveTubWslPath(
                            filteredTubPath,
                            mycarWslPath));
                }

                string dataPathDisplay = string.Join(";", trainingSourceFolders);

                modelName =
                    "mypilot_" +
                    DateTime.Now.ToString(
                        "yyyyMMdd_HHmmss") +
                    ".h5";

                string modelRelativePath =
                    ModelDirectoryName + "/" + modelName;

                string modelWslPath =
                    mycarWslPath.TrimEnd('/') +
                    "/" +
                    modelRelativePath;

                modelWindowsPath =
                    Path.Combine(
                        wslBasePath,
                        ModelDirectoryName,
                        modelName);

                string trainingLogPath =
                    CreateTrainingLogPath(modelName);

                ProcessStartInfo psi =
                    new ProcessStartInfo
                    {
                        FileName = "wsl.exe",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                AddWslArguments(
                    psi,
                    BuildTrainCommand(
                        mycarWslPath,
                        trainingTubWslPaths,
                        modelRelativePath));

                btnTrain.Enabled = false;
                btnTrain.Text = "학습 실행 중...";

                statusForm =
                    new TrainerStatus();
                trainingProcessWslPid = 0;

                statusForm.CancelRequested +=
                    (cancelSender, cancelArgs) =>
                    {
                        trainingCancelled = true;
                        statusForm.AppendLog(
                            "학습 취소 요청됨. 실행 중인 WSL 학습 프로세스를 종료합니다.");
                        TryTerminateTrainingProcess();
                    };
                statusForm.StopTrainingRequested +=
                    (stopSender, stopArgs) =>
                    {
                        trainingStopRequested = true;
                        statusForm.AppendLog(
                            "학습 중단 요청됨. 현재 학습을 즉시 멈추고 모델 저장을 시도합니다.");
                        TryRequestTrainingStop(statusForm);
                    };

                statusForm.SetStatus(
                    "WSL 학습 준비 중",
                    dataPathDisplay,
                    modelWindowsPath,
                    trainingLogPath);

                statusForm.Show(this);

                wslProcess = Process.Start(psi);

                if (wslProcess == null)
                {
                    throw new InvalidOperationException(
                        "wsl.exe 프로세스를 시작하지 못했습니다.");
                }

                statusForm.SetStatus(
                    "WSL 학습 실행 중",
                    dataPathDisplay,
                    modelWindowsPath,
                    trainingLogPath);

                System.Threading.Tasks.Task outputTask =
                    CopyStreamToLogAsync(
                        wslProcess.StandardOutput,
                        trainingLogPath,
                        statusForm);

                System.Threading.Tasks.Task errorTask =
                    CopyStreamToLogAsync(
                        wslProcess.StandardError,
                        trainingLogPath,
                        statusForm);

                await wslProcess.WaitForExitAsync();

                await System.Threading.Tasks.Task.WhenAll(
                    outputTask,
                    errorTask);

                if (wslProcess.ExitCode != 0)
                {
                    if (trainingCancelled)
                    {
                        CleanupGeneratedModel(
                            modelName,
                            modelWindowsPath);

                        statusForm.AppendLog(
                            "학습이 취소되어 생성 중이던 모델 데이터를 정리했습니다.");

                        statusForm.MarkFinished(
                            "학습 취소됨");

                        LoadModelsToList();
                        return;
                    }

                    if (trainingStopRequested && File.Exists(modelWindowsPath))
                    {
                        statusForm.AppendLog(
                            "학습 중단 후 모델 파일이 생성되어 등록을 계속합니다.");
                    }
                    else
                    {
                        CleanupGeneratedModel(
                            modelName,
                            modelWindowsPath);

                        statusForm.SetStatus(
                            "학습 실패",
                            dataPathDisplay,
                            modelWindowsPath,
                            trainingLogPath);

                        statusForm.MarkFinished(
                            "학습 실패");

                        throw new InvalidOperationException(
                            "train.py 실행이 실패했습니다.\n\n" +
                            "자세한 내용은 로그 파일을 확인하세요.\n" +
                            trainingLogPath);
                    }
                }

                if (!File.Exists(modelWindowsPath))
                {
                    throw new FileNotFoundException(
                        "학습은 종료되었지만 모델 파일을 찾지 못했습니다.",
                        modelWindowsPath);
                }

                ModelRegistryEntry model =
                    new ModelRegistryEntry()
                    {
                        Name = modelName,
                        WindowsPath = modelWindowsPath,
                        WslPath = modelWslPath,
                        SourceTubWindowsPath = dataPathDisplay,
                        SourceTubWslPath = string.Join(",", sourceTubWslPaths),
                        CreatedAt = DateTime.Now
                    };

                UpsertModelRegistry(model);
                UpdateDonkeyModelDatabaseTubs(model, sourceTubWslPaths);
                AddOrUpdateModelList(model);

                statusForm.SetStatus(
                    trainingStopRequested ? "학습 중단 후 모델 생성 완료" : "학습 완료",
                    dataPathDisplay,
                    modelWindowsPath,
                    trainingLogPath);

                statusForm.MarkFinished(
                    trainingStopRequested ? "학습 중단 후 모델 생성 완료" : "학습 완료");

                if (!statusForm.IsDisposed)
                {
                    statusForm.Close();
                }

                MessageBox.Show(
                    "AI 학습이 완료되었습니다.\n\n" +
                    "모델:\n" +
                    modelName +
                    "\n\n저장 경로:\n" +
                    modelWindowsPath,
                    "학습 완료",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (statusForm != null && !statusForm.IsDisposed)
                {
                    statusForm.SetStatus(
                        "학습 실패",
                        selectedDataPath,
                        "",
                        "");

                    statusForm.AppendLog(
                        "ERROR: " + ex.Message);

                    statusForm.MarkFinished(
                        "학습 실패");
                }

                MessageBox.Show(
                    $"학습 실행 실패\n\n{ex.Message}",
                    "학습 실패",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                foreach (string filteredTrainingTubPath in filteredTrainingTubPaths)
                {
                    DeleteDirectorySafe(filteredTrainingTubPath);
                }

                btnTrain.Enabled = true;
                btnTrain.Text = "\U0001f9e0 AI 학습 시작";
            }
        }

        private string BuildTrainCommand(
            string mycarWslPath,
            List<string> selectedTubWslPaths,
            string modelRelativePath)
        {
            string tubArgument = string.Join(",", selectedTubWslPaths);
            string tubChecks = "";

            foreach (string tubPath in selectedTubWslPaths)
            {
                tubChecks +=
                    "if [ ! -d " +
                    QuoteForBash(tubPath) +
                    " ]; then " +
                    "echo 'Selected tub directory does not exist: " +
                    EscapeForDoubleQuotedBash(tubPath) +
                    "' >&2; " +
                    "exit 23; " +
                    "fi; " +
                    "if [ ! -f " +
                    QuoteForBash(tubPath.TrimEnd('/') + "/manifest.json") +
                    " ]; then " +
                    "echo 'Selected folder is not a DonkeyCar tub. manifest.json was not found: " +
                    EscapeForDoubleQuotedBash(tubPath) +
                    "' >&2; " +
                    "exit 24; " +
                    "fi; ";
            }

            // WSL bash에서 실행할 실제 학습 명령 문자열을 만듭니다.
            // 여기서 manifest.json 존재를 검사하므로, tub 파일명이 바뀌면 학습이 시작되기 전에 실패합니다.
            return
                "set -e; " +
                "export PYTHONUNBUFFERED=1; " +
                "cd " +
                QuoteForBash(mycarWslPath) +
                "; " +
                "if [ ! -f train.py ]; then " +
                "echo 'train.py was not found in the resolved mycar folder: " +
                EscapeForDoubleQuotedBash(mycarWslPath) +
                "' >&2; " +
                "exit 21; " +
                "fi; " +
                tubChecks +
                "mkdir -p " +
                QuoteForBash(ModelDirectoryName) +
                "; " +
                BuildPythonResolverCommand() +
                "echo 'Using Python after conda activate:'; " +
                "python -c 'import sys; print(sys.executable)'; " +
                "echo \"Training tub: " +
                EscapeForDoubleQuotedBash(tubArgument) +
                "\"; " +
                "echo \"Saving model: " +
                EscapeForDoubleQuotedBash(modelRelativePath) +
                "\"; " +
                "setsid python train.py --tubs " +
                QuoteForBash(tubArgument) +
                " --model " +
                QuoteForBash(modelRelativePath) +
                " & TRAIN_PID=$!; " +
                "echo __TRAIN_PID__:$TRAIN_PID; " +
                "wait $TRAIN_PID";
        }

        private void TryRequestTrainingStop(TrainerStatus statusForm)
        {
            try
            {
                if (trainingProcessWslPid <= 0)
                {
                    statusForm.AppendLog(
                        "학습 프로세스 PID를 아직 확인하지 못해 중단 신호를 보내지 못했습니다.");
                    return;
                }

                ProcessStartInfo psi =
                    new ProcessStartInfo
                    {
                        FileName = "wsl.exe",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                AddWslArguments(
                    psi,
                    BuildStopTrainingCommand(trainingProcessWslPid));

                using (Process proc = Process.Start(psi))
                {
                    if (proc == null)
                    {
                        statusForm.AppendLog(
                            "학습 중단 신호 전송 실패: wsl.exe를 시작하지 못했습니다.");
                        return;
                    }

                    proc.WaitForExit();

                    if (proc.ExitCode == 0)
                    {
                        statusForm.AppendLog(
                            "학습 프로세스 그룹에 중단 신호를 보냈습니다. 모델 저장을 기다립니다.");
                    }
                    else
                    {
                        string error =
                            proc.StandardError
                                .ReadToEnd()
                                .Replace("\0", "")
                                .Trim();

                        statusForm.AppendLog(
                            "학습 중단 신호 전송 실패: " +
                            (string.IsNullOrWhiteSpace(error) ? "알 수 없는 오류" : error));
                    }
                }
            }
            catch (Exception ex)
            {
                statusForm.AppendLog(
                    "학습 중단 신호 전송 오류: " + ex.Message);
            }
        }

        private string BuildStopTrainingCommand(int pid)
        {
            string pidText = pid.ToString();
            return
                "PID=" + pidText + "; " +
                "PGID=$(ps -o pgid= -p \"$PID\" 2>/dev/null | tr -d ' '); " +
                "if [ -z \"$PGID\" ]; then PGID=\"$PID\"; fi; " +
                "CHILDREN=$(pgrep -P \"$PID\" 2>/dev/null || true); " +
                "echo \"Requesting immediate training stop pid=$PID pgid=$PGID\"; " +
                "kill -INT -- -\"$PGID\" 2>/dev/null || kill -INT \"$PID\" 2>/dev/null || true; " +
                "for CHILD in $CHILDREN; do kill -INT \"$CHILD\" 2>/dev/null || true; done; " +
                "for i in 1 2 3 4 5; do " +
                "if ! kill -0 \"$PID\" 2>/dev/null; then echo \"Training process stopped after interrupt.\"; exit 0; fi; " +
                "sleep 1; " +
                "done; " +
                "echo \"Training still running after interrupt. Sending terminate signal.\"; " +
                "kill -TERM -- -\"$PGID\" 2>/dev/null || kill -TERM \"$PID\" 2>/dev/null || true; " +
                "sleep 1; " +
                "if kill -0 \"$PID\" 2>/dev/null; then " +
                "echo \"Training still running. Forcing stop.\"; " +
                "kill -KILL -- -\"$PGID\" 2>/dev/null || kill -KILL \"$PID\" 2>/dev/null || true; " +
                "fi; " +
                "echo \"Stop signal sent. Checking saved model.\"";
        }

        private void TryTerminateTrainingProcess()
        {
            try
            {
                if (
                    wslProcess != null &&
                    !wslProcess.HasExited)
                {
                    wslProcess.Kill(true);
                }
            }
            catch
            {

            }
        }

        private void CleanupGeneratedModel(
            string modelName,
            string modelWindowsPath)
        {
            ModelRegistryEntry generatedEntry =
                new ModelRegistryEntry()
                {
                    Name = modelName,
                    WindowsPath = modelWindowsPath,
                    WslPath =
                        string.IsNullOrWhiteSpace(modelName)
                            ? ""
                            : GetModelWslPath(modelName)
                };

            try
            {
                DeleteModelFiles(generatedEntry);
                DeleteDonkeyModelDatabaseEntry(generatedEntry);
            }
            catch
            {

            }

            RemoveModelRegistryEntry(
                modelName,
                modelWindowsPath);
        }

        private void RemoveModelRegistryEntry(
            string modelName,
            string modelWindowsPath)
        {
            List<ModelRegistryEntry> entries =
                LoadModelRegistry();

            entries.RemoveAll(
                entry =>
                    !string.IsNullOrWhiteSpace(modelWindowsPath)
                        ? string.Equals(
                            entry.WindowsPath,
                            modelWindowsPath,
                            StringComparison.OrdinalIgnoreCase)
                        : string.Equals(
                            entry.Name,
                            modelName,
                            StringComparison.OrdinalIgnoreCase));

            SaveModelRegistry(entries);
        }

        private List<string> GetRelatedModelFiles(
            ModelRegistryEntry entry)
        {
            List<string> files =
                new List<string>();

            if (
                entry == null ||
                string.IsNullOrWhiteSpace(entry.WindowsPath))
            {
                return files;
            }

            string modelPath =
                entry.WindowsPath;

            string modelsPath =
                Path.GetDirectoryName(modelPath);

            if (string.IsNullOrWhiteSpace(modelsPath))
            {
                return files;
            }

            string modelFileName =
                Path.GetFileName(modelPath);

            string modelBaseName =
                Path.GetFileNameWithoutExtension(modelPath);

            if (Directory.Exists(modelsPath))
            {
                foreach (string file in Directory.GetFiles(modelsPath))
                {
                    string fileName =
                        Path.GetFileName(file);

                    if (
                        string.Equals(
                            fileName,
                            "database.json",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (
                        IsRelatedModelFileName(
                            fileName,
                            modelFileName,
                            modelBaseName))
                    {
                        files.Add(file);
                    }
                }
            }

            if (File.Exists(modelPath))
            {
                files.Add(modelPath);
            }

            return
                files
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
        }

        private bool IsRelatedModelFileName(
            string fileName,
            string modelFileName,
            string modelBaseName)
        {
            return
                string.Equals(
                    fileName,
                    modelFileName,
                    StringComparison.OrdinalIgnoreCase) ||
                fileName.StartsWith(
                    modelFileName + ".",
                    StringComparison.OrdinalIgnoreCase) ||
                fileName.StartsWith(
                    modelBaseName + ".",
                    StringComparison.OrdinalIgnoreCase);
        }

        private List<string> GetRelatedModelFilesFromPath(string modelPath)
        {
            ModelRegistryEntry entry =
                new ModelRegistryEntry
                {
                    Name = Path.GetFileName(modelPath),
                    WindowsPath = modelPath
                };

            return GetRelatedModelFiles(entry);
        }

        private void ImportDonkeyModelDatabaseEntry(
            string sourceModelPath,
            string destinationModelPath,
            bool copyWholeDatabaseWhenDestinationEmpty)
        {
            string sourceDatabasePath =
                Path.Combine(
                    Path.GetDirectoryName(sourceModelPath) ?? "",
                    "database.json");

            if (!File.Exists(sourceDatabasePath))
            {
                return;
            }

            string destinationModelsPath =
                Path.GetDirectoryName(destinationModelPath);

            if (string.IsNullOrWhiteSpace(destinationModelsPath))
            {
                return;
            }

            Directory.CreateDirectory(destinationModelsPath);

            string destinationDatabasePath =
                Path.Combine(
                    destinationModelsPath,
                    "database.json");

            if (copyWholeDatabaseWhenDestinationEmpty &&
                !File.Exists(destinationDatabasePath))
            {
                File.Copy(sourceDatabasePath, destinationDatabasePath, overwrite: true);
                return;
            }

            JsonNode sourceRoot =
                JsonNode.Parse(File.ReadAllText(sourceDatabasePath));

            if (sourceRoot is not JsonArray sourceArray)
            {
                return;
            }

            string sourceModelName =
                Path.GetFileName(sourceModelPath);

            string sourceModelBaseName =
                Path.GetFileNameWithoutExtension(sourceModelPath);

            JsonObject sourceEntry =
                sourceArray
                    .OfType<JsonObject>()
                    .FirstOrDefault(
                        modelObject =>
                            IsDonkeyModelDatabaseEntry(
                                modelObject,
                                sourceModelBaseName,
                                sourceModelName));

            if (sourceEntry == null)
            {
                return;
            }

            string destinationModelName =
                Path.GetFileName(destinationModelPath);

            string destinationModelBaseName =
                Path.GetFileNameWithoutExtension(destinationModelPath);

            JsonObject importedEntry =
                JsonNode.Parse(sourceEntry.ToJsonString()) as JsonObject;

            if (importedEntry == null)
            {
                return;
            }

            ReplaceModelNameInJson(
                importedEntry,
                sourceModelBaseName,
                destinationModelBaseName,
                sourceModelName,
                destinationModelName);

            JsonArray destinationArray;

            if (File.Exists(destinationDatabasePath))
            {
                JsonNode destinationRoot =
                    JsonNode.Parse(File.ReadAllText(destinationDatabasePath));

                destinationArray =
                    destinationRoot as JsonArray ??
                    new JsonArray();
            }
            else
            {
                destinationArray =
                    new JsonArray();
            }

            for (int i = destinationArray.Count - 1; i >= 0; i--)
            {
                if (destinationArray[i] is JsonObject modelObject &&
                    IsDonkeyModelDatabaseEntry(
                        modelObject,
                        destinationModelBaseName,
                        destinationModelName))
                {
                    destinationArray.RemoveAt(i);
                }
            }

            destinationArray.Add(importedEntry);
            WriteJsonNode(destinationDatabasePath, destinationArray);
        }

        private bool HasDonkeyModelDatabaseEntry(
            string sourceModelPath,
            out string errorMessage)
        {
            errorMessage = "";

            string sourceDatabasePath =
                Path.Combine(
                    Path.GetDirectoryName(sourceModelPath) ?? "",
                    "database.json");

            if (!File.Exists(sourceDatabasePath))
            {
                errorMessage =
                    "원본 모델 폴더에서 database.json 파일을 찾지 못했습니다.\n" +
                    sourceDatabasePath;

                return false;
            }

            JsonNode sourceRoot;

            try
            {
                sourceRoot =
                    JsonNode.Parse(File.ReadAllText(sourceDatabasePath));
            }
            catch (Exception ex)
            {
                errorMessage =
                    "database.json 파일을 읽는 중 오류가 발생했습니다.\n" +
                    ex.Message;

                return false;
            }

            if (sourceRoot is not JsonArray sourceArray)
            {
                errorMessage =
                    "database.json 파일 구조가 올바르지 않습니다.";

                return false;
            }

            string sourceModelName =
                Path.GetFileName(sourceModelPath);

            string sourceModelBaseName =
                Path.GetFileNameWithoutExtension(sourceModelPath);

            bool hasEntry =
                sourceArray
                    .OfType<JsonObject>()
                    .Any(
                        modelObject =>
                            IsDonkeyModelDatabaseEntry(
                                modelObject,
                                sourceModelBaseName,
                                sourceModelName));

            if (!hasEntry)
            {
                errorMessage =
                    "database.json 파일에서 선택한 모델 데이터를 찾지 못했습니다.\n" +
                    sourceModelName;
            }

            return hasEntry;
        }

        private List<(string OldPath, string NewPath)> BuildModelRenamePlan(
            ModelRegistryEntry oldEntry,
            string newModelPath)
        {
            List<(string OldPath, string NewPath)> plan =
                new List<(string OldPath, string NewPath)>();

            string oldModelFileName =
                Path.GetFileName(oldEntry.WindowsPath);

            string oldBaseName =
                Path.GetFileNameWithoutExtension(oldEntry.WindowsPath);

            string newModelFileName =
                Path.GetFileName(newModelPath);

            string newBaseName =
                Path.GetFileNameWithoutExtension(newModelPath);

            string modelsPath =
                Path.GetDirectoryName(newModelPath);

            if (string.IsNullOrWhiteSpace(modelsPath))
            {
                return plan;
            }

            foreach (string oldPath in GetRelatedModelFiles(oldEntry))
            {
                string fileName =
                    Path.GetFileName(oldPath);

                string newFileName;

                if (
                    string.Equals(
                        fileName,
                        oldModelFileName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    newFileName = newModelFileName;
                }
                else if (
                    fileName.StartsWith(
                        oldModelFileName + ".",
                        StringComparison.OrdinalIgnoreCase))
                {
                    newFileName =
                        newModelFileName +
                        fileName.Substring(oldModelFileName.Length);
                }
                else if (
                    fileName.StartsWith(
                        oldBaseName + ".",
                        StringComparison.OrdinalIgnoreCase))
                {
                    newFileName =
                        newBaseName +
                        fileName.Substring(oldBaseName.Length);
                }
                else
                {
                    continue;
                }

                plan.Add(
                    (oldPath, Path.Combine(modelsPath, newFileName)));
            }

            return plan;
        }

        private void EnsureRenamePlanHasNoConflicts(
            List<(string OldPath, string NewPath)> plan)
        {
            foreach (var item in plan)
            {
                if (
                    !string.Equals(
                        item.OldPath,
                        item.NewPath,
                        StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(item.NewPath))
                {
                    throw new IOException(
                        "동일한 이름의 관련 모델 파일이 이미 존재합니다.\n" +
                        item.NewPath);
                }
            }
        }

        private void RenameModelFiles(
            List<(string OldPath, string NewPath)> plan)
        {
            foreach (var item in plan)
            {
                if (
                    string.Equals(
                        item.OldPath,
                        item.NewPath,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                File.Move(
                    item.OldPath,
                    item.NewPath);
            }
        }

        private void DeleteModelFiles(
            ModelRegistryEntry entry)
        {
            foreach (string file in GetRelatedModelFiles(entry))
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        private string GetDonkeyModelDatabasePath(
            ModelRegistryEntry entry)
        {
            if (
                entry == null ||
                string.IsNullOrWhiteSpace(entry.WindowsPath))
            {
                return "";
            }

            string modelsPath =
                Path.GetDirectoryName(entry.WindowsPath);

            if (string.IsNullOrWhiteSpace(modelsPath))
            {
                return "";
            }

            return
                Path.Combine(
                    modelsPath,
                    "database.json");
        }

        private void RenameDonkeyModelDatabaseEntry(
            ModelRegistryEntry oldEntry,
            string newName)
        {
            string databasePath =
                GetDonkeyModelDatabasePath(oldEntry);

            if (!File.Exists(databasePath))
            {
                return;
            }

            string oldBaseName =
                Path.GetFileNameWithoutExtension(oldEntry.Name);

            string newBaseName =
                Path.GetFileNameWithoutExtension(newName);

            JsonNode root =
                JsonNode.Parse(File.ReadAllText(databasePath));

            if (root == null)
            {
                return;
            }

            bool changed = false;

            if (root is JsonArray array)
            {
                foreach (JsonNode node in array)
                {
                    if (
                        node is JsonObject modelObject &&
                        IsDonkeyModelDatabaseEntry(
                            modelObject,
                            oldBaseName,
                            oldEntry.Name))
                    {
                        ReplaceModelNameInJson(
                            modelObject,
                            oldBaseName,
                            newBaseName,
                            oldEntry.Name,
                            newName);

                        changed = true;
                    }
                }
            }

            if (changed)
            {
                WriteJsonNode(databasePath, root);
            }
        }

        private void DeleteDonkeyModelDatabaseEntry(
            ModelRegistryEntry entry)
        {
            string databasePath =
                GetDonkeyModelDatabasePath(entry);

            if (!File.Exists(databasePath))
            {
                return;
            }

            string modelBaseName =
                Path.GetFileNameWithoutExtension(entry.Name);

            JsonNode root =
                JsonNode.Parse(File.ReadAllText(databasePath));

            if (root == null)
            {
                return;
            }

            bool changed = false;

            if (root is JsonArray array)
            {
                for (int i = array.Count - 1; i >= 0; i--)
                {
                    if (
                        array[i] is JsonObject modelObject &&
                        IsDonkeyModelDatabaseEntry(
                            modelObject,
                            modelBaseName,
                            entry.Name))
                    {
                        array.RemoveAt(i);
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                WriteJsonNode(databasePath, root);
            }
        }

        private void UpdateDonkeyModelDatabaseTubs(
            ModelRegistryEntry entry,
            List<string> sourceTubWslPaths)
        {
            string databasePath =
                GetDonkeyModelDatabasePath(entry);

            if (!File.Exists(databasePath) || sourceTubWslPaths == null || sourceTubWslPaths.Count == 0)
            {
                return;
            }

            JsonNode root =
                JsonNode.Parse(File.ReadAllText(databasePath));

            if (root == null)
            {
                return;
            }

            string modelBaseName =
                Path.GetFileNameWithoutExtension(entry.Name);

            bool changed = false;

            if (root is JsonArray array)
            {
                foreach (JsonNode node in array)
                {
                    if (node is JsonObject modelObject &&
                        IsDonkeyModelDatabaseEntry(modelObject, modelBaseName, entry.Name))
                    {
                        JsonArray tubs = new JsonArray();
                        foreach (string tubPath in sourceTubWslPaths
                            .Where(path => !string.IsNullOrWhiteSpace(path))
                            .Distinct(StringComparer.OrdinalIgnoreCase))
                        {
                            tubs.Add(tubPath);
                        }

                        modelObject["Tubs"] = tubs;
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                WriteJsonNode(databasePath, root);
            }
        }

        private bool IsDonkeyModelDatabaseEntry(
            JsonObject modelObject,
            string modelBaseName,
            string modelFileName)
        {
            if (
                modelObject.TryGetPropertyValue(
                    "Name",
                    out JsonNode nameNode))
            {
                string name =
                    nameNode == null
                        ? ""
                        : nameNode.GetValue<string>();

                return
                    string.Equals(
                        name,
                        modelBaseName,
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        name,
                        modelFileName,
                        StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private void ReplaceModelNameInJson(
            JsonNode node,
            string oldBaseName,
            string newBaseName,
            string oldFileName,
            string newFileName)
        {
            if (node is JsonObject obj)
            {
                foreach (string key in obj.Select(pair => pair.Key).ToList())
                {
                    JsonNode child =
                        obj[key];

                    if (child is JsonValue value)
                    {
                        string text =
                            TryGetJsonString(value);

                        if (
                            string.Equals(
                                text,
                                oldBaseName,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            obj[key] = newBaseName;
                        }
                        else if (
                            string.Equals(
                                text,
                                oldFileName,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            obj[key] = newFileName;
                        }
                    }
                    else if (child != null)
                    {
                        ReplaceModelNameInJson(
                            child,
                            oldBaseName,
                            newBaseName,
                            oldFileName,
                            newFileName);
                    }
                }
            }
            else if (node is JsonArray array)
            {
                foreach (JsonNode child in array)
                {
                    if (child != null)
                    {
                        ReplaceModelNameInJson(
                            child,
                            oldBaseName,
                            newBaseName,
                            oldFileName,
                            newFileName);
                    }
                }
            }
        }

        private string TryGetJsonString(
            JsonValue value)
        {
            try
            {
                return value.GetValue<string>();
            }
            catch
            {
                return "";
            }
        }

        private void WriteJsonNode(
            string path,
            JsonNode node)
        {
            JsonSerializerOptions options =
                new JsonSerializerOptions()
                {
                    WriteIndented = true
                };

            File.WriteAllText(
                path,
                node.ToJsonString(options));
        }

        private string BuildPythonResolverCommand()
        {
            // conda가 설치된 위치가 사용자 환경마다 다르기 때문에 여러 후보를 순서대로 검사합니다.
            // e2e_env를 활성화한 뒤 python 명령이 학습 환경의 Python을 가리키게 만듭니다.
            string configuredUserHome =
                string.IsNullOrWhiteSpace(wslUsername)
                    ? ""
                    : "/home/" + wslUsername;

            List<string> condaShellPaths =
                BuildCondaShellCandidates(configuredUserHome);

            return
                "echo 'Activating DonkeyCar Conda environment: " +
                TrainingCondaEnvironment +
                "'; " +
                "echo 'Resolver strategy: conda-activate-explicit-v2'; " +
                "echo 'Configured WSL user: " +
                EscapeForDoubleQuotedBash(wslUsername) +
                "'; " +
                "echo 'Configured WSL home: " +
                EscapeForDoubleQuotedBash(configuredUserHome) +
                "'; " +
                BuildCondaShellSourceCommand(condaShellPaths) +
                "if ! conda --version >/dev/null 2>&1; then " +
                "echo 'Conda initialization failed after loading conda.sh.' >&2; " +
                "exit 22; " +
                "fi; " +
                "conda --version; " +
                "echo 'Activating conda env: " +
                EscapeForDoubleQuotedBash(TrainingCondaEnvironment) +
                "'; " +
                "conda activate " +
                QuoteForBash(TrainingCondaEnvironment) +
                " || { " +
                "echo 'Conda environment activation failed: " +
                EscapeForDoubleQuotedBash(TrainingCondaEnvironment) +
                "' >&2; " +
                "conda env list >&2; " +
                "exit 22; " +
                "}; " +
                "if ! python -c 'import sys' >/dev/null 2>&1; then " +
                "echo 'Python was not found or cannot be executed after conda activate.' >&2; " +
                "exit 22; " +
                "fi; ";
        }

        private List<string> BuildCondaShellCandidates(string configuredUserHome)
        {
            List<string> bases =
                new List<string>();

            if (!string.IsNullOrWhiteSpace(configuredUserHome))
            {
                bases.Add(configuredUserHome);
            }

            if (!string.IsNullOrWhiteSpace(wslUsername))
            {
                bases.Add("/home/" + wslUsername);
            }

            bases.Add("/opt/conda");

            List<string> candidates =
                new List<string>();

            foreach (string basePath in bases.Distinct(StringComparer.Ordinal))
            {
                if (basePath == "/opt/conda")
                {
                    candidates.Add("/opt/conda/etc/profile.d/conda.sh");
                    continue;
                }

                candidates.Add(basePath + "/miniconda3/etc/profile.d/conda.sh");
                candidates.Add(basePath + "/anaconda3/etc/profile.d/conda.sh");
                candidates.Add(basePath + "/miniforge3/etc/profile.d/conda.sh");
                candidates.Add(basePath + "/mambaforge/etc/profile.d/conda.sh");
                candidates.Add(basePath + "/micromamba/etc/profile.d/conda.sh");
            }

            return candidates.Distinct(StringComparer.Ordinal).ToList();
        }

        private string BuildCondaShellSourceCommand(List<string> condaShellPaths)
        {
            StringBuilder builder =
                new StringBuilder();

            if (condaShellPaths.Count == 0)
            {
                return
                    "echo 'Conda was not found in WSL.' >&2; " +
                    "echo 'No explicit conda.sh locations were available to check.' >&2; " +
                    "exit 22; ";
            }

            for (int i = 0; i < condaShellPaths.Count; i++)
            {
                string condaShellPath =
                    condaShellPaths[i];

                builder.Append(
                    i == 0
                        ? "if [ -f "
                        : "elif [ -f ");

                builder.Append(
                    QuoteForBash(condaShellPath) +
                    " ]; then " +
                    "echo 'Using conda.sh: " +
                    EscapeForDoubleQuotedBash(condaShellPath) +
                    "'; " +
                    ". " +
                    QuoteForBash(condaShellPath) +
                    "; ");
            }

            builder.Append(
                "else " +
                "echo 'Conda was not found in WSL.' >&2; " +
                "echo 'Checked explicit conda.sh locations under the resolved WSL user home.' >&2; " +
                "exit 22; " +
                "fi; ");

            return builder.ToString();
        }

        private string EscapeForSingleQuotedBash(string value)
        {
            return value.Replace("'", "'\"'\"'");
        }

        private string EscapeForDoubleQuotedBash(string value)
        {
            return
                value
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("$", "\\$")
                    .Replace("`", "\\`");
        }

        private string CreateTrainingLogPath(string modelName)
        {
            string logDirectory =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "DonkeyDataManager",
                    "TrainingLogs");

            Directory.CreateDirectory(logDirectory);

            string safeName =
                Path.GetFileNameWithoutExtension(modelName);

            string logPath =
                Path.Combine(
                    logDirectory,
                    safeName + ".log");

            File.WriteAllText(
                logPath,
                "DonkeyCar training log" +
                Environment.NewLine +
                "Started: " +
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                Environment.NewLine +
                Environment.NewLine);

            return logPath;
        }

        private System.Threading.Tasks.Task CopyStreamToLogAsync(
            StreamReader reader,
            string logPath,
            TrainerStatus statusForm)
        {
            return System.Threading.Tasks.Task.Run(
                () =>
                {
                    string line;

                    try
                    {
                        while ((line = reader.ReadLine()) != null)
                        {
                            string cleanLine =
                                line.Replace("\0", "");

                            if (TryCaptureTrainingProcessPid(cleanLine, logPath, statusForm))
                            {
                                continue;
                            }

                            AppendTrainingLog(
                                logPath,
                                cleanLine);

                            if (
                                statusForm != null &&
                                !statusForm.IsDisposed)
                            {
                                statusForm.AppendLog(
                                    cleanLine);
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        AppendTrainingLog(
                            logPath,
                            "Log stream closed: " + ex.Message);
                    }
                    catch (ObjectDisposedException)
                    {

                    }
                });
        }

        private bool TryCaptureTrainingProcessPid(
            string line,
            string logPath,
            TrainerStatus? statusForm)
        {
            const string prefix = "__TRAIN_PID__:";

            if (!line.StartsWith(prefix, StringComparison.Ordinal))
            {
                return false;
            }

            string value =
                line.Substring(prefix.Length).Trim();

            if (int.TryParse(value, out int pid) && pid > 0)
            {
                trainingProcessWslPid = pid;

                string message =
                    "학습 프로세스 PID 확인: " + pid;

                AppendTrainingLog(
                    logPath,
                    message);

                if (
                    statusForm != null &&
                    !statusForm.IsDisposed)
                {
                    statusForm.AppendLog(message);
                }
            }

            return true;
        }

        private void AppendTrainingLog(
            string logPath,
            string line)
        {
            lock (trainingLogLock)
            {
                File.AppendAllText(
                    logPath,
                    line + Environment.NewLine);
            }
        }

        private void OpenTrainingLogWindow(string logPath)
        {
            try
            {
                ProcessStartInfo psi =
                    new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        UseShellExecute = true
                    };

                psi.ArgumentList.Add("-NoExit");
                psi.ArgumentList.Add("-Command");
                psi.ArgumentList.Add(
                    "$host.UI.RawUI.WindowTitle = 'DonkeyCar WSL Training'; " +
                    "Get-Content -LiteralPath " +
                    QuoteForPowerShell(logPath) +
                    " -Wait");

                Process.Start(psi);
            }
            catch
            {

            }
        }

        private string QuoteForPowerShell(string value)
        {
            return
                "'" +
                value.Replace("'", "''") +
                "'";
        }

        // =====================================================
        // TUB SELECT
        // =====================================================

        private string PromptTubFolderSelection()
        {
            using (
                FolderBrowserDialog fbd =
                new FolderBrowserDialog())
            {
                fbd.Description =
                    "학습할 데이터 폴더 선택";

                fbd.ShowNewFolderButton =
                    false;

                string initialPath =
                    Path.Combine(
                        wslBasePath,
                        "data");

                if (
                    Directory.Exists(
                        initialPath))
                {
                    fbd.SelectedPath =
                        initialPath;
                }

                if (
                    fbd.ShowDialog() ==
                    DialogResult.OK)
                {
                    string folderName =
                        Path.GetFileName(
                            fbd.SelectedPath);

                    return folderName;
                }

                return null;
            }
        }

        // =====================================================
        // DRIVE
        // =====================================================

        private void BtnDrive_Click(
    object sender,
    EventArgs e)
        {
            try
            {
                if (lstModels.SelectedItems.Count == 0)
                {
                    MessageBox.Show(
                        "모델을 먼저 선택하세요.");

                    return;
                }

                string selectedModel =
                    GetSelectedModelName();

                RefreshWslPaths();

                ProcessStartInfo psi =
                    new ProcessStartInfo
                    {
                        FileName = "wsl.exe",
                        UseShellExecute = false,
                        CreateNoWindow = false
                    };

                AddWslArguments(
                    psi,
                    BuildDriveCommand(selectedModel));

                wslProcess =
                    Process.Start(psi);

                OpenBrowserAfterDelay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"자율주행 실행 실패\n\n{ex.Message}");
            }
        }

        private string BuildDriveCommand(string selectedModel)
        {
            return
                "set -e; " +
                "cd " +
                QuoteForBash(wslMycarPath) +
                "; " +
                "if [ ! -f manage.py ]; then " +
                "echo 'manage.py was not found in the resolved mycar folder: " +
                EscapeForDoubleQuotedBash(wslMycarPath) +
                "' >&2; " +
                "exit 25; " +
                "fi; " +
                BuildPythonResolverCommand() +
                "echo 'Using Python after conda activate:'; " +
                "python -c 'import sys; print(sys.executable)'; " +
                "python manage.py drive --model " +
                QuoteForBash(
                    "./" +
                    ModelDirectoryName +
                    "/" +
                    selectedModel);
        }

        // =====================================================
        // MODEL SELECT
        // =====================================================

        private string PromptModelSelection()
        {
            using (
                OpenFileDialog ofd =
                new OpenFileDialog())
            {
                ofd.Title =
                    "AI 모델 파일 선택";

                string modelsPath =
                    Path.Combine(
                        wslBasePath,
                        ModelDirectoryName);

                if (
                    Directory.Exists(
                        modelsPath))
                {
                    ofd.InitialDirectory =
                        modelsPath;
                }

                ofd.Filter =
                    "H5 파일 (*.h5)|*.h5";

                if (
                    ofd.ShowDialog() ==
                    DialogResult.OK)
                {
                    return
                        Path.GetFileName(
                            ofd.FileName);
                }

                return null;
            }
        }

        // =====================================================
        // BROWSER
        // =====================================================

        private async void OpenBrowserAfterDelay()
        {
            try
            {
                string url =
                    "http://localhost:8887";

                await System.Threading.Tasks
                    .Task.Delay(10000);

                bool serverReady =
                    await WaitForServerReady(
                        url,
                        50);

                if (serverReady)
                {
                    OpenBrowserToUrl(url);
                }
            }
            catch
            {

            }
        }

        private void OpenBrowserToUrl(
            string url)
        {
            try
            {
                AttentionWindow();

                browserProcess =
                    Process.Start(url);
            }
            catch
            {

            }
        }

        private async System.Threading.Tasks.Task<bool>
            WaitForServerReady(
            string url,
            int maxWaitSeconds = 60)
        {
            using (
                var client =
                new System.Net.Http.HttpClient())
            {
                client.Timeout =
                    TimeSpan.FromSeconds(5);

                for (
                    int i = 0;
                    i < maxWaitSeconds;
                    i++)
                {
                    try
                    {
                        var response =
                            await client.GetAsync(
                                url);

                        if (
                            response
                            .IsSuccessStatusCode)
                        {
                            return true;
                        }
                    }
                    catch
                    {

                    }

                    await System.Threading.Tasks
                        .Task.Delay(1000);
                }
            }

            return false;
        }

        private void TryRestartBrowser()
        {
            try
            {

            }
            catch
            {

            }
        }

        // =====================================================
        // JSON VALUE
        // =====================================================

        private string ExtractJsonValue(
            string json,
            string key)
        {
            try
            {
                string searchKey =
                    $"\"{key}\":";

                int startIdx =
                    json.IndexOf(searchKey);

                if (startIdx == -1)
                    return "";

                startIdx +=
                    searchKey.Length;

                while (
                    startIdx < json.Length &&
                    json[startIdx] == ' ')
                {
                    startIdx++;
                }

                if (
                    json[startIdx] == '"')
                {
                    startIdx++;

                    int endIdx =
                        json.IndexOf(
                            '"',
                            startIdx);

                    return json.Substring(
                        startIdx,
                        endIdx - startIdx);
                }
                else
                {
                    int endIdx =
                        json.IndexOfAny(
                            new char[]
                            {
                                ',',
                                '}'
                            },
                            startIdx);

                    return json.Substring(
                        startIdx,
                        endIdx - startIdx)
                        .Trim();
                }
            }
            catch
            {
                return "";
            }
        }
        // =====================================================
        // 모델 가져오기, 제외, 복원, 영구 삭제
        // =====================================================

        private void BtnImportModel_Click(object sender, EventArgs e)
        {
            try
            {
                using OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = "가져올 AI 모델 선택";
                dialog.Filter = "H5 모델 (*.h5)|*.h5";
                dialog.Multiselect = false;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                RefreshWslPaths();

                string sourceModelPath =
                    Path.GetFullPath(dialog.FileName);

                string modelName =
                    Path.GetFileName(sourceModelPath);

                string destinationModelsPath =
                    Path.Combine(
                        wslBasePath,
                        ModelDirectoryName);

                if (string.IsNullOrWhiteSpace(destinationModelsPath))
                {
                    MessageBox.Show("모델 저장 폴더를 확인하지 못했습니다.");
                    return;
                }

                Directory.CreateDirectory(destinationModelsPath);

                ModelImportResult importResult =
                    ModelImportService.ImportModelToFolder(
                        sourceModelPath,
                        destinationModelsPath);

                string destinationModelPath =
                    importResult.DestinationModelPath;

                modelName =
                    importResult.ModelFileName;

                ModelRegistryEntry importedEntry =
                    new ModelRegistryEntry
                    {
                        Name = modelName,
                        WindowsPath = destinationModelPath,
                        WslPath = GetModelWslPath(modelName),
                        CreatedAt = DateTime.Now,
                        IsDeleted = false
                    };

                UpsertModelRegistry(importedEntry);
                LoadModelsToList();

                MessageBox.Show("모델을 가져왔습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "모델 가져오기 중 오류가 발생했습니다.\n" +
                    ex.Message);
            }
        }

        private void BtnModelDlt_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                if (lstModels.SelectedItems.Count == 0)
                {
                    MessageBox.Show(
                        "삭제할 모델을 선택하세요.");

                    return;
                }

                List<ModelRegistryEntry> selectedEntries =
                    GetSelectedModelEntries();

                if (selectedEntries.Count == 0)
                {
                    MessageBox.Show(
                        "선택한 모델의 경로 정보를 확인할 수 없습니다.");

                    return;
                }

                List<ModelRegistryEntry> entries =
                    LoadModelRegistry();

                foreach (ModelRegistryEntry selectedEntry in selectedEntries)
                {
                    ModelRegistryEntry existingEntry =
                        entries.FirstOrDefault(entry =>
                            IsSameModelRegistryEntry(entry, selectedEntry));

                    if (existingEntry == null)
                    {
                        existingEntry =
                            new ModelRegistryEntry
                        {
                            Name = selectedEntry.Name,
                            WindowsPath = selectedEntry.WindowsPath,
                            WslPath = string.IsNullOrWhiteSpace(selectedEntry.WslPath)
                                ? GetModelWslPath(selectedEntry.Name)
                                : selectedEntry.WslPath,
                            SourceTubWindowsPath = selectedEntry.SourceTubWindowsPath,
                            SourceTubWslPath = selectedEntry.SourceTubWslPath,
                            CreatedAt = selectedEntry.CreatedAt == default
                                ? DateTime.Now
                                : selectedEntry.CreatedAt
                        };

                        entries.Add(existingEntry);
                    }

                    existingEntry.IsDeleted = true;
                }

                SaveModelRegistry(entries);
                LoadModelsToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message);
            }
        }

        private bool IsSameModelRegistryEntry(
            ModelRegistryEntry left,
            ModelRegistryEntry right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(left.WindowsPath) &&
                !string.IsNullOrWhiteSpace(right.WindowsPath))
            {
                return string.Equals(
                    left.WindowsPath,
                    right.WindowsPath,
                    StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(
                left.Name,
                right.Name,
                StringComparison.OrdinalIgnoreCase);
        }

        private void DeleteModelsPermanently(List<ModelRegistryEntry> selectedEntries)
        {
            selectedEntries =
                (selectedEntries ?? new List<ModelRegistryEntry>())
                    .Where(entry => entry != null)
                    .GroupBy(entry => entry.WindowsPath ?? entry.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First())
                    .ToList();

            if (selectedEntries.Count == 0)
            {
                return;
            }

            string modelListText =
                string.Join(
                    Environment.NewLine,
                    selectedEntries.Select(entry => entry.Name));

            DialogResult result =
                MessageBox.Show(
                    modelListText +
                    "\n\n선택한 제외 모델 " +
                    selectedEntries.Count +
                    "개를 완전히 삭제하시겠습니까?",
                    "모델 영구 삭제",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            foreach (ModelRegistryEntry selectedEntry in selectedEntries)
            {
                DeleteModelFiles(selectedEntry);
                DeleteDonkeyModelDatabaseEntry(selectedEntry);
            }

            List<ModelRegistryEntry> entries =
                LoadModelRegistry();

            foreach (ModelRegistryEntry selectedEntry in selectedEntries)
            {
                entries.RemoveAll(
                    entry =>
                        !string.IsNullOrWhiteSpace(selectedEntry.WindowsPath)
                            ? string.Equals(
                                entry.WindowsPath,
                                selectedEntry.WindowsPath,
                                StringComparison.OrdinalIgnoreCase)
                            : string.Equals(
                                entry.Name,
                                selectedEntry.Name,
                                StringComparison.OrdinalIgnoreCase));
            }

            SaveModelRegistry(entries);
            LoadModelsToList();

            MessageBox.Show(
                "선택한 모델 파일과 관련 정보가 완전히 삭제되었습니다.");
        }

        private void BtnModelRestore_Click(object sender, EventArgs e)
        {
            try
            {
                List<ModelRegistryEntry> selectedEntries =
                    GetSelectedModelEntries()
                        .Where(entry => entry.IsDeleted)
                        .ToList();

                if (selectedEntries.Count == 0)
                {
                    MessageBox.Show("복원할 제외 모델을 모델 리스트에서 선택하세요.");
                    return;
                }

                List<ModelRegistryEntry> entries =
                    LoadModelRegistry();

                foreach (ModelRegistryEntry entry in entries)
                {
                    foreach (ModelRegistryEntry selectedEntry in selectedEntries)
                    {
                        bool sameEntry =
                            !string.IsNullOrWhiteSpace(selectedEntry.WindowsPath)
                                ? string.Equals(
                                    entry.WindowsPath,
                                    selectedEntry.WindowsPath,
                                    StringComparison.OrdinalIgnoreCase)
                                : string.Equals(
                                    entry.Name,
                                    selectedEntry.Name,
                                    StringComparison.OrdinalIgnoreCase);

                        if (sameEntry)
                        {
                            entry.IsDeleted = false;
                        }
                    }
                }

                SaveModelRegistry(entries);
                LoadModelsToList();

                MessageBox.Show("선택한 모델을 복원했습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        // =====================================================
        // 모델 이름 변경
        // =====================================================

        private void BtnNameCh_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                if (lstModels.SelectedItems.Count == 0)
                {
                    MessageBox.Show(
                        "이름을 변경할 모델을 선택하세요.");

                    return;
                }

                string oldName =
                    GetSelectedModelName();

                ModelRegistryEntry oldEntry =
                    GetSelectedModelEntry();

                if (oldEntry == null)
                {
                    MessageBox.Show(
                        "선택한 모델의 경로 정보를 확인할 수 없습니다.");

                    return;
                }

                string newName =
                    Microsoft.VisualBasic.Interaction.InputBox(
                        "새 모델명을 입력하세요.",
                        "모델 이름 변경",
                        oldName);

                if (string.IsNullOrWhiteSpace(newName))
                    return;

                newName =
    newName
    .Replace("\0", "")
    .Trim();

                string oldFilePath =
                    string.IsNullOrWhiteSpace(oldEntry.WindowsPath)
                        ? Path.Combine(
                            wslBasePath
                                .Replace("\0", "")
                                .Trim(),
                            ModelDirectoryName,
                            oldName)
                        : oldEntry.WindowsPath;

                string modelsPath =
                    Path.GetDirectoryName(oldFilePath);

                if (string.IsNullOrWhiteSpace(modelsPath))
                {
                    MessageBox.Show(
                        "모델 경로를 확인할 수 없습니다.");

                    return;
                }

                if (!newName.EndsWith(".h5"))
                {
                    newName += ".h5";
                }

                string newFilePath =
                    Path.Combine(
                        modelsPath,
                        newName);

                if (File.Exists(newFilePath))
                {
                    MessageBox.Show(
                        "동일한 이름의 모델이 이미 존재합니다.");

                    return;
                }

                List<(string OldPath, string NewPath)> renamePlan =
                    BuildModelRenamePlan(
                        oldEntry,
                        newFilePath);

                if (renamePlan.Count == 0)
                {
                    MessageBox.Show(
                        "변경할 모델 파일을 찾을 수 없습니다.\n" +
                        oldFilePath);

                    return;
                }

                EnsureRenamePlanHasNoConflicts(renamePlan);
                RenameModelFiles(renamePlan);
                RenameDonkeyModelDatabaseEntry(oldEntry, newName);

                ModelRegistryEntry newEntry =
                    new ModelRegistryEntry()
                    {
                        Name = newName,
                        WindowsPath = newFilePath,
                        WslPath =
                            GetRenamedModelWslPath(
                                oldEntry,
                                newName),
                        SourceTubWindowsPath =
                            oldEntry.SourceTubWindowsPath,
                        SourceTubWslPath =
                            oldEntry.SourceTubWslPath,
                        CreatedAt =
                            oldEntry.CreatedAt
                    };

                List<ModelRegistryEntry> entries =
                    LoadModelRegistry();

                entries.RemoveAll(
                    entry =>
                        !string.IsNullOrWhiteSpace(oldFilePath)
                            ? string.Equals(
                                entry.WindowsPath,
                                oldFilePath,
                                StringComparison.OrdinalIgnoreCase)
                            : string.Equals(
                                entry.Name,
                                oldName,
                                StringComparison.OrdinalIgnoreCase));

                entries.Add(newEntry);
                SaveModelRegistry(entries);

                LoadModelsToList();

                AddOrUpdateModelList(newEntry);

                MessageBox.Show(
                    "모델 이름이 변경되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message);
            }
        }
    }
}
