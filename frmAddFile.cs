#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AD_AI_LearningData_Editor;

namespace Data_Manager
{
    // 로컬 폴더의 파일을 프로그램의 bin\UploadedFile 폴더로 복사하는 업로드 폼입니다.
    // 주의: DonkeyCar tub 데이터는 manifest/catalog/image 파일명이 서로 연결되어 있으므로
    // 파일명을 바꾸면 학습용 tub 구조가 깨질 수 있습니다.
    public partial class frmAddFile : Form
    {
        private frmMain _mainForm;

        // selectedPaths는 사용자가 선택한 원본 파일 경로,
        // copyTargetPaths는 실제 복사 버튼을 눌렀을 때 복사할 파일 경로입니다.
        private List<string> selectedPaths = new List<string>();
        private List<string> copyTargetPaths = new List<string>();
        private List<string> selectedSourceRoots = new List<string>();
        private List<CopyPlanItem> copyPlanItems = new List<CopyPlanItem>();
        private ToolTip AddFileToolTip;

        private HashSet<string> allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".bmp",
            ".bit",
            ".gif",
            ".tif",
            ".tiff",
            ".json",
            ".txt",
            ".csv",
            ".xml",
            ".yaml",
            ".yml",
            ".catalog",
            ".catalog_manifest"
        };

        private class CopyPlanItem
        {
            public string SourceRoot { get; set; } = "";
            public string SourcePath { get; set; } = "";
            public string RelativePath { get; set; } = "";
        }

        public frmAddFile()
        {
            InitializeComponent();
            AddFileToolTip = propToolTip.CreateDefaultToolTip();
            InitializeToolTips();

            IconProperty.SetAutoImageByMargins(
                btnSelctFile,
                Data_Manager.Properties.Resources.AddFolder14970929,
                leftMargin: 22,
                topMargin: 7,
                rightMargin: 22,
                bottomMargin: 19
            );

            IconProperty.SetAutoImageByMargins(
               btnAddFile,
               Data_Manager.Properties.Resources.A_download,
               leftMargin: 22,
               topMargin: 7,
               rightMargin: 22,
               bottomMargin: 19
           );


            InitListViewStyles();

            RegisterEvents();
        }

        public frmAddFile(frmMain mainForm)
            : this()
        {
            _mainForm = mainForm;
        }

        // =====================================================
        // ListView 스타일
        // =====================================================

        private void InitListViewStyles()
        {
            lstviewCopyFile.View =
                View.Details;

            lstviewCopyFile.FullRowSelect =
                true;

            lstviewCopyFile.HeaderStyle =
                ColumnHeaderStyle.None;

            lstviewCopyFile.Columns.Add(
                "Frame Data",
                lstviewCopyFile.Width - 25);

            lstviewAddFile.View =
                View.Details;

            lstviewAddFile.FullRowSelect =
                true;

            lstviewAddFile.HeaderStyle =
                ColumnHeaderStyle.None;

            lstviewAddFile.Columns.Add(
                "Copy Data",
                lstviewAddFile.Width - 25);
        }

        // =====================================================
        // 이벤트 등록
        // =====================================================

        private void RegisterEvents()
        {
            btnSelctFile.Click += btnSelctFile_Click;
            btnAddFile.Click += btnAddFile_Click;
        }

