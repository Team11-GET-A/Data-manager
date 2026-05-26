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
        private DoubleBufferedPictureBox picVideoBox;
        private List<string> slideImages = new List<string>();
        private int currentSlideIndex = 0;
        private ListViewItem lastHighlightedItem = null;
        private bool isUpdatingSlider = false;
        private FileSystemWatcher trashWatcher;

        private string gammaBackupPath = null;
        private string colorFilterBackupPath = null;
        private Button activePaletteButton = null;
        private List<Button> paletteButtons = new List<Button>();

        // [추가] ROI 상태값 및 복구를 위한 백업 필드 복합 레이어 설정
        private bool[,] roiState = new bool[3, 3];
        private string roiBackupPath = null;
        private string lastRoiTargetPath = null;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED (화면 깜빡임 완화)
                return cp;
            }
        }

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

            btnColorProperty.Click += (s, e) => { ShowPropertyPanel(pnlColorProperty); crdProperty.Visible = false; };
            btnContrastProperty.Click += (s, e) => { ShowPropertyPanel(pnlContrastProperty); crdProperty.Visible = false; };
            btnROI.Click += (s, e) => { ShowPropertyPanel(pnlROI); crdProperty.Visible = false; };

            btnNoise.Click += btnNoise_Click;
            btnMirror.Click += btnMirror_Click;

            // ROI 3x3 개별 버튼 이벤트 연결 (토글 감지 구조 적용)
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

            // [수정] 5개 팔레트 버튼별 프리셋 필터 실행 핸들러 직접 연결
            btnPalette1.Click += (s, e) => HandlePaletteClick(1, btnPalette1); // 흑백 필터
            btnPalette2.Click += (s, e) => HandlePaletteClick(2, btnPalette2); // 색 반전 필터
            btnPalette3.Click += (s, e) => HandlePaletteClick(3, btnPalette3); // 파란 빛 필터 (차가움)
            btnPalette4.Click += (s, e) => HandlePaletteClick(4, btnPalette4); // 노란 빛 필터 (따뜻함)
            btnPalette5.Click += (s, e) => HandlePaletteClick(5, btnPalette5); // 세피아 필터 (AI 추천 프리셋 구상)


            btnColorCfm.Click += btnColorCfm_Click;
            btnColorCancle.Click += btnColorCancle_Click;

            Application.AddMessageFilter(new PropertyPanelFilter(this));
        }

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

        // [수정] ROI 영역 동일 위치 한 번 더 누를 시 원상복구(토글) 처리 로직 구현
        private void ApplyROIBlackout(int row, int col)
        {
            string targetPath = GetTargetImagePath();
            if (string.IsNullOrEmpty(targetPath) || !File.Exists(targetPath)) return;

            // 현재 가공 대상 파일이 변경되었다면 이전 캐시 및 기록 초기화 처리
            if (lastRoiTargetPath != targetPath)
            {
                if (!string.IsNullOrEmpty(roiBackupPath) && File.Exists(roiBackupPath))
                {
                    try { File.Delete(roiBackupPath); } catch { }
                }
                roiBackupPath = null;
                Array.Clear(roiState, 0, roiState.Length);
                lastRoiTargetPath = targetPath;
            }

            // 최초 진입 시 무수정 상태의 원본 이미지 백업 생성 (.roiback)
            if (string.IsNullOrEmpty(roiBackupPath) || !File.Exists(roiBackupPath))
            {
                roiBackupPath = targetPath + ".roiback";
                if (File.Exists(roiBackupPath)) File.Delete(roiBackupPath);
                File.Copy(targetPath, roiBackupPath);
            }

            // 해당 좌표의 블랙아웃 유무 토글 변경
            roiState[row, col] = !roiState[row, col];

            if (picVideoBox.Image != null)
            {
                picVideoBox.Image.Dispose();
                picVideoBox.Image = null;
            }

            Bitmap compositeBmp = null;
            try
            {
                // 항상 무수정 원본 백업본 스트림으로부터 복합 캔버스를 로드하여 연산
                using (FileStream fs = new FileStream(roiBackupPath, FileMode.Open, FileAccess.Read))
                {
                    using (Image originalImg = Image.FromStream(fs))
                    {
                        compositeBmp = new Bitmap(originalImg);
                    }
                }

                using (Graphics g = Graphics.FromImage(compositeBmp))
                {
                    int w = compositeBmp.Width / 3;
                    int h = compositeBmp.Height / 3;

                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            if (roiState[r, c])
                            {
                                int x = c * w;
                                int y = r * h;
                                int rectWidth = (c == 2) ? compositeBmp.Width - x : w;
                                int rectHeight = (r == 2) ? compositeBmp.Height - y : h;

                                g.FillRectangle(Brushes.Black, new Rectangle(x, y, rectWidth, rectHeight));
                            }
                        }
                    }
                }

                compositeBmp.Save(targetPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch { }
            finally
            {
                if (compositeBmp != null) compositeBmp.Dispose();
            }

            UpdateSlideDisplay();
        }

        private void trcbrContrastProperty_Scroll(object sender, EventArgs e)
        {
            string targetPath = GetTargetImagePath();
            if (string.IsNullOrEmpty(targetPath) || !File.Exists(targetPath)) return;

            if (string.IsNullOrEmpty(gammaBackupPath) || !File.Exists(gammaBackupPath))
            {
                gammaBackupPath = targetPath + ".gback";
                if (File.Exists(gammaBackupPath)) File.Delete(gammaBackupPath);
                File.Copy(targetPath, gammaBackupPath);
            }

            int trackValue = trcbrContrastProperty.Value;

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

        // [추가] 프리셋 필터 버튼 클릭 시 임시 가공 처리 공용 코어 메서드
        private void HandlePaletteClick(int filterType, Button targetButton)
        {
            // 1. 자동으로 btnColorCancle을 연동 실행하여 기교 폴더 정리
            btnColorCancle_Click(this, EventArgs.Empty);

            activePaletteButton = targetButton;


            string targetPath = GetTargetImagePath();
            if (string.IsNullOrEmpty(targetPath) || !File.Exists(targetPath)) return;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string tempFolder = Path.GetFullPath(Path.Combine(baseDir, @"..\..\ColorTempFile"));
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            // 2. 포커스 혹은 현재 슬라이드 이미지 명칭 뒤에 -Temp 접미사 바인딩 복사
            string fileNameOnly = Path.GetFileNameWithoutExtension(targetPath);
            string ext = Path.GetExtension(targetPath);
            string tempFilePath = Path.Combine(tempFolder, $"{fileNameOnly}-Temp{ext}");

            try
            {
                File.Copy(targetPath, tempFilePath, true);

                // 3. 프리셋 색조 필터 연산 적용
                ApplyPresetColorFilter(tempFilePath, filterType);

                // 4. 메인 슬라이드 리스트를 ColorTempFile 내부 임시 가공 파일 슬라이드로 전환 대체
                slideImages.Clear();
                slideImages.Add(tempFilePath);
                currentSlideIndex = 0;

                sdrSeekBar.RangeMin = 0;
                sdrSeekBar.RangeMax = 0;

                UpdateSlideDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"임시 필터 구성 중 오류가 발생했습니다: {ex.Message}");
            }
        }

        // [추가] ColorMatrix 고속 이미지 필터 하드 연산 유닛
        private void ApplyPresetColorFilter(string filePath, int filterType)
        {
            float[][] matrixElements;
            switch (filterType)
            {
                case 1: // 흑백 필터
                    matrixElements = new float[][] {
                        new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                        new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                        new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 1}
                    };
                    break;
                case 2: // 색 반전 필터
                    matrixElements = new float[][] {
                        new float[] {-1, 0, 0, 0, 0},
                        new float[] {0, -1, 0, 0, 0},
                        new float[] {0, 0, -1, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {1, 1, 1, 0, 1}
                    };
                    break;
                case 3: // 파란 빛 필터 (차가운 색조 증폭)
                    matrixElements = new float[][] {
                        new float[] {0.8f, 0, 0, 0, 0},
                        new float[] {0, 0.8f, 0, 0, 0},
                        new float[] {0, 0, 1.3f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0.15f, 0, 1}
                    };
                    break;
                case 4: // 노란 빛 필터 (따뜻한 색조 증폭)
                    matrixElements = new float[][] {
                        new float[] {1.2f, 0, 0, 0, 0},
                        new float[] {0, 1.2f, 0, 0, 0},
                        new float[] {0, 0, 0.7f, 0, 0},
                        new float[] {0.15f, 0.15f, 0, 0, 1}
                    };
                    break;
                case 5: // 세피아 필터 (고전 예술풍 분위기 구상)
                    matrixElements = new float[][] {
                        new float[] {0.393f, 0.349f, 0.272f, 0, 0},
                        new float[] {0.769f, 0.686f, 0.534f, 0, 0},
                        new float[] {0.189f, 0.168f, 0.131f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 1}
                    };
                    break;
                default:
                    return;
            }

            Bitmap bmp = null;
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (Image originalImg = Image.FromStream(fs))
                    {
                        bmp = new Bitmap(originalImg);
                    }
                }

                using (Bitmap tempCopy = (Bitmap)bmp.Clone())
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        using (System.Drawing.Imaging.ImageAttributes attributes = new System.Drawing.Imaging.ImageAttributes())
                        {
                            System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix(matrixElements);
                            attributes.SetColorMatrix(colorMatrix);
                            g.DrawImage(tempCopy, new Rectangle(0, 0, bmp.Width, bmp.Height),
                                0, 0, tempCopy.Width, tempCopy.Height, GraphicsUnit.Pixel, attributes);
                        }
                    }
                }
                bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch { }
            finally
            {
                if (bmp != null) bmp.Dispose();
            }
        }

        private void ColorTrackBar_Scroll(object sender, EventArgs e) { }

        // [수정] btnColorCfm 클릭 시 -Temp 제거 및 UploadedFile 동일 경로 파일 완전 덮어쓰기 복구 구조
        private void btnColorCfm_Click(object sender, EventArgs e)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string tempFolder = Path.GetFullPath(Path.Combine(baseDir, @"..\..\ColorTempFile"));
            string uploadFolder = Path.GetFullPath(Path.Combine(baseDir, @"..\..\UploadedFile"));

            if (Directory.Exists(tempFolder))
            {
                try
                {
                    string[] tempFiles = Directory.GetFiles(tempFolder);
                    foreach (string tempFile in tempFiles)
                    {
                        string fileName = Path.GetFileName(tempFile);
                        if (fileName.Contains("-Temp"))
                        {
                            string originalName = fileName.Replace("-Temp", "");
                            string destPath = Path.Combine(uploadFolder, originalName);

                            if (picVideoBox.Image != null)
                            {
                                picVideoBox.Image.Dispose();
                                picVideoBox.Image = null;
                            }

                            if (File.Exists(destPath)) File.Delete(destPath);
                            File.Move(tempFile, destPath);
                        }
                    }
                }
                catch { }
            }

            colorFilterBackupPath = null;
            ResetPaletteStatus();

            // 메인 정규 업로드 파일 슬라이드로 전면 교체 로드 시동
            LoadUploadedFilesToD();
        }

        // [수정] btnColorCancle 클릭 시 ColorTempFile 내부 파일 모두 제거 및 메인 원본 복원
        private void btnColorCancle_Click(object sender, EventArgs e)
        {
            if (picVideoBox.Image != null)
            {
                picVideoBox.Image.Dispose();
                picVideoBox.Image = null;
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string tempFolder = Path.GetFullPath(Path.Combine(baseDir, @"..\..\ColorTempFile"));

            if (Directory.Exists(tempFolder))
            {
                try
                {
                    string[] files = Directory.GetFiles(tempFolder);
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                }
                catch { }
            }

            colorFilterBackupPath = null;
            ResetPaletteStatus();

            // 원본 메인 데이터 슬라이드 복구 로드 실행
            LoadUploadedFilesToD();
        }

        private void ResetPaletteStatus()
        {
            activePaletteButton = null;
            foreach (var btn in paletteButtons)
            {
                btn.Enabled = true;
            }

        }

        private void InitializeVideoPlayer()
        {
            picVideoBox = new DoubleBufferedPictureBox();
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

            Data_Manager.trainer form3 = new Data_Manager.trainer();
            form3.TopLevel = false;
            form3.FormBorderStyle = FormBorderStyle.None;
            form3.Dock = DockStyle.Fill;
            tabTrainer.Controls.Add(form3);
            form3.Show();

            Data_Manager.Pliot form2 = new Data_Manager.Pliot();
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
                    if (_form.pnlContrastProperty.Visible || _form.pnlROI.Visible || _form.pnlColorProperty.Visible) { return false; }
                    {
                        Point mousePos = Control.MousePosition;

                        if (IsOutside(_form.pnlContrastProperty, mousePos) && IsOutside(_form.btnContrastProperty, mousePos) &&
                            IsOutside(_form.pnlROI, mousePos) && IsOutside(_form.btnROI, mousePos) &&
                            IsOutside(_form.pnlColorProperty, mousePos) && IsOutside(_form.btnColorProperty, mousePos) &&
                            IsOutside(_form.btnPalette1, mousePos) && IsOutside(_form.btnPalette2, mousePos) &&
                            IsOutside(_form.btnPalette3, mousePos) && IsOutside(_form.btnPalette4, mousePos) &&
                            IsOutside(_form.btnPalette5, mousePos))
                        {
                            _form.Invoke(new Action(() =>
                            {
                                _form.pnlContrastProperty.Visible = false;
                                _form.pnlROI.Visible = false;
                                _form.pnlColorProperty.Visible = false;
                                _form.crdProperty.Visible = true;
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

        private void GBPalete_Enter(object sender, EventArgs e)
        {

        }
    }

    public class ClickOutsideFilter : IMessageFilter
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

    public class DoubleBufferedPictureBox : PictureBox
    {
        public DoubleBufferedPictureBox()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
    }
}