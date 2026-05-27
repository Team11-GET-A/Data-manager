using Data_Manager;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AD_AI_LearningData_Editor; /////////////////////////////////////////

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
        private Button activePaletteButton = null;
        private List<Button> paletteButtons = new List<Button>();
        private bool[,] roiState = new bool[3, 3];
        private Dictionary<string, string> gammaBackupPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff", ".bit" };
        private string mirrorYBackupFolderName = "MirrorYBackupFile";

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        public frmMain()
        {
            InitializeComponent();
            IconProperty.SetAutoImageByWidthHeight(
                btnOpnFolderList,
                Data_Manager.Properties.Resources.UTurnArrow12262463,
                10,
                10
            );
            IconProperty.SetAutoImageByWidthHeight(
                btnOpnFileExplrr,
                Data_Manager.Properties.Resources.SearchFolder214608660,
                6,
                6
            );
            IconProperty.SetAutoImageByWidthHeight(
                btnRemove,
                Data_Manager.Properties.Resources.TrashCan11538270,
                10,
                10
            );
            IconProperty.SetAutoImageByWidthHeight(
                btnRestoration,
                Data_Manager.Properties.Resources.recycle6992289,
                10,
                10
            );






            this.AutoScaleMode = AutoScaleMode.None;

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
            btnSave.Click += btnSave_Click;

            btnRestoration.Visible = false;

            ConfigureListViewNameLabel();
            SetupTabs();
            LoadUploadedFilesToD();
            LoadTrashCanFiles();
            SetupTrashWatcher();

            this.lstviewMain.MouseDoubleClick += lstviewMain_MouseDoubleClick;
            this.lstviewFileList.MouseDoubleClick += lstviewFileList_MouseDoubleClick;

            InitializeSpeedController();
            InitializeImageEditor();
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
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }

        private string GetTrashFolder()
        {
            string folder = Path.Combine(GetBinFolder(), "TrashCan");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }

        private string GetColorTempFolder()
        {
            string folder = Path.Combine(GetBinFolder(), "ColorTempFile");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }

        private string GetMirrorYBackupFolder()
        {
            string folder = Path.Combine(GetBinFolder(), mirrorYBackupFolderName);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }

        private void ConfigureListViewNameLabel()
        {
            lblLstVwName.AutoSize = false;
            lblLstVwName.UseCompatibleTextRendering = true;
            lblLstVwName.Font = new Font("맑은 고딕", 14F, FontStyle.Bold, GraphicsUnit.Point);
        }

        private void SetListViewName(string text)
        {
            lblLstVwName.Text = text;
            lblLstVwName.Font = new Font("맑은 고딕", 14F, FontStyle.Bold, GraphicsUnit.Point);
            lblLstVwName.UseCompatibleTextRendering = true;
            lblLstVwName.Refresh();
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

            if (speed > 0 && videoTimer != null)
            {
                videoTimer.Interval = Math.Max(1, (int)(33 / speed));
            }
        }

        private void InitializeImageEditor()
        {
            lstviewFileListD.HideSelection = false;

            pnlContrastProperty.Visible = false;
            pnlColorProperty.Visible = false;
            pnlROI.Visible = false;
            crdProperty.Visible = true;

            paletteButtons = new List<Button> { btnPalette1, btnPalette2, btnPalette3, btnPalette4, btnPalette5 };

            btnColorProperty.Click += (s, e) => ShowPropertyPanel(pnlColorProperty);
            btnContrastProperty.Click += (s, e) => ShowPropertyPanel(pnlContrastProperty);
            btnROI.Click += (s, e) => ShowPropertyPanel(pnlROI);

            btnNoise.Click += btnNoise_Click;
            btnMirror.Click += btnMirror_Click;
            btnMirrorY.Click += btnMirrorY_Click;

            btnROILU.Click += (s, e) => ApplyROIBlackoutToAllImages(0, 0);
            btnROIU.Click += (s, e) => ApplyROIBlackoutToAllImages(0, 1);
            btnROIRU.Click += (s, e) => ApplyROIBlackoutToAllImages(0, 2);
            btnROIL.Click += (s, e) => ApplyROIBlackoutToAllImages(1, 0);
            btnROICenter.Click += (s, e) => ApplyROIBlackoutToAllImages(1, 1);
            btnROIR.Click += (s, e) => ApplyROIBlackoutToAllImages(1, 2);
            btnROILD.Click += (s, e) => ApplyROIBlackoutToAllImages(2, 0);
            btnROID.Click += (s, e) => ApplyROIBlackoutToAllImages(2, 1);
            btnROIRD.Click += (s, e) => ApplyROIBlackoutToAllImages(2, 2);

            trcbrContrastProperty.Minimum = -10;
            trcbrContrastProperty.Maximum = 10;
            trcbrContrastProperty.Value = 0;
            trcbrContrastProperty.Scroll += trcbrContrastProperty_Scroll;

            btnPalette1.Click += (s, e) => HandlePaletteClick(1, btnPalette1);
            btnPalette2.Click += (s, e) => HandlePaletteClick(2, btnPalette2);
            btnPalette3.Click += (s, e) => HandlePaletteClick(3, btnPalette3);
            btnPalette4.Click += (s, e) => HandlePaletteClick(4, btnPalette4);
            btnPalette5.Click += (s, e) => HandlePaletteClick(5, btnPalette5);

            btnColorCfm.Click += btnColorCfm_Click;
            btnColorCancle.Click += btnColorCancle_Click;

            this.Deactivate += (s, e) => HidePropertyPanels();
            Application.AddMessageFilter(new PropertyPanelFilter(this));
        }

        private void ShowPropertyPanel(Control activeControl)
        {
            pnlContrastProperty.Visible = activeControl == pnlContrastProperty;
            pnlROI.Visible = activeControl == pnlROI;
            pnlColorProperty.Visible = activeControl == pnlColorProperty;
            crdProperty.Visible = false;
            activeControl.BringToFront();
        }

        private void HidePropertyPanels()
        {
            pnlContrastProperty.Visible = false;
            pnlROI.Visible = false;
            pnlColorProperty.Visible = false;
            crdProperty.Visible = true;
        }

        private List<string> GetUploadedImageFiles()
        {
            string uploadFolder = GetUploadedFolder();
            if (!Directory.Exists(uploadFolder)) return new List<string>();

            return Directory.GetFiles(uploadFolder)
                .Where(path => imageExtensions.Contains(Path.GetExtension(path)))
                .Where(path => !path.EndsWith(".gback", StringComparison.OrdinalIgnoreCase))
                .Where(path => !path.EndsWith(".roiback", StringComparison.OrdinalIgnoreCase))
                .Where(path => !Path.GetFileNameWithoutExtension(path).EndsWith("-Temp", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => File.GetCreationTime(path))
                .ToList();
        }

        private bool IsImageFile(string path)
        {
            return imageExtensions.Contains(Path.GetExtension(path));
        }

        private Bitmap LoadBitmapWithoutLock(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (Image img = Image.FromStream(fs))
                {
                    return new Bitmap(img);
                }
            }
        }

        private ImageFormat GetImageFormatByExtension(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".png") return ImageFormat.Png;
            if (ext == ".bmp" || ext == ".bit") return ImageFormat.Bmp;
            if (ext == ".gif") return ImageFormat.Gif;
            if (ext == ".tif" || ext == ".tiff") return ImageFormat.Tiff;
            return ImageFormat.Jpeg;
        }

        private void SaveBitmapToPath(Bitmap bitmap, string path)
        {
            string tempPath = path + ".editingtmp";
            if (File.Exists(tempPath)) File.Delete(tempPath);
            bitmap.Save(tempPath, GetImageFormatByExtension(path));
            if (File.Exists(path)) File.Delete(path);
            File.Move(tempPath, path);
        }

        private void ReleaseCurrentImage()
        {
            if (picVideoBox.Image != null)
            {
                Image oldImage = picVideoBox.Image;
                picVideoBox.Image = null;
                oldImage.Dispose();
            }
        }

        private void ModifyAllUploadedImages(Action<Bitmap, string> modifyAction)
        {
            List<string> targets = GetUploadedImageFiles();
            if (targets.Count == 0) return;

            ReleaseCurrentImage();

            foreach (string targetPath in targets)
            {
                Bitmap targetBitmap = null;
                try
                {
                    targetBitmap = LoadBitmapWithoutLock(targetPath);
                    modifyAction(targetBitmap, targetPath);
                    SaveBitmapToPath(targetBitmap, targetPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"이미지 편집 중 오류가 발생했습니다.\n{Path.GetFileName(targetPath)}\n{ex.Message}");
                }
                finally
                {
                    if (targetBitmap != null) targetBitmap.Dispose();
                }
            }

            LoadUploadedFilesToD();
        }

        private void btnNoise_Click(object sender, EventArgs e)
        {
            ModifyAllUploadedImages((bmp, path) =>
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
            ModifyAllUploadedImages((bmp, path) =>
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            });
        }

        private void btnMirrorY_Click(object sender, EventArgs e)
        {
            List<string> targets = GetUploadedImageFiles();
            if (targets.Count == 0) return;

            string backupFolder = GetMirrorYBackupFolder();
            bool hasBackup = Directory.Exists(backupFolder) && Directory.GetFiles(backupFolder).Any();

            ReleaseCurrentImage();

            try
            {
                if (!hasBackup)
                {
                    foreach (string targetPath in targets)
                    {
                        string backupPath = Path.Combine(backupFolder, Path.GetFileName(targetPath));
                        File.Copy(targetPath, backupPath, true);
                    }

                    foreach (string targetPath in targets)
                    {
                        Bitmap targetBitmap = null;
                        try
                        {
                            targetBitmap = LoadBitmapWithoutLock(targetPath);
                            targetBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                            SaveBitmapToPath(targetBitmap, targetPath);
                        }
                        finally
                        {
                            if (targetBitmap != null) targetBitmap.Dispose();
                        }
                    }
                }
                else
                {
                    string uploadFolder = GetUploadedFolder();
                    foreach (string backupPath in Directory.GetFiles(backupFolder))
                    {
                        string destPath = Path.Combine(uploadFolder, Path.GetFileName(backupPath));
                        File.Copy(backupPath, destPath, true);
                    }

                    try
                    {
                        Directory.Delete(backupFolder, true);
                    }
                    catch { }
                }

                LoadUploadedFilesToD();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"상하 반전 처리 중 오류가 발생했습니다: {ex.Message}");
                LoadUploadedFilesToD();
            }
        }

        private void ApplyROIBlackoutToAllImages(int row, int col)
        {
            List<string> targets = GetUploadedImageFiles();
            if (targets.Count == 0) return;

            roiState[row, col] = !roiState[row, col];
            ReleaseCurrentImage();

            foreach (string targetPath in targets)
            {
                string backupPath = targetPath + ".roiback";
                Bitmap compositeBmp = null;

                try
                {
                    if (!File.Exists(backupPath))
                    {
                        File.Copy(targetPath, backupPath, true);
                    }

                    compositeBmp = LoadBitmapWithoutLock(backupPath);

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
                                    int rectWidth = c == 2 ? compositeBmp.Width - x : w;
                                    int rectHeight = r == 2 ? compositeBmp.Height - y : h;
                                    g.FillRectangle(Brushes.Black, new Rectangle(x, y, rectWidth, rectHeight));
                                }
                            }
                        }
                    }

                    SaveBitmapToPath(compositeBmp, targetPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ROI 처리 중 오류가 발생했습니다.\n{Path.GetFileName(targetPath)}\n{ex.Message}");
                }
                finally
                {
                    if (compositeBmp != null) compositeBmp.Dispose();
                }
            }

            LoadUploadedFilesToD();
        }

        private void trcbrContrastProperty_Scroll(object sender, EventArgs e)
        {
            List<string> targets = GetUploadedImageFiles();
            if (targets.Count == 0) return;

            int trackValue = trcbrContrastProperty.Value;
            ReleaseCurrentImage();

            foreach (string targetPath in targets)
            {
                try
                {
                    if (!gammaBackupPaths.ContainsKey(targetPath) || !File.Exists(gammaBackupPaths[targetPath]))
                    {
                        string backupPath = targetPath + ".gback";
                        File.Copy(targetPath, backupPath, true);
                        gammaBackupPaths[targetPath] = backupPath;
                    }

                    string sourcePath = gammaBackupPaths[targetPath];

                    if (trackValue == 0)
                    {
                        File.Copy(sourcePath, targetPath, true);
                        continue;
                    }

                    double gammaCalculationValue = 1.0;
                    if (trackValue > 0)
                    {
                        gammaCalculationValue = 1.0 - trackValue * 0.08;
                    }
                    else if (trackValue < 0)
                    {
                        gammaCalculationValue = 1.0 + -trackValue * 0.2;
                    }

                    Bitmap targetBitmap = null;
                    try
                    {
                        targetBitmap = LoadBitmapWithoutLock(sourcePath);

                        using (Bitmap tempCopy = (Bitmap)targetBitmap.Clone())
                        {
                            using (Graphics g = Graphics.FromImage(targetBitmap))
                            {
                                using (ImageAttributes attributes = new ImageAttributes())
                                {
                                    attributes.SetGamma((float)gammaCalculationValue, ColorAdjustType.Bitmap);
                                    g.DrawImage(tempCopy, new Rectangle(0, 0, targetBitmap.Width, targetBitmap.Height), 0, 0, tempCopy.Width, tempCopy.Height, GraphicsUnit.Pixel, attributes);
                                }
                            }
                        }

                        SaveBitmapToPath(targetBitmap, targetPath);
                    }
                    finally
                    {
                        if (targetBitmap != null) targetBitmap.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"대비 처리 중 오류가 발생했습니다.\n{Path.GetFileName(targetPath)}\n{ex.Message}");
                }
            }

            LoadUploadedFilesToD();
        }

        private void HandlePaletteClick(int filterType, Button targetButton)
        {
            activePaletteButton = targetButton;
            ResetPaletteStatus();
            activePaletteButton = targetButton;
            if (activePaletteButton != null) activePaletteButton.Enabled = false;

            ModifyAllUploadedImages((bmp, path) =>
            {
                ApplyPresetColorFilterToBitmap(bmp, filterType);
            });
        }

        private void ApplyPresetColorFilterToBitmap(Bitmap bmp, int filterType)
        {
            float[][] matrixElements;
            switch (filterType)
            {
                case 1:
                    matrixElements = new float[][]
                    {
                        new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                        new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                        new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 1}
                    };
                    break;
                case 2:
                    matrixElements = new float[][]
                    {
                        new float[] {-1, 0, 0, 0, 0},
                        new float[] {0, -1, 0, 0, 0},
                        new float[] {0, 0, -1, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {1, 1, 1, 0, 1}
                    };
                    break;
                case 3:
                    matrixElements = new float[][]
                    {
                        new float[] {0.8f, 0, 0, 0, 0},
                        new float[] {0, 0.8f, 0, 0, 0},
                        new float[] {0, 0, 1.3f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0.15f, 0, 1}
                    };
                    break;
                case 4:
                    matrixElements = new float[][]
                    {
                        new float[] {1.2f, 0, 0, 0, 0},
                        new float[] {0, 1.2f, 0, 0, 0},
                        new float[] {0, 0, 0.7f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0.15f, 0.15f, 0, 0, 1}
                    };
                    break;
                case 5:
                    matrixElements = new float[][]
                    {
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

            using (Bitmap tempCopy = (Bitmap)bmp.Clone())
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    using (ImageAttributes attributes = new ImageAttributes())
                    {
                        ColorMatrix colorMatrix = new ColorMatrix(matrixElements);
                        attributes.SetColorMatrix(colorMatrix);
                        g.DrawImage(tempCopy, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, tempCopy.Width, tempCopy.Height, GraphicsUnit.Pixel, attributes);
                    }
                }
            }
        }

        private void btnColorCfm_Click(object sender, EventArgs e)
        {
            ResetPaletteStatus();
            LoadUploadedFilesToD();
        }

        private void btnColorCancle_Click(object sender, EventArgs e)
        {
            ResetPaletteStatus();
            LoadUploadedFilesToD();
        }

        private void ResetPaletteStatus()
        {
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
            string trashFolder = GetTrashFolder();

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

            string targetFolder = GetUploadedFolder();
            DirectoryInfo di = new DirectoryInfo(targetFolder);

            var folders = di.GetDirectories().OrderBy(d => d.CreationTime).ToList();
            foreach (var folder in folders)
            {
                ListViewItem item = new ListViewItem("[폴더] " + folder.Name);
                item.Tag = "추가된파일";
                lstviewFileListD.Items.Add(item);
            }

            var files = di.GetFiles()
                .Where(f => !f.Name.EndsWith(".gback", StringComparison.OrdinalIgnoreCase))
                .Where(f => !f.Name.EndsWith(".roiback", StringComparison.OrdinalIgnoreCase))
                .Where(f => !f.Name.EndsWith(".editingtmp", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f.CreationTime)
                .ToList();

            foreach (var file in files)
            {
                ListViewItem item = new ListViewItem(file.Name);
                item.Tag = "추가된파일";
                lstviewFileListD.Items.Add(item);

                if (IsImageFile(file.FullName))
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
            else
            {
                ReleaseCurrentImage();
                sdrSeekBar.RangeMin = 0;
                sdrSeekBar.RangeMax = 0;
                sdrSeekBar.Value = 0;
                sdrSeekBar.Text = "0/0";
            }
        }

        private void LoadTrashCanFiles()
        {
            lstviewTrash.Items.Clear();

            string trashFolder = GetTrashFolder();
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

            if (currentSlideIndex < 0) currentSlideIndex = 0;
            if (currentSlideIndex >= slideImages.Count) currentSlideIndex = slideImages.Count - 1;

            string currentImagePath = slideImages[currentSlideIndex];
            if (!File.Exists(currentImagePath)) return;

            ReleaseCurrentImage();

            try
            {
                picVideoBox.Image = LoadBitmapWithoutLock(currentImagePath);
            }
            catch
            {
                return;
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
            string trashFolder = GetTrashFolder();
            string uploadFolder = GetUploadedFolder();
            List<string> targets = new List<string>();

            if (lstviewFileListD.SelectedItems.Count > 0)
            {
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
            ReleaseCurrentImage();

            foreach (string target in targets)
            {
                try
                {
                    string dest = GetNonConflictingPath(Path.Combine(trashFolder, Path.GetFileName(target)));
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

        private string GetNonConflictingPath(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path)) return path;

            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            int index = 1;

            while (true)
            {
                string candidate = Path.Combine(directory, $"{fileName} ({index}){extension}");
                if (!File.Exists(candidate) && !Directory.Exists(candidate)) return candidate;
                index++;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string binFolder = GetBinFolder();
            string uploadFolder = GetUploadedFolder();

            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "저장할 새 폴더 이름을 입력하세요.";
                dialog.InitialDirectory = binFolder;
                dialog.FileName = "새 폴더 이름";
                dialog.Filter = "폴더 이름|*.*";
                dialog.AddExtension = false;
                dialog.DefaultExt = "";
                dialog.CheckPathExists = true;
                dialog.CheckFileExists = false;
                dialog.OverwritePrompt = false;
                dialog.ValidateNames = true;

                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                string selectedFolder = Path.GetFullPath(dialog.FileName);
                string folderName = Path.GetFileName(selectedFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

                if (string.IsNullOrWhiteSpace(folderName))
                {
                    MessageBox.Show("생성할 폴더 이름을 입력해야 합니다.");
                    return;
                }

                char[] invalidChars = Path.GetInvalidFileNameChars();
                if (folderName.IndexOfAny(invalidChars) >= 0)
                {
                    MessageBox.Show("폴더 이름에 사용할 수 없는 문자가 포함되어 있습니다.");
                    return;
                }

                if (File.Exists(selectedFolder))
                {
                    MessageBox.Show("같은 이름의 파일이 이미 존재합니다. 다른 폴더 이름을 입력하세요.");
                    return;
                }

                if (string.Equals(selectedFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), uploadFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("UploadedFile 폴더 자체는 저장 대상 폴더로 사용할 수 없습니다.");
                    return;
                }

                try
                {
                    Directory.CreateDirectory(selectedFolder);
                    ReleaseCurrentImage();

                    string[] files = Directory.GetFiles(uploadFolder)
                        .Where(path => !path.EndsWith(".gback", StringComparison.OrdinalIgnoreCase))
                        .Where(path => !path.EndsWith(".roiback", StringComparison.OrdinalIgnoreCase))
                        .Where(path => !path.EndsWith(".editingtmp", StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                    foreach (string file in files)
                    {
                        string dest = GetNonConflictingPath(Path.Combine(selectedFolder, Path.GetFileName(file)));
                        File.Move(file, dest);
                    }

                    foreach (string backupFile in Directory.GetFiles(uploadFolder).Where(path => path.EndsWith(".gback", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".roiback", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".editingtmp", StringComparison.OrdinalIgnoreCase)).ToArray())
                    {
                        try { File.Delete(backupFile); } catch { }
                    }

                    string mirrorBackupFolder = Path.Combine(GetBinFolder(), mirrorYBackupFolderName);
                    if (Directory.Exists(mirrorBackupFolder))
                    {
                        try { Directory.Delete(mirrorBackupFolder, true); } catch { }
                    }

                    gammaBackupPaths.Clear();
                    Array.Clear(roiState, 0, roiState.Length);
                    LoadUploadedFilesToD();
                    MessageBox.Show("입력한 이름의 폴더를 만들고 UploadedFile 안의 파일을 이동했습니다.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 저장 중 오류가 발생했습니다: {ex.Message}");
                }
            }
        }

        private void btnOpnFileExplrr_Click(object sender, EventArgs e)
        {
            string binFolder = GetBinFolder();
            System.Diagnostics.Process.Start("explorer.exe", binFolder);
        }

        private void btnRestoration_Click(object sender, EventArgs e)
        {
            if (lstviewTrash.SelectedItems.Count == 0) return;

            string trashFolder = GetTrashFolder();
            string uploadFolder = GetUploadedFolder();

            ReleaseCurrentImage();

            foreach (ListViewItem item in lstviewTrash.SelectedItems)
            {
                string fileName = item.Text.Replace("[폴더] ", "");
                string sourcePath = Path.Combine(trashFolder, fileName);
                string destPath = GetNonConflictingPath(Path.Combine(uploadFolder, fileName));

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
            LoadTrashCanFiles();
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

            DonkeyDataManager.frmNewtrainer form3 = new DonkeyDataManager.frmNewtrainer();
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

            SetListViewName("");
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

                    SetListViewName("[파일목록]");
                    btnRestoration.Visible = false;
                }
                else if (itemTag == "휴지통")
                {
                    lstviewMain.Visible = false;
                    lstviewFileList.Visible = false;
                    lstviewFileListD.Visible = false;
                    lstviewTrash.Visible = true;

                    SetListViewName("[휴지통]");
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
        private void GBPalete_Enter(object sender, EventArgs e) { }
        private void ColorTrackBar_Scroll(object sender, EventArgs e) { }

        private class PropertyPanelFilter : IMessageFilter
        {
            private frmMain form;
            private const int WM_LBUTTONDOWN = 0x0201;

            public PropertyPanelFilter(frmMain form)
            {
                this.form = form;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg != WM_LBUTTONDOWN) return false;
                if (form == null || form.IsDisposed) return false;
                if (!form.pnlContrastProperty.Visible && !form.pnlROI.Visible && !form.pnlColorProperty.Visible) return false;

                Point mousePos = Control.MousePosition;

                bool clickedInsidePropertyArea =
                    IsInside(form.pnlContrastProperty, mousePos) ||
                    IsInside(form.pnlROI, mousePos) ||
                    IsInside(form.pnlColorProperty, mousePos) ||
                    IsInside(form.btnContrastProperty, mousePos) ||
                    IsInside(form.btnROI, mousePos) ||
                    IsInside(form.btnColorProperty, mousePos);

                if (!clickedInsidePropertyArea)
                {
                    form.BeginInvoke(new Action(() => form.HidePropertyPanels()));
                }

                return false;
            }

            private bool IsInside(Control c, Point p)
            {
                if (c == null || !c.Visible) return false;
                Rectangle r = c.RectangleToScreen(c.ClientRectangle);
                return r.Contains(p);
            }
        }
    }

    public class ClickOutsideFilter : IMessageFilter
    {
        private Control panel;
        private Control button;
        private const int WM_LBUTTONDOWN = 0x0201;

        public ClickOutsideFilter(Control panel, Control button)
        {
            this.panel = panel;
            this.button = button;
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN && panel.Visible)
            {
                Point mousePos = Control.MousePosition;
                Rectangle panelRect = panel.RectangleToScreen(panel.ClientRectangle);
                Rectangle btnRect = button.RectangleToScreen(button.ClientRectangle);

                if (!panelRect.Contains(mousePos) && !btnRect.Contains(mousePos))
                {
                    panel.Invoke(new Action(() => panel.Visible = false));
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
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
    }
}