        private string GetBinFolder()
        {
            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (dir != null)
            {
                if (string.Equals(dir.Name, "bin", StringComparison.OrdinalIgnoreCase))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }

            return AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private string GetUploadedFolder()
        {
            string folder = Path.Combine(GetBinFolder(), "UploadedFile");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }

        private string GetUploadedDataFolder()
        {
            string folder = Path.Combine(GetUploadedFolder(), "data");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }

        private string GetInitialSelectFileDirectory()
        {
            // 기본 선택 위치는 WSL의 ~/mycar/data를 우선 사용하고,
            // 찾지 못하면 Data-manager\bin 또는 현재 실행 bin 폴더로 대체합니다.
            string wslPath = FindWslMycarDataPath();

            if (!string.IsNullOrWhiteSpace(wslPath) && Directory.Exists(wslPath))
            {
                return wslPath;
            }

            string dataManagerBinPath = FindDataManagerBinPath();

            if (!string.IsNullOrWhiteSpace(dataManagerBinPath) && Directory.Exists(dataManagerBinPath))
            {
                return dataManagerBinPath;
            }

            return GetBinFolder();
        }

        private string FindWslMycarDataPath()
        {
            string[] wslHomeRoots =
            {
                @"\\wsl.localhost\Ubuntu\home",
                @"\\wsl.localhost\Ubuntu-22.04\home"
            };

            foreach (string homeRoot in wslHomeRoots)
            {
                try
                {
                    if (!Directory.Exists(homeRoot))
                    {
                        continue;
                    }

                    foreach (string linuxUserDirectory in Directory.GetDirectories(homeRoot))
                    {
                        string candidate = Path.Combine(linuxUserDirectory, "mycar", "data");

                        if (Directory.Exists(candidate))
                        {
                            return candidate;
                        }
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private string FindDataManagerBinPath()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            List<string> searchRoots = new List<string>();

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string downloads = Path.Combine(userProfile, "Downloads");
            string repos = Path.Combine(userProfile, "source", "repos");

            if (!string.IsNullOrWhiteSpace(desktop)) searchRoots.Add(desktop);
            if (!string.IsNullOrWhiteSpace(downloads)) searchRoots.Add(downloads);
            if (!string.IsNullOrWhiteSpace(repos)) searchRoots.Add(repos);

            foreach (string root in searchRoots.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                string result = FindDataManagerBinPathInRoot(root, 4);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    return result;
                }
            }

            return null;
        }

        private string FindDataManagerBinPathInRoot(string root, int maxDepth)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                {
                    return null;
                }

                string directCandidate = Path.Combine(root, "Data-manager", "bin");

                if (Directory.Exists(directCandidate))
                {
                    return directCandidate;
                }

                return SearchDirectoryForDataManagerBin(root, 0, maxDepth);
            }
            catch
            {
                return null;
            }
        }

        private string SearchDirectoryForDataManagerBin(string currentDirectory, int depth, int maxDepth)
        {
            if (depth > maxDepth)
            {
                return null;
            }

            try
            {
                string folderName = Path.GetFileName(currentDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

                if (string.Equals(folderName, "Data-manager", StringComparison.OrdinalIgnoreCase))
                {
                    string binPath = Path.Combine(currentDirectory, "bin");

                    if (Directory.Exists(binPath))
                    {
                        return binPath;
                    }
                }

                foreach (string subDirectory in Directory.GetDirectories(currentDirectory))
                {
                    string subFolderName = Path.GetFileName(subDirectory);

                    if (ShouldSkipDirectory(subFolderName))
                    {
                        continue;
                    }

                    string result = SearchDirectoryForDataManagerBin(subDirectory, depth + 1, maxDepth);

                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        return result;
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private bool ShouldSkipDirectory(string directoryName)
        {
            if (string.IsNullOrWhiteSpace(directoryName))
            {
                return true;
            }

            string[] skipNames =
            {
                ".git",
                ".vs",
                "bin",
                "obj",
                "node_modules",
                "packages",
                ".nuget",
                "__pycache__"
            };

            return skipNames.Any(name => string.Equals(name, directoryName, StringComparison.OrdinalIgnoreCase));
        }

        // =====================================================
        // 데이터 폴더 선택
        // =====================================================

        private void btnSelctFile_Click(
            object sender,
            EventArgs e)
        {
            // 폴더 하나를 선택하면 내부의 허용 확장자 파일들을 재귀적으로 모읍니다.
            // 임시/백업 파일은 UploadedFile에 섞이지 않도록 제외합니다.
            lstviewCopyFile.Items.Clear();
            lstviewAddFile.Items.Clear();
            selectedPaths.Clear();
            copyTargetPaths.Clear();
            selectedSourceRoots.Clear();
            copyPlanItems.Clear();
            txtbSelctFile.Clear();

            string initialDirectory = GetInitialSelectFileDirectory();

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "UploadedFile\\data에 복사할 파일들이 들어있는 폴더를 선택하세요.";
                fbd.ShowNewFolderButton = false;

                if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
                {
                    fbd.SelectedPath = initialDirectory;
                }

                if (fbd.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                string selectedFolder = fbd.SelectedPath;

                if (string.IsNullOrWhiteSpace(selectedFolder) || !Directory.Exists(selectedFolder))
                {
                    return;
                }

                selectedSourceRoots = GetCopySourceRoots(selectedFolder);
                string[] files = selectedSourceRoots
                    .SelectMany(root => GetSelectableFilesFromFolder(root))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (files.Length == 0)
                {
                    MessageBox.Show("선택한 폴더 안에서 복사할 수 있는 파일을 찾지 못했습니다.");
                    return;
                }

                foreach (string path in files)
                {
                    AddSelectedFilePath(path);
                }

                UpdateSelectedFileListView(selectedFolder);
            }
        }

        private List<string> GetCopySourceRoots(string selectedFolder)
        {
            List<string> roots = new List<string>();

            if (IsTubRoot(selectedFolder))
            {
                roots.Add(selectedFolder);
                return roots;
            }

            try
            {
                roots.AddRange(
                    Directory.GetDirectories(selectedFolder)
                        .Where(IsTubRoot)
                        .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase));
            }
            catch
            {
            }

            if (roots.Count == 0)
            {
                roots.Add(selectedFolder);
            }

            return roots
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private bool IsTubRoot(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                return false;
            }

            bool hasManifest =
                File.Exists(Path.Combine(folder, "manifest.json")) ||
                File.Exists(Path.Combine(folder, "catalog_manifest.json"));
            bool hasCatalog =
                Directory.GetFiles(folder, "catalog_*.catalog", SearchOption.TopDirectoryOnly).Length > 0;
            bool hasRecords =
                Directory.GetFiles(folder, "record_*.json", SearchOption.TopDirectoryOnly).Length > 0;

            return hasManifest || hasCatalog || hasRecords;
        }

        private string[] GetSelectableFilesFromFolder(string selectedFolder)
        {
            try
            {
                return Directory.GetFiles(selectedFolder, "*.*", SearchOption.AllDirectories)
                    .Where(path => IsAllowedFile(path))
                    .Where(path => !IsTemporaryOrBackupFile(path))
                    .OrderBy(path => path)
                    .ToArray();
            }
            catch
            {
                try
                {
                    return Directory.GetFiles(selectedFolder, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(path => IsAllowedFile(path))
                        .Where(path => !IsTemporaryOrBackupFile(path))
                        .OrderBy(path => path)
                        .ToArray();
                }
                catch
                {
                    return new string[0];
                }
            }
        }

        private bool IsAllowedFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return false;
            }

            string ext = Path.GetExtension(path);

            if (string.IsNullOrWhiteSpace(ext))
            {
                return true;
            }

            return allowedExtensions.Contains(ext);
        }

        private bool IsTemporaryOrBackupFile(string path)
        {
            string fileName = Path.GetFileName(path);

            if (fileName.EndsWith(".gback", StringComparison.OrdinalIgnoreCase)) return true;
            if (fileName.EndsWith(".roiback", StringComparison.OrdinalIgnoreCase)) return true;
            if (fileName.EndsWith(".editingtmp", StringComparison.OrdinalIgnoreCase)) return true;
            if (fileName.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase)) return true;
            if (fileName.StartsWith("~", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }

        private void AddSelectedFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            if (selectedPaths.Any(x => string.Equals(x, path, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            selectedPaths.Add(path);
        }

        private void UpdateSelectedFileListView(string selectedFolder)
        {
            lstviewCopyFile.Items.Clear();

            foreach (string path in selectedPaths)
            {
                lstviewCopyFile.Items.Add(new ListViewItem(GetDisplayPathForSelectedFile(path)));
            }

            if (selectedPaths.Count > 0)
            {
                txtbSelctFile.Text = selectedFolder + $" 안의 파일 {selectedPaths.Count}개";
            }
        }

        private string GetDisplayPathForSelectedFile(string path)
        {
            string root = FindSelectedRootForPath(path);
            if (string.IsNullOrWhiteSpace(root))
            {
                return Path.GetFileName(path);
            }

            string rootName = Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string relativePath = Path.GetRelativePath(root, path);
            return Path.Combine(rootName, relativePath);
        }

        private string FindSelectedRootForPath(string path)
        {
            string fullPath = Path.GetFullPath(path);

            return selectedSourceRoots
                .OrderByDescending(root => root.Length)
                .FirstOrDefault(root =>
                    fullPath.StartsWith(
                        Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase));
        }

        private void PrepareCopyFileList()
        {
            // 왼쪽 목록(selectedPaths)을 오른쪽 복사 예정 목록(copyTargetPaths)으로 옮깁니다.
            // tub 내부 파일명은 manifest/catalog가 참조하므로 변경하지 않고 상대 경로 그대로 복사합니다.
            lstviewAddFile.Items.Clear();

            copyTargetPaths.Clear();
            copyPlanItems.Clear();

            if (selectedPaths.Count == 0)
            {
                MessageBox.Show(
                    "먼저 폴더를 선택하세요.");

                return;
            }

            foreach (string path in selectedPaths)
            {
                string root = FindSelectedRootForPath(path);
                if (string.IsNullOrWhiteSpace(root))
                {
                    root = Path.GetDirectoryName(path);
                }

                string relativePath = Path.GetRelativePath(root, path);
                string rootName = Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                string copyName = Path.Combine(rootName, relativePath);

                lstviewAddFile.Items.Add(new ListViewItem(copyName));
                copyTargetPaths.Add(path);
                copyPlanItems.Add(new CopyPlanItem
                {
                    SourceRoot = root,
                    SourcePath = path,
                    RelativePath = relativePath
                });
            }
        }

        private async void btnAddFile_Click(object sender, EventArgs e)
        {
            // UI는 잠그고 백그라운드에서 파일을 복사합니다.
            // 중간 실패나 취소가 생기면 이미 복사한 파일을 지워 UploadedFile을 되돌립니다.
            PrepareCopyFileList();

            if (lstviewAddFile.Items.Count == 0)
            {
                MessageBox.Show("먼저 폴더를 선택하세요.");
                return;
            }

            string targetFolder = GetUploadedDataFolder();

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            List<string> rollbackPaths = new List<string>();
            List<string> rollbackRootFolders = new List<string>();
            List<string> copiedRootFolders = new List<string>();
            CancellationTokenSource cts = new CancellationTokenSource();
            frmWoking popup = new frmWoking();
            bool isCancelled = false;
            Exception copyException = null;

            popup.Cts = cts;

            // frmWoking이 frmAddFile의 중앙에 뜨도록 직접 위치를 계산합니다.
            // Show(this) + CenterParent만으로는 모델리스 폼에서 원하는 위치가 안 잡히는 경우가 있어
            // StartPosition.Manual을 사용합니다.
            popup.StartPosition = FormStartPosition.Manual;
            CenterWorkingPopupOnThisForm(popup);

            popup.Show(this);

            this.Enabled = false;

            try
            {
                await Task.Run(() =>
                {
                    Task.Delay(300).Wait();
                    Dictionary<string, string> destinationRoots =
                        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    for (int i = 0; i < copyPlanItems.Count; i++)
                    {
                        if (cts.Token.IsCancellationRequested)
                        {
                            isCancelled = true;
                            break;
                        }

                        CopyPlanItem plan = copyPlanItems[i];

                        if (!destinationRoots.TryGetValue(plan.SourceRoot, out string destinationRoot))
                        {
                            string rootName =
                                Path.GetFileName(
                                    plan.SourceRoot.TrimEnd(
                                        Path.DirectorySeparatorChar,
                                        Path.AltDirectorySeparatorChar));

                            bool copyRootContentsToUploadData =
                                string.Equals(rootName, "data", StringComparison.OrdinalIgnoreCase);

                            destinationRoot = copyRootContentsToUploadData
                                ? targetFolder
                                : GetNonConflictingPath(
                                    Path.Combine(targetFolder, rootName));

                            destinationRoots[plan.SourceRoot] = destinationRoot;
                            if (!copyRootContentsToUploadData)
                            {
                                rollbackRootFolders.Add(destinationRoot);
                            }
                            copiedRootFolders.Add(destinationRoot);
                        }

                        string destinationPath =
                            Path.Combine(destinationRoot, plan.RelativePath);

                        string destinationDirectory =
                            Path.GetDirectoryName(destinationPath);

                        if (!string.IsNullOrWhiteSpace(destinationDirectory))
                        {
                            Directory.CreateDirectory(destinationDirectory);
                        }

                        File.Copy(plan.SourcePath, destinationPath, overwrite: true);
                        rollbackPaths.Add(destinationPath);

                        int progress = (int)(((double)(i + 1) / copyPlanItems.Count) * 100);

                        popup.UpdateProgress(progress);

                        Thread.Sleep(1);
                    }
                });
            }
            catch (Exception ex)
            {
                copyException = ex;
            }
            finally
            {
                this.Enabled = true;
            }

            if (isCancelled || copyException != null)
            {
                foreach (string path in rollbackPaths)
                {
                    try
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                    catch
                    {
                    }
                }

                foreach (string folder in rollbackRootFolders.OrderByDescending(path => path.Length))
                {
                    try
                    {
                        if (Directory.Exists(folder))
                        {
                            Directory.Delete(folder, recursive: true);
                        }
                    }
                    catch
                    {
                    }
                }

                if (!popup.IsDisposed)
                {
                    popup.Close();
                }

                if (copyException != null)
                {
                    MessageBox.Show($"파일 복사 중 오류가 발생했습니다.\r\n{copyException.Message}");
                }
                else
                {
                    MessageBox.Show("파일 복사가 취소되었습니다.");
                }

                return;
            }

            popup.ShowDone();

            MessageBox.Show(
                $"총 {copyPlanItems.Count}개 파일 복사 완료");

            if (_mainForm != null)
            {
                _mainForm.Invoke(
                    new Action(() =>
                    {
                        _mainForm.LoadUploadedFilesToD();
                        if (copiedRootFolders.Count > 0)
                        {
                            _mainForm.LoadTrainerDataFolder(copiedRootFolders[0]);
                        }
                    }));
            }
        }

        // =====================================================
        // JSON VALUE 추출
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
                {
                    return "";
                }

                startIdx +=
                    searchKey.Length;

                while (
                    startIdx < json.Length
                    &&
                    json[startIdx] == ' ')
                {
                    startIdx++;
                }

                // 문자열
                if (json[startIdx] == '"')
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
                // 숫자
                else
                {
                    int endIdx =
                        json.IndexOfAny(
                            new char[] { ',', '}' },
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
        // 닫기
        // =====================================================

        private void CenterWorkingPopupOnThisForm(Form popup)
        {
            if (popup == null)
            {
                return;
            }

            // 현재 frmAddFile의 실제 화면 좌표를 기준으로 중앙 위치를 계산합니다.
            Rectangle ownerBounds = this.Bounds;

            int x = ownerBounds.Left + (ownerBounds.Width - popup.Width) / 2;
            int y = ownerBounds.Top + (ownerBounds.Height - popup.Height) / 2;

            // 팝업이 모니터 밖으로 나가지 않도록 보정합니다.
            Rectangle workingArea = Screen.FromControl(this).WorkingArea;

            if (x < workingArea.Left) x = workingArea.Left;
            if (y < workingArea.Top) y = workingArea.Top;
            if (x + popup.Width > workingArea.Right) x = workingArea.Right - popup.Width;
            if (y + popup.Height > workingArea.Bottom) y = workingArea.Bottom - popup.Height;

            popup.Location = new System.Drawing.Point(Math.Max(workingArea.Left, x), Math.Max(workingArea.Top, y));
        }

        private string GetNonConflictingPath(string path)
        {
            if (!File.Exists(path))
            {
                return path;
            }

            string directory = Path.GetDirectoryName(path);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);
            int index = 1;

            while (true)
            {
                string candidate = Path.Combine(directory, $"{nameWithoutExt} ({index}){ext}");

                if (!File.Exists(candidate))
                {
                    return candidate;
                }

                index++;
            }
        }

        private void InitializeToolTips()
        {
            SetToolTipByName("btnSelctFile", "파일 업로드하기");
            SetToolTipByName("btnAddFile", "업로드된 파일을 프로그램에 추가");
        }
        private void SetToolTipByName(string controlName, string text)
        {
            if (AddFileToolTip == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(controlName))
            {
                return;
            }

            Control control = this.Controls.Find(controlName, true).FirstOrDefault();

            if (control == null)
            {
                return;
            }

            propToolTip.Set(AddFileToolTip, control, text);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // =====================================================
        // FORM LOAD
        // =====================================================

        private void frmAddFile_Load(
            object sender,
            EventArgs e)
        {
        }
    }
}
