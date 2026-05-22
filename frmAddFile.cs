using System;
using System.Collections.Generic;
using System.IO;
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
            btnClose.Click += btnClose_Click;
        }

        private void btnSelctFile_Click(object sender, EventArgs e)
        {
            lstviewCopyFile.Items.Clear();
            selectedPaths.Clear();
            txtbSelctFile.Clear();

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = true;
                ofd.CheckFileExists = false;
                ofd.ValidateNames = false;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string path in ofd.FileNames)
                    {
                        selectedPaths.Add(path);
                        if (Directory.Exists(path))
                        {
                            lstviewCopyFile.Items.Add(new ListViewItem("[폴더] " + Path.GetFileName(path)));
                        }
                        else
                        {
                            lstviewCopyFile.Items.Add(new ListViewItem(Path.GetFileName(path)));
                        }
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

                string copyName = Directory.Exists(path) ? $"{nameWithoutExt}-Copy" : $"{nameWithoutExt}-Copy{ext}";

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

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string targetFolder = Path.Combine(baseDir, @"..\..\UploadedFile");

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            frmWoking popup = new frmWoking();
            popup.StartPosition = FormStartPosition.CenterParent;
            popup.Show(this);
            this.Enabled = false;

            await Task.Run(() =>
            {
                Task.Delay(1000).Wait();

                for (int i = 0; i < copyTargetPaths.Count; i++)
                {
                    string sourcePath = copyTargetPaths[i];
                    string targetName = lstviewAddFile.Items[i].Text;
                    string destinationPath = Path.Combine(targetFolder, targetName);

                    try
                    {
                        if (Directory.Exists(sourcePath))
                        {
                            CopyDirectory(sourcePath, destinationPath);
                        }
                        else if (File.Exists(sourcePath))
                        {
                            File.Copy(sourcePath, destinationPath, true);
                        }
                    }
                    catch
                    {
                    }
                }
            });

            this.Enabled = true;
            popup.Close();

            if (_mainForm != null)
            {
                _mainForm.Invoke(new Action(() =>
                {
                    _mainForm.LoadUploadedFilesToD();
                }));
            }
        }

        private void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string dest = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }

            foreach (string folder in Directory.GetDirectories(sourceDir))
            {
                string dest = Path.Combine(targetDir, Path.GetFileName(folder));
                CopyDirectory(folder, dest);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}