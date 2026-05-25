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
        private FileSystemWatcher trashWatcher;

        // [추가 변수] 감마 및 색 필터 관리를 위한 백업 경로 및 상태 변수
        private string gammaBackupPath = null;
        private string colorFilterBackupPath = null;
        private Button activePaletteButton = null;
        private List<Button> paletteButtons = new List<Button>();

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

            InitializeSpeedController();
            InitializeImageEditor();
        }

        private void InitializeSpeedController()
        {
            pnlSpeedPopup.Visible = false;

            sdrSpeedController.RangeMin = 1;
            sdrSpeedController.RangeMax = 30;
            sdrSpeedController.Value = 10;

            btnSpeedPopup.Click += btnSpeedPopup_Click;
            btnSpeedPlus.Click += btnSpeedPlus_Click;
            btnSpeedMinus.Click += btnSpeedMinus_Click;
            sdrSpeedController.onValueChanged += sdrSpeedController_onValueChanged;

            sdrSpeedController.BackColor = Color.FromArgb(255, 45, 45, 45);
            pnlSpeedPopup.BackColor = Color.FromArgb(255, 35, 35, 35);

            lblSpeedText.Parent = pnlSpeedPopup;
            lblSpeedText.BackColor = Color.Transparent;
            lblSpeedText.ForeColor = Color.White;
            lblSpeedText.BringToFront();

            this.Deactivate += (s, e) => pnlSpeedPopup.Visible = false;

            Application.AddMessageFilter(new ClickOutsideFilter(pnlSpeedPopup, btnSpeedPopup));

            UpdateSpeedDisplay(sdrSpeedController.Value);
        }

        private void btnSpeedPopup_Click(object sender, EventArgs e)
        {
            pnlSpeedPopup.Visible = !pnlSpeedPopup.Visible;
            if (pnlSpeedPopup.Visible)
            {
                pnlSpeedPopup.BringToFront();
                UpdateSpeedDisplay(sdrSpeedController.Value);
            }
        }

        private void btnSpeedPlus_Click(object sender, EventArgs e)
        {
            if (sdrSpeedController.Value < sdrSpeedController.RangeMax)
            {
                sdrSpeedController.Value += 1;
                UpdateSpeedDisplay(sdrSpeedController.Value);
            }
        }

        private void btnSpeedMinus_Click(object sender, EventArgs e)
        {
            if (sdrSpeedController.Value > sdrSpeedController.RangeMin)
            {
                sdrSpeedController.Value -= 1;
                UpdateSpeedDisplay(sdrSpeedController.Value);
            }
        }

        private void sdrSpeedController_onValueChanged(object sender, int newValue)
        {
            UpdateSpeedDisplay(newValue);
        }

        private void UpdateSpeedDisplay(int sliderValue)
        {
            double speed = sliderValue / 10.0;
            lblSpeedText.Text = $"{speed:0.0}x";

            if (speed > 0)
            {
                videoTimer.Interval = Math.Max(1, (int)(33 / speed));
            }
        }

        private void InitializeImageEditor()
        {
            lstviewFileListD.HideSelection = false;

            // [수정] 각 버튼 클릭 시 해당 패널을 열고 crdProperty을 숨김
            btnColorProperty.Click += (s, e) => { ShowPropertyPanel(pnlColorProperty); crdProperty.Visible = false; };
            btnContrastProperty.Click += (s, e) => { ShowPropertyPanel(pnlContrastProperty); crdProperty.Visible = false; };
            btnROI.Click += (s, e) => { ShowPropertyPanel(pnlROI); crdProperty.Visible = false; };

            btnNoise.Click += btnNoise_Click;
            btnMirror.Click += btnMirror_Click;

            btnROILU.Click += (s, e) => ApplyROIBlackout(0, 0);
            btnROIU.Click += (s, e) => ApplyROIBlackout(0, 1);
            btnROIRU.Click += (s, e) => ApplyROIBlackout(0, 2);
            btnROIL.Click += (s, e) => ApplyROIBlackout(1, 0);
            btnROICenter.Click += (s, e) => ApplyROIBlackout(1, 1);
            btnROIR.Click += (s, e) => ApplyROIBlackout(1, 2);
            btnROILD.Click += (s, e) => ApplyROIBlackout(2, 0);
            btnROID.Click += (s, e) => ApplyROIBlackout(2, 1);
            btnROIRD.Click += (s, e) => ApplyROIBlackout(2, 2);

            trcbrContrastProperty.Minimum = -10;
            trcbrContrastProperty.Maximum = 10;
            trcbrContrastProperty.Value = 0;
            trcbrContrastProperty.Scroll += trcbrContrastProperty_Scroll;

            // [추가] 색 필터 관련 컨트롤 이벤트 및 리스트 초기화
            paletteButtons = new List<Button> { btnPalette1, btnPalette2, btnPalette3, btnPalette4, btnPalette5};
            foreach (var btn in paletteButtons)
            {
                btn.Click += PaletteButton_Click;
            }

            trcbrRed.Scroll += ColorTrackBar_Scroll;
            trcbrGreen.Scroll += ColorTrackBar_Scroll;
            trcbrBlue.Scroll += ColorTrackBar_Scroll;

            btnColorCfm.Click += btnColorCfm_Click;
            btnColorCancle.Click += btnColorCancle_Click;

            // [추가] 외부 영역 클릭 감지를 위한 메시지 필터 등록 (추가 기능 2번)
            Application.AddMessageFilter(new PropertyPanelFilter(this));
        }

        // [수정] 패널 제어 전용 메서드 변경
        private void ShowPropertyPanel(Control activeControl)
        {
            pnlContrastProperty.Visible = (activeControl == pnlContrastProperty);
            pnlROI.Visible = (activeControl == pnlROI);
            pnlColorProperty.Visible = (activeControl == pnlColorProperty);
        }

        private string GetTargetImagePath()
        {
            if (lstviewFileListD.SelectedItems.Count > 0)
            {
                string selectedText = lstviewFileListD.SelectedItems[0].Text;
                if (selectedText.StartsWith("[폴더]")) return null;

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string uploadFolder = Path.GetFullPath(Path.Combine(baseDir, @"..\..\UploadedFile"));
                return Path.Combine(uploadFolder, selectedText);
            }

            if (slideImages.Count > 0 && currentSlideIndex >= 0 && currentSlideIndex < slideImages.Count)
            {
                return slideImages[currentSlideIndex];
            }
            return null;
        }

        private void ModifyTargetImage(Action<Bitmap> modifyAction)
        {
            string targetPath = GetTargetImagePath();
            if (string.IsNullOrEmpty(targetPath) || !File.Exists(targetPath)) return;

            if (picVideoBox.Image != null)
            {
                picVideoBox.Image.Dispose();
                picVideoBox.Image = null;
            }

            Bitmap targetBitmap = null;
            try
            {
                using (FileStream fs = new FileStream(targetPath, FileMode.Open, FileAccess.Read))
                {
                    using (Image originalImg = Image.FromStream(fs))
                    {
                        targetBitmap = new Bitmap(originalImg);
                    }
                }

                modifyAction(targetBitmap);

                targetBitmap.Save(targetPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"이미지 편집 중 오류가 발생했습니다: {ex.Message}");
            }
            finally
            {
                if (targetBitmap != null) targetBitmap.Dispose();
            }

            UpdateSlideDisplay();
        }

        private void btnNoise_Click(object sender, EventArgs e)
        {
            ModifyTargetImage(bmp =>
            {
                int degradedWidth = Math.Max(1, bmp.Width / 4);
                int degradedHeight = Math.Max(1, bmp.Height / 4);

                using (Bitmap lowResBmp = new Bitmap(bmp, degradedWidth, degradedHeight))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                        g.DrawImage(lowResBmp, 0, 0, bmp.Width, bmp.Height);
                    }
                }
            });
        }

        private void btnMirror_Click(object sender, EventArgs e)
        {
            ModifyTargetImage(bmp =>
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            });
        }

        private void ApplyROIBlackout(int row, int col)
        {
            ModifyTargetImage(bmp =>
            {
                int w = bmp.Width / 3;
                int h = bmp.Height / 3;
                int x = col * w;
                int y = row * h;

                int rectWidth = (col == 2) ? bmp.Width - x : w;
                int rectHeight = (row == 2) ? bmp.Height - y : h;

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.FillRectangle(Brushes.Black, new Rectangle(x, y, rectWidth, rectHeight));
                }
            });
        }

        // [수정] 감마 조절 트랙바 스크롤 로직 변경 (중앙값 복구 기능 반영)
        private void trcbrContrastProperty_Scroll(object sender, EventArgs e)
        {
            string targetPath = GetTargetImagePath();
            if (string.IsNullOrEmpty(targetPath) || !File.Exists(targetPath)) return;

            // 백업 파일 생성 처리
            if (string.IsNullOrEmpty(gammaBackupPath) || !File.Exists(gammaBackupPath))
            {
                gammaBackupPath = targetPath + ".gback";
                if (File.Exists(gammaBackupPath)) File.Delete(gammaBackupPath);
                File.Copy(targetPath, gammaBackupPath);
            }

            int trackValue = trcbrContrastProperty.Value;

            // 트랙바를 중앙(0)으로 옮기면 원래 백업 상태 파일로 복원 후 종료
            if (trackValue == 0)
            {
                if (picVideoBox.Image != null)
                {
                    picVideoBox.Image.Dispose();
                    picVideoBox.Image = null;
                }
                File.Delete(targetPath);
                File.Copy(gammaBackupPath, targetPath);
                UpdateSlideDisplay();
                return;
            }

            double gammaCalculationValue = 1.0;
            if (trackValue > 0)
            {
                gammaCalculationValue = 1.0 - (trackValue * 0.08);
            }
            else if (trackValue < 0)
            {
                gammaCalculationValue = 1.0 + (-trackValue * 0.2);
            }

            // 실시간 감마 연산 시 항상 백업본(원본)을 기준으로 계산하여 원본 타겟 파일에 덮어씀
            if (picVideoBox.Image != null)
            {
                picVideoBox.Image.Dispose();
                picVideoBox.Image = null;
            }

            Bitmap targetBitmap = null;
            try
            {
                using (FileStream fs = new FileStream(gammaBackupPath, FileMode.Open, FileAccess.Read))
                {
                    using (Image originalImg = Image.FromStream(fs))
                    {
                        targetBitmap = new Bitmap(originalImg);
                    }
                }

                using (Bitmap tempCopy = (Bitmap)targetBitmap.Clone())
                {
                    using (Graphics g = Graphics.FromImage(targetBitmap))
                    {
                        using (System.Drawing.Imaging.ImageAttributes attributes = new System.Drawing.Imaging.ImageAttributes())
                        {
                            attributes.SetGamma((float)gammaCalculationValue, System.Drawing.Imaging.ColorAdjustType.Bitmap);
                            g.DrawImage(tempCopy, new Rectangle(0, 0, targetBitmap.Width, targetBitmap.Height),
                                0, 0, tempCopy.Width, tempCopy.Height, GraphicsUnit.Pixel, attributes);
                        }
                    }
                }
                targetBitmap.Save(targetPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch { }
            finally
            {
                if (targetBitmap != null) targetBitmap.Dispose();
            }

            UpdateSlideDisplay();
        }

        // [추가] 3번 기능: 팔레트 버튼 클릭 이벤트 처리
        private void PaletteButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton == null) return;

            activePaletteButton = clickedButton;

            // 클릭 버튼 활성화, 나머지 5개 버튼 비활성화
            foreach (var btn in paletteButtons)
            {
                btn.Enabled = (btn == clickedButton);
            }

            // 트랙바 값을 왼쪽 끝값(=0)으로 초기화
            trcbrRed.Value = 0;
            trcbrGreen.Value = 0;
            trcbrBlue.Value = 0;

            // pnlColorPalette 노출
            pnlColorPalette.Visible = true;

            // 색 필터 적용 전 원본 파일 임시 백업 복사본 생성
            string targetPath = GetTargetImagePath();
            if (!string.IsNullOrEmpty(targetPath) && File.Exists(targetPath))
            {
                colorFilterBackupPath = targetPath + ".cback";
                if (File.Exists(colorFilterBackupPath)) File.Delete(colorFilterBackupPath);
                File.Copy(targetPath, colorFilterBackupPath);
            }
        }

        // [추가] 3번 기능: RGB 트랙바 실시간 스크롤 연동 기능
        private void ColorTrackBar_Scroll(object sender, EventArgs e)
        {
            if (activePaletteButton == null || string.IsNullOrEmpty(colorFilterBackupPath) || !File.Exists(colorFilterBackupPath)) return;

            int r = trcbrRed.Value;
            int g = trcbrGreen.Value;
            int b = trcbrBlue.Value;

            // 활성화된 버튼의 BackColor 변경
            activePaletteButton.BackColor = Color.FromArgb(r, g, b);

            string targetPath = GetTargetImagePath();
            if (string.IsNullOrEmpty(targetPath)) return;

            if (picVideoBox.Image != null)
            {
                picVideoBox.Image.Dispose();
                picVideoBox.Image = null;
            }

            Bitmap filterBitmap = null;
            try
            {
                // 항상 백업 파일(원본)로부터 이미지를 오픈
                using (FileStream fs = new FileStream(colorFilterBackupPath, FileMode.Open, FileAccess.Read))
                {
                    using (Image originalImg = Image.FromStream(fs))
                    {
                        filterBitmap = new Bitmap(originalImg);
                    }
                }

                // 이미지에 색상 필터(Tint) 오버레이 처리
                using (Graphics gr = Graphics.FromImage(filterBitmap))
                {
                    using (System.Drawing.Imaging.ImageAttributes attributes = new System.Drawing.Imaging.ImageAttributes())
                    {
                        // 색상 오프셋 가산 행렬 (RGB 강도를 적절한 비율로 이미지 크기에 맞춤 오버레이)
                        float[][] colorMatrixElements = {
                            new float[] {1, 0, 0, 0, 0},
                            new float[] {0, 1, 0, 0, 0},
                            new float[] {0, 0, 1, 0, 0},
                            new float[] {0, 0, 0, 1, 0},
                            new float[] {r / 255f * 0.4f, g / 255f * 0.4f, b / 255f * 0.4f, 0, 1}
                        };
                        System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix(colorMatrixElements);
                        attributes.SetColorMatrix(colorMatrix);

                        gr.DrawImage(filterBitmap, new Rectangle(0, 0, filterBitmap.Width, filterBitmap.Height),
                            0, 0, filterBitmap.Width, filterBitmap.Height, GraphicsUnit.Pixel, attributes);
                    }
                }

                filterBitmap.Save(targetPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch { }
            finally
            {
                if (filterBitmap != null) filterBitmap.Dispose();
            }

            UpdateSlideDisplay();
        }

        // [추가] 3번 기능: 색 필터 확인 버튼 클릭 처리
        private void btnColorCfm_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(colorFilterBackupPath) && File.Exists(colorFilterBackupPath))
            {
                try { File.Delete(colorFilterBackupPath); } catch { }
            }
            colorFilterBackupPath = null;
            ResetPaletteStatus();
        }

        // [추가] 3번 기능: 색 필터 취소 버튼 클릭 처리 (삭제 및 이름 대체 복구)
        private void btnColorCancle_Click(object sender, EventArgs e)
        {
            string targetPath = GetTargetImagePath();

            if (!string.IsNullOrEmpty(colorFilterBackupPath) && File.Exists(colorFilterBackupPath) && !string.IsNullOrEmpty(targetPath))
            {
                if (picVideoBox.Image != null)
                {
                    picVideoBox.Image.Dispose();
                    picVideoBox.Image = null;
                }

                try
                {
                    if (File.Exists(targetPath)) File.Delete(targetPath);
                    File.Move(colorFilterBackupPath, targetPath); // 임시 파일명을 대상 원본 파일명으로 변경 대체
                }
                catch { }
            }
            colorFilterBackupPath = null;
            ResetPaletteStatus();
            UpdateSlideDisplay();
        }

        // [추가] 팔레트 상태 초기화 공용 헬퍼 메서드
        private void ResetPaletteStatus()
        {
            activePaletteButton = null;
            foreach (var btn in paletteButtons)
            {
                btn.Enabled = true;
            }
            pnlColorPalette.Visible = false;
        }

        // [추가 인터널 클래스] 속성 패널 외부 클릭 감지용 메시지 필터 (추가 기능 2번)
        private class PropertyPanelFilter : IMessageFilter
        {
            private frmMain _form;
            private const int WM_LBUTTONDOWN = 0x0201;

            public PropertyPanelFilter(frmMain form)
            {
                _form = form;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_LBUTTONDOWN)
                {
                    if (_form.pnlContrastProperty.Visible || _form.pnlROI.Visible || _form.pnlColorProperty.Visible || _form.pnlColorPalette.Visible)
                    {
                        Point mousePos = Control.MousePosition;

                        if (IsOutside(_form.pnlContrastProperty, mousePos) && IsOutside(_form.btnContrastProperty, mousePos) &&
                            IsOutside(_form.pnlROI, mousePos) && IsOutside(_form.btnROI, mousePos) &&
                            IsOutside(_form.pnlColorProperty, mousePos) && IsOutside(_form.btnColorProperty, mousePos) &&
                            IsOutside(_form.pnlColorPalette, mousePos) &&
                            IsOutside(_form.btnPalette1, mousePos) && IsOutside(_form.btnPalette2, mousePos) &&
                            IsOutside(_form.btnPalette3, mousePos) && IsOutside(_form.btnPalette4, mousePos))
                        {
                            _form.Invoke(new Action(() =>
                            {
                                _form.pnlContrastProperty.Visible = false;
                                _form.pnlROI.Visible = false;
                                _form.pnlColorProperty.Visible = false;
                                _form.pnlColorPalette.Visible = false;
                                _form.crdProperty.Visible = true; // 숨겨진 crdProperty 노출
                            }));
                        }
                    }
                }
                return false;
            }

            private bool IsOutside(Control c, Point p)
            {
                if (c == null || !c.Visible) return true;
                Rectangle r = c.RectangleToScreen(c.ClientRectangle);
                return !r.Contains(p);
            }
        }

        private class ClickOutsideFilter : IMessageFilter
        {
            private Control _panel;
            private Control _button;
            private const int WM_LBUTTONDOWN = 0x0201;

            public ClickOutsideFilter(Control panel, Control button)
            {
                _panel = panel;
                _button = button;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_LBUTTONDOWN && _panel.Visible)
                {
                    Point mousePos = Control.MousePosition;
                    Rectangle panelRect = _panel.RectangleToScreen(_panel.ClientRectangle);
                    Rectangle btnRect = _button.RectangleToScreen(_button.ClientRectangle);

                    if (!panelRect.Contains(mousePos) && !btnRect.Contains(mousePos))
                    {
                        _panel.Invoke(new Action(() => _panel.Visible = false));
                    }
                }
                return false;
            }
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

        private void SetupTrashWatcher()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string trashFolder = Path.GetFullPath(Path.Combine(baseDir, @"..\..\TrashCan"));

            if (!Directory.Exists(trashFolder))
            {
                Directory.CreateDirectory(trashFolder);
            }

            trashWatcher = new FileSystemWatcher(trashFolder);
            trashWatcher.EnableRaisingEvents = true;

            trashWatcher.Created += TrashWatcher_Changed;
            trashWatcher.Deleted += TrashWatcher_Changed;
            trashWatcher.Renamed += TrashWatcher_Changed;
        }

        private void TrashWatcher_Changed(object sender, FileSystemEventArgs e)
        {
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
            string binFolder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\"));
            System.Diagnostics.Process.Start("explorer.exe", binFolder);
        }

        private void btnRestoration_Click(object sender, EventArgs e)
        {
            if (lstviewTrash.SelectedItems.Count == 0) return;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
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

        private void trackBar1_Scroll(object sender, EventArgs e) { }
        private void pnlContrastProperty_Paint(object sender, PaintEventArgs e) { }
        private void pnlCloseProperty_Paint(object sender, PaintEventArgs e) { }
    }
}