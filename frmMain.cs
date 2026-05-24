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
        private System.Windows.Forms.Timer videoTimer;
        private PictureBox picVideoBox;
        private List<string> slideImages = new List<string>();
        private int currentSlideIndex = 0;
        private ListViewItem lastHighlightedItem = null;
        private bool isUpdatingSlider = false;

        // 실시간 감지를 위한 File(라틴어:실) 감시자
        private FileSystemWatcher trashWatcher;

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

            InitializeVideoPlayer();

            btnNxt1F.Click += btnNxt1F_Click;
            btnNxt5F.Click += btnNxt5F_Click;
            btnPre1F.Click += btnPre1F_Click;
            btnPre5F.Click += btnPre5F_Click;
            btnDel.Click += btnDel_Click;
            sdrSeekBar.onValueChanged += SdrSeekBar_onValueChanged;

            btnOpnFileExplrr.Click += btnOpnFileExplrr_Click;
            btnRestoration.Click += btnRestoration_Click;

            btnRestoration.Visible = false;

            SetupTabs();
            LoadUploadedFilesToD();
            LoadTrashCanFiles();
            SetupTrashWatcher();

            this.lstviewMain.MouseDoubleClick += lstviewMain_MouseDoubleClick;
            this.lstviewFileList.MouseDoubleClick += lstviewFileList_MouseDoubleClick;
        }

        private void InitializeVideoPlayer()
        {
            picVideoBox = new PictureBox();
            picVideoBox.Dock = DockStyle.Fill;
            picVideoBox.SizeMode = PictureBoxSizeMode.StretchImage;

            if (this.Controls.Find("pnlVideo", true).FirstOrDefault() is Panel pnl)
            {
                pnl.Controls.Add(picVideoBox);
            }

            videoTimer = new System.Windows.Forms.Timer();
            videoTimer.Interval = 33;
            videoTimer.Tick += VideoTimer_Tick;
        }

        // 휴지통 실시간 감지 설정
        private void SetupTrashWatcher()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string trashFolder = Path.GetFullPath(Path.Combine(baseDir, @"..\..\TrashCan"));

            if (!Directory.Exists(trashFolder))
            {
                // Directory(라틴어:바르게하다)
                Directory.CreateDirectory(trashFolder);
            }

            trashWatcher = new FileSystemWatcher(trashFolder);
            trashWatcher.EnableRaisingEvents = true;

            // Event(라틴어:나오다) 연결
            trashWatcher.Created += TrashWatcher_Changed;
            trashWatcher.Deleted += TrashWatcher_Changed;
            trashWatcher.Renamed += TrashWatcher_Changed;
        }

        private void TrashWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            // UI 스레드 접근을 위한 Invoke(라틴어:부르다) 처리
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(LoadTrashCanFiles));
            }
            else
            {
                LoadTrashCanFiles();
            }
        }

        public void LoadUploadedFilesToD()
        {
            lstviewFileListD.Items.Clear();
            slideImages.Clear();
            currentSlideIndex = 0;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // 2단계 위 폴더 기준
            string targetFolder = Path.GetFullPath(Path.Combine(baseDir, @"..\..\UploadedFile"));

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

                string ext = file.Extension.ToLower();
                if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".bit")
                {
                    slideImages.Add(file.FullName);
                }
            }

            if (slideImages.Count > 0)
            {
                sdrSeekBar.RangeMin = 0;
                sdrSeekBar.RangeMax = slideImages.Count - 1;
                UpdateSlideDisplay();
            }
        }

        private void LoadTrashCanFiles()
        {
            lstviewTrash.Items.Clear();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // 2단계 위 폴더(bin) 지정
            string trashFolder = Path.GetFullPath(Path.Combine(baseDir, @"..\..\TrashCan"));

            if (!Directory.Exists(trashFolder)) return;

            DirectoryInfo di = new DirectoryInfo(trashFolder);

            var folders = di.GetDirectories().OrderBy(d => d.CreationTime).ToList();
            foreach (var folder in folders)
            {
                ListViewItem item = new ListViewItem("[폴더] " + folder.Name);
                item.Tag = "휴지통파일";
                lstviewTrash.Items.Add(item);
            }

            var files = di.GetFiles().OrderBy(f => f.CreationTime).ToList();
            foreach (var file in files)
            {
                ListViewItem item = new ListViewItem(file.Name);
                item.Tag = "휴지통파일";
                lstviewTrash.Items.Add(item);
            }
        }

        private void UpdateSlideDisplay()
        {
            if (slideImages.Count == 0) return;

            string currentImagePath = slideImages[currentSlideIndex];

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

            isUpdatingSlider = true;
            sdrSeekBar.Value = currentSlideIndex;
            sdrSeekBar.Text = $"{currentSlideIndex + 1}/{slideImages.Count}";
            isUpdatingSlider = false;

            if (lastHighlightedItem != null)
            {
                lastHighlightedItem.ForeColor = Color.Black;
            }

            string currentFileName = Path.GetFileName(currentImagePath);
            foreach (ListViewItem item in lstviewFileListD.Items)
            {
                if (item.Text == currentFileName)
                {
                    item.ForeColor = Color.Green;
                    lastHighlightedItem = item;
                    item.EnsureVisible();
                    break;
                }
            }
        }

        private void btnPlayStop_Click(object sender, EventArgs e)
        {
            if (slideImages.Count == 0) return;

            if (videoTimer.Enabled)
            {
                videoTimer.Stop();
            }
            else
            {
                if (currentSlideIndex >= slideImages.Count - 1)
                {
                    currentSlideIndex = 0;
                }
                videoTimer.Start();
            }
        }

        private void VideoTimer_Tick(object sender, EventArgs e)
        {
            if (currentSlideIndex >= slideImages.Count - 1)
            {
                videoTimer.Stop();
                return;
            }
            currentSlideIndex++;
            UpdateSlideDisplay();
        }

        private void SdrSeekBar_onValueChanged(object sender, int newValue)
        {
            if (isUpdatingSlider || slideImages.Count == 0) return;
            currentSlideIndex = newValue;
            UpdateSlideDisplay();
        }

        private void MoveSlide(int frames)
        {
            if (slideImages.Count == 0) return;

            currentSlideIndex += frames;

            if (currentSlideIndex < 0) currentSlideIndex = 0;
            if (currentSlideIndex >= slideImages.Count) currentSlideIndex = slideImages.Count - 1;

            UpdateSlideDisplay();
        }

        private void btnNxt1F_Click(object sender, EventArgs e) { MoveSlide(1); }
        private void btnNxt5F_Click(object sender, EventArgs e) { MoveSlide(5); }
        private void btnPre1F_Click(object sender, EventArgs e) { MoveSlide(-1); }
        private void btnPre5F_Click(object sender, EventArgs e) { MoveSlide(-5); }

        private void btnDel_Click(object sender, EventArgs e)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // 2단계 위 폴더(bin) 지정
            string trashFolder = Path.GetFullPath(Path.Combine(baseDir, @"..\..\TrashCan"));

            if (!Directory.Exists(trashFolder))
            {
                Directory.CreateDirectory(trashFolder);
            }

            List<string> targets = new List<string>();

            if (lstviewFileListD.SelectedItems.Count > 0)
            {
                string uploadFolder = Path.GetFullPath(Path.Combine(baseDir, @"..\..\UploadedFile"));
                foreach (ListViewItem item in lstviewFileListD.SelectedItems)
                {
                    string name = item.Text.Replace("[폴더] ", "");
                    targets.Add(Path.Combine(uploadFolder, name));
                }
            }
            else
            {
                if (slideImages.Count > 0 && currentSlideIndex >= 0 && currentSlideIndex < slideImages.Count)
                {
                    targets.Add(slideImages[currentSlideIndex]);
                }
            }

            if (targets.Count == 0) return;

            if (videoTimer.Enabled) videoTimer.Stop();

            foreach (string target in targets)
            {
                if (picVideoBox.Image != null && slideImages.Count > 0 && target == slideImages[currentSlideIndex])
                {
                    Image old = picVideoBox.Image;
                    picVideoBox.Image = null;
                    old.Dispose();
                }

                try
                {
                    string dest = Path.Combine(trashFolder, Path.GetFileName(target));
                    if (Directory.Exists(target))
                    {
                        Directory.Move(target, dest);
                    }
                    else if (File.Exists(target))
                    {
                        File.Move(target, dest);
                    }
                }
                catch { }
            }

            LoadUploadedFilesToD();
        }

        private void btnOpnFileExplrr_Click(object sender, EventArgs e)
        {
            // 경로 2단계 위(bin) 지정
            string binFolder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\"));
            // Process(라틴어:나아가다)
            System.Diagnostics.Process.Start("explorer.exe", binFolder);
        }

        private void btnRestoration_Click(object sender, EventArgs e)
        {
            if (lstviewTrash.SelectedItems.Count == 0) return;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // 2단계 위 폴더(bin) 지정
            string trashFolder = Path.GetFullPath(Path.Combine(baseDir, @"..\..\TrashCan"));
            string uploadFolder = Path.GetFullPath(Path.Combine(baseDir, @"..\..\UploadedFile"));

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            foreach (ListViewItem item in lstviewTrash.SelectedItems)
            {
                string fileName = item.Text.Replace("[폴더] ", "");
                string sourcePath = Path.Combine(trashFolder, fileName);
                string destPath = Path.Combine(uploadFolder, fileName);

                try
                {
                    if (Directory.Exists(sourcePath))
                    {
                        Directory.Move(sourcePath, destPath);
                    }
                    else if (File.Exists(sourcePath))
                    {
                        File.Move(sourcePath, destPath);
                    }
                }
                catch { }
            }

            LoadUploadedFilesToD();
        }

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

            lblLstVwName.Text = "";
            btnRestoration.Visible = false;
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

                    lblLstVwName.Text = "[파일 목록]";
                    btnRestoration.Visible = false;
                }
                else if (itemTag == "휴지통")
                {
                    lstviewMain.Visible = false;
                    lstviewFileList.Visible = false;
                    lstviewFileListD.Visible = false;
                    lstviewTrash.Visible = true;

                    lblLstVwName.Text = "[휴지통]";
                    btnRestoration.Visible = true;

                    // 폴더 진입 시 확실한 동기화를 위해 한 번 더 갱신
                    LoadTrashCanFiles();
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