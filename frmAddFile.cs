using System;
using System.Collections.Generic;
using System.IO;
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
            btnClose.Click += btnClose_Click;
        }

        // 파일 선택 버튼
        private void btnSelctFile_Click(object sender, EventArgs e)
        {
            lstviewCopyFile.Items.Clear();
            selectedPaths.Clear();
            txtbSelctFile.Clear();

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = true;

                // WSL Linux 경로
                // Ubuntu 이름은 환경마다 다를 수 있음
                // powershell -> wsl -l 로 확인 가능
                string linuxPath = @"\\wsl$\Ubuntu\home\mycar\data";

                // 경로가 존재하면 시작 위치 설정
                if (Directory.Exists(linuxPath))
                {
                    ofd.InitialDirectory = linuxPath;
                }

                ofd.Title = "mycar/data 파일 선택";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string path in ofd.FileNames)
                    {
                        selectedPaths.Add(path);

                        lstviewCopyFile.Items.Add(
                            new ListViewItem(Path.GetFileName(path)));
                    }

                    if (selectedPaths.Count > 0)
                    {
                        txtbSelctFile.Text =
                            selectedPaths[0] +
                            (selectedPaths.Count > 1
                                ? $" 외 {selectedPaths.Count - 1}건"
                                : "");
                    }
                }
            }
        }

        // Copy 이름 생성
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

        // 실제 복사
        private async void btnAddFile_Click(object sender, EventArgs e)
        {
            if (lstviewAddFile.Items.Count == 0)
            {
                return;
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string targetFolder =
                Path.Combine(baseDir, @"..\..\UploadedFile");

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            List<string> rollbackPaths = new List<string>();

            CancellationTokenSource cts =
                new CancellationTokenSource();

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

                        string targetName =
                            lstviewAddFile.Items[i].Text;

                        string destinationPath =
                            Path.Combine(targetFolder, targetName);

                        rollbackPaths.Add(destinationPath);

                        File.Copy(sourcePath, destinationPath, true);

                        int progress =
                            (int)(((double)(i + 1)
                            / copyTargetPaths.Count) * 100);

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