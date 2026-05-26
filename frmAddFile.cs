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

            // 이번 작업에서 추가된 파일/폴더 경로만 추적하기 위한 리스트
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
                    Task.Delay(500).Wait(); // 팝업 UI 표시될 시간 약간 확보

                    for (int i = 0; i < copyTargetPaths.Count; i++)
                    {
                        // 작업 중 취소 확인
                        if (cts.Token.IsCancellationRequested)
                        {
                            isCancelled = true;
                            break;
                        }

                        string sourcePath = copyTargetPaths[i];
                        string targetName = lstviewAddFile.Items[i].Text;
                        string destinationPath = Path.Combine(targetFolder, targetName);

                        // 롤백을 위해 복사 시도하는 경로를 기록
                        rollbackPaths.Add(destinationPath);

                        if (Directory.Exists(sourcePath))
                        {
                            CopyDirectory(sourcePath, destinationPath, cts.Token);
                        }
                        else if (File.Exists(sourcePath))
                        {
                            File.Copy(sourcePath, destinationPath, true);
                        }

                        // 작업 후 취소 확인
                        if (cts.Token.IsCancellationRequested)
                        {
                            isCancelled = true;
                            break;
                        }

                        // 프로그레스바 퍼센트 계산 및 갱신
                        int progressPercent = (int)(((double)(i + 1) / copyTargetPaths.Count) * 100);
                        popup.UpdateProgress(progressPercent);
                    }
                }
                catch (OperationCanceledException)
                {
                    isCancelled = true;
                }
                catch
                {
                    // 예기치 않은 오류 발생 시 롤백 처리 유도
                    isCancelled = true;
                }
            });

            if (isCancelled)
            {
                // 취소된 경우: 이번 작업에 추가된(rollbackPaths) 파일 및 폴더만 삭제
                foreach (string path in rollbackPaths)
                {
                    try
                    {
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                        else if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                    catch
                    {
                        // 삭제 중 오류가 발생해도 진행
                    }
                }

                this.Enabled = true;
                popup.Close(); // 취소되었으므로 팝업을 바로 닫음
            }
            else
            {
                // 정상 완료된 경우: 완료 버튼 표시
                this.Enabled = true;
                popup.ShowDone();

                if (_mainForm != null)
                {
                    _mainForm.Invoke(new Action(() =>
                    {
                        _mainForm.LoadUploadedFilesToD();
                    }));
                }
            }
        }

        // 내부 폴더/파일 복사 시에도 취소를 감지할 수 있도록 CancellationToken 추가
        private void CopyDirectory(string sourceDir, string targetDir, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;

            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                if (cancellationToken.IsCancellationRequested) return;
                string dest = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }

            foreach (string folder in Directory.GetDirectories(sourceDir))
            {
                if (cancellationToken.IsCancellationRequested) return;
                string dest = Path.Combine(targetDir, Path.GetFileName(folder));
                CopyDirectory(folder, dest, cancellationToken);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}