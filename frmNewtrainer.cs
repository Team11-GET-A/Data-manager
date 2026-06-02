using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text;

namespace DonkeyDataManager
{
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
        // 전역 변수
        // =====================================================

        private string selectedDataPath = "";

        private List<CatalogRecord> integratedCatalogList =
            new List<CatalogRecord>();

        private System.Windows.Forms.Timer playbackTimer =
            new System.Windows.Forms.Timer();

        private Process wslProcess = null;
        private Process browserProcess = null;

        private System.Windows.Forms.Timer browserMonitorTimer =
            new System.Windows.Forms.Timer();

        private string wslDistroName = "Ubuntu";
        private string wslUsername = "";
        private string wslBasePath = "";
        private const string DonkeyCarWslPath = "~/mycar";
        private const string ModelDirectoryName = "models";
        private const string TrainingCondaEnvironment = "e2e_env";
        private readonly string modelRegistryPath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "DonkeyDataManager",
                "models.json");
        private readonly object trainingLogLock = new object();

        // =====================================================
        // ⭐ 모델 자동 로드 추가
        // =====================================================

        private System.Windows.Forms.Timer modelRefreshTimer =
            new System.Windows.Forms.Timer();

        // =====================================================
        // 데이터 구조
        // =====================================================

        public class CatalogRecord
        {
            public string OriginalLine { get; set; }

            public string SourceFilePath { get; set; }

            public int LineIndex { get; set; }

            public string ImageFileName { get; set; }

            public string Angle { get; set; }

            public string Throttle { get; set; }

            public string Index { get; set; }

            public bool IsDeleted { get; set; }

            public bool IsAnomaly { get; set; }
        }

        public class ModelRegistryEntry
        {
            public string Name { get; set; } = "";

            public string WindowsPath { get; set; } = "";

            public string WslPath { get; set; } = "";

            public string SourceTubWindowsPath { get; set; } = "";

            public string SourceTubWslPath { get; set; } = "";

            public DateTime CreatedAt { get; set; }

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

            WireUiEvents();

            InitializePlaybackTimer();

            InitializeBrowserMonitor();

            InitializeWSLPaths();

            // ⭐ 추가
            InitializeModelRefreshTimer();

            // ⭐ 추가
            LoadModelsToList();
        }

        // =====================================================
        // ⭐ 모델 감시 타이머
        // =====================================================

        private void InitializeModelRefreshTimer()
        {
            modelRefreshTimer.Interval = 5000;

            modelRefreshTimer.Tick += (s, e) =>
            {
                LoadModelsToList();
            };

            modelRefreshTimer.Start();
        }

        // =====================================================
        // ⭐ 모델 리스트 로드
        // =====================================================

        private void LoadModelsToList()
        {
            try
            {
                if (lstModels == null)
                    return;

                List<ModelRegistryEntry> entries =
                    LoadModelRegistry();

                string modelFolder =
                    Path.Combine(
                        wslBasePath,
                        ModelDirectoryName);

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
                                        entry.Name,
                                        name,
                                        StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        entries.Add(
                            new ModelRegistryEntry()
                            {
                                Name = name,
                                WindowsPath = file,
                                WslPath =
                                    "/home/" +
                                    wslUsername +
                                    "/mycar/" +
                                    ModelDirectoryName +
                                    "/" +
                                    name,
                                CreatedAt =
                                    File.GetCreationTime(file)
                            });
                    }
                }

                entries.Sort(
                    (left, right) =>
                        string.Compare(
                            left.Name,
                            right.Name,
                            StringComparison.OrdinalIgnoreCase));

                lstModels.Items.Clear();

                foreach (ModelRegistryEntry entry in entries)
                {
                    lstModels.Items.Add(entry);
                }

                SaveModelRegistry(entries);
            }
            catch
            {

            }
        }

        // =====================================================
        // 타이머 초기화
        // =====================================================

        private void InitializePlaybackTimer()
        {
            playbackTimer.Interval = 100;

            playbackTimer.Tick += PlaybackTimer_Tick;
        }

        private void WireUiEvents()
        {
            btnPlay.Click += (s, e) => playbackTimer.Start();

            btnPause.Click += (s, e) => playbackTimer.Stop();

            btnStop.Click += BtnStop_Click;

            btnDetectAnomalies.Click += BtnDetectAnomalies_Click;

            lstCatalogRows.SelectedIndexChanged +=
                LstCatalogRows_SelectedIndexChanged;

            cmbSpeed.SelectedIndexChanged +=
                CmbSpeed_SelectedIndexChanged;

            btnCleanData.Click += BtnCleanData_Click;

            btnRestoreData.Click += BtnRestoreData_Click;

            btnModelDlt.Click += BtnModelDlt_Click;

            btnNameCh.Click += BtnNameCh_Click;

            cmbSpeed.SelectedIndex = 1;
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
        // WSL PATH
        // =====================================================

        private void InitializeWSLPaths()
        {
            try
            {
                var distros = GetWSLDistros();

                wslDistroName =
                    ChooseWslDistro(distros);

                wslUsername = GetWSLUserName();

                wslBasePath =
                    BuildWslSharePath(
                        wslDistroName,
                        wslUsername);
            }
            catch
            {
                if (string.IsNullOrWhiteSpace(wslUsername))
                {
                    wslUsername = Environment.UserName;
                }

                wslBasePath =
                    BuildWslSharePath(
                        "Ubuntu",
                        wslUsername);
            }
        }

        private string BuildWslSharePath(
            string distroName,
            string username)
        {
            string wslDollarPath =
                $@"\\wsl$\{distroName}\home\{username}\mycar";

            string wslLocalhostPath =
                $@"\\wsl.localhost\{distroName}\home\{username}\mycar";

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
            foreach (string distro in distros)
            {
                if (
                    TryRunWslCommand(
                        distro,
                        "test -f ~/mycar/train.py"))
                {
                    return distro;
                }
            }

            foreach (string distro in distros)
            {
                if (
                    TryRunWslCommand(
                        distro,
                        "test -d ~/mycar"))
                {
                    return distro;
                }
            }

            if (distros.Count > 0)
            {
                return distros[0];
            }

            return "Ubuntu";
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

                AddWslArguments(psi, "whoami");

                using (Process proc =
                    Process.Start(psi))
                {
                    string user =
                        proc.StandardOutput
                            .ReadToEnd()
                            .Replace("\0", "")
                            .Trim();

                    proc.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(user))
                        return user;
                }
            }
            catch
            {

            }

            return Environment.UserName;
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
            if (lstModels.SelectedItem is ModelRegistryEntry entry)
            {
                return entry.Name;
            }

            return
                lstModels.SelectedItem == null
                    ? ""
                    : lstModels.SelectedItem
                        .ToString()
                        .Replace("\0", "")
                        .Trim();
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

            File.WriteAllText(
                modelRegistryPath,
                JsonSerializer.Serialize(entries, options));
        }

        private void UpsertModelRegistry(
            ModelRegistryEntry model)
        {
            List<ModelRegistryEntry> entries =
                LoadModelRegistry();

            entries.RemoveAll(
                entry =>
                    string.Equals(
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
                    lstModels.Items[i] is ModelRegistryEntry entry
                        ? entry.Name
                        : lstModels.Items[i].ToString();

                if (
                    string.Equals(
                        itemName,
                        model.Name,
                        StringComparison.OrdinalIgnoreCase))
                {
                    lstModels.Items[i] = model;
                    lstModels.SelectedIndex = i;
                    return;
                }
            }

            lstModels.Items.Add(model);
            lstModels.SelectedItem = model;
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
        // DATA LOAD
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
                    selectedDataPath =
                        fbd.SelectedPath;

                    integratedCatalogList.Clear();

                    lstCatalogRows.Items.Clear();

                    string[] catalogFiles =
                        Directory.GetFiles(
                            selectedDataPath,
                            "catalog_*.catalog");

                    Array.Sort(catalogFiles);

                    foreach (
                        string catalogPath
                        in catalogFiles)
                    {
                        string[] lines =
                            File.ReadAllLines(
                                catalogPath);

                        for (
                            int i = 0;
                            i < lines.Length;
                            i++)
                        {
                            string line = lines[i];

                            if (
                                string.IsNullOrWhiteSpace(
                                    line))
                            {
                                continue;
                            }

                            CatalogRecord record =
                                new CatalogRecord()
                                {
                                    OriginalLine = line,

                                    SourceFilePath =
                                        catalogPath,

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

                            integratedCatalogList
                                .Add(record);

                            UpdateListBoxItem(record);
                        }
                    }

                    MessageBox.Show(
                        $"총 {integratedCatalogList.Count}개 프레임 로드 완료");
                }
            }
        }

        // =====================================================
        // LIST UPDATE
        // =====================================================

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

            lstCatalogRows.Items.Add(text);
        }

        // =====================================================
        // PLAYBACK
        // =====================================================

        private void PlaybackTimer_Tick(
            object sender,
            EventArgs e)
        {
            if (
                lstCatalogRows.Items.Count == 0)
                return;

            int next =
                lstCatalogRows.SelectedIndex + 1;

            if (
                next >=
                lstCatalogRows.Items.Count)
            {
                playbackTimer.Stop();

                return;
            }

            lstCatalogRows.SelectedIndex =
                next;
        }

        private void BtnStop_Click(
            object sender,
            EventArgs e)
        {
            playbackTimer.Stop();
        }

        private void CmbSpeed_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            switch (
                cmbSpeed.SelectedIndex)
            {
                case 0:
                    playbackTimer.Interval = 200;
                    break;

                case 1:
                    playbackTimer.Interval = 100;
                    break;

                case 2:
                    playbackTimer.Interval = 50;
                    break;

                case 3:
                    playbackTimer.Interval = 20;
                    break;
            }
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
                idx >= integratedCatalogList.Count)
            {
                return;
            }

            CatalogRecord record =
                integratedCatalogList[idx];

            string imgPath =
                Path.Combine(
                    selectedDataPath,
                    "images",
                    record.ImageFileName);

            if (!File.Exists(imgPath))
            {
                picDriveImage.Image = null;

                return;
            }

            try
            {
                if (
                    picDriveImage.Image != null)
                {
                    picDriveImage.Image.Dispose();

                    picDriveImage.Image = null;
                }

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
                        picDriveImage.Image =
                            new Bitmap(temp);
                    }
                }
            }
            catch
            {
                picDriveImage.Image = null;
            }
        }

        // =====================================================
        // ANOMALY
        // =====================================================

        private void BtnDetectAnomalies_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "이상 데이터 탐지 완료");
        }

        // =====================================================
        // CLEAN
        // =====================================================

        private void BtnCleanData_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "프레임 제외 완료");
        }

        // =====================================================
        // RESTORE
        // =====================================================

        private void BtnRestoreData_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "프레임 복원 완료");
        }

        // =====================================================
        // TRAIN
        // =====================================================

        private async void BtnTrain_Click(
            object sender,
            EventArgs e)
        {
            TrainerStatus statusForm = null;

            try
            {
                if (
                    string.IsNullOrEmpty(
                        selectedDataPath) ||
                    !Directory.Exists(
                        selectedDataPath))
                {
                    MessageBox.Show(
                        "먼저 데이터 폴더를 로드하세요.",
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

                if (!IsWslAvailable(out string wslMessage))
                {
                    MessageBox.Show(
                        wslMessage,
                        "WSL 실행 실패",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                string mycarWslPath =
                    RunWslCommandForOutput(
                        "cd " +
                        DonkeyCarWslPath +
                        " && pwd");

                string selectedTubWslPath =
                    ResolveTubWslPath(
                        selectedDataPath,
                        mycarWslPath);

                string modelName =
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

                string modelWindowsPath =
                    Path.Combine(
                        wslBasePath,
                        ModelDirectoryName,
                        modelName);

                string trainingLogPath =
                    CreateTrainingLogPath(modelName);

                DialogResult result =
                    MessageBox.Show(
                        "AI 학습을 시작합니다.\n\n" +
                        "데이터 폴더:\n" +
                        selectedDataPath +
                        "\n\nWSL 경로:\n" +
                        selectedTubWslPath +
                        "\n\n생성 모델:\n" +
                        modelName +
                        "\n\n로그 파일:\n" +
                        trainingLogPath,
                        "AI 학습 시작",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Information);

                if (result != DialogResult.OK)
                {
                    return;
                }

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
                        selectedTubWslPath,
                        modelRelativePath));

                btnTrain.Enabled = false;
                btnTrain.Text = "학습 실행 중...";

                statusForm =
                    new TrainerStatus();

                statusForm.SetStatus(
                    "WSL 학습 준비 중",
                    selectedDataPath,
                    selectedTubWslPath,
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
                    selectedDataPath,
                    selectedTubWslPath,
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
                    statusForm.SetStatus(
                        "학습 실패",
                        selectedDataPath,
                        selectedTubWslPath,
                        modelWindowsPath,
                        trainingLogPath);

                    throw new InvalidOperationException(
                        "train.py 실행이 실패했습니다.\n\n" +
                        "자세한 내용은 로그 파일을 확인하세요.\n" +
                        trainingLogPath);
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
                        SourceTubWindowsPath = selectedDataPath,
                        SourceTubWslPath = selectedTubWslPath,
                        CreatedAt = DateTime.Now
                    };

                UpsertModelRegistry(model);
                AddOrUpdateModelList(model);

                statusForm.SetStatus(
                    "학습 완료",
                    selectedDataPath,
                    selectedTubWslPath,
                    modelWindowsPath,
                    trainingLogPath);

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
                        "",
                        "");

                    statusForm.AppendLog(
                        "ERROR: " + ex.Message);
                }

                MessageBox.Show(
                    $"학습 실행 실패\n\n{ex.Message}",
                    "학습 실패",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnTrain.Enabled = true;
                btnTrain.Text = "\U0001f9e0 AI 학습 시작";
            }
        }

        private string BuildTrainCommand(
            string mycarWslPath,
            string selectedTubWslPath,
            string modelRelativePath)
        {
            return
                "set -e; " +
                "export PYTHONUNBUFFERED=1; " +
                "cd " +
                QuoteForBash(mycarWslPath) +
                "; " +
                "if [ ! -f train.py ]; then " +
                "echo 'train.py was not found in ~/mycar.' >&2; " +
                "exit 21; " +
                "fi; " +
                "if [ ! -d " +
                QuoteForBash(selectedTubWslPath) +
                " ]; then " +
                "echo 'Selected tub directory does not exist: " +
                EscapeForDoubleQuotedBash(selectedTubWslPath) +
                "' >&2; " +
                "exit 23; " +
                "fi; " +
                "if [ ! -f " +
                QuoteForBash(selectedTubWslPath.TrimEnd('/') + "/manifest.json") +
                " ]; then " +
                "echo 'Selected folder is not a DonkeyCar tub. manifest.json was not found.' >&2; " +
                "exit 24; " +
                "fi; " +
                "mkdir -p " +
                QuoteForBash(ModelDirectoryName) +
                "; " +
                "if [ -x \"$HOME/miniconda3/envs/" +
                TrainingCondaEnvironment +
                "/bin/python\" ]; then " +
                "PYTHON_BIN=\"$HOME/miniconda3/envs/" +
                TrainingCondaEnvironment +
                "/bin/python\"; " +
                "elif [ -f \"$HOME/miniconda3/etc/profile.d/conda.sh\" ]; then " +
                "source \"$HOME/miniconda3/etc/profile.d/conda.sh\"; " +
                "if conda env list | awk '{print $1}' | grep -qx " +
                QuoteForBash(TrainingCondaEnvironment) +
                "; then conda activate " +
                QuoteForBash(TrainingCondaEnvironment) +
                "; fi; " +
                "PYTHON_BIN=$(command -v python || command -v python3 || true); " +
                "else " +
                "PYTHON_BIN=$(command -v python || command -v python3 || true); " +
                "fi; " +
                "if [ -z \"$PYTHON_BIN\" ]; then " +
                "echo 'python or python3 was not found in WSL. Check the conda environment.' >&2; " +
                "exit 22; " +
                "fi; " +
                "echo \"Using Python: $PYTHON_BIN\"; " +
                "echo \"Training tub: " +
                EscapeForDoubleQuotedBash(selectedTubWslPath) +
                "\"; " +
                "echo \"Saving model: " +
                EscapeForDoubleQuotedBash(modelRelativePath) +
                "\"; " +
                "\"$PYTHON_BIN\" train.py --tubs " +
                QuoteForBash(selectedTubWslPath) +
                " --model " +
                QuoteForBash(modelRelativePath);
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
                            AppendTrainingLog(
                                logPath,
                                line.Replace("\0", ""));

                            if (
                                statusForm != null &&
                                !statusForm.IsDisposed)
                            {
                                statusForm.AppendLog(
                                    line.Replace("\0", ""));
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
                if (lstModels.SelectedItem == null)
                {
                    MessageBox.Show(
                        "모델을 먼저 선택하세요.");

                    return;
                }

                string selectedModel =
                    GetSelectedModelName();

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
                DonkeyCarWslPath +
                "; " +
                "if [ -x \"$HOME/miniconda3/envs/" +
                TrainingCondaEnvironment +
                "/bin/python\" ]; then " +
                "PYTHON_BIN=\"$HOME/miniconda3/envs/" +
                TrainingCondaEnvironment +
                "/bin/python\"; " +
                "elif [ -f \"$HOME/miniconda3/etc/profile.d/conda.sh\" ]; then " +
                "source \"$HOME/miniconda3/etc/profile.d/conda.sh\"; " +
                "if conda env list | awk '{print $1}' | grep -qx " +
                QuoteForBash(TrainingCondaEnvironment) +
                "; then conda activate " +
                QuoteForBash(TrainingCondaEnvironment) +
                "; fi; " +
                "PYTHON_BIN=$(command -v python || command -v python3 || true); " +
                "else " +
                "PYTHON_BIN=$(command -v python || command -v python3 || true); " +
                "fi; " +
                "if [ -z \"$PYTHON_BIN\" ]; then " +
                "echo 'python or python3 was not found in WSL. Check the conda environment.' >&2; " +
                "exit 22; " +
                "fi; " +
                "\"$PYTHON_BIN\" manage.py drive --model " +
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
        // MODEL DELETE
        // =====================================================

        private void BtnModelDlt_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                if (lstModels.SelectedItem == null)
                {
                    MessageBox.Show(
                        "삭제할 모델을 선택하세요.");

                    return;
                }

                string selectedModel =
                    GetSelectedModelName();

                DialogResult result =
                    MessageBox.Show(
                        selectedModel +
                        "\n\n선택한 모델을 삭제하시겠습니까?",
                        "모델 삭제",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;

                object selectedItem =
                    lstModels.SelectedItem;

                lstModels.Items.Remove(
                    selectedItem);

                List<ModelRegistryEntry> entries =
                    LoadModelRegistry();

                entries.RemoveAll(
                    entry =>
                        string.Equals(
                            entry.Name,
                            selectedModel,
                            StringComparison.OrdinalIgnoreCase));

                SaveModelRegistry(entries);

                MessageBox.Show(
                    "모델 목록에서 삭제되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message);
            }
        }
        // =====================================================
        // MODEL RENAME
        // =====================================================

        private void BtnNameCh_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                if (lstModels.SelectedItem == null)
                {
                    MessageBox.Show(
                        "이름을 변경할 모델을 선택하세요.");

                    return;
                }

                string oldName =
                    GetSelectedModelName();

                ModelRegistryEntry oldEntry =
                    lstModels.SelectedItem
                        as ModelRegistryEntry;

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

                string modelsPath =
    Path.Combine(
        wslBasePath
            .Replace("\0", "")
            .Trim(),
        ModelDirectoryName);

                string oldFilePath =
                    Path.Combine(
                        modelsPath,
                        oldName);

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

                File.Move(
                    oldFilePath,
                    newFilePath);

                int selectedIndex =
                    lstModels.SelectedIndex;

                ModelRegistryEntry newEntry =
                    new ModelRegistryEntry()
                    {
                        Name = newName,
                        WindowsPath = newFilePath,
                        WslPath =
                            "/home/" +
                            wslUsername +
                            "/mycar/" +
                            ModelDirectoryName +
                            "/" +
                            newName,
                        SourceTubWindowsPath =
                            oldEntry == null
                                ? ""
                                : oldEntry.SourceTubWindowsPath,
                        SourceTubWslPath =
                            oldEntry == null
                                ? ""
                                : oldEntry.SourceTubWslPath,
                        CreatedAt =
                            oldEntry == null
                                ? DateTime.Now
                                : oldEntry.CreatedAt
                    };

                List<ModelRegistryEntry> entries =
                    LoadModelRegistry();

                entries.RemoveAll(
                    entry =>
                        string.Equals(
                            entry.Name,
                            oldName,
                            StringComparison.OrdinalIgnoreCase));

                entries.Add(newEntry);
                SaveModelRegistry(entries);

                lstModels.Items[selectedIndex] =
                    newEntry;

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
