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
    public partial class frmAddFile : Form
    {
        private frmMain _mainForm;

        private List<string> selectedPaths = new List<string>();
        private List<string> copyTargetPaths = new List<string>();

        public frmAddFile()
        {
            InitializeComponent();

            IconProperty.SetAutoImageByWidthHeight(
                btnSelctFile,
                Data_Manager.Properties.Resources.search3979322,
                22,
                10
            );

            IconProperty.SetAutoImageByWidthHeight(
                btnAddFile,
                Data_Manager.Properties.Resources.A_download,
                28,
                10
            );

            InitListViewStyles();
            RegisterEvents();
        }

        public frmAddFile(frmMain mainForm) : this()
        {
            _mainForm = mainForm;
        }

        private void InitListViewStyles()
        {
            lstviewCopyFile.View = View.Details;
            lstviewCopyFile.FullRowSelect = true;
            lstviewCopyFile.HeaderStyle = ColumnHeaderStyle.None;
            lstviewCopyFile.Columns.Add("File Path", lstviewCopyFile.Width - 25);

            lstviewAddFile.View = View.Details;
            lstviewAddFile.FullRowSelect = true;
            lstviewAddFile.HeaderStyle = ColumnHeaderStyle.None;
            lstviewAddFile.Columns.Add("Copy Name", lstviewAddFile.Width - 25);
        }

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

        private string GetInitialSelectFileDirectory()
        {
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
                ".nuget"
            };

            return skipNames.Any(name => string.Equals(name, directoryName, StringComparison.OrdinalIgnoreCase));
        }

        private void btnSelctFile_Click(object sender, EventArgs e)
        {
            lstviewCopyFile.Items.Clear();
            lstviewAddFile.Items.Clear();
            selectedPaths.Clear();
            copyTargetPaths.Clear();
            txtbSelctFile.Clear();

            string initialDirectory = GetInitialSelectFileDirectory();

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "UploadedFile에 복사할 파일들이 들어있는 폴더를 선택하세요.";
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

                string[] files = Directory.GetFiles(selectedFolder, "*.*", SearchOption.TopDirectoryOnly);

                if (files.Length == 0)
                {
                    MessageBox.Show("선택한 폴더 안에 파일이 없습니다.");
                    return;
                }

                foreach (string path in files)
                {
                    AddSelectedFilePath(path);
                }

                UpdateSelectedFileListView(selectedFolder);
            }
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
                lstviewCopyFile.Items.Add(new ListViewItem(Path.GetFileName(path)));
            }

            if (selectedPaths.Count > 0)
            {
                txtbSelctFile.Text = selectedFolder + $" 안의 파일 {selectedPaths.Count}개";
            }
        }

        private void PrepareCopyFileList()
        {
            lstviewAddFile.Items.Clear();
            copyTargetPaths.Clear();

            if (selectedPaths.Count == 0)
            {
                return;
            }

            HashSet<string> usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string path in selectedPaths)
            {
                string copyName = CreateCopyFileName(path, usedNames);

                lstviewAddFile.Items.Add(new ListViewItem(copyName));
                copyTargetPaths.Add(path);
            }
        }

        private string CreateCopyFileName(string sourcePath, HashSet<string> usedNames)
        {
            string nameWithoutExt = Path.GetFileNameWithoutExtension(sourcePath);
            string ext = Path.GetExtension(sourcePath);
            string copyName = $"{nameWithoutExt}-Copy{ext}";

            if (!usedNames.Contains(copyName))
            {
                usedNames.Add(copyName);
                return copyName;
            }

            int index = 1;

            while (true)
            {
                string candidate = $"{nameWithoutExt}-Copy({index}){ext}";

                if (!usedNames.Contains(candidate))
                {
                    usedNames.Add(candidate);
                    return candidate;
                }

                index++;
            }
        }

        private async void btnAddFile_Click(object sender, EventArgs e)
        {
            PrepareCopyFileList();

            if (lstviewAddFile.Items.Count == 0)
            {
                MessageBox.Show("먼저 폴더를 선택하세요.");
                return;
            }

            string targetFolder = GetUploadedFolder();

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            List<string> rollbackPaths = new List<string>();
            CancellationTokenSource cts = new CancellationTokenSource();
            frmWoking popup = new frmWoking();

            popup.Cts = cts;
            popup.StartPosition = FormStartPosition.CenterParent;

            popup.Show(this);

            this.Enabled = false;

            bool isCancelled = false;

            await Task.Run(() =>
            {
                try
                {
                    Task.Delay(300).Wait();

                    for (int i = 0; i < copyTargetPaths.Count; i++)
                    {
                        if (cts.Token.IsCancellationRequested)
                        {
                            isCancelled = true;
                            break;
                        }

                        string sourcePath = copyTargetPaths[i];
                        string targetName = "";

                        this.Invoke(new Action(() =>
                        {
                            targetName = lstviewAddFile.Items[i].Text;
                        }));

                        string destinationPath = GetNonConflictingPath(Path.Combine(targetFolder, targetName));

                        rollbackPaths.Add(destinationPath);

                        File.Copy(sourcePath, destinationPath, true);

                        int progress = (int)(((double)(i + 1) / copyTargetPaths.Count) * 100);

                        popup.UpdateProgress(progress);
                    }
                }
                catch
                {
                    isCancelled = true;
                }
            });

            if (isCancelled)
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

                popup.Close();
            }
            else
            {
                popup.ShowDone();

                if (_mainForm != null)
                {
                    _mainForm.Invoke(new Action(() =>
                    {
                        _mainForm.LoadUploadedFilesToD();
                    }));
                }
            }

            this.Enabled = true;
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

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmAddFile_Load(object sender, EventArgs e)
        {
        }
    }
}
