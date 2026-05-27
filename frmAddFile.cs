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
            btnCopyFile.Click += btnCopyFile_Click;
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
                @"\\wsl.localhost\Ubuntu22.04\home"
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
            selectedPaths.Clear();
            txtbSelctFile.Clear();

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = true;
                ofd.Title = "mycar/data 파일 선택";

                string initialDirectory = GetInitialSelectFileDirectory();

                if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
                {
                    ofd.InitialDirectory = initialDirectory;
                }

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string path in ofd.FileNames)
                    {
                        selectedPaths.Add(path);
                        lstviewCopyFile.Items.Add(new ListViewItem(Path.GetFileName(path)));
                    }

                    if (selectedPaths.Count > 0)
                    {
                        txtbSelctFile.Text = selectedPaths[0] + (selectedPaths.Count > 1 ? $" 외 {selectedPaths.Count - 1}건" : "");
                    }
                }
            }
        }

        private void btnCopyFile_Click(object sender, EventArgs e)
        {
            lstviewAddFile.Items.Clear();
            copyTargetPaths.Clear();

            if (selectedPaths.Count == 0)
            {
                return;
            }

            foreach (string path in selectedPaths)
            {
                string nameWithoutExt = Path.GetFileNameWithoutExtension(path);
                string ext = Path.GetExtension(path);
                string copyName = $"{nameWithoutExt}-Copy{ext}";

                lstviewAddFile.Items.Add(new ListViewItem(copyName));
                copyTargetPaths.Add(path);
            }
        }

        private async void btnAddFile_Click(object sender, EventArgs e)
        {
            if (lstviewAddFile.Items.Count == 0)
            {
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

                        string destinationPath = Path.Combine(targetFolder, targetName);

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

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmAddFile_Load(object sender, EventArgs e)
        {
        }
    }
}
