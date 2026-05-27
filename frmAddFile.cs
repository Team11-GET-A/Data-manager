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
        // 업로드 완료 후 목록을 갱신할 메인 폼 참조
        private frmMain _mainForm;

        // 사용자가 선택한 원본 파일 경로 목록
        private readonly List<string> selectedPaths = new List<string>();
        // 실제 복사 대상 경로 목록 (원본 경로 유지)
        private readonly List<string> copyTargetPaths = new List<string>();

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

        // 리스트뷰 표시 형식과 컬럼 구성
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

        // 버튼 클릭 이벤트 연결
        private void RegisterEvents()
        {
            btnSelctFile.Click += btnSelctFile_Click;
            btnCopyFile.Click += btnCopyFile_Click;
            btnAddFile.Click += btnAddFile_Click;
            btnClose.Click += btnClose_Click;
        }

        // 파일 선택 버튼 처리
        private void btnSelctFile_Click(object sender, EventArgs e)
        {
            lstviewCopyFile.Items.Clear();
            selectedPaths.Clear();
            txtbSelctFile.Clear();

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = true;

                // WSL 기본 경로가 있으면 시작 경로로 사용
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

        // 선택한 파일에 대해 복사본 이름 생성
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

        // 실제 파일 복사 및 진행 팝업 표시
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

            // 작업 취소 시 롤백할 경로 수집
            List<string> rollbackPaths = new List<string>();

            // 진행 상태 팝업에서 사용할 취소 토큰
            CancellationTokenSource cts = new CancellationTokenSource();

            frmWoking popup = new frmWoking();

            popup.Cts = cts;
            popup.StartPosition = FormStartPosition.CenterParent;

            popup.Show(this);

            // 팝업 작업 중 메인 폼 비활성화
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

                        string targetName = lstviewAddFile.Items[i].Text;

                        string destinationPath = Path.Combine(targetFolder, targetName);

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
                // 실패/취소 시 생성된 파일 삭제
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
                    // 메인 폼 목록 갱신
                    _mainForm.Invoke(new Action(() =>
                    {
                        _mainForm.LoadUploadedFilesToD();
                    }));
                }
            }

            this.Enabled = true;
        }

        // 닫기 버튼 처리
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}