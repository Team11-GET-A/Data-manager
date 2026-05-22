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
        // 메인 폼의 리스트뷰에 접근하기 위한 참조 변수
        private frmMain _mainForm;

        // 선택된 대상이 파일인지 폴더인지 경로를 관리하기 위한 리스트
        private List<string> selectedPaths = new List<string>();
        // 복사 대상으로 확정된 가상 복사 목록 리스트
        private List<string> copyTargetPaths = new List<string>();

        public frmAddFile()
        {
            InitializeComponent();
            InitListViewStyles();
            RegisterEvents();
        }

        // frmMain에서 이 폼을 호출할 때 부모 객체를 전달받는 생성자 오버로드
        public frmAddFile(frmMain mainForm) : this()
        {
            _mainForm = mainForm;
        }

        private void InitListViewStyles()
        {
            // 세로열 배치를 직관적으로 보여주기 위해 Details 뷰 설정 및 전체 행 선택 활성화
            lstviewCopyFile.View = View.Details;
            lstviewCopyFile.FullRowSelect = true;
            lstviewCopyFile.HeaderStyle = ColumnHeaderStyle.None; // 헤더 숨김
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

        // 1. 파일/폴더 선택 버튼 클릭 시
        private void btnSelctFile_Click(object sender, EventArgs e)
        {
            // 사용자에게 파일과 폴더 중 선택할 방식을 묻는 가벼운 메시지박스 안내
            DialogResult choice = MessageBox.Show("파일을 선택하시려면 [예(Yes)], 폴더를 선택하시려면 [아니요(No)]를 눌러주세요.", "선택 방식 확인", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            lstviewCopyFile.Items.Clear();
            selectedPaths.Clear();
            txtbSelctFile.Clear();

            if (choice == DialogResult.Yes)
            {
                // 파일 선택 탐색기 팝업
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Multiselect = true;
                    ofd.Title = "복사할 로컬 파일을 선택하세요";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        foreach (string filePath in ofd.FileNames)
                        {
                            selectedPaths.Add(filePath);
                            lstviewCopyFile.Items.Add(new ListViewItem(Path.GetFileName(filePath)));
                        }
                        if (ofd.FileNames.Length > 0)
                        {
                            txtbSelctFile.Text = ofd.FileNames[0] + (ofd.FileNames.Length > 1 ? $" 외 {ofd.FileNames.Length - 1}건" : "");
                        }
                    }
                }
            }
            else if (choice == DialogResult.No)
            {
                // 폴더 선택 탐색기 팝업
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.Description = "복사할 파일들이 포함된 폴더를 선택하세요.";
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        string selectedDir = fbd.SelectedPath;
                        txtbSelctFile.Text = selectedDir;

                        // 하위 파일 및 폴더 리스트업
                        string[] files = Directory.GetFiles(selectedDir);
                        string[] dirs = Directory.GetDirectories(selectedDir);

                        foreach (string dirPath in dirs)
                        {
                            selectedPaths.Add(dirPath);
                            lstviewCopyFile.Items.Add(new ListViewItem("[폴더] " + Path.GetFileName(dirPath)));
                        }
                        foreach (string filePath in files)
                        {
                            selectedPaths.Add(filePath);
                            lstviewCopyFile.Items.Add(new ListViewItem(Path.GetFileName(filePath)));
                        }
                    }
                }
            }
        }

        // 2. 복사 대상 목록 생성 버튼 클릭 시 (-Copy 명명 규칙 적용)
        private void btnCopyFile_Click(object sender, EventArgs e)
        {
            lstviewAddFile.Items.Clear();
            copyTargetPaths.Clear();

            if (selectedPaths.Count == 0)
            {
                MessageBox.Show("먼저 대상을 선택해 주세요.", "안내", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (string path in selectedPaths)
            {
                string nameWithoutExt = Path.GetFileNameWithoutExtension(path);
                string ext = Path.GetExtension(path);

                // 폴더인 경우 확장자가 없으므로 이름 뒤에 바로 붙이고, 파일은 확장자 앞에 붙임
                string copyName = Directory.Exists(path) ? $"{nameWithoutExt}-Copy" : $"{nameWithoutExt}-Copy{ext}";

                lstviewAddFile.Items.Add(new ListViewItem(copyName));
                copyTargetPaths.Add(path); // 원본 주소 매핑 보관
            }
        }

        // 3. 파일 추가 (실제 폴더 복사 및 파일 이름 변경 진행) 버튼 클릭 시
        private async void btnAddFile_Click(object sender, EventArgs e)
        {
            if (lstviewAddFile.Items.Count == 0)
            {
                MessageBox.Show("복사 목록에 생성된 파일이 없습니다. [복사] 버튼을 먼저 눌러주세요.", "안내", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 솔루션 내부의 UploadedFile 상대 경로 설정
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string targetFolder = Path.Combine(baseDir, @"..\..\UploadedFile");

            // 폴더가 실제 존재하지 않는다면 안전하게 자동 생성
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            // 요구사항에 따른 진행 상황 표시용 동적 작은 팝업 폼 내부 생성
            Form popup = new Form();
            popup.Size = new Size(320, 150);
            popup.Text = "진행 상태";
            popup.StartPosition = FormStartPosition.CenterParent;
            popup.FormBorderStyle = FormBorderStyle.FixedDialog;
            popup.MaximizeBox = false;
            popup.MinimizeBox = false;

            Label lblStatus = new Label();
            lblStatus.Text = "파일 추가 중...";
            lblStatus.Location = new Point(30, 35);
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);

            Button btnConfirm = new Button();
            btnConfirm.Text = "확인";
            btnConfirm.Location = new Point(115, 75);
            btnConfirm.Size = new Size(80, 30);
            btnConfirm.Visible = false; // 완료 전까지 숨김
            btnConfirm.Click += (s, args) => { popup.Close(); };

            popup.Controls.Add(lblStatus);
            popup.Controls.Add(btnConfirm);

            // 비동기로 파일 복사 및 이름 변경 작업 수행 (UI 블로킹 방지)
            List<string> finalCopiedNames = new List<string>();

            // 팝업을 먼저 노출시키고 연산 시작
            popup.Show(this);
            this.Enabled = false; // 부모 창 조작 금지

            await Task.Run(() =>
            {
                // 디버깅 및 안전성을 위한 최소 대기 시간 부여
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
                            // 디렉토리 복사 로직 호출
                            CopyDirectory(sourcePath, destinationPath);
                        }
                        else if (File.Exists(sourcePath))
                        {
                            // 파일 복사 진행 (기존 파일 존재 시 덮어쓰기 허용)
                            File.Copy(sourcePath, destinationPath, true);
                        }
                        finalCopiedNames.Add(targetName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"복사 중 오류 발생 ({Path.GetFileName(sourcePath)}): {ex.Message}");
                    }
                }
            });

            // 연산 완료 후 알림창 UI 업데이트 처리
            lblStatus.Text = "파일 추가 완료";
            btnConfirm.Visible = true;
            this.Enabled = true;

            // 메인 폼(frmMain)의 lstviewFileList 리스트 업데이트 처리
            if (_mainForm != null && finalCopiedNames.Count > 0)
            {
                _mainForm.Invoke(new Action(() =>
                {
                    // 기존 목록 유지하면서 새로운 복사본 리스트를 세로열로 추가 기입
                    foreach (string name in finalCopiedNames)
                    {
                        ListViewItem newItem = new ListViewItem(name);
                        newItem.Tag = "추가된파일"; // 식별 태그 임의 지정 가능
                        _mainForm.lstviewFileList.Items.Add(newItem);
                    }
                }));
            }
        }

        // 디렉토리 복사를 처리하기 위한 재귀 메서드
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

        // 4. 닫기 버튼 클릭 시
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}