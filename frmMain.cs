using Data_Manager;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AD_AI_LearningData_Editor;

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
        private Dictionary<string, DrivingInfo> drivingInfoCache = new Dictionary<string, DrivingInfo>(StringComparer.OrdinalIgnoreCase);
        private DateTime drivingInfoCacheTime = DateTime.MinValue;
        private List<int> intervalPointIndices = new List<int>();
        private int selectedIntervalStartIndex = -1;
        private int selectedIntervalEndIndex = -1;
        private Font lblSetIntervalDesignerFont = null;

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
            CaptureIntervalLabelDesignerFont();

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
            IconProperty.SetAutoImageByWidthHeight(
                 btnSave,
                 Data_Manager.Properties.Resources.Save,
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
            UpdatePlayStopButtonState();

            btnNxt1F.Click += btnNxt1F_Click;
            btnNxt5F.Click += btnNxt5F_Click;
            btnPre1F.Click += btnPre1F_Click;
            btnPre5F.Click += btnPre5F_Click;
            btnDel.Click += btnDel_Click;
            sdrSeekBar.onValueChanged += SdrSeekBar_onValueChanged;

            btnOpnFileExplrr.Click += btnOpnFileExplrr_Click;
            btnRestoration.Click += btnRestoration_Click;
            btnSave.Click += btnSave_Click;
            RegisterIntervalControls();

            btnRestoration.Visible = false;

            ConfigureListViewNameLabel();
            ConfigureFileListDView();
            SetupTabs();
            LoadUploadedFilesToD();
            LoadTrashCanFiles();
            SetupTrashWatcher();

            this.lstviewMain.MouseDoubleClick += lstviewMain_MouseDoubleClick;

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

        private void ConfigureFileListDView()
        {
            lstviewFileListD.BeginUpdate();
            lstviewFileListD.View = View.Details;
            lstviewFileListD.HeaderStyle = ColumnHeaderStyle.None;
            lstviewFileListD.FullRowSelect = true;
            lstviewFileListD.MultiSelect = true;
            lstviewFileListD.Scrollable = true;
            lstviewFileListD.HideSelection = false;
            lstviewFileListD.Columns.Clear();
            lstviewFileListD.Columns.Add("FileName", Math.Max(1, lstviewFileListD.ClientSize.Width - 4));
            lstviewFileListD.EndUpdate();

            lstviewFileListD.Resize += (s, e) =>
            {
                if (lstviewFileListD.Columns.Count > 0)
                {
                    lstviewFileListD.Columns[0].Width = Math.Max(1, lstviewFileListD.ClientSize.Width - 4);
                }
            };
        }

        private void SetListViewName(string text)
        {
            lblLstVwName.Text = text;
            lblLstVwName.Font = new Font("맑은 고딕", 14F, FontStyle.Bold, GraphicsUnit.Point);
            lblLstVwName.UseCompatibleTextRendering = true;
            lblLstVwName.Refresh();
        }

        private void RegisterIntervalControls()
        {
            Control btn = this.Controls.Find("btnSetInterval", true).FirstOrDefault();
            if (btn != null)
            {
                btn.Click += btnSetInterval_Click;
            }

            ConfigureIntervalLabelFont();
            SetIntervalLabelText("");
        }

        private void btnSetInterval_Click(object sender, EventArgs e)
        {
            if (slideImages.Count == 0)
            {
                ResetSelectedInterval();
                return;
            }

            if (HasSelectedInterval() || intervalPointIndices.Count >= 2)
            {
                ResetSelectedInterval();
            }

            intervalPointIndices.Add(currentSlideIndex);

            if (intervalPointIndices.Count == 1)
            {
                int displayIndex = currentSlideIndex + 1;
                SetIntervalLabelText($"({displayIndex}~ )");
                return;
            }

            int first = intervalPointIndices[0];
            int second = intervalPointIndices[1];

            selectedIntervalStartIndex = Math.Min(first, second);
            selectedIntervalEndIndex = Math.Max(first, second);

            SetIntervalLabelText($"({selectedIntervalStartIndex + 1}~{selectedIntervalEndIndex + 1})");
            SelectIntervalItemsInListView();
        }

        private bool HasSelectedInterval()
        {
            return selectedIntervalStartIndex >= 0 &&
                   selectedIntervalEndIndex >= selectedIntervalStartIndex &&
                   selectedIntervalStartIndex < slideImages.Count;
        }

        private void ResetSelectedInterval()
        {
            intervalPointIndices.Clear();
            selectedIntervalStartIndex = -1;
            selectedIntervalEndIndex = -1;
            SetIntervalLabelText("");
        }

        private void CaptureIntervalLabelDesignerFont()
        {
            Control label = this.Controls.Find("lblSetInterval", true).FirstOrDefault();

            if (label != null && lblSetIntervalDesignerFont == null)
            {
                lblSetIntervalDesignerFont = (Font)label.Font.Clone();
            }
        }

        private void ConfigureIntervalLabelFont()
        {
            Control label = this.Controls.Find("lblSetInterval", true).FirstOrDefault();

            if (label == null)
            {
                return;
            }

            if (lblSetIntervalDesignerFont == null)
            {
                lblSetIntervalDesignerFont = (Font)label.Font.Clone();
            }

            label.Font = lblSetIntervalDesignerFont;

            if (label is Label winLabel)
            {
                winLabel.UseCompatibleTextRendering = true;
            }
        }

        private void SetIntervalLabelText(string text)
        {
            Control label = this.Controls.Find("lblSetInterval", true).FirstOrDefault();
            if (label != null)
            {
                ConfigureIntervalLabelFont();
                label.Text = text;
                label.Font = lblSetIntervalDesignerFont;
                label.Refresh();
            }
        }

        private List<string> GetIntervalImageFiles()
        {
            List<string> targets = new List<string>();

            if (!HasSelectedInterval())
            {
                return targets;
            }

            int start = Math.Max(0, selectedIntervalStartIndex);
            int end = Math.Min(slideImages.Count - 1, selectedIntervalEndIndex);

            for (int i = start; i <= end; i++)
            {
                string path = slideImages[i];

                if (File.Exists(path) && IsImageFile(path))
                {
                    targets.Add(path);
                }
            }

            return targets;
        }

        private List<string> GetSelectedListViewImageFiles()
        {
            List<string> targets = new List<string>();
            string uploadFolder = GetUploadedFolder();

            if (lstviewFileListD.SelectedItems.Count == 0)
            {
                return targets;
            }

            foreach (ListViewItem item in lstviewFileListD.SelectedItems)
            {
                string name = item.Text.Replace("[폴더] ", "");
                string path = Path.Combine(uploadFolder, name);

                if (File.Exists(path) && IsImageFile(path))
                {
                    targets.Add(path);
                }
            }

            return targets
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private List<string> GetTargetImageFilesForEdit()
        {
            List<string> intervalTargets = GetIntervalImageFiles();

            if (intervalTargets.Count > 0)
            {
                return intervalTargets;
            }

            List<string> selectedTargets = GetSelectedListViewImageFiles();

            if (selectedTargets.Count > 0)
            {
                return selectedTargets;
            }

            return GetUploadedImageFiles();
        }

        private void SelectIntervalItemsInListView()
        {
            if (!HasSelectedInterval())
            {
                return;
            }

            HashSet<string> selectedNames = new HashSet<string>(
                GetIntervalImageFiles().Select(path => Path.GetFileName(path)),
                StringComparer.OrdinalIgnoreCase
            );

            lstviewFileListD.BeginUpdate();

            foreach (ListViewItem item in lstviewFileListD.Items)
            {
                item.Selected = selectedNames.Contains(item.Text);
            }

            lstviewFileListD.EndUpdate();
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
            List<string> targets = GetTargetImageFilesForEdit();
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
            SelectIntervalItemsInListView();
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
            List<string> targets = GetTargetImageFilesForEdit();
            if (targets.Count == 0) return;

            string backupFolder = GetMirrorYBackupFolder();
            bool hasBackupForTargets = targets.All(targetPath =>
                File.Exists(Path.Combine(backupFolder, Path.GetFileName(targetPath)))
            );

            ReleaseCurrentImage();

            try
            {
                if (!hasBackupForTargets)
                {
                    foreach (string targetPath in targets)
                    {
                        string backupPath = Path.Combine(backupFolder, Path.GetFileName(targetPath));

                        if (!File.Exists(backupPath))
                        {
                            File.Copy(targetPath, backupPath, true);
                        }
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
                    foreach (string targetPath in targets)
                    {
                        string backupPath = Path.Combine(backupFolder, Path.GetFileName(targetPath));

                        if (File.Exists(backupPath))
                        {
                            File.Copy(backupPath, targetPath, true);
                            try { File.Delete(backupPath); } catch { }
                        }
                    }

                    try
                    {
                        if (Directory.Exists(backupFolder) && !Directory.GetFiles(backupFolder).Any())
                        {
                            Directory.Delete(backupFolder, true);
                        }
                    }
                    catch { }
                }

                LoadUploadedFilesToD();
                SelectIntervalItemsInListView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"상하 반전 처리 중 오류가 발생했습니다: {ex.Message}");
                LoadUploadedFilesToD();
                SelectIntervalItemsInListView();
            }
        }

        private void ApplyROIBlackoutToAllImages(int row, int col)
        {
            List<string> targets = GetTargetImageFilesForEdit();
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
            SelectIntervalItemsInListView();
        }

        private void trcbrContrastProperty_Scroll(object sender, EventArgs e)
        {
            List<string> targets = GetTargetImageFilesForEdit();
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
            SelectIntervalItemsInListView();
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
            picVideoBox.Cursor = Cursors.Hand;
            picVideoBox.OverlayIcon = GetResourceImageByName("PlaySlide4655096");
            picVideoBox.Click += VideoArea_Click;

            if (this.Controls.Find("pnlVideo", true).FirstOrDefault() is Panel pnl)
            {
                pnl.Click += VideoArea_Click;
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
            if (lstviewFileListD.Columns.Count == 0)
            {
                ConfigureFileListDView();
            }

            lstviewFileListD.BeginUpdate();
            lstviewFileListD.Items.Clear();
            slideImages.Clear();
            currentSlideIndex = 0;

            string targetFolder = GetUploadedFolder();
            DirectoryInfo di = new DirectoryInfo(targetFolder);

            var folders = di.GetDirectories()
                .OrderBy(d => d.Name, new NaturalFileNameComparer())
                .ToList();

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
                .Where(f => !f.Name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var filesForListView = files
                .OrderBy(f => f.Name, new NaturalFileNameComparer())
                .ToList();

            foreach (var file in filesForListView)
            {
                ListViewItem item = new ListViewItem(file.Name);
                item.Tag = "추가된파일";
                lstviewFileListD.Items.Add(item);
            }

            slideImages = files
                .Where(f => IsImageFile(f.FullName))
                .OrderBy(f => GetSlideImageSortNumber(f.Name))
                .ThenBy(f => NormalizeDrivingImageName(f.Name), new NaturalFileNameComparer())
                .ThenBy(f => f.Name, new NaturalFileNameComparer())
                .Select(f => f.FullName)
                .ToList();

            lstviewFileListD.EndUpdate();

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
                SetTempDrivingInfoText("", "");
            }

            UpdatePlayStopButtonState();
        }

        private int GetSlideImageSortNumber(string fileName)
        {
            string normalizedName = NormalizeDrivingImageName(fileName);
            string numberText = ExtractLeadingNumber(normalizedName);

            if (int.TryParse(numberText, out int number))
            {
                return number;
            }

            return int.MaxValue;
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


        private class DrivingInfo
        {
            public string Angle { get; set; }
            public string Throttle { get; set; }
        }

        private class CatalogFormatInfo
        {
            public List<string> Columns { get; set; } = new List<string>();
            public List<string> CatalogFileNames { get; set; } = new List<string>();
            public int ImageIndex { get; set; } = -1;
            public int AngleIndex { get; set; } = -1;
            public int ThrottleIndex { get; set; } = -1;
        }

        private void UpdateCurrentDrivingInfo(string imagePath)
        {
            DrivingInfo info = FindDrivingInfoForImage(imagePath);

            if (info == null)
            {
                SetTempDrivingInfoText("", "");
                return;
            }

            SetTempDrivingInfoText(info.Angle, info.Throttle);
        }

        private void SetTempDrivingInfoText(string angle, string throttle)
        {
            Control angleBox = this.Controls.Find("txtTempAngle", true).FirstOrDefault();
            Control speedBox = this.Controls.Find("txtTempSpeed", true).FirstOrDefault();

            if (angleBox != null)
            {
                angleBox.Text = angle;
            }

            if (speedBox != null)
            {
                speedBox.Text = throttle;
            }
        }

        private DrivingInfo FindDrivingInfoForImage(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return null;
            }

            BuildDrivingInfoCacheIfNeeded();

            string fileName = Path.GetFileName(imagePath);
            string normalizedName = NormalizeDrivingImageName(fileName);
            string normalizedNameWithoutExtension = Path.GetFileNameWithoutExtension(normalizedName);

            if (drivingInfoCache.TryGetValue(fileName, out DrivingInfo directInfo))
            {
                return directInfo;
            }

            if (drivingInfoCache.TryGetValue(normalizedName, out DrivingInfo normalizedInfo))
            {
                return normalizedInfo;
            }

            if (drivingInfoCache.TryGetValue(normalizedNameWithoutExtension, out DrivingInfo nameWithoutExtInfo))
            {
                return nameWithoutExtInfo;
            }

            string index = ExtractLeadingNumber(normalizedName);

            if (!string.IsNullOrWhiteSpace(index) && drivingInfoCache.TryGetValue("INDEX:" + index, out DrivingInfo indexInfo))
            {
                return indexInfo;
            }

            return null;
        }

        private void BuildDrivingInfoCacheIfNeeded()
        {
            string uploadFolder = GetUploadedFolder();

            if (!Directory.Exists(uploadFolder))
            {
                drivingInfoCache.Clear();
                drivingInfoCacheTime = DateTime.Now;
                return;
            }

            List<string> dataFiles = Directory.GetFiles(uploadFolder, "*.*", SearchOption.AllDirectories)
                .Where(path =>
                    string.Equals(Path.GetExtension(path), ".json", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(Path.GetExtension(path), ".catalog", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path)
                .ToList();

            DateTime newestWriteTime = dataFiles
                .Select(path => File.GetLastWriteTime(path))
                .DefaultIfEmpty(DateTime.MinValue)
                .Max();

            if (drivingInfoCache.Count > 0 && newestWriteTime <= drivingInfoCacheTime)
            {
                return;
            }

            drivingInfoCache.Clear();

            List<CatalogFormatInfo> manifestFormats = new List<CatalogFormatInfo>();

            foreach (string jsonFile in dataFiles.Where(path => string.Equals(Path.GetExtension(path), ".json", StringComparison.OrdinalIgnoreCase)))
            {
                CatalogFormatInfo manifest = TryReadManifestFormat(jsonFile);

                if (manifest != null)
                {
                    manifestFormats.Add(manifest);
                }
            }

            foreach (string catalogFile in dataFiles.Where(path => string.Equals(Path.GetExtension(path), ".catalog", StringComparison.OrdinalIgnoreCase)))
            {
                CatalogFormatInfo format = FindManifestForCatalog(catalogFile, manifestFormats);

                if (format == null)
                {
                    format = CreateDefaultCatalogFormat();
                }

                TryReadCatalogFile(catalogFile, format);
            }

            foreach (string jsonFile in dataFiles.Where(path => string.Equals(Path.GetExtension(path), ".json", StringComparison.OrdinalIgnoreCase)))
            {
                if (TryReadManifestFormat(jsonFile) != null)
                {
                    continue;
                }

                TryReadJsonRecordFile(jsonFile);
            }

            drivingInfoCacheTime = newestWriteTime;
        }

        private CatalogFormatInfo TryReadManifestFormat(string jsonFile)
        {
            try
            {
                string[] lines = File.ReadAllLines(jsonFile);

                if (lines.Length < 2)
                {
                    return null;
                }

                List<string> columns = TryReadStringArrayLine(lines[0]);

                if (columns == null || columns.Count == 0)
                {
                    return null;
                }

                bool looksLikeManifest =
                    columns.Any(x => x.IndexOf("image", StringComparison.OrdinalIgnoreCase) >= 0) &&
                    columns.Any(x => x.IndexOf("angle", StringComparison.OrdinalIgnoreCase) >= 0) &&
                    columns.Any(x => x.IndexOf("throttle", StringComparison.OrdinalIgnoreCase) >= 0);

                if (!looksLikeManifest)
                {
                    return null;
                }

                CatalogFormatInfo format = new CatalogFormatInfo();
                format.Columns = columns;
                format.ImageIndex = FindColumnIndex(columns, "image");
                format.AngleIndex = FindColumnIndex(columns, "angle");
                format.ThrottleIndex = FindColumnIndex(columns, "throttle");

                foreach (string line in lines)
                {
                    foreach (string path in ExtractCatalogPathsFromManifestLine(line))
                    {
                        string fileName = Path.GetFileName(path);
                        if (!string.IsNullOrWhiteSpace(fileName))
                        {
                            format.CatalogFileNames.Add(fileName);
                            format.CatalogFileNames.Add(NormalizeDrivingFileName(fileName));
                        }
                    }
                }

                return format;
            }
            catch
            {
                return null;
            }
        }

        private List<string> TryReadStringArrayLine(string line)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(line))
                {
                    if (doc.RootElement.ValueKind != JsonValueKind.Array)
                    {
                        return null;
                    }

                    List<string> result = new List<string>();

                    foreach (JsonElement item in doc.RootElement.EnumerateArray())
                    {
                        result.Add(item.ToString());
                    }

                    return result;
                }
            }
            catch
            {
                return null;
            }
        }

        private List<string> ExtractCatalogPathsFromManifestLine(string line)
        {
            List<string> result = new List<string>();

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(line))
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                        doc.RootElement.TryGetProperty("paths", out JsonElement pathsElement) &&
                        pathsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement item in pathsElement.EnumerateArray())
                        {
                            result.Add(item.ToString());
                        }
                    }
                }
            }
            catch
            {
            }

            return result;
        }

        private int FindColumnIndex(List<string> columns, string key)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                if (columns[i].IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return i;
                }
            }

            return -1;
        }

        private CatalogFormatInfo FindManifestForCatalog(string catalogFile, List<CatalogFormatInfo> formats)
        {
            string catalogName = Path.GetFileName(catalogFile);
            string normalizedCatalogName = NormalizeDrivingFileName(catalogName);

            foreach (CatalogFormatInfo format in formats)
            {
                if (format.CatalogFileNames.Any(name =>
                    string.Equals(name, catalogName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, normalizedCatalogName, StringComparison.OrdinalIgnoreCase)))
                {
                    return format;
                }
            }

            return formats.FirstOrDefault();
        }

        private CatalogFormatInfo CreateDefaultCatalogFormat()
        {
            return new CatalogFormatInfo
            {
                Columns = new List<string> { "cam/image_array", "user/angle", "user/throttle", "user/mode" },
                ImageIndex = 0,
                AngleIndex = 1,
                ThrottleIndex = 2
            };
        }

        private void TryReadCatalogFile(string catalogFile, CatalogFormatInfo format)
        {
            try
            {
                foreach (string line in File.ReadLines(catalogFile))
                {
                    TryAddDrivingInfoFromCatalogLine(line, catalogFile, format);
                }
            }
            catch
            {
            }
        }

        private void TryAddDrivingInfoFromCatalogLine(string line, string catalogFile, CatalogFormatInfo format)
        {
            if (string.IsNullOrWhiteSpace(line) || format == null)
            {
                return;
            }

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(line))
                {
                    JsonElement root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Object && TryAddDrivingInfoFromDonkeyCatalogObject(root))
                    {
                        return;
                    }

                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        TryAddDrivingInfoFromCatalogArray(root, format);
                    }
                    else if (root.ValueKind == JsonValueKind.Object)
                    {
                        TryAddDrivingInfoFromJsonObject(root, catalogFile);
                    }
                }
            }
            catch
            {
            }
        }

        private bool TryAddDrivingInfoFromDonkeyCatalogObject(JsonElement root)
        {
            if (root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!root.TryGetProperty("cam/image_array", out JsonElement imageElement))
            {
                return false;
            }

            string imageName = imageElement.ToString();

            if (string.IsNullOrWhiteSpace(imageName))
            {
                return false;
            }

            string angle = "";
            string throttle = "";

            if (root.TryGetProperty("user/angle", out JsonElement angleElement))
            {
                angle = angleElement.ToString();
            }

            if (root.TryGetProperty("user/throttle", out JsonElement throttleElement))
            {
                throttle = throttleElement.ToString();
            }

            DrivingInfo info = new DrivingInfo
            {
                Angle = angle,
                Throttle = throttle
            };

            AddDrivingInfoForImageName(imageName, info);

            if (root.TryGetProperty("_index", out JsonElement indexElement))
            {
                string index = indexElement.ToString();

                if (!string.IsNullOrWhiteSpace(index))
                {
                    AddDrivingInfoCacheItem("INDEX:" + index, info);
                }
            }

            return true;
        }

        private void TryAddDrivingInfoFromCatalogArray(JsonElement array, CatalogFormatInfo format)
        {
            int count = array.GetArrayLength();

            if (format.ImageIndex < 0 || format.ImageIndex >= count)
            {
                return;
            }

            string imageName = Path.GetFileName(array[format.ImageIndex].ToString());

            if (string.IsNullOrWhiteSpace(imageName))
            {
                return;
            }

            string angle = "";
            string throttle = "";

            if (format.AngleIndex >= 0 && format.AngleIndex < count)
            {
                angle = array[format.AngleIndex].ToString();
            }

            if (format.ThrottleIndex >= 0 && format.ThrottleIndex < count)
            {
                throttle = array[format.ThrottleIndex].ToString();
            }

            AddDrivingInfoForImageName(imageName, new DrivingInfo
            {
                Angle = angle,
                Throttle = throttle
            });
        }

        private void TryReadJsonRecordFile(string jsonFile)
        {
            try
            {
                string text = File.ReadAllText(jsonFile);

                using (JsonDocument doc = JsonDocument.Parse(text))
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        TryAddDrivingInfoFromJsonObject(doc.RootElement, jsonFile);
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        CatalogFormatInfo format = CreateDefaultCatalogFormat();
                        TryAddDrivingInfoFromCatalogArray(doc.RootElement, format);
                    }
                }
            }
            catch
            {
                try
                {
                    foreach (string line in File.ReadLines(jsonFile))
                    {
                        TryAddDrivingInfoFromCatalogLine(line, jsonFile, CreateDefaultCatalogFormat());
                    }
                }
                catch
                {
                }
            }
        }

        private void TryAddDrivingInfoFromJsonObject(JsonElement root, string filePath)
        {
            string imageName = FindFirstImageNameInJson(root);
            string angle = FindNumberLikeJsonValue(root, "angle");
            string throttle = FindNumberLikeJsonValue(root, "throttle");

            if (string.IsNullOrWhiteSpace(angle) && string.IsNullOrWhiteSpace(throttle))
            {
                return;
            }

            DrivingInfo info = new DrivingInfo
            {
                Angle = angle,
                Throttle = throttle
            };

            if (!string.IsNullOrWhiteSpace(imageName))
            {
                AddDrivingInfoForImageName(imageName, info);
            }

            string extension = Path.GetExtension(filePath);

            if (!string.Equals(extension, ".catalog", StringComparison.OrdinalIgnoreCase))
            {
                string recordIndex = ExtractRecordIndexFromPath(filePath);

                if (!string.IsNullOrWhiteSpace(recordIndex))
                {
                    AddDrivingInfoCacheItem("INDEX:" + recordIndex, info);
                }
            }
        }

        private string FindFirstImageNameInJson(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                string value = element.GetString();

                if (LooksLikeImageFile(value))
                {
                    return Path.GetFileName(value);
                }

                return "";
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (property.Name.IndexOf("image", StringComparison.OrdinalIgnoreCase) >= 0 &&
                        property.Value.ValueKind == JsonValueKind.String &&
                        LooksLikeImageFile(property.Value.GetString()))
                    {
                        return Path.GetFileName(property.Value.GetString());
                    }

                    string nested = FindFirstImageNameInJson(property.Value);

                    if (!string.IsNullOrWhiteSpace(nested))
                    {
                        return nested;
                    }
                }
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in element.EnumerateArray())
                {
                    string nested = FindFirstImageNameInJson(item);

                    if (!string.IsNullOrWhiteSpace(nested))
                    {
                        return nested;
                    }
                }
            }

            return "";
        }

        private string FindNumberLikeJsonValue(JsonElement element, string keyName)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (property.Name.IndexOf(keyName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (property.Value.ValueKind == JsonValueKind.Number ||
                            property.Value.ValueKind == JsonValueKind.String)
                        {
                            return property.Value.ToString();
                        }
                    }

                    string nested = FindNumberLikeJsonValue(property.Value, keyName);

                    if (!string.IsNullOrWhiteSpace(nested))
                    {
                        return nested;
                    }
                }
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in element.EnumerateArray())
                {
                    string nested = FindNumberLikeJsonValue(item, keyName);

                    if (!string.IsNullOrWhiteSpace(nested))
                    {
                        return nested;
                    }
                }
            }

            return "";
        }

        private bool LooksLikeImageFile(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string ext = Path.GetExtension(value);
            return imageExtensions.Contains(ext);
        }

        private void AddDrivingInfoForImageName(string imageName, DrivingInfo info)
        {
            if (string.IsNullOrWhiteSpace(imageName) || info == null)
            {
                return;
            }

            string fileName = Path.GetFileName(imageName.Replace("\\", "/"));
            string normalizedName = NormalizeDrivingImageName(fileName);
            string imageIndex = ExtractLeadingNumber(normalizedName);

            AddDrivingInfoCacheItem(fileName, info);
            AddDrivingInfoCacheItem(normalizedName, info);
            AddDrivingInfoCacheItem(Path.GetFileNameWithoutExtension(normalizedName), info);

            if (!string.IsNullOrWhiteSpace(imageIndex))
            {
                AddDrivingInfoCacheItem("INDEX:" + imageIndex, info);
            }
        }

        private void AddDrivingInfoCacheItem(string key, DrivingInfo info)
        {
            if (string.IsNullOrWhiteSpace(key) || info == null)
            {
                return;
            }

            drivingInfoCache[key] = info;
        }

        private string NormalizeDrivingImageName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "";
            }

            string onlyFileName = Path.GetFileName(fileName.Replace("\\", "/"));
            string name = Path.GetFileNameWithoutExtension(onlyFileName);
            string ext = Path.GetExtension(onlyFileName);

            name = Regex.Replace(name, @"\s\(\d+\)$", "", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, @"-Copy$", "", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, @"_Copy$", "", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, @"\sCopy$", "", RegexOptions.IgnoreCase);

            return name + ext;
        }

        private string NormalizeDrivingFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "";
            }

            string onlyFileName = Path.GetFileName(fileName.Replace("\\", "/"));
            string name = Path.GetFileNameWithoutExtension(onlyFileName);
            string ext = Path.GetExtension(onlyFileName);

            name = Regex.Replace(name, @"\s\(\d+\)$", "", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, @"-Copy$", "", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, @"_Copy$", "", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, @"\sCopy$", "", RegexOptions.IgnoreCase);

            return name + ext;
        }

        private string ExtractLeadingNumber(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "";
            }

            string name = Path.GetFileNameWithoutExtension(fileName);
            Match match = Regex.Match(name, @"^(\d+)");

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return "";
        }

        private string ExtractRecordIndexFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "";
            }

            string name = Path.GetFileNameWithoutExtension(path);
            Match match = Regex.Match(name, @"(?:record[_-]?|catalog[_-]?|^)(\d+)", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return "";
        }

        private void UpdateSlideDisplay()
        {
            if (slideImages.Count == 0)
            {
                return;
            }

            if (currentSlideIndex < 0) currentSlideIndex = 0;
            if (currentSlideIndex >= slideImages.Count) currentSlideIndex = slideImages.Count - 1;

            string currentImagePath = slideImages[currentSlideIndex];

            isUpdatingSlider = true;
            sdrSeekBar.Value = currentSlideIndex;
            sdrSeekBar.Text = $"{currentSlideIndex + 1}/{slideImages.Count}";
            isUpdatingSlider = false;

            if (!File.Exists(currentImagePath))
            {
                ReleaseCurrentImage();
                SetTempDrivingInfoText("", "");
                return;
            }

            ReleaseCurrentImage();

            try
            {
                picVideoBox.Image = LoadBitmapWithoutLock(currentImagePath);
            }
            catch
            {
                SetTempDrivingInfoText("", "");
                return;
            }

            UpdateCurrentDrivingInfo(currentImagePath);
            UpdatePlayStopButtonState();
        }


        private void VideoArea_Click(object sender, EventArgs e)
        {
            btnPlayStop_Click(btnPlayStop, EventArgs.Empty);
        }

        private void UpdatePlayStopButtonState()
        {
            bool isPlaying = videoTimer != null && videoTimer.Enabled;

            if (btnPlayStop == null)
            {
                return;
            }

            btnPlayStop.Text = isPlaying ? "정지" : "재생";

            Image icon = isPlaying
                ? GetResourceImageByName("pause")
                : GetResourceImageByName("PlaySlide4655096");

            if (icon != null)
            {
                int iconSize = Math.Max(1, btnPlayStop.Height - 14);

                if (btnPlayStop is MaterialButton materialButton)
                {
                    IconProperty.SetIcon(materialButton, icon, iconSize);
                }
                else
                {
                    IconProperty.SetImage(btnPlayStop, icon, iconSize, iconSize);
                }
            }

            if (picVideoBox != null)
            {
                picVideoBox.OverlayIcon = GetResourceImageByName("PlaySlide4655096");
                picVideoBox.ShowOverlayIcon = !isPlaying && slideImages.Count > 0;
                picVideoBox.Invalidate();
            }
        }

        private Image GetResourceImageByName(string resourceName)
        {
            try
            {
                object resource = Data_Manager.Properties.Resources.ResourceManager.GetObject(resourceName);

                if (resource is Image image)
                {
                    return image;
                }
            }
            catch
            {
            }

            return null;
        }

        private Image GetResourceImageByKeyword(string keyword)
        {
            try
            {
                System.Resources.ResourceSet resourceSet =
                    Data_Manager.Properties.Resources.ResourceManager.GetResourceSet(
                        System.Globalization.CultureInfo.CurrentUICulture,
                        true,
                        true
                    );

                if (resourceSet == null)
                {
                    return null;
                }

                foreach (System.Collections.DictionaryEntry entry in resourceSet)
                {
                    string key = entry.Key?.ToString();

                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    if (key.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 &&
                        entry.Value is Image image)
                    {
                        return image;
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private void btnPlayStop_Click(object sender, EventArgs e)
        {
            if (slideImages.Count == 0)
            {
                if (videoTimer != null && videoTimer.Enabled)
                {
                    videoTimer.Stop();
                }

                UpdatePlayStopButtonState();
                return;
            }

            if (videoTimer.Enabled)
            {
                videoTimer.Stop();
            }
            else
            {
                if (currentSlideIndex >= slideImages.Count - 1)
                {
                    currentSlideIndex = 0;
                    UpdateSlideDisplay();
                }

                videoTimer.Start();
            }

            UpdatePlayStopButtonState();
        }

        private void VideoTimer_Tick(object sender, EventArgs e)
        {
            if (currentSlideIndex >= slideImages.Count - 1)
            {
                videoTimer.Stop();
                UpdatePlayStopButtonState();
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

            List<string> intervalTargets = GetIntervalImageFiles();

            if (intervalTargets.Count > 0)
            {
                targets.AddRange(intervalTargets);
            }
            else if (lstviewFileListD.SelectedItems.Count > 0)
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

            targets = targets
                .Where(path => File.Exists(path) || Directory.Exists(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (targets.Count == 0) return;

            if (videoTimer.Enabled) videoTimer.Stop();
            UpdatePlayStopButtonState();
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

            ResetSelectedInterval();
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
            string uploadFolder = GetUploadedFolder();

            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "저장할 새 폴더 이름을 입력하세요.";
                dialog.InitialDirectory = GetBinFolder();
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
            lstviewFileListD.Visible = false;
            lstviewTrash.Visible = false;
            lstviewMain.Visible = true;

            SetListViewName("");
            btnRestoration.Visible = false;
        }

        private void lstviewMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstviewMain.SelectedItems.Count == 0)
            {
                return;
            }

            string itemTag = lstviewMain.SelectedItems[0].Tag?.ToString();

            if (itemTag == "파일목록")
            {
                lstviewMain.Visible = false;
                lstviewFileListD.Visible = true;
                lstviewTrash.Visible = false;

                SetListViewName("[파일목록]");
                btnRestoration.Visible = false;
            }
            else if (itemTag == "휴지통")
            {
                lstviewMain.Visible = false;
                lstviewFileListD.Visible = false;
                lstviewTrash.Visible = true;

                SetListViewName("[휴지통]");
                btnRestoration.Visible = true;

                LoadTrashCanFiles();
            }
            else if (itemTag == "파일추가")
            {
                frmAddFile addFileForm = new frmAddFile(this);
                addFileForm.ShowDialog();
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e) { }
        private void pnlContrastProperty_Paint(object sender, PaintEventArgs e) { }
        private void pnlCloseProperty_Paint(object sender, PaintEventArgs e) { }
        private void GBPalete_Enter(object sender, EventArgs e) { }
        private void ColorTrackBar_Scroll(object sender, EventArgs e) { }
        private class NaturalFileNameComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                MatchCollection xParts = Regex.Matches(x, @"\d+|\D+");
                MatchCollection yParts = Regex.Matches(y, @"\d+|\D+");

                int count = Math.Min(xParts.Count, yParts.Count);

                for (int i = 0; i < count; i++)
                {
                    string xPart = xParts[i].Value;
                    string yPart = yParts[i].Value;

                    bool xIsNumber = long.TryParse(xPart, out long xNumber);
                    bool yIsNumber = long.TryParse(yPart, out long yNumber);

                    int result;

                    if (xIsNumber && yIsNumber)
                    {
                        result = xNumber.CompareTo(yNumber);

                        if (result == 0)
                        {
                            result = xPart.Length.CompareTo(yPart.Length);
                        }
                    }
                    else
                    {
                        result = string.Compare(xPart, yPart, StringComparison.CurrentCultureIgnoreCase);
                    }

                    if (result != 0)
                    {
                        return result;
                    }
                }

                return xParts.Count.CompareTo(yParts.Count);
            }
        }
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

        private void lstviewFileListD_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

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
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image OverlayIcon { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowOverlayIcon { get; set; }

        public DoubleBufferedPictureBox()
        {
            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
            this.UpdateStyles();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if (!ShowOverlayIcon || OverlayIcon == null)
            {
                return;
            }

            int size = Math.Max(1, Math.Min(this.ClientSize.Width, this.ClientSize.Height) / 4);
            size = Math.Min(size, 120);

            int x = (this.ClientSize.Width - size) / 2;
            int y = (this.ClientSize.Height - size) / 2;

            pe.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            pe.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            pe.Graphics.DrawImage(OverlayIcon, new Rectangle(x, y, size, size));
        }
    }
}
