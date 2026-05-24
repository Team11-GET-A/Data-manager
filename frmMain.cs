using Data_Manager;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AD_AI_LearningData_Editor
{
    public partial class frmMain : MaterialForm
    {
        // 슬라이드 기능 제어를 위한 전역 변수
        private System.Windows.Forms.Timer videoTimer;
        private PictureBox picVideoBox;
        private List<string> slideImages = new List<string>();
        private int currentSlideIndex = 0;
        private ListViewItem lastHighlightedItem = null;

        public frmMain()
        {
            InitializeComponent();

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);

            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.BlueGrey800, Primary.BlueGrey900,
                Primary.BlueGrey500, Accent.Red400,
                TextShade.WHITE
            );

            // 비디오 플레이어(PictureBox 및 Timer) 초기화
            InitializeVideoPlayer();

            SetupTabs();
            LoadUploadedFilesToD();

            this.lstviewMain.MouseDoubleClick += lstviewMain_MouseDoubleClick;
            this.lstviewFileList.MouseDoubleClick += lstviewFileList_MouseDoubleClick;
        }

        private void InitializeVideoPlayer()
        {
            // Image(라틴어:모방) 출력을 위한 PictureBox 동적 생성
            picVideoBox = new PictureBox();
            picVideoBox.Dock = DockStyle.Fill;
            picVideoBox.SizeMode = PictureBoxSizeMode.StretchImage;

            // pnlVideo 컨트롤 안에 PictureBox 삽입
            if (this.Controls.Find("pnlVideo", true).FirstOrDefault() is Panel pnl)
            {
                pnl.Controls.Add(picVideoBox);
            }

            // 30 FPS를 위한 Timer 설정 (1000ms / 30 = 약 33ms)
            videoTimer = new System.Windows.Forms.Timer();
            videoTimer.Interval = 33;
            videoTimer.Tick += VideoTimer_Tick;
        }

        public void LoadUploadedFilesToD()
        {
            lstviewFileListD.Items.Clear();
            slideImages.Clear();
            currentSlideIndex = 0;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string targetFolder = Path.Combine(baseDir, @"..\..\UploadedFile");

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            DirectoryInfo di = new DirectoryInfo(targetFolder);

            var folders = di.GetDirectories().OrderBy(d => d.CreationTime).ToList();
            foreach (var folder in folders)
            {
                ListViewItem item = new ListViewItem("[폴더] " + folder.Name);
                item.Tag = "추가된파일";
                lstviewFileListD.Items.Add(item);
            }

            var files = di.GetFiles().OrderBy(f => f.CreationTime).ToList();
            foreach (var file in files)
            {
                ListViewItem item = new ListViewItem(file.Name);
                item.Tag = "추가된파일";
                lstviewFileListD.Items.Add(item);

                // 이미지 파일 추출 (.png, .jpg, .bmp, .bit 포함)
                string ext = file.Extension.ToLower();
                if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".bit")
                {
                    slideImages.Add(file.FullName);
                }
            }

            // 이미지가 한 장 이상 존재할 경우 첫 화면 로드 및 슬라이더 초기화
            if (slideImages.Count > 0)
            {
                sdrSeekBar.RangeMax = slideImages.Count;
                sdrSeekBar.RangeMin = 1;
                UpdateSlideDisplay();
            }
        }

        private void UpdateSlideDisplay()
        {
            if (slideImages.Count == 0) return;

            string currentImagePath = slideImages[currentSlideIndex];

            // 이전 이미지 리소스 해제 (메모리 누수 및 파일 잠금 방지)
            if (picVideoBox.Image != null)
            {
                Image oldImage = picVideoBox.Image;
                picVideoBox.Image = null;
                oldImage.Dispose();
            }

            using (FileStream fs = new FileStream(currentImagePath, FileMode.Open, FileAccess.Read))
            {
                picVideoBox.Image = Image.FromStream(fs);
            }

            // Slider(게르만어:미끄러짐) UI 및 Text 업데이트
            sdrSeekBar.Value = currentSlideIndex + 1;
            sdrSeekBar.Text = $"{currentSlideIndex + 1}/{slideImages.Count}";

            // 리스트뷰 색상 초기화 (이전 항목 검은색 복구)
            if (lastHighlightedItem != null)
            {
                lastHighlightedItem.ForeColor = Color.Black;
            }

            // 현재 출력 중인 파일을 찾아 초록색으로 변경
            string currentFileName = Path.GetFileName(currentImagePath);
            foreach (ListViewItem item in lstviewFileListD.Items)
            {
                if (item.Text == currentFileName)
                {
                    item.ForeColor = Color.Green;
                    lastHighlightedItem = item;
                    break;
                }
            }
        }

        // 재생 및 정지 버튼 클릭 이벤트
        private void btnPlayStop_Click(object sender, EventArgs e)
        {
            if (slideImages.Count == 0) return;

            if (videoTimer.Enabled)
            {
                videoTimer.Stop(); // 타이머 중단 시 현재 이미지에서 멈춤
            }
            else
            {
                videoTimer.Start();
            }
        }

        private void VideoTimer_Tick(object sender, EventArgs e)
        {
            currentSlideIndex++;
            if (currentSlideIndex >= slideImages.Count)
            {
                currentSlideIndex = 0; // 끝까지 재생 시 처음으로 루프
            }
            UpdateSlideDisplay();
        }

        // 기존 탭 및 기타 이벤트 메서드 유지
        private void SetupTabs()
        {
            MaterialTabControl tabControl = new MaterialTabControl();
            tabControl.Dock = DockStyle.Fill;

            TabPage tabManager = new TabPage("매니저");
            TabPage tabTrainer = new TabPage("트레이너");
            TabPage tabPilot = new TabPage("파일럿");

            tabControl.Controls.Add(tabManager);
            tabControl.Controls.Add(tabTrainer);
            tabControl.Controls.Add(tabPilot);

            var controlsToMove = new System.Collections.Generic.List<Control>();
            foreach (Control c in this.Controls)
            {
                if (c != tabControl && c.Name != "DrawerTabControl")
                {
                    controlsToMove.Add(c);
                }
            }
            foreach (Control c in controlsToMove)
            {
                tabManager.Controls.Add(c);
            }

            this.Controls.Add(tabControl);

            Data_Manager.Form3 form3 = new Data_Manager.Form3();
            form3.TopLevel = false;
            form3.FormBorderStyle = FormBorderStyle.None;
            form3.Dock = DockStyle.Fill;
            tabTrainer.Controls.Add(form3);
            form3.Show();

            Data_Manager.Form2 form2 = new Data_Manager.Form2();
            form2.TopLevel = false;
            form2.FormBorderStyle = FormBorderStyle.None;
            form2.Dock = DockStyle.Fill;
            tabPilot.Controls.Add(form2);
            form2.Show();

            this.DrawerTabControl = tabControl;
        }

        private void materialSlider1_Click(object sender, EventArgs e) { }
        private void materialButton2_Click(object sender, EventArgs e) { }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void listView1_SelectedIndexChanged_1(object sender, EventArgs e) { }

        private void btnOpnFolderList1_Click(object sender, EventArgs e)
        {
            lstviewFileList.Visible = false;
            lstviewFileListD.Visible = false;
            lstviewTrash.Visible = false;
            lstviewMain.Visible = true;
        }

        private void lstviewMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstviewMain.SelectedItems.Count > 0)
            {
                string itemTag = lstviewMain.SelectedItems[0].Tag?.ToString();

                if (itemTag == "파일목록")
                {
                    lstviewMain.Visible = false;
                    lstviewFileList.Visible = true;
                    lstviewFileListD.Visible = true;
                    lstviewTrash.Visible = false;
                }
                else if (itemTag == "휴지통")
                {
                    lstviewMain.Visible = false;
                    lstviewFileList.Visible = false;
                    lstviewFileListD.Visible = false;
                    lstviewTrash.Visible = true;
                }
            }
        }

        private void lstviewFileList_SelectedIndexChanged(object sender, EventArgs e) { }

        private void lstviewFileList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstviewFileList.SelectedItems.Count > 0)
            {
                string itemTag = lstviewFileList.SelectedItems[0].Tag?.ToString();

                if (itemTag == "파일추가")
                {
                    frmAddFile addFileForm = new frmAddFile(this);
                    addFileForm.ShowDialog();
                }
            }
        }
    }
}