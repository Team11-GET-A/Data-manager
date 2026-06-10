using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Data_Manager
{
    // Pilot 화면입니다.
    // 학습된 모델을 선택하고 tub 데이터를 연결한 뒤,
    // 실제 주행값과 AI 추론값(angle/throttle)을 프레임 단위로 비교합니다.
    public partial class Pliot : Form
    {
        private const int OverlayAlpha = 120;
        private static readonly Color OverlayBackColor = Color.FromArgb(OverlayAlpha, 22, 26, 32);
        private static readonly Color PilotBackColor = Color.White;
        private static readonly Color PilotPanelColor = Color.White;
        private static readonly Color PilotSurfaceColor = Color.White;
        private static readonly Color PilotSurfaceLightColor = Color.FromArgb(245, 247, 250);
        private static readonly Color PilotBorderColor = Color.FromArgb(205, 214, 225);
        private static readonly Color PilotTextColor = Color.FromArgb(30, 39, 50);
        private static readonly Color PilotMutedTextColor = Color.FromArgb(92, 105, 122);
        private static readonly Color PilotBlueColor = Color.FromArgb(62, 150, 255);
        private static readonly Color PilotCyanColor = Color.FromArgb(44, 205, 220);
        private static readonly Color PilotGreenColor = Color.FromArgb(65, 190, 125);
        private static readonly Color PilotOrangeColor = Color.FromArgb(255, 168, 72);
        private const int PilotBaseClientWidth = 1600;
        private const int PilotBaseClientHeight = 900;
        private const int PilotBaseSplitWidth = 1584;
        private const int PilotBaseSplitDistance = 404;
        private const int PilotBaseSplitHeight = 884;
        private const int PilotBaseLeftPanelWidth = 404;
        private const int PilotBaseCardWidth = 1168;
        private const int PilotBaseCardHeight = 884;

        private readonly List<ModelListItem> _models = new List<ModelListItem>();

        // 현재 선택 모델에 연결된 tub/AI 추론 결과를 프레임 단위로 합친 목록입니다.
        private readonly List<DonkeyAsyncWorker.PilotFrameData> _frameList =
            new List<DonkeyAsyncWorker.PilotFrameData>();

        private ModelListItem? _selectedModel;
        private DonkeyAsyncWorker.PilotCardState? _cardState;
        private CancellationTokenSource? _loadCts;
        private System.Windows.Forms.Timer? _playbackTimer;

        private int _currentFrameIndex;
        private bool _isUpdatingTrackBar;
        private bool _isPlaying;
        private bool _isReversePlaying;
        private bool _isChartOpen;
        private bool _isSyncingModels;
        private bool _pendingModelSync;
        private double _playbackSpeed = 1.0;
        private string _currentImagePath = "";
        private string _myCarModelsDirectory = "";
        private string _lastPilotModelSignature = "";
        private Size _currentImageRenderSize = Size.Empty;
        private Size _lastOverlayHostSize = Size.Empty;
        private bool _isApplyingPilotLayout;

        public Pliot()
        {
            InitializeComponent();
            InitializePilotUi();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys keyCode = keyData & Keys.KeyCode;
            bool shiftPressed = (keyData & Keys.Shift) == Keys.Shift;

            if (IsTextInputFocused())
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            if (keyCode == Keys.Space)
            {
                btnPlayPause.PerformClick();
                return true;
            }

            if (keyCode == Keys.Escape)
            {
                StopPlayback();
                return true;
            }

            if (keyCode == Keys.Right)
            {
                MoveToFrame(_currentFrameIndex + (shiftPressed ? 5 : 1));
                return true;
            }

            if (keyCode == Keys.Left)
            {
                if ((keyData & Keys.Control) == Keys.Control)
                {
                    btnReversePlay.PerformClick();
                }
                else
                {
                    MoveToFrame(_currentFrameIndex - (shiftPressed ? 5 : 1));
                }

                return true;
            }

            if (keyData == Keys.Enter && lvModelList.ContainsFocus)
            {
                SelectFocusedModelFromShortcut();
                return true;
            }

            if (keyData == (Keys.Control | Keys.I))
            {
                btnImportModel.PerformClick();
                return true;
            }

            if (keyData == (Keys.Control | Keys.T))
            {
                btnTubInput.PerformClick();
                return true;
            }

            if (keyData == (Keys.Control | Keys.G))
            {
                btnPilotChart.PerformClick();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async void SelectFocusedModelFromShortcut()
        {
            if (!TryGetSelectedModel(out ModelListItem? model))
            {
                return;
            }

            await SelectModelAsync(model!);
        }

        private bool IsTextInputFocused()
        {
            return IsTextInputFocusedRecursive(this);
        }

        private bool IsTextInputFocusedRecursive(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control.Focused &&
                    (control is TextBoxBase || control is ComboBox))
                {
                    return true;
                }

                if (control.HasChildren &&
                    IsTextInputFocusedRecursive(control))
                {
                    return true;
                }
            }

            return false;
        }

        #region Initialization

        private void InitializePilotUi()
        {
            // Designer 파일은 배치만 담당하고, 이벤트 연결과 초기 화면 상태는 여기서 모읍니다.
            cmbSpeed.SelectedIndex = 1;
            ApplyOverlayStyles();
            ConfigurePlaybackTimer();
            ApplyPilotDesign();
            ConfigurePilotCardRegions();
            btnImportModel.BringToFront();

            // 이벤트 연결은 이 메서드에 모아 둡니다.
            // 디자이너 파일은 컨트롤 생성/기본 배치만 담당하게 유지하기 위한 규칙입니다.
            btnImportModel.Click += BtnImportModel_Click;
            lvModelList.SelectedIndexChanged += LvModelList_SelectedIndexChanged;
            lvModelList.SizeChanged += (s, e) => ResizeModelColumns();
            EnableDoubleBuffering(lvModelList);

            btnTubInput.Click += BtnTubInput_Click;
            btnPilotChart.Click += BtnPilotChart_Click;
            trbLocation.ValueChanged += trbLocation_ValueChanged;
            trbLocation.MouseDown += TrbLocation_MouseDown;
            btnJumpPrev5.Click += (s, e) => MoveToFrame(_currentFrameIndex - 5);
            btnPrevImage.Click += (s, e) => MoveToFrame(_currentFrameIndex - 1);
            btnNextImage.Click += (s, e) => MoveToFrame(_currentFrameIndex + 1);
            btnJumpNext5.Click += (s, e) => MoveToFrame(_currentFrameIndex + 5);
            btnPlayPause.Click += BtnPlayPause_Click;
            btnReversePlay.Click += BtnReversePlay_Click;
            cmbSpeed.SelectedIndexChanged += CmbSpeed_SelectedIndexChanged;

            pnlImageHost.Resize += (s, e) =>
            {
                _currentImageRenderSize = Size.Empty;
                _lastOverlayHostSize = Size.Empty;
                PositionImageOverlays();
            };
            picPilotImage.Resize += (s, e) =>
            {
                _currentImageRenderSize = Size.Empty;
                _lastOverlayHostSize = Size.Empty;
                PositionImageOverlays();
            };
            pliotAngleIndicator.Resize += (s, e) => ConfigureAngleOverlayLayout();
            Resize += (s, e) => ApplyPilotResponsiveLayout();
            Shown += (s, e) => ApplyPilotResponsiveLayout();
            Load += async (s, e) => await InitializePilotDataSafelyAsync();
            FormClosed += Pliot_FormClosed;
            SharedModelRegistry.ModelsChanged +=
                SharedModelRegistry_ModelsChanged;

            ResizeModelColumns();
            _ = SyncModelsFromSharedRegistryAsync();
            ConfigureLocationTrackBar();
            ClearModelLabels();
            ConfigurePilotValueControls();
            DrawTubRequiredMessage();
            ConfigureAngleOverlayLayout();
            EnsureImageOverlayParent();
            ApplyPilotResponsiveLayout();
            picPilotImage.SendToBack();
            pnlImageIndexOverlay.BringToFront();
            pliotAiThrottleGauge.BringToFront();
            pliotTubThrottleGauge.BringToFront();
            pliotAngleIndicator.BringToFront();
        }

        private async Task InitializePilotDataSafelyAsync()
        {
            try
            {
                await RefreshMyCarModelsDirectoryAsync(createIfMissing: false);
                await SyncModelsFromSharedRegistryAsync();
            }
            catch (Exception ex)
            {
                ReportPilotException(ex);
            }

            if (!IsDisposed)
            {
                ApplyPilotResponsiveLayout();
            }
        }

        private static void ReportPilotException(Exception ex)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            catch
            {
            }
        }

        private void ApplyOverlayStyles()
        {
            pnlImageIndexOverlay.BackColor = OverlayBackColor;
            lblImageIndexOverlay.BackColor = Color.Transparent;
        }

        private void ConfigurePilotValueControls()
        {
            pliotAiThrottleGauge.GaugeTitle = "AI";
            pliotAiThrottleGauge.Mirrored = true;
            pliotAiThrottleGauge.FillColor = Color.FromArgb(255, 55, 145, 255);

            pliotTubThrottleGauge.GaugeTitle = "사람";
            pliotTubThrottleGauge.Mirrored = true;
            pliotTubThrottleGauge.FillColor = Color.FromArgb(255, 255, 92, 76);
        }

        private void ConfigurePlaybackTimer()
        {
            _playbackTimer = new System.Windows.Forms.Timer();
            _playbackTimer.Interval = GetPlaybackInterval();
            _playbackTimer.Tick += PlaybackTimer_Tick;
        }

        private void EnableDoubleBuffering(Control control)
        {
            try
            {
                typeof(Control)
                    .GetProperty(
                        "DoubleBuffered",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(control, true, null);
            }
            catch
            {
            }
        }

        private void ApplyPilotDesign()
        {
            Font = new Font("맑은 고딕", 10.5F, FontStyle.Regular);
            Text = "파일럿";
            BackColor = PilotBackColor;
            MinimumSize = new Size(900, 520);

            splitMain.BackColor = PilotBackColor;
            pnlLeft.BackColor = PilotBackColor;
            pnlLeft.Padding = new Padding(8);
            pnlRight.BackColor = PilotBackColor;
            pnlRight.Padding = new Padding(8, 0, 0, 0);
            pnlPilotCard.BackColor = PilotPanelColor;
            pnlPilotCard.Padding = new Padding(14);
            pnlLeft.BorderStyle = BorderStyle.FixedSingle;
            pnlPilotCard.BorderStyle = BorderStyle.FixedSingle;
            pnlPilotHeader.BackColor = PilotPanelColor;
            pnlPlaybackControls.BackColor = PilotPanelColor;
            pnlPlaybackControls.BorderStyle = BorderStyle.FixedSingle;
            pnlPlaybackControls.Padding = new Padding(8, 4, 8, 4);
            pnlTrackBar.BackColor = PilotPanelColor;
            pnlTrackBar.BorderStyle = BorderStyle.FixedSingle;
            pnlTrackBar.Padding = new Padding(8, 4, 8, 4);
            pnlImageHost.BackColor = PilotBackColor;
            pnlImageHost.BorderStyle = BorderStyle.FixedSingle;
            pnlImageHost.Padding = new Padding(4);
            picPilotImage.BackColor = PilotBackColor;

            grpSelectedModel.Text = "선택한 모델 정보";
            grpSelectedModel.ForeColor = PilotTextColor;
            grpSelectedModel.BackColor = PilotPanelColor;
            grpSelectedModel.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);

            lblSelectedModelNameTitle.Text = "모델명";
            lblSelectedModelPathTitle.Text = "파일 경로";
            lblSelectedModelTypeTitle.Text = "타입";
            lblSelectedTubPathTitle.Text = "주행데이터 경로";

            foreach (Label label in new[]
            {
                lblSelectedModelNameTitle,
                lblSelectedModelPathTitle,
                lblSelectedModelTypeTitle,
                lblSelectedTubPathTitle
            })
            {
                label.Font = new Font("맑은 고딕", 9.5F, FontStyle.Bold);
                label.ForeColor = PilotMutedTextColor;
            }

            foreach (Label label in new[]
            {
                lblSelectedModelName,
                lblSelectedModelPath,
                lblSelectedModelType,
                lblSelectedTubPath,
                lblTubPathValue
            })
            {
                label.Font = new Font("맑은 고딕", 9.5F, FontStyle.Regular);
                label.ForeColor = PilotTextColor;
            }

            lblTubPathTitle.Text = "선택 모델";
            lblTubPathTitle.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            lblTubPathTitle.ForeColor = PilotMutedTextColor;

            lblPilotShortcutGuide.Text =
                "Space 재생/일시정지 | Esc 정지\r\n" +
                "←/→ 1프레임 | Shift+←/→ 5프레임\r\n" +
                "Enter 모델 로드 | Ctrl+I 모델 가져오기\r\n" +
                "Ctrl+T 주행데이터 입력 | Ctrl+G 그래프 | Ctrl+J AI 판단";
            lblPilotShortcutGuide.Font = new Font("맑은 고딕", 8.5F, FontStyle.Regular);
            lblPilotShortcutGuide.ForeColor = PilotMutedTextColor;
            lblPilotShortcutGuide.BackColor = PilotPanelColor;

            StyleModelList();
            StyleComboBox();
            StylePilotButton(btnImportModel, PilotBlueColor, Color.White);
            StylePilotButton(btnTubInput, PilotCyanColor, Color.FromArgb(10, 24, 32));
            StylePilotButton(btnPilotChart, PilotGreenColor, Color.FromArgb(9, 30, 20));
            StylePlaybackButton(btnJumpPrev5);
            StylePlaybackButton(btnPrevImage);
            StylePlaybackButton(btnPlayPause);
            StylePlaybackButton(btnReversePlay);
            StylePlaybackButton(btnNextImage);
            StylePlaybackButton(btnJumpNext5);
            UpdatePlaybackButtonImages();
        }

        private void StyleModelList()
        {
            lvModelList.BackColor = PilotSurfaceColor;
            lvModelList.ForeColor = PilotTextColor;
            lvModelList.Font = new Font("맑은 고딕", 9.5F, FontStyle.Regular);
            lvModelList.BorderStyle = BorderStyle.FixedSingle;
            lvModelList.GridLines = true;
            colModelNo.Text = "번호";
            colModelName.Text = "모델 이름";
            colModelPath.Text = "경로";
        }

        private void StyleComboBox()
        {
            cmbSpeed.BackColor = PilotSurfaceLightColor;
            cmbSpeed.ForeColor = PilotTextColor;
            cmbSpeed.FlatStyle = FlatStyle.Flat;
            cmbSpeed.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
        }

        private void StylePlaybackButton(Button button)
        {
            button.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            StylePilotButton(button, PilotSurfaceLightColor, PilotTextColor);
        }

        private void StylePilotButton(Button button, Color backColor, Color foreColor)
        {
            button.UseVisualStyleBackColor = false;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(
                Math.Min(255, backColor.R + 34),
                Math.Min(255, backColor.G + 34),
                Math.Min(255, backColor.B + 34));
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.MouseEnter += (s, e) =>
            {
                button.BackColor = LightenColor(backColor, 18);
            };
            button.MouseLeave += (s, e) =>
            {
                button.BackColor = backColor;
            };
        }

        private void ConfigurePilotCardRegions()
        {
            splitMain.Panel1MinSize = 1;
            splitMain.Panel2MinSize = 1;

            splitMain.Dock = DockStyle.None;
            splitMain.Anchor = AnchorStyles.None;
            pnlLeft.Dock = DockStyle.None;
            pnlLeft.Anchor = AnchorStyles.None;
            pnlRight.Dock = DockStyle.None;
            pnlRight.Anchor = AnchorStyles.None;
            pnlPilotCard.Dock = DockStyle.None;
            pnlPilotCard.Anchor = AnchorStyles.None;
            btnImportModel.Dock = DockStyle.None;
            btnImportModel.Anchor = AnchorStyles.None;
            lvModelList.Dock = DockStyle.None;
            lvModelList.Anchor = AnchorStyles.None;
            lblPilotShortcutGuide.Dock = DockStyle.None;
            lblPilotShortcutGuide.Anchor = AnchorStyles.None;
            grpSelectedModel.Dock = DockStyle.None;
            grpSelectedModel.Anchor = AnchorStyles.None;
            tblSelectedModel.Dock = DockStyle.None;
            tblSelectedModel.Anchor = AnchorStyles.None;
            pnlPilotHeader.Dock = DockStyle.None;
            pnlPilotHeader.Anchor = AnchorStyles.None;
            pnlImageHost.Dock = DockStyle.None;
            pnlImageHost.Anchor = AnchorStyles.None;
            pnlTrackBar.Dock = DockStyle.None;
            pnlTrackBar.Anchor = AnchorStyles.None;
            pnlPlaybackControls.Dock = DockStyle.None;
            pnlPlaybackControls.Anchor = AnchorStyles.None;
            picPilotImage.Dock = DockStyle.Fill;

            trbLocation.Anchor = AnchorStyles.None;
            btnJumpPrev5.Anchor = AnchorStyles.None;
            btnPrevImage.Anchor = AnchorStyles.None;
            btnPlayPause.Anchor = AnchorStyles.None;
            cmbSpeed.Anchor = AnchorStyles.None;
            btnReversePlay.Anchor = AnchorStyles.None;
            btnNextImage.Anchor = AnchorStyles.None;
            btnJumpNext5.Anchor = AnchorStyles.None;
            btnPilotChart.Anchor = AnchorStyles.None;
            btnTubInput.Anchor = AnchorStyles.None;
            lblTubPathValue.Anchor = AnchorStyles.None;
        }

        private int ScaleFromBaseX(int value)
        {
            double scale = ClientSize.Width <= 0
                ? 1.0
                : ClientSize.Width / (double)PilotBaseClientWidth;

            return Math.Max(1, (int)Math.Round(value * scale));
        }

        private int ScaleFromBaseY(int value)
        {
            double scale = ClientSize.Height <= 0
                ? 1.0
                : ClientSize.Height / (double)PilotBaseClientHeight;

            return Math.Max(1, (int)Math.Round(value * scale));
        }

        private static Color LightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, color.R + amount),
                Math.Min(255, color.G + amount),
                Math.Min(255, color.B + amount));
        }

        #endregion

        #region Model Discovery

        private async void BtnModelLoad_Click(object? sender, EventArgs e)
        {
            try
            {
            using FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "모델 파일이 들어 있는 폴더 선택";
            dialog.ShowNewFolderButton = false;

            string initialDirectory = await GetModelFolderInitialDirectoryAsync();
            if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
            {
                dialog.SelectedPath = initialDirectory;
            }

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            int addedCount = AddModelsFromFolder(dialog.SelectedPath);
            if (addedCount == 0)
            {
                MessageBox.Show(
                    "선택한 폴더에서 모델 파일을 찾지 못했습니다.",
                    "모델 파일",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (lvModelList.SelectedItems.Count == 0 && lvModelList.Items.Count > 0)
            {
                lvModelList.Items[^1].Selected = true;
                lvModelList.Items[^1].Focused = true;
            }
            }
            catch (Exception ex)
            {
                ReportPilotException(ex);
                MessageBox.Show(ex.Message, "Pilot", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static async Task<string> GetModelFolderInitialDirectoryAsync()
        {
            try
            {
            // Ubuntu-22.04의 mycar 폴더를 찾으면 모델 선택 시작 위치로 사용합니다.
            // WSL/mycar를 찾지 못하면 현재 실행 폴더로 되돌립니다.
            string currentDirectory = Environment.CurrentDirectory;
            string distroName = await DonkeyAsyncWorker.GetPreferredWslDistroNameAsync(CancellationToken.None);
            DonkeyAsyncWorker.OperationResult<string> myCarResult =
                await DonkeyAsyncWorker.FindMyCarPathInWslAsync(
                    distroName,
                    null,
                    CancellationToken.None);

            if (myCarResult.Success && !string.IsNullOrWhiteSpace(myCarResult.Data))
            {
                string windowsMyCarPath = DonkeyAsyncWorker.ToWindowsPathFromWslPath(
                    myCarResult.Data,
                    distroName);
                if (Directory.Exists(windowsMyCarPath))
                {
                    return windowsMyCarPath;
                }
            }

            return Directory.Exists(currentDirectory)
                ? currentDirectory
                : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            catch (Exception ex)
            {
                ReportPilotException(ex);
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
        }

        public Task ReceiveSelectedModelFromPilotModelListAsync(string modelName, string modelPath)
        {
            AddOrSelectModel(modelName, modelPath);
            if (_selectedModel == null)
            {
                return Task.CompletedTask;
            }

            return SelectModelAsync(_selectedModel);
        }

        private int AddModelsFromFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                return 0;
            }

            string[] files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(IsSupportedModelFile)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            int addedCount = 0;
            lvModelList.BeginUpdate();
            try
            {
                foreach (string file in files)
                {
                    if (AddModel(file))
                    {
                        addedCount++;
                    }
                }
            }
            finally
            {
                lvModelList.EndUpdate();
            }

            ResizeModelColumns();
            return addedCount;
        }

        private static bool IsSupportedModelFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return string.Equals(extension, ".h5", StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Overlay Setup

        private void ConfigureAngleOverlayLayout()
        {
            // Paint로 그리는 앵글 선이 라벨 배치에 잘리지 않도록 고정 크기 오버레이로 유지합니다.
            pliotAngleIndicator.Anchor = AnchorStyles.None;
        }

        private void EnsureImageOverlayParent()
        {
            if (IsDisposed || picPilotImage.IsDisposed)
            {
                return;
            }

            // PictureBox child controls can show the current image through transparent overlay backgrounds.
            MoveOverlayToPictureBox(pnlImageIndexOverlay);
            MoveOverlayToPictureBox(pliotAiThrottleGauge);
            MoveOverlayToPictureBox(pliotTubThrottleGauge);
            MoveOverlayToPictureBox(pliotAngleIndicator);
        }

        private void MoveOverlayToPictureBox(Control overlay)
        {
            if (overlay.IsDisposed || picPilotImage.IsDisposed)
            {
                return;
            }

            if (overlay.Parent == picPilotImage)
            {
                overlay.Visible = true;
                overlay.BackColor =
                    ReferenceEquals(overlay, pnlImageIndexOverlay)
                        ? OverlayBackColor
                        : Color.Transparent;
                overlay.BringToFront();
                return;
            }

            Point location = overlay.Location;
            overlay.Parent?.Controls.Remove(overlay);
            picPilotImage.Controls.Add(overlay);
            overlay.Location = location;
            overlay.Visible = true;
            overlay.BackColor =
                ReferenceEquals(overlay, pnlImageIndexOverlay)
                    ? OverlayBackColor
                    : Color.Transparent;
            overlay.BringToFront();
        }

        #endregion

        #region Model Selection

        private void AddOrSelectModel(string modelName, string modelPath)
        {
            ModelListItem? existing = _models.FirstOrDefault(model =>
                string.Equals(model.Path, modelPath, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                existing =
                    AddModelToList(
                        modelName,
                        modelPath);
            }

            foreach (ListViewItem item in lvModelList.Items)
            {
                item.Selected = ReferenceEquals(item.Tag, existing);
                item.Focused = item.Selected;
            }

            _selectedModel = existing;
            UpsertSharedModel(modelName, modelPath);
            ResizeModelColumns();
        }

        private async void BtnImportModel_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "가져올 AI 모델 선택";
            dialog.Filter = "H5 모델 (*.h5)|*.h5";
            dialog.Multiselect = false;

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string sourceModelPath = Path.GetFullPath(dialog.FileName);

            try
            {
                string modelsDirectory =
                    await RefreshMyCarModelsDirectoryAsync(createIfMissing: true);

                if (string.IsNullOrWhiteSpace(modelsDirectory))
                {
                    MessageBox.Show("mycar/models 폴더를 확인하지 못했습니다.");
                    return;
                }

                ModelImportResult importResult =
                    ModelImportService.ImportModelToFolder(
                        sourceModelPath,
                        modelsDirectory);

                AddOrSelectModel(
                    Path.GetFileNameWithoutExtension(importResult.DestinationModelPath),
                    importResult.DestinationModelPath);

                await SyncModelsFromSharedRegistryAsync();

                MessageBox.Show("모델을 가져왔습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "모델 가져오기 중 오류가 발생했습니다.\n" +
                    ex.Message);
            }
        }

        private bool AddModel(string modelPath)
        {
            string modelName = Path.GetFileNameWithoutExtension(modelPath);
            if (_models.Any(model =>
                string.Equals(model.Path, modelPath, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            AddModelToList(
                modelName,
                modelPath);

            UpsertSharedModel(
                modelName,
                modelPath);

            return true;
        }

        private ModelListItem AddModelToList(
            string modelName,
            string modelPath)
        {
            ModelListItem model =
                new ModelListItem(
                    modelName,
                    modelPath);

            _models.Add(model);

            ListViewItem item = new ListViewItem(_models.Count.ToString());
            item.SubItems.Add(modelName);
            item.SubItems.Add(modelPath);
            item.Tag = model;
            lvModelList.Items.Add(item);

            return model;
        }

        private void UpsertSharedModel(
            string modelName,
            string modelPath)
        {
            if (
                string.IsNullOrWhiteSpace(modelPath) ||
                !File.Exists(modelPath))
            {
                return;
            }

            SharedModelRegistry.Upsert(
                new SharedModelRegistryEntry()
                {
                    Name = Path.GetFileName(modelPath),
                    WindowsPath = modelPath,
                    CreatedAt = File.GetCreationTime(modelPath)
                });
        }

        private void SharedModelRegistry_ModelsChanged(
            object? sender,
            EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            try
            {
                if (!IsHandleCreated)
                {
                    return;
                }

                BeginInvoke(
                    new Action(
                        () =>
                        {
                            if (!IsDisposed)
                            {
                                _ = SyncModelsFromSharedRegistryAsync();
                            }
                        }));
            }
            catch (ObjectDisposedException ex)
            {
                ReportPilotException(ex);
            }
            catch (InvalidOperationException ex)
            {
                ReportPilotException(ex);
            }
        }

        private void SyncModelsFromSharedRegistry()
        {
            _ = SyncModelsFromSharedRegistryAsync();
        }

        private async Task SyncModelsFromSharedRegistryAsync()
        {
            if (_isSyncingModels)
            {
                _pendingModelSync = true;
                return;
            }

            _isSyncingModels = true;

            try
            {
            string modelsDirectory =
                await RefreshMyCarModelsDirectoryAsync(createIfMissing: false);

            if (string.IsNullOrWhiteSpace(modelsDirectory) ||
                !Directory.Exists(modelsDirectory))
            {
                return;
            }

            List<SharedModelRegistryEntry> sharedModels =
                BuildPilotModelEntries(modelsDirectory);

            string newSignature =
                BuildPilotModelSignature(sharedModels);

            if (string.Equals(
                _lastPilotModelSignature,
                newSignature,
                StringComparison.Ordinal))
            {
                return;
            }

            _lastPilotModelSignature = newSignature;

            HashSet<string> sharedPaths =
                sharedModels
                    .Select(model => model.WindowsPath)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

            lvModelList.BeginUpdate();
            try
            {
                for (int i = _models.Count - 1; i >= 0; i--)
                {
                    if (sharedPaths.Contains(_models[i].Path))
                    {
                        continue;
                    }

                    bool wasSelected =
                        ReferenceEquals(_selectedModel, _models[i]);

                    _models.RemoveAt(i);
                    lvModelList.Items.RemoveAt(i);

                    if (wasSelected)
                    {
                        _selectedModel = null;
                        ClearModelLabels();
                        _frameList.Clear();
                        ConfigureLocationTrackBar();
                        DrawTubRequiredMessage();
                    }
                }

                foreach (SharedModelRegistryEntry sharedModel in sharedModels)
                {
                    bool exists =
                        _models.Any(
                            model =>
                                string.Equals(
                                    model.Path,
                                    sharedModel.WindowsPath,
                                    StringComparison.OrdinalIgnoreCase));

                    if (exists)
                    {
                        continue;
                    }

                    AddModelToList(
                        Path.GetFileNameWithoutExtension(
                            sharedModel.WindowsPath),
                        sharedModel.WindowsPath);
                }

                RenumberModelListItems();
            }
            finally
            {
                lvModelList.EndUpdate();
            }

            ResizeModelColumns();
            }
            catch (Exception ex)
            {
                ReportPilotException(ex);
            }
            finally
            {
                _isSyncingModels = false;

                if (_pendingModelSync)
                {
                    _pendingModelSync = false;
                    _ = SyncModelsFromSharedRegistryAsync();
                }
            }
        }

        private string BuildPilotModelSignature(List<SharedModelRegistryEntry> entries)
        {
            return string.Join(
                "|",
                entries
                    .OrderBy(entry => entry.WindowsPath, StringComparer.OrdinalIgnoreCase)
                    .Select(entry =>
                        entry.Name +
                        ":" +
                        entry.WindowsPath +
                        ":" +
                        entry.IsDeleted));
        }

        private List<SharedModelRegistryEntry> BuildPilotModelEntries(string modelsDirectory)
        {
            List<SharedModelRegistryEntry> entries =
                SharedModelRegistry.Load()
                    .Where(entry =>
                        ModelImportService.IsPathInsideDirectory(
                            entry.WindowsPath,
                            modelsDirectory))
                    .ToList();

            foreach (string modelFile in Directory.GetFiles(modelsDirectory, "*.h5", SearchOption.TopDirectoryOnly))
            {
                string fullPath = Path.GetFullPath(modelFile);

                if (entries.Any(entry =>
                    string.Equals(entry.WindowsPath, fullPath, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                entries.Add(
                    new SharedModelRegistryEntry
                    {
                        Name = Path.GetFileName(fullPath),
                        WindowsPath = fullPath,
                        CreatedAt = File.GetCreationTime(fullPath)
                    });
            }

            return entries
                .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<string> RefreshMyCarModelsDirectoryAsync(bool createIfMissing)
        {
            try
            {
            if (!string.IsNullOrWhiteSpace(_myCarModelsDirectory) &&
                Directory.Exists(_myCarModelsDirectory))
            {
                return _myCarModelsDirectory;
            }

            string distroName =
                await DonkeyAsyncWorker.GetPreferredWslDistroNameAsync(CancellationToken.None);

            DonkeyAsyncWorker.OperationResult<string> myCarResult =
                await DonkeyAsyncWorker.FindMyCarPathInWslAsync(
                    distroName,
                    null,
                    CancellationToken.None);

            if (!myCarResult.Success || string.IsNullOrWhiteSpace(myCarResult.Data))
            {
                return "";
            }

            string windowsMyCarPath =
                DonkeyAsyncWorker.ToWindowsPathFromWslPath(
                    myCarResult.Data,
                    distroName);

            string modelsDirectory =
                Path.Combine(
                    windowsMyCarPath,
                    "models");

            if (createIfMissing)
            {
                Directory.CreateDirectory(modelsDirectory);
            }

            if (Directory.Exists(modelsDirectory))
            {
                _myCarModelsDirectory = modelsDirectory;
            }

            return _myCarModelsDirectory;
            }
            catch (Exception ex)
            {
                ReportPilotException(ex);
                return "";
            }
        }

        private void ApplyPilotResponsiveLayout()
        {
            // 1600x900 디자이너 기준 좌표를 현재 폼 크기에 맞춰 다시 계산합니다.
            // Dock/Anchor와 수동 배치를 섞으면 이중 스케일이 생기므로,
            // Pilot 화면은 이 메서드와 하위 Layout* 메서드에서만 크기와 위치를 조정합니다.
            if (_isApplyingPilotLayout ||
                IsDisposed ||
                ClientSize.Width <= 0 ||
                ClientSize.Height <= 0)
            {
                return;
            }

            _isApplyingPilotLayout = true;
            try
            {
                SuspendLayout();
                splitMain.SuspendLayout();
                splitMain.Panel1.SuspendLayout();
                splitMain.Panel2.SuspendLayout();
                pnlLeft.SuspendLayout();
                pnlRight.SuspendLayout();
                pnlPilotCard.SuspendLayout();

                LayoutPilotShell();
                ApplyPilotSplitRatio();
                LayoutPilotSplitPanels();
                LayoutPilotLeftPanel();
                LayoutPilotCard();
                ResizeModelColumns();

                _lastOverlayHostSize = Size.Empty;
                PositionImageOverlays();
            }
            finally
            {
                pnlPilotCard.ResumeLayout(false);
                pnlRight.ResumeLayout(false);
                pnlLeft.ResumeLayout(false);
                splitMain.Panel2.ResumeLayout(false);
                splitMain.Panel1.ResumeLayout(false);
                splitMain.ResumeLayout(false);
                ResumeLayout(false);
                _isApplyingPilotLayout = false;
            }
        }

        private double PilotScaleX =>
            Math.Max(0.1, ClientSize.Width / (double)PilotBaseClientWidth);

        private double PilotScaleY =>
            Math.Max(0.1, ClientSize.Height / (double)PilotBaseClientHeight);

        private double PilotUniformScale =>
            Math.Max(0.1, Math.Min(PilotScaleX, PilotScaleY));

        private int ScalePilotX(int value) =>
            Math.Max(1, (int)Math.Round(value * PilotScaleX));

        private int ScalePilotY(int value) =>
            Math.Max(1, (int)Math.Round(value * PilotScaleY));

        private int ScalePilotUniform(int value) =>
            Math.Max(1, (int)Math.Round(value * PilotUniformScale));

        private void LayoutPilotShell()
        {
            splitMain.Panel1MinSize = 1;
            splitMain.Panel2MinSize = 1;

            int margin = Math.Max(4, ScalePilotUniform(8));
            int width = Math.Max(1, ClientSize.Width - margin * 2);
            int height = Math.Max(1, ClientSize.Height - margin * 2);

            splitMain.SetBounds(margin, margin, width, height);
        }

        private void ApplyPilotSplitRatio()
        {
            int availableWidth =
                Math.Max(1, splitMain.Width - splitMain.SplitterWidth);

            int panel1Min =
                Math.Min(
                    ScalePilotX(180),
                    Math.Max(1, availableWidth / 2));
            int panel2Min =
                Math.Min(
                    ScalePilotX(260),
                    Math.Max(1, availableWidth - panel1Min));

            if (panel1Min + panel2Min > availableWidth)
            {
                panel2Min = Math.Max(1, availableWidth - panel1Min);
            }

            splitMain.Panel1MinSize = panel1Min;
            splitMain.Panel2MinSize = panel2Min;

            int targetDistance =
                (int)Math.Round(
                    splitMain.Width *
                    (PilotBaseSplitDistance / (double)PilotBaseSplitWidth));
            int minDistance = splitMain.Panel1MinSize;
            int maxDistance =
                splitMain.Width -
                splitMain.Panel2MinSize -
                splitMain.SplitterWidth;

            if (maxDistance < minDistance)
            {
                return;
            }

            targetDistance = ClampInt(targetDistance, minDistance, maxDistance);

            if (splitMain.SplitterDistance != targetDistance)
            {
                splitMain.SplitterDistance = targetDistance;
            }
        }

        private void LayoutPilotSplitPanels()
        {
            pnlLeft.SetBounds(
                0,
                0,
                Math.Max(1, splitMain.Panel1.ClientSize.Width),
                Math.Max(1, splitMain.Panel1.ClientSize.Height));

            pnlRight.SetBounds(
                0,
                0,
                Math.Max(1, splitMain.Panel2.ClientSize.Width),
                Math.Max(1, splitMain.Panel2.ClientSize.Height));

            int rightGap = Math.Max(0, ScalePilotX(8));
            pnlPilotCard.SetBounds(
                rightGap,
                0,
                Math.Max(1, pnlRight.ClientSize.Width - rightGap),
                Math.Max(1, pnlRight.ClientSize.Height));
        }

        private void LayoutPilotLeftPanel()
        {
            if (pnlLeft.ClientSize.Width <= 0 ||
                pnlLeft.ClientSize.Height <= 0)
            {
                return;
            }

            double scaleX = pnlLeft.ClientSize.Width / (double)PilotBaseLeftPanelWidth;
            double scaleY = pnlLeft.ClientSize.Height / (double)PilotBaseSplitHeight;
            double uniformScale = Math.Max(0.1, Math.Min(scaleX, scaleY));

            int margin = Math.Max(6, (int)Math.Round(8 * uniformScale));
            int gap = Math.Max(5, (int)Math.Round(8 * uniformScale));
            int contentWidth = Math.Max(1, pnlLeft.ClientSize.Width - margin * 2);
            int contentHeight = Math.Max(1, pnlLeft.ClientSize.Height - margin * 2);

            int importHeight = ClampInt((int)Math.Round(48 * scaleY), 28, 60);
            int guideHeight = ClampInt((int)Math.Round(88 * scaleY), 42, 120);
            int infoHeight = ClampInt((int)Math.Round(200 * scaleY), 90, 260);

            int reservedHeight =
                importHeight +
                guideHeight +
                infoHeight +
                gap * 3;

            if (reservedHeight > contentHeight)
            {
                int excess = reservedHeight - contentHeight;
                int shrinkGuide = Math.Min(excess, Math.Max(0, guideHeight - 36));
                guideHeight -= shrinkGuide;
                excess -= shrinkGuide;

                int shrinkInfo = Math.Min(excess, Math.Max(0, infoHeight - 74));
                infoHeight -= shrinkInfo;
                excess -= shrinkInfo;

                int shrinkImport = Math.Min(excess, Math.Max(0, importHeight - 26));
                importHeight -= shrinkImport;
            }

            int x = margin;
            int y = margin;
            btnImportModel.SetBounds(x, y, contentWidth, importHeight);

            y += importHeight + gap;
            int listBottom = pnlLeft.ClientSize.Height - margin - guideHeight - gap - infoHeight - gap;
            int listHeight = Math.Max(1, listBottom - y);
            lvModelList.SetBounds(x, y, contentWidth, listHeight);

            y += listHeight + gap;
            guideHeight = Math.Max(1, Math.Min(guideHeight, pnlLeft.ClientSize.Height - margin - y));
            lblPilotShortcutGuide.SetBounds(x, y, contentWidth, guideHeight);

            y += guideHeight + gap;
            int remainingInfoHeight = Math.Max(1, pnlLeft.ClientSize.Height - margin - y);
            grpSelectedModel.SetBounds(x, y, contentWidth, remainingInfoHeight);
            int tableMarginX = Math.Min(10, Math.Max(2, grpSelectedModel.ClientSize.Width / 12));
            int tableTop = Math.Min(22, Math.Max(2, grpSelectedModel.ClientSize.Height / 5));
            tblSelectedModel.SetBounds(
                tableMarginX,
                tableTop,
                Math.Max(1, grpSelectedModel.ClientSize.Width - tableMarginX * 2),
                Math.Max(1, grpSelectedModel.ClientSize.Height - tableTop - Math.Max(2, tableMarginX)));
        }

        private void LayoutPilotCard()
        {
            if (pnlPilotCard.ClientSize.Width <= 0 ||
                pnlPilotCard.ClientSize.Height <= 0)
            {
                return;
            }

            double scaleX = pnlPilotCard.ClientSize.Width / (double)PilotBaseCardWidth;
            double scaleY = pnlPilotCard.ClientSize.Height / (double)PilotBaseCardHeight;

            int marginX = Math.Max(6, (int)Math.Round(14 * scaleX));
            int marginY = Math.Max(6, (int)Math.Round(14 * scaleY));
            int gap = Math.Max(4, (int)Math.Round(8 * Math.Min(scaleX, scaleY)));
            int width = Math.Max(1, pnlPilotCard.ClientSize.Width - marginX * 2);
            int height = Math.Max(1, pnlPilotCard.ClientSize.Height - marginY * 2);

            int headerHeight = ClampInt((int)Math.Round(52 * scaleY), 32, 64);
            int trackHeight = ClampInt((int)Math.Round(57 * scaleY), 32, 68);
            int playbackHeight = ClampInt((int)Math.Round(73 * scaleY), 38, 84);

            int reservedHeight =
                headerHeight +
                trackHeight +
                playbackHeight +
                gap * 3;

            if (reservedHeight > height)
            {
                double shrink =
                    Math.Max(1, height - gap * 3) /
                    (double)Math.Max(1, headerHeight + trackHeight + playbackHeight);
                headerHeight = Math.Max(26, (int)Math.Floor(headerHeight * shrink));
                trackHeight = Math.Max(24, (int)Math.Floor(trackHeight * shrink));
                playbackHeight = Math.Max(30, (int)Math.Floor(playbackHeight * shrink));
            }

            int imageHeight =
                Math.Max(
                    40,
                    height -
                    headerHeight -
                    trackHeight -
                    playbackHeight -
                    gap * 3);

            int y = marginY;
            pnlPilotHeader.SetBounds(marginX, y, width, headerHeight);
            y += headerHeight + gap;

            pnlImageHost.SetBounds(marginX, y, width, imageHeight);
            y += imageHeight + gap;

            pnlTrackBar.SetBounds(marginX, y, width, trackHeight);
            y += trackHeight + gap;

            pnlPlaybackControls.SetBounds(marginX, y, width, playbackHeight);

            LayoutPilotHeaderControls();
            LayoutPilotTrackBar();
            LayoutPilotPlaybackControls();
        }

        private void LayoutPilotPlaybackControls()
        {
            if (pnlPlaybackControls.Width <= 0)
            {
                return;
            }

            Control[] controls =
            {
                btnJumpPrev5,
                btnPrevImage,
                btnPlayPause,
                cmbSpeed,
                btnReversePlay,
                btnNextImage,
                btnJumpNext5
            };

            int availableWidth =
                Math.Max(
                    1,
                    pnlPlaybackControls.ClientSize.Width -
                    pnlPlaybackControls.Padding.Horizontal);

            double scaleX = pnlPlaybackControls.ClientSize.Width / 1138.0;
            double scaleY = pnlPlaybackControls.ClientSize.Height / 73.0;
            int gap = Math.Max(4, (int)Math.Round(12 * scaleX));
            int comboWidth = Math.Max(78, (int)Math.Round(126 * scaleX));
            int buttonWidth = Math.Max(50, (int)Math.Round(116 * scaleX));
            int buttonHeight = Math.Max(28, (int)Math.Round(36 * scaleY));
            int totalPreferredWidth =
                buttonWidth * (controls.Length - 1) +
                comboWidth +
                gap * (controls.Length - 1);

            if (totalPreferredWidth > availableWidth)
            {
                double shrink = availableWidth / (double)Math.Max(1, totalPreferredWidth);
                buttonWidth = Math.Max(44, (int)Math.Floor(buttonWidth * shrink));
                comboWidth = Math.Max(70, (int)Math.Floor(comboWidth * shrink));
                gap = Math.Max(3, (int)Math.Floor(gap * shrink));
            }

            foreach (Control control in controls)
            {
                control.Size = control == cmbSpeed
                    ? new Size(comboWidth, Math.Max(26, Math.Min(buttonHeight, cmbSpeed.PreferredHeight)))
                    : new Size(buttonWidth, buttonHeight);
            }

            int totalWidth = controls.Sum(control => control.Width) + gap * (controls.Length - 1);
            int x =
                pnlPlaybackControls.Padding.Left +
                Math.Max(0, (availableWidth - totalWidth) / 2);

            foreach (Control control in controls)
            {
                control.Left = x;
                control.Top =
                    pnlPlaybackControls.Padding.Top +
                    Math.Max(
                        0,
                        (pnlPlaybackControls.ClientSize.Height -
                         pnlPlaybackControls.Padding.Vertical -
                         control.Height) / 2);
                x += control.Width + gap;
            }
        }

        private void LayoutPilotTrackBar()
        {
            if (pnlTrackBar.Width <= 0 || trbLocation == null)
            {
                return;
            }

            int left = pnlTrackBar.Padding.Left;
            int top =
                pnlTrackBar.Padding.Top +
                Math.Max(
                    0,
                    (pnlTrackBar.ClientSize.Height -
                     pnlTrackBar.Padding.Vertical -
                     trbLocation.Height) / 2);
            int width =
                Math.Max(
                    1,
                    pnlTrackBar.ClientSize.Width -
                    pnlTrackBar.Padding.Horizontal);

            trbLocation.SetBounds(
                left,
                top,
                width,
                trbLocation.Height);
        }

        private void LayoutPilotHeaderControls()
        {
            if (pnlPilotHeader.Width <= 0)
            {
                return;
            }

            double scaleX = pnlPilotHeader.ClientSize.Width / 1142.0;
            double scaleY = pnlPilotHeader.ClientSize.Height / 52.0;
            int margin = Math.Max(2, (int)Math.Round(4 * scaleX));
            int gap = Math.Max(4, (int)Math.Round(8 * scaleX));
            int buttonHeight = Math.Max(28, (int)Math.Round(36 * scaleY));
            int chartWidth = Math.Max(72, (int)Math.Round(110 * scaleX));
            int tubWidth = Math.Max(118, (int)Math.Round(154 * scaleX));
            int totalButtonsWidth = chartWidth + tubWidth + gap;

            int availableWidth = Math.Max(1, pnlPilotHeader.ClientSize.Width - margin * 2);
            if (totalButtonsWidth > availableWidth)
            {
                double shrink =
                    availableWidth /
                    (double)Math.Max(1, totalButtonsWidth);
                chartWidth = Math.Max(40, (int)Math.Floor(chartWidth * shrink));
                tubWidth = Math.Max(58, (int)Math.Floor(tubWidth * shrink));
                gap = Math.Max(2, (int)Math.Floor(gap * shrink));
            }

            int buttonTop = Math.Max(4, (pnlPilotHeader.ClientSize.Height - buttonHeight) / 2);
            int x = pnlPilotHeader.ClientSize.Width - margin - tubWidth;

            btnTubInput.SetBounds(x, buttonTop, tubWidth, buttonHeight);
            x -= chartWidth + gap;
            btnPilotChart.SetBounds(x, buttonTop, chartWidth, buttonHeight);

            lblTubPathTitle.Location = new Point(margin, Math.Max(0, (pnlPilotHeader.ClientSize.Height - lblTubPathTitle.Height) / 2));
            int valueLeft = lblTubPathTitle.Right + 12;
            int valueRight = Math.Max(valueLeft, btnPilotChart.Left - gap);
            lblTubPathValue.SetBounds(
                valueLeft,
                Math.Max(0, (pnlPilotHeader.ClientSize.Height - lblTubPathValue.Height) / 2),
                Math.Max(0, valueRight - valueLeft),
                lblTubPathValue.Height);
            lblTubPathValue.Visible = lblTubPathValue.Width > 24;
        }

        private void RenumberModelListItems()
        {
            for (int i = 0; i < lvModelList.Items.Count; i++)
            {
                lvModelList.Items[i].Text =
                    (i + 1).ToString();
            }
        }

        private async void LvModelList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                if (!TryGetSelectedModel(out ModelListItem? model))
                {
                    return;
                }

                await SelectModelAsync(model!);
            }
            catch (Exception ex)
            {
                ReportPilotException(ex);
                MessageBox.Show(ex.Message, "Pilot", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TryGetSelectedModel(out ModelListItem? model)
        {
            model = null;
            if (lvModelList.SelectedItems.Count == 0)
            {
                return false;
            }

            model = lvModelList.SelectedItems[0].Tag as ModelListItem;
            return model != null;
        }

        private async Task SelectModelAsync(ModelListItem model)
        {
            StopPlayback();
            SaveCurrentModelViewState();
            _selectedModel = model;
            ApplyModelInfoToLabels(model);

            if (model.IsLoaded)
            {
                RestoreCachedModel(model);
                return;
            }

            await LoadSelectedModelAsync(model);
        }

        private void ApplyModelInfoToLabels(ModelListItem model)
        {
            lblSelectedModelName.Text = model.Name;
            lblSelectedModelPath.Text = model.Path;
            lblSelectedModelType.Text = string.IsNullOrWhiteSpace(model.ModelType) ? "-" : model.ModelType;
            lblTubPathValue.Text = model.Name;
            SetTubPathLabels(SplitTubPathList(model.TubPath));
        }

        private void SetTubPathLabels(string? tubPath)
        {
            string displayPath = GetDisplayTubPath(tubPath);
            lblSelectedTubPath.Text = displayPath;
        }

        private void SetTubPathLabels(IEnumerable<string>? tubPaths)
        {
            List<string> paths = tubPaths?
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Select(GetDisplayTubPath)
                .ToList() ?? new List<string>();

            lblSelectedTubPath.Text = paths.Count == 0
                ? "-"
                : string.Join("; ", paths);
        }

        private string GetDisplayTubPath(string? tubPath)
        {
            if (string.IsNullOrWhiteSpace(tubPath))
            {
                return "-";
            }

            string trimmed = tubPath.Trim();
            if (trimmed.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("/home/", StringComparison.OrdinalIgnoreCase))
            {
                string distroName = _cardState?.WslDistroName ?? "Ubuntu-22.04";
                string windowsPath = DonkeyAsyncWorker.ToWindowsPathFromWslPath(trimmed, distroName);
                return string.IsNullOrWhiteSpace(windowsPath) ? trimmed : windowsPath;
            }

            return trimmed;
        }

        private List<string> SplitTubPathList(string? tubPaths)
        {
            if (string.IsNullOrWhiteSpace(tubPaths))
            {
                return new List<string>();
            }

            return tubPaths
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(path => path.Trim())
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .ToList();
        }

        private void ClearModelLabels()
        {
            lblSelectedModelName.Text = "-";
            lblSelectedModelPath.Text = "-";
            lblSelectedModelType.Text = "-";
            lblSelectedTubPath.Text = "-";
            lblTubPathValue.Text = "-";
            lblImageIndexOverlay.Text = "0 / 0";
            pliotAiThrottleGauge.SetThrottleValue(null);
            pliotTubThrottleGauge.SetThrottleValue(null);
            pliotAngleIndicator.SetAngleValues(null, null);
        }

        private void SaveCurrentModelViewState()
        {
            if (_selectedModel != null)
            {
                _selectedModel.CurrentFrameIndex = _currentFrameIndex;
            }
        }

        private void RestoreCachedModel(ModelListItem model)
        {
            _cardState = model.CardState;
            _frameList.Clear();
            _frameList.AddRange(model.Frames.Select(CloneFrame));
            _currentFrameIndex = Math.Max(0, Math.Min(model.CurrentFrameIndex, Math.Max(0, _frameList.Count - 1)));
            ApplyModelInfoToLabels(model);
            ConfigureLocationTrackBar();
            ShowCurrentFrame();
        }

        private async Task LoadSelectedModelAsync(ModelListItem model)
        {
            // 모델 registry/database에서 모델 정보와 이전에 연결했던 tub 상태를 읽어 화면에 복원합니다.
            // tub가 이미 연결돼 있으면 프레임도 즉시 다시 로드합니다.
            // Each model owns its cached card/frame state; switching models should not reparse loaded tubs.
            _loadCts?.Cancel();
            _loadCts = new CancellationTokenSource();
            CancellationToken token = _loadCts.Token;

            _cardState = new DonkeyAsyncWorker.PilotCardState
            {
                ModelName = model.Name,
                ModelPath = model.Path
            };

            lblSelectedModelName.Text = model.Name;
            lblSelectedModelPath.Text = model.Path;
            lblSelectedModelType.Text = "-";
            lblSelectedTubPath.Text = "-";
            lblTubPathValue.Text = model.Name;

            using ProgressStatusForm progressForm = new ProgressStatusForm();
            progressForm.SetTitle("모델 데이터 연결 중...");
            progressForm.SetIndeterminate(true);
            progressForm.CancelRequested += () => _loadCts?.Cancel();
            progressForm.Show(this);

            IProgress<DonkeyAsyncWorker.ProgressReport> progress = CreateProgress(progressForm);

            try
            {
                _cardState.WslDistroName =
                    await DonkeyAsyncWorker.GetPreferredWslDistroNameAsync(token);

                DonkeyAsyncWorker.OperationResult<string> myCarResult =
                    await DonkeyAsyncWorker.FindMyCarPathInWslAsync(
                        _cardState.WslDistroName,
                        progress,
                        token);

                if (!myCarResult.Success || string.IsNullOrWhiteSpace(myCarResult.Data))
                {
                    throw new InvalidOperationException(myCarResult.ErrorMessage);
                }

                _cardState.MyCarPath = myCarResult.Data;

                DonkeyAsyncWorker.OperationResult<DonkeyAsyncWorker.PilotCardState> modelResult =
                    await DonkeyAsyncWorker.LoadModelInfoFromDatabaseAsync(
                        _cardState,
                        progress,
                        token);

                if (!modelResult.Success || modelResult.Data == null)
                {
                    throw new InvalidOperationException(modelResult.ErrorMessage);
                }

                _cardState = modelResult.Data;
                CacheCurrentModelInfo();
                ApplyModelInfoToLabels(model);

                List<string> tubPaths = _cardState.TrainingTubPaths ?? new List<string>();
                if (tubPaths.Count == 0)
                {
                    _frameList.Clear();
                    ConfigureLocationTrackBar();
                    DrawTubRequiredMessage();
                    CacheCurrentModelFrames();
                    progressForm.MarkCompleted("모델 정보 연결 완료, 주행데이터는 별도 입력이 필요합니다.");
                    return;
                }

                SetTubPathLabels(tubPaths);
                await LoadTubFramesAsync(tubPaths, progress, token);
                await LoadAndMergeJudementAsync(progress, token);

                bool hasJudement = _frameList.Any(
                    f => f.PilotAngle.HasValue || f.PilotThrottle.HasValue);

                if (!hasJudement)
                {
                    try
                    {
                        progress.Report(new DonkeyAsyncWorker.ProgressReport
                        {
                            Step = "AI 판단 데이터 자동 생성 중...",
                            Log = "AI 판단 데이터가 없어 자동 생성을 시도합니다.",
                            IsIndeterminate = true
                        });

                        DonkeyAsyncWorker.OperationResult<List<DonkeyAsyncWorker.JudementRecord>> autoResult =
                            await DonkeyAsyncWorker.GenerateJudementAsync(
                                _cardState,
                                progress,
                                token);

                        if (autoResult.Success && autoResult.Data != null)
                        {
                            _cardState.JudementRecords = autoResult.Data;
                            MergeJudementRecords(autoResult.Data);
                        }
                        else
                        {
                            MessageBox.Show(
                                autoResult.ErrorMessage ?? "AI 판단 데이터를 생성하지 못했습니다.",
                                "AI 판단 생성",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception judementEx)
                    {
                        MessageBox.Show(
                            "AI 판단 데이터 자동 생성 실패: " + judementEx.Message,
                            "AI 판단 생성",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }

                ConfigureLocationTrackBar();
                MoveToFrame(0);
                CacheCurrentModelFrames();
                progressForm.MarkCompleted("모델 데이터 연결 완료");
            }
            catch (OperationCanceledException)
            {
                progressForm.MarkCanceled("작업이 취소되었습니다.");
            }
            catch (Exception ex)
            {
                progressForm.MarkFailed($"오류: {ex.Message}");
                MessageBox.Show(ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Tub And AI Data

        private async void BtnTubInput_Click(object? sender, EventArgs e)
        {
            try
            {
            // 사용자가 선택한 tub 폴더를 WSL 경로로 저장하고,
            // catalog/record/image 정보를 파싱해 프레임 리스트에 올립니다.
            if (_cardState == null)
            {
                if (_selectedModel == null)
                {
                    MessageBox.Show("먼저 모델을 선택해 주세요.", "주행데이터 입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _cardState = new DonkeyAsyncWorker.PilotCardState
                {
                    ModelName = _selectedModel.Name,
                    ModelPath = _selectedModel.Path
                };
                _cardState.WslDistroName = await DonkeyAsyncWorker.GetPreferredWslDistroNameAsync(CancellationToken.None);
            }

            using FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "주행데이터 폴더 선택";
            dialog.ShowNewFolderButton = false;

            if (string.IsNullOrWhiteSpace(_cardState.WslDistroName))
            {
                _cardState.WslDistroName = await DonkeyAsyncWorker.GetPreferredWslDistroNameAsync(CancellationToken.None);
            }

            DonkeyAsyncWorker.OperationResult<string> homeResult =
                await DonkeyAsyncWorker.GetWslHomePathAsync(
                    _cardState.WslDistroName,
                    null,
                    CancellationToken.None);

            if (homeResult.Success && !string.IsNullOrWhiteSpace(homeResult.Data))
            {
                dialog.SelectedPath = homeResult.Data;
            }

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            List<string> selectedTubFolders = FindTubFolders(dialog.SelectedPath);
            if (selectedTubFolders.Count == 0)
            {
                MessageBox.Show("선택한 폴더에서 주행데이터를 찾지 못했습니다.", "주행데이터 입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<string> selectedPathWsls = selectedTubFolders
                .Select(DonkeyAsyncWorker.ToWslPathFromWindowsPath)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            _cardState.TrainingTubPaths = selectedPathWsls;
            CacheCurrentModelInfo();
            SetTubPathLabels(selectedPathWsls);

            using ProgressStatusForm progressForm = new ProgressStatusForm();
            progressForm.SetTitle("주행데이터 연결 중...");
            progressForm.SetIndeterminate(true);
            _loadCts?.Cancel();
            _loadCts = new CancellationTokenSource();
            CancellationToken token = _loadCts.Token;
            progressForm.CancelRequested += () => _loadCts?.Cancel();
            progressForm.Show(this);

            IProgress<DonkeyAsyncWorker.ProgressReport> progress = CreateProgress(progressForm);

            try
            {
                await LoadTubFramesAsync(selectedPathWsls, progress, token);
                await LoadAndMergeJudementAsync(progress, token);
                ConfigureLocationTrackBar();
                MoveToFrame(0);

                bool hasJudement = _frameList.Any(f => f.PilotAngle.HasValue || f.PilotThrottle.HasValue);
                if (!hasJudement)
                {
                    progress.Report(new DonkeyAsyncWorker.ProgressReport { Step = "AI 판단 데이터 생성 중..." });
                    try
                    {
                        DonkeyAsyncWorker.OperationResult<List<DonkeyAsyncWorker.JudementRecord>> judResult =
                            await DonkeyAsyncWorker.GenerateJudementAsync(
                                _cardState,
                                progress,
                                token,
                                forceRegenerate: false);
                        if (judResult.Success && judResult.Data != null)
                        {
                            _cardState.JudementRecords = judResult.Data;
                            MergeJudementRecords(judResult.Data);
                            ShowCurrentFrame();
                        }
                    }
                    catch (OperationCanceledException) { throw; }
                    catch { }
                }

                CacheCurrentModelFrames();
                progressForm.MarkCompleted("주행데이터 연결 완료");
            }
            catch (OperationCanceledException)
            {
                progressForm.MarkCanceled("주행데이터 연결이 취소되었습니다.");
            }
            catch (Exception ex)
            {
                progressForm.MarkFailed($"오류: {ex.Message}");
                MessageBox.Show(ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
            catch (Exception ex)
            {
                ReportPilotException(ex);
                MessageBox.Show(ex.Message, "Pilot", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPilotChart_Click(object? sender, EventArgs e)
        {
            if (_isChartOpen)
            {
                return;
            }

            // The chart needs both recorded tub values and AI judgment values to compare the two lines.
            if (_selectedModel == null)
            {
                MessageBox.Show("먼저 모델을 선택해 주세요.", "그래프", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_frameList.Count == 0)
            {
                MessageBox.Show("먼저 주행데이터를 연결해 주세요.", "그래프", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool hasAiJudement = _frameList.Any(frame =>
                frame.PilotAngle.HasValue || frame.PilotThrottle.HasValue);
            if (!hasAiJudement)
            {
                MessageBox.Show("AI 판단 데이터를 먼저 생성하거나 불러와 주세요.", "그래프", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<DonkeyAsyncWorker.PilotFrameData> chartFrames =
                _frameList.Select(CloneFrame).ToList();

            try
            {
                _isChartOpen = true;
                btnPilotChart.Enabled = false;
                using PliotChart chart = new PliotChart(_selectedModel.Name, chartFrames);
                chart.ShowDialog(this);
            }
            finally
            {
                btnPilotChart.Enabled = true;
                _isChartOpen = false;
            }
        }

        private async Task LoadTubFramesAsync(
            string tubPath,
            IProgress<DonkeyAsyncWorker.ProgressReport> progress,
            CancellationToken token)
        {
            if (_cardState == null)
            {
                return;
            }

            DonkeyAsyncWorker.OperationResult<List<DonkeyAsyncWorker.PilotFrameData>> tubResult =
                await DonkeyAsyncWorker.ParseSingleTubFolderAsync(
                    tubPath,
                    _cardState.WslDistroName,
                    progress,
                    token);

            if ((!tubResult.Success || tubResult.Data == null || tubResult.Data.Count == 0)
                && tubPath.StartsWith("/", StringComparison.Ordinal))
            {
                string windowsTubPath = DonkeyAsyncWorker.ToWindowsPathFromWslPath(
                    tubPath,
                    _cardState.WslDistroName);

                if (!string.IsNullOrWhiteSpace(windowsTubPath)
                    && !string.Equals(windowsTubPath, tubPath, StringComparison.OrdinalIgnoreCase))
                {
                    progress.Report(new DonkeyAsyncWorker.ProgressReport
                    {
                        Log = $"Windows tub 寃쎈줈濡? ?ъ떆 ?쎄린: {windowsTubPath}"
                    });

                    tubResult = await DonkeyAsyncWorker.ParseSingleTubFolderAsync(
                        windowsTubPath,
                        _cardState.WslDistroName,
                        progress,
                        token);
                }
            }

            _frameList.Clear();
            if (!tubResult.Success || tubResult.Data == null || tubResult.Data.Count == 0)
            {
                ConfigureLocationTrackBar();
                DrawTubRequiredMessage();
                return;
            }

            _frameList.AddRange(tubResult.Data);
            _currentFrameIndex = 0;
        }

        private async Task LoadTubFramesAsync(
            List<string> tubPaths,
            IProgress<DonkeyAsyncWorker.ProgressReport> progress,
            CancellationToken token)
        {
            _frameList.Clear();

            foreach (string tubPath in tubPaths.Where(path => !string.IsNullOrWhiteSpace(path)))
            {
                token.ThrowIfCancellationRequested();

                DonkeyAsyncWorker.OperationResult<List<DonkeyAsyncWorker.PilotFrameData>> tubResult =
                    await DonkeyAsyncWorker.ParseSingleTubFolderAsync(
                        tubPath,
                        _cardState?.WslDistroName ?? string.Empty,
                        progress,
                        token);

                if ((!tubResult.Success || tubResult.Data == null || tubResult.Data.Count == 0)
                    && tubPath.StartsWith("/", StringComparison.Ordinal))
                {
                    string windowsTubPath = DonkeyAsyncWorker.ToWindowsPathFromWslPath(
                        tubPath,
                        _cardState?.WslDistroName ?? string.Empty);

                    if (!string.IsNullOrWhiteSpace(windowsTubPath)
                        && !string.Equals(windowsTubPath, tubPath, StringComparison.OrdinalIgnoreCase))
                    {
                        tubResult = await DonkeyAsyncWorker.ParseSingleTubFolderAsync(
                            windowsTubPath,
                            _cardState?.WslDistroName ?? string.Empty,
                            progress,
                            token);
                    }
                }

                if (tubResult.Success && tubResult.Data != null && tubResult.Data.Count > 0)
                {
                    _frameList.AddRange(tubResult.Data);
                }
            }

            if (_frameList.Count == 0)
            {
                ConfigureLocationTrackBar();
                DrawTubRequiredMessage();
                return;
            }

            _currentFrameIndex = 0;
        }

        private List<string> FindTubFolders(string folderPath)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                return result;
            }

            void Search(string path)
            {
                if (IsTubDataFolder(path))
                {
                    result.Add(Path.GetFullPath(path));
                    return;
                }

                foreach (string child in Directory.GetDirectories(path))
                {
                    Search(child);
                }
            }

            Search(folderPath);

            return result
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private bool IsTubDataFolder(string folderPath)
        {
            return !string.IsNullOrWhiteSpace(folderPath) &&
                Directory.Exists(folderPath) &&
                Directory.GetFiles(folderPath, "catalog_*.catalog", SearchOption.TopDirectoryOnly).Length > 0 &&
                File.Exists(Path.Combine(folderPath, "manifest.json"));
        }

        private async Task LoadAndMergeJudementAsync(
            IProgress<DonkeyAsyncWorker.ProgressReport> progress,
            CancellationToken token)
        {
            if (_cardState == null || _frameList.Count == 0)
            {
                return;
            }

            DonkeyAsyncWorker.OperationResult<List<DonkeyAsyncWorker.JudementRecord>> result =
                await DonkeyAsyncWorker.CheckOrLoadJudementAsync(_cardState, progress, token);

            if (!result.Success || result.Data == null || result.Data.Count == 0)
            {
                progress.Report(new DonkeyAsyncWorker.ProgressReport
                {
                    Log = "AI 판단 데이터가 아직 없습니다. 생성 버튼을 눌러 생성하세요."
                });
                return;
            }

            _cardState.JudementRecords = result.Data;
            MergeJudementRecords(result.Data);
        }

        private void MergeJudementRecords(List<DonkeyAsyncWorker.JudementRecord> records)
        {
            // Prefer tub path + frame index, then fall back to image file name for regenerated judgment files.
            Dictionary<string, DonkeyAsyncWorker.JudementRecord> byTubAndIndex =
                records.Where(record => !string.IsNullOrWhiteSpace(record.TubPath))
                    .GroupBy(record => BuildTubFrameKey(record.TubPath, record.Index), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);

            Dictionary<string, DonkeyAsyncWorker.JudementRecord> byImage =
                records.Where(record => !string.IsNullOrWhiteSpace(record.ImagePath))
                    .GroupBy(record => Path.GetFileName(record.ImagePath), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);

            foreach (DonkeyAsyncWorker.PilotFrameData frame in _frameList)
            {
                DonkeyAsyncWorker.JudementRecord? match = null;
                if (!byTubAndIndex.TryGetValue(BuildTubFrameKey(frame.TubPath, frame.Index), out match))
                {
                    string fileName = Path.GetFileName(frame.ImagePath);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        byImage.TryGetValue(fileName, out match);
                    }
                }

                if (match == null)
                {
                    continue;
                }

                frame.PilotAngle = ClampAiJudementValue(match.PilotAngle);
                frame.PilotThrottle = ClampAiJudementValue(match.PilotThrottle);
            }
        }

        private string BuildTubFrameKey(string tubPath, int index)
        {
            return NormalizeTubPathKey(tubPath) + "#" + index.ToString();
        }

        private string NormalizeTubPathKey(string tubPath)
        {
            return (tubPath ?? string.Empty)
                .Trim()
                .Replace('\\', '/')
                .TrimEnd('/');
        }

        #endregion

        #region Model Cache

        private void CacheCurrentModelInfo()
        {
            if (_selectedModel == null || _cardState == null)
            {
                return;
            }

            _selectedModel.CardState = _cardState;
            _selectedModel.ModelType = _cardState.ModelType;
            _selectedModel.TubPath = string.Join(";", _cardState.TrainingTubPaths ?? new List<string>());
        }

        private void CacheCurrentModelFrames()
        {
            if (_selectedModel == null || _cardState == null)
            {
                return;
            }

            _selectedModel.CardState = _cardState;
            _selectedModel.ModelType = _cardState.ModelType;
            _selectedModel.TubPath = string.Join(";", _cardState.TrainingTubPaths ?? new List<string>());
            // Cache loaded frames so switching back to a model does not parse the tub again.
            _selectedModel.Frames = _frameList.Select(CloneFrame).ToList();
            _selectedModel.CurrentFrameIndex = _currentFrameIndex;
            _selectedModel.IsLoaded = true;
            ApplyModelInfoToLabels(_selectedModel);
        }

        private static DonkeyAsyncWorker.PilotFrameData CloneFrame(DonkeyAsyncWorker.PilotFrameData frame)
        {
            return new DonkeyAsyncWorker.PilotFrameData
            {
                Index = frame.Index,
                TubPath = frame.TubPath,
                ImagePath = frame.ImagePath,
                UserAngle = frame.UserAngle,
                UserThrottle = frame.UserThrottle,
                PilotAngle = ClampAiJudementValue(frame.PilotAngle),
                PilotThrottle = ClampAiJudementValue(frame.PilotThrottle),
                Mode = frame.Mode
            };
        }

        private static double? ClampAiJudementValue(double? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return Math.Max(-1.0, Math.Min(1.0, value.Value));
        }

        #endregion

        #region Progress

        private static IProgress<DonkeyAsyncWorker.ProgressReport> CreateProgress(
            ProgressStatusForm progressForm)
        {
            return new Progress<DonkeyAsyncWorker.ProgressReport>(report =>
            {
                if (!string.IsNullOrWhiteSpace(report.Title))
                {
                    progressForm.SetTitle(report.Title);
                }

                if (!string.IsNullOrWhiteSpace(report.Step))
                {
                    progressForm.SetStep(report.Step);
                }

                if (!string.IsNullOrWhiteSpace(report.Log))
                {
                    progressForm.AppendLog(report.Log);
                }

                if (report.Percent.HasValue)
                {
                    progressForm.SetProgress(report.Percent.Value);
                }

                progressForm.SetIndeterminate(report.IsIndeterminate);
            });
        }

        #endregion

        #region Navigation And Image Display

        private void ConfigureLocationTrackBar()
        {
            _isUpdatingTrackBar = true;

            if (_frameList.Count == 0)
            {
                trbLocation.Minimum = 0;
                trbLocation.Maximum = 0;
                trbLocation.Value = 0;
                trbLocation.Enabled = false;
                _isUpdatingTrackBar = false;
                return;
            }

            _currentFrameIndex = Math.Max(0, Math.Min(_currentFrameIndex, _frameList.Count - 1));
            trbLocation.Minimum = 0;
            trbLocation.Maximum = _frameList.Count - 1;
            trbLocation.SmallChange = 1;
            trbLocation.LargeChange = Math.Max(1, _frameList.Count / 20);
            trbLocation.TickFrequency = Math.Max(1, _frameList.Count / 20);
            trbLocation.Value = _currentFrameIndex;
            trbLocation.Enabled = true;

            _isUpdatingTrackBar = false;
        }

        private void trbLocation_ValueChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingTrackBar || _frameList.Count == 0)
            {
                return;
            }

            _currentFrameIndex = trbLocation.Value;
            ShowCurrentFrame();
            SaveCurrentModelViewState();
        }

        private void TrbLocation_MouseDown(object? sender, MouseEventArgs e)
        {
            if (_frameList.Count == 0 || e.Button != MouseButtons.Left)
            {
                return;
            }

            // Jump directly to the clicked frame instead of waiting for TrackBar thumb animation.
            int channelWidth = Math.Max(1, trbLocation.ClientSize.Width);
            double ratio = Math.Max(0.0, Math.Min(1.0, e.X / (double)channelWidth));
            int targetValue = trbLocation.Minimum
                + (int)Math.Round((trbLocation.Maximum - trbLocation.Minimum) * ratio);

            MoveToFrame(targetValue);
        }

        private void MoveToFrame(int newIndex)
        {
            if (_frameList.Count == 0)
            {
                StopPlayback();
                ShowCurrentFrame();
                return;
            }

            int boundedIndex = Math.Max(0, Math.Min(newIndex, _frameList.Count - 1));
            _currentFrameIndex = boundedIndex;
            ShowCurrentFrame();
            SaveCurrentModelViewState();

            if ((_isPlaying && boundedIndex == _frameList.Count - 1)
                || (_isReversePlaying && boundedIndex == 0))
            {
                StopPlayback();
            }
        }

        private void ShowCurrentFrame()
        {
            // 현재 프레임의 이미지, 사용자 조향/스로틀, AI 조향/스로틀을 한 화면에 반영합니다.
            if (_frameList.Count == 0)
            {
                lblImageIndexOverlay.Text = "0 / 0";
                pliotAiThrottleGauge.SetThrottleValue(null);
                pliotTubThrottleGauge.SetThrottleValue(null);
                pliotAngleIndicator.SetAngleValues(null, null);
                DrawTubRequiredMessage();
                ConfigureLocationTrackBar();
                pliotAngleIndicator.Invalidate();
                return;
            }

            _currentFrameIndex = Math.Max(0, Math.Min(_currentFrameIndex, _frameList.Count - 1));
            DonkeyAsyncWorker.PilotFrameData frame = _frameList[_currentFrameIndex];

            ShowImageInPictureBox(frame.ImagePath);
            PositionImageOverlaysIfNeeded();
            lblImageIndexOverlay.Text = $"{_currentFrameIndex + 1} / {_frameList.Count}";
            pliotAiThrottleGauge.SetThrottleValue(frame.PilotThrottle);
            pliotTubThrottleGauge.SetThrottleValue(frame.UserThrottle);
            pliotAngleIndicator.SetAngleValues(frame.UserAngle, frame.PilotAngle);
            pliotAngleIndicator.Invalidate();

            _isUpdatingTrackBar = true;
            trbLocation.Value = _currentFrameIndex;
            _isUpdatingTrackBar = false;
        }

        private void ShowImageInPictureBox(string imagePath)
        {
            if (string.Equals(_currentImagePath, imagePath, StringComparison.OrdinalIgnoreCase) &&
                _currentImageRenderSize != Size.Empty &&
                picPilotImage.Image != null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                DisposeCurrentImage();
                DrawImageMissingMessage();
                return;
            }

            string distroName = _cardState?.WslDistroName ?? "Ubuntu-22.04";
            string windowsPath = DonkeyAsyncWorker.ToWindowsPathFromWslPath(imagePath, distroName);
            if (!File.Exists(windowsPath))
            {
                DisposeCurrentImage();
                DrawImageMissingMessage();
                return;
            }

            using FileStream stream = new FileStream(windowsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using Image image = Image.FromStream(stream);

            Size targetSize = GetImageRenderSize(image.Size);
            if (string.Equals(_currentImagePath, imagePath, StringComparison.OrdinalIgnoreCase) &&
                _currentImageRenderSize == targetSize &&
                picPilotImage.Image != null)
            {
                return;
            }

            Bitmap bitmap =
                targetSize.Width > 0 && targetSize.Height > 0
                    ? new Bitmap(image, targetSize)
                    : new Bitmap(image);

            DisposeCurrentImage();
            picPilotImage.Image = bitmap;
            _currentImagePath = imagePath;
            _currentImageRenderSize = targetSize;
            _lastOverlayHostSize = Size.Empty;
            picPilotImage.SendToBack();
            PositionImageOverlays();
        }

        private Size GetImageRenderSize(Size sourceSize)
        {
            if (sourceSize.Width <= 0 || sourceSize.Height <= 0)
            {
                return Size.Empty;
            }

            int maxWidth = Math.Max(1, picPilotImage.ClientSize.Width);
            int maxHeight = Math.Max(1, picPilotImage.ClientSize.Height);
            double scale = Math.Min(
                maxWidth / (double)sourceSize.Width,
                maxHeight / (double)sourceSize.Height);

            if (scale >= 1.0)
            {
                return sourceSize;
            }

            return new Size(
                Math.Max(1, (int)Math.Round(sourceSize.Width * scale)),
                Math.Max(1, (int)Math.Round(sourceSize.Height * scale)));
        }

        private void DrawTubRequiredMessage()
        {
            DrawMessageImage("주행데이터 필요");
        }

        private void DrawImageMissingMessage()
        {
            DrawMessageImage("이미지 없음");
        }

        private void DrawMessageImage(string text)
        {
            DisposeCurrentImage();
            _currentImagePath = "";
            _currentImageRenderSize = Size.Empty;

            int width = Math.Max(320, picPilotImage.Width);
            int height = Math.Max(180, picPilotImage.Height);
            Bitmap bmp = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bmp))
            using (Font font = new Font("맑은 고딕", 18, FontStyle.Bold))
            using (Brush brush = new SolidBrush(Color.Gainsboro))
            {
                g.Clear(Color.FromArgb(35, 39, 44));
                SizeF size = g.MeasureString(text, font);
                float x = (width - size.Width) / 2;
                float y = (height - size.Height) / 2;
                g.DrawString(text, font, brush, x, y);
            }

            picPilotImage.Image = bmp;
            _lastOverlayHostSize = Size.Empty;
            picPilotImage.SendToBack();
            PositionImageOverlays();
        }

        private void DisposeCurrentImage()
        {
            if (picPilotImage.Image != null)
            {
                Image oldImage = picPilotImage.Image;
                picPilotImage.Image = null;
                oldImage.Dispose();
            }

            _currentImagePath = "";
            _currentImageRenderSize = Size.Empty;
        }

        #endregion

        #region Playback

        private void BtnPlayPause_Click(object? sender, EventArgs e)
        {
            if (_isPlaying)
            {
                StopPlayback();
                return;
            }

            _isPlaying = true;
            _isReversePlaying = false;
            UpdatePlaybackButtonImages();
            StartPlaybackTimer();
        }

        private void BtnReversePlay_Click(object? sender, EventArgs e)
        {
            if (_isReversePlaying)
            {
                StopPlayback();
                return;
            }

            _isReversePlaying = true;
            _isPlaying = false;
            UpdatePlaybackButtonImages();
            StartPlaybackTimer();
        }

        private void PlaybackTimer_Tick(object? sender, EventArgs e)
        {
            if (_frameList.Count == 0)
            {
                StopPlayback();
                return;
            }

            MoveToFrame(_isReversePlaying ? _currentFrameIndex - 1 : _currentFrameIndex + 1);
        }

        private void CmbSpeed_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string selected = cmbSpeed.SelectedItem?.ToString() ?? "1.0x";
            string numeric = selected.Replace("x", string.Empty);
            if (!double.TryParse(numeric, out _playbackSpeed))
            {
                _playbackSpeed = 1.0;
            }

            if (_playbackTimer != null)
            {
                _playbackTimer.Interval = GetPlaybackInterval();
            }
        }

        private void StartPlaybackTimer()
        {
            if (_frameList.Count == 0 || _playbackTimer == null)
            {
                StopPlayback();
                return;
            }

            _playbackTimer.Interval = GetPlaybackInterval();
            _playbackTimer.Start();
        }

        private void StopPlayback()
        {
            _playbackTimer?.Stop();
            _isPlaying = false;
            _isReversePlaying = false;
            UpdatePlaybackButtonImages();
        }

        private void UpdatePlaybackButtonImages()
        {
            if (btnPlayPause == null || btnPlayPause.IsDisposed)
            {
                return;
            }

            btnPlayPause.BackgroundImage =
                _isPlaying
                    ? Properties.Resources.pause
                    : Properties.Resources.PlaySlide4655096;
            btnPlayPause.BackgroundImageLayout = ImageLayout.Zoom;
            btnPlayPause.AccessibleName =
                _isPlaying
                    ? "일시정지"
                    : "재생";

            if (btnReversePlay != null && !btnReversePlay.IsDisposed)
            {
                btnReversePlay.AccessibleName =
                    _isReversePlaying
                        ? "역재생 중"
                        : "역재생";
            }
        }

        private int GetPlaybackInterval()
        {
            return AD_AI_LearningData_Editor.frmMain.GetPlaybackIntervalForSpeed(_playbackSpeed);
        }

        #endregion

        #region Overlay Rendering

        private void PositionImageOverlays()
        {
            if (IsDisposed ||
                picPilotImage.IsDisposed ||
                picPilotImage.ClientSize.Width <= 0 ||
                picPilotImage.ClientSize.Height <= 0)
            {
                return;
            }

            EnsureImageOverlayParent();

            Rectangle imageBounds = GetVisibleImageOverlayBounds();
            int visibleHostHeight = Math.Max(1, imageBounds.Height);

            ConfigureAngleOverlayLayout();
            UpdatePilotOverlaySizes(imageBounds.Width, visibleHostHeight);
            int margin = Math.Max(8, (int)Math.Round(imageBounds.Width * 0.016));
            pnlImageIndexOverlay.Location = new Point(
                ClampInt(imageBounds.Left + margin, 0, Math.Max(0, picPilotImage.ClientSize.Width - pnlImageIndexOverlay.Width)),
                ClampInt(imageBounds.Top + margin, 0, Math.Max(0, picPilotImage.ClientSize.Height - pnlImageIndexOverlay.Height)));

            int throttleX = imageBounds.Left + margin;
            int throttleGap = Math.Max(6, (int)(10 * GetOverlayScale(imageBounds.Width, visibleHostHeight)));
            int aiThrottleY =
                imageBounds.Bottom - pliotAiThrottleGauge.Height - margin;
            int tubThrottleY =
                aiThrottleY - pliotTubThrottleGauge.Height - throttleGap;
            int angleX = imageBounds.Left + (imageBounds.Width - pliotAngleIndicator.Width) / 2;
            int angleY = imageBounds.Bottom - pliotAngleIndicator.Height - margin;

            pliotTubThrottleGauge.Location = new Point(
                ClampInt(throttleX, 0, Math.Max(0, picPilotImage.ClientSize.Width - pliotTubThrottleGauge.Width)),
                ClampInt(tubThrottleY, imageBounds.Top + margin, Math.Max(imageBounds.Top + margin, imageBounds.Bottom - pliotTubThrottleGauge.Height - margin)));
            pliotAiThrottleGauge.Location = new Point(
                ClampInt(throttleX, 0, Math.Max(0, picPilotImage.ClientSize.Width - pliotAiThrottleGauge.Width)),
                ClampInt(aiThrottleY, imageBounds.Top + margin, Math.Max(imageBounds.Top + margin, imageBounds.Bottom - pliotAiThrottleGauge.Height - margin)));

            pliotAngleIndicator.Location = new Point(
                ClampInt(angleX, imageBounds.Left + margin, Math.Max(imageBounds.Left + margin, imageBounds.Right - pliotAngleIndicator.Width - margin)),
                ClampInt(angleY, imageBounds.Top + margin, Math.Max(imageBounds.Top + margin, imageBounds.Bottom - pliotAngleIndicator.Height - margin)));

            picPilotImage.SendToBack();
            pnlImageIndexOverlay.BringToFront();
            pliotAiThrottleGauge.BringToFront();
            pliotTubThrottleGauge.BringToFront();
            pliotAngleIndicator.BringToFront();
            pliotAiThrottleGauge.Invalidate();
            pliotTubThrottleGauge.Invalidate();
            pliotAngleIndicator.Invalidate();
        }

        private Rectangle GetVisibleImageOverlayBounds()
        {
            Rectangle displayedImageBounds = GetDisplayedImageBounds();
            Rectangle visiblePictureBounds = picPilotImage.ClientRectangle;

            int coveredTop = int.MaxValue;
            AddCoveringPanelTop(pnlTrackBar, ref coveredTop);
            AddCoveringPanelTop(pnlPlaybackControls, ref coveredTop);

            if (coveredTop != int.MaxValue)
            {
                int visibleBottom = Math.Max(
                    visiblePictureBounds.Top,
                    coveredTop - 8);

                visiblePictureBounds.Height =
                    Math.Max(
                        1,
                        Math.Min(
                            visiblePictureBounds.Height,
                            visibleBottom - visiblePictureBounds.Top));
            }

            Rectangle visibleImageBounds =
                Rectangle.Intersect(displayedImageBounds, visiblePictureBounds);

            if (visibleImageBounds.Width <= 0 || visibleImageBounds.Height <= 0)
            {
                return displayedImageBounds;
            }

            return visibleImageBounds;
        }

        private void AddCoveringPanelTop(Control panel, ref int coveredTop)
        {
            if (panel.Parent != pnlPilotCard || pnlImageHost.Parent != pnlPilotCard)
            {
                return;
            }

            int panelTopInPictureBox =
                panel.Top -
                pnlImageHost.Top -
                picPilotImage.Top;

            if (panelTopInPictureBox <= 0 ||
                panelTopInPictureBox >= picPilotImage.ClientSize.Height)
            {
                return;
            }

            coveredTop = Math.Min(coveredTop, panelTopInPictureBox);
        }

        private Rectangle GetDisplayedImageBounds()
        {
            Rectangle client = picPilotImage.ClientRectangle;

            if (picPilotImage.Image == null ||
                client.Width <= 0 ||
                client.Height <= 0)
            {
                return client;
            }

            Size imageSize = picPilotImage.Image.Size;
            if (imageSize.Width <= 0 || imageSize.Height <= 0)
            {
                return client;
            }

            double imageRatio = imageSize.Width / (double)imageSize.Height;
            double clientRatio = client.Width / (double)client.Height;

            int width;
            int height;

            if (clientRatio > imageRatio)
            {
                height = client.Height;
                width = Math.Max(1, (int)Math.Round(height * imageRatio));
            }
            else
            {
                width = client.Width;
                height = Math.Max(1, (int)Math.Round(width / imageRatio));
            }

            int x = client.Left + (client.Width - width) / 2;
            int y = client.Top + (client.Height - height) / 2;
            return new Rectangle(x, y, width, height);
        }

        private void PositionImageOverlaysIfNeeded()
        {
            Size hostSize = picPilotImage.ClientSize;
            Size imageSize = picPilotImage.Image?.Size ?? Size.Empty;
            Size layoutKey = new Size(
                hostSize.Width ^ imageSize.Width,
                hostSize.Height ^ imageSize.Height);

            if (layoutKey == _lastOverlayHostSize)
            {
                return;
            }

            _lastOverlayHostSize = layoutKey;
            PositionImageOverlays();
        }

        private void UpdatePilotOverlaySizes(int hostWidth, int visibleHostHeight)
        {
            double scale = GetOverlayScale(hostWidth, visibleHostHeight);
            int throttleWidth = Math.Max(1, (int)Math.Round(240 * scale));
            int throttleHeight = Math.Max(1, (int)Math.Round(120 * scale));
            int angleWidth = Math.Max(1, (int)Math.Round(420 * scale));
            int angleHeight = Math.Max(1, (int)Math.Round(164 * scale));

            pliotTubThrottleGauge.Size = new Size(throttleWidth, throttleHeight);
            pliotAiThrottleGauge.Size = new Size(throttleWidth, throttleHeight);
            pliotAngleIndicator.Size = new Size(angleWidth, angleHeight);
        }

        private static double GetOverlayScale(int hostWidth, int visibleHostHeight)
        {
            double widthScale = hostWidth / 1130.0;
            double heightScale = visibleHostHeight / 629.0;
            double angleFitScale = Math.Max(0.1, (hostWidth - 24) / 420.0);
            double throttleFitScale = Math.Max(0.1, (visibleHostHeight - 42) / 250.0);
            double scale = Math.Min(widthScale, heightScale);
            scale = Math.Min(scale, angleFitScale);
            scale = Math.Min(scale, throttleFitScale);
            return Math.Max(0.34, Math.Min(1.35, scale));
        }

        private static int ClampInt(int value, int min, int max)
        {
            if (max < min) return min;
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

#if false
        private void PnlAngleOverlay_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int centerX = pnlAngleOverlay.Width / 2;
            int centerY = 74;
            int length = Math.Min(185, Math.Max(130, pnlAngleOverlay.Width / 2 - 34));

            using Pen centerPen = new Pen(Color.FromArgb(190, Color.Gainsboro), 3);
            using Pen axisPen = new Pen(Color.FromArgb(90, Color.White), 2);
            using Pen userPen = new Pen(Color.Lime, 6);
            using Pen pilotPen = new Pen(Color.DeepSkyBlue, 6);
            using Brush titleBrush = new SolidBrush(Color.White);
            using Font titleFont = new Font("맑은 고딕", 9F, FontStyle.Bold);

            e.Graphics.DrawLine(centerPen, centerX, 18, centerX, centerY + 18);
            e.Graphics.DrawLine(axisPen, centerX - length, centerY, centerX + length, centerY);
            e.Graphics.DrawString("사용자 방향", titleFont, titleBrush, 14, 12);
            e.Graphics.DrawString("AI 방향", titleFont, titleBrush, pnlAngleOverlay.Width - 66, 12);
            DrawAngleLine(e.Graphics, userPen, centerX, centerY, length, GetCurrentAngleValue(true));
            DrawAngleLine(e.Graphics, pilotPen, centerX, centerY, length, GetCurrentAngleValue(false));
        }

        private double? GetCurrentAngleValue(bool userAngle)
        {
            if (_frameList.Count == 0 || _currentFrameIndex < 0 || _currentFrameIndex >= _frameList.Count)
            {
                return null;
            }

            DonkeyAsyncWorker.PilotFrameData frame = _frameList[_currentFrameIndex];
            return userAngle ? frame.UserAngle : frame.PilotAngle;
        }

        private static void DrawAngleLine(Graphics graphics, Pen pen, int centerX, int centerY, int length, double? angle)
        {
            if (!angle.HasValue)
            {
                return;
            }

            double clamped = Math.Max(-1.0, Math.Min(1.0, angle.Value));
            double radians = clamped * Math.PI / 3.0;
            int endX = centerX + (int)(Math.Sin(radians) * length);
            int endY = centerY - (int)(Math.Cos(radians) * length);

            graphics.DrawLine(pen, centerX, centerY, endX, endY);
            using Brush brush = new SolidBrush(pen.Color);
            graphics.FillEllipse(brush, endX - 5, endY - 5, 10, 10);
        }
#endif

        private int GetVisibleImageHostHeight(int fallbackHeight)
        {
            if (pnlTrackBar.Parent == pnlPilotCard && pnlImageHost.Parent == pnlPilotCard)
            {
                int coveredStartY = pnlTrackBar.Top - pnlImageHost.Top;
                if (coveredStartY > 0)
                {
                    return Math.Max(120, Math.Min(fallbackHeight, coveredStartY - 8));
                }
            }

            return fallbackHeight;
        }

        #endregion

        #region Form Utilities

        private void ResizeModelColumns()
        {
            int width = Math.Max(360, lvModelList.ClientSize.Width);
            colModelNo.Width = Math.Max(45, width / 8);
            colModelName.Width = Math.Max(120, width * 3 / 8);
            colModelPath.Width = Math.Max(160, width - colModelNo.Width - colModelName.Width - 8);
        }

        private void Pliot_FormClosed(object? sender, FormClosedEventArgs e)
        {
            SharedModelRegistry.ModelsChanged -=
                SharedModelRegistry_ModelsChanged;

            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _playbackTimer?.Stop();
            _playbackTimer?.Dispose();
            DisposeCurrentImage();
        }

        #endregion

        #region Nested Types

        private sealed class ModelListItem
        {
            public ModelListItem(string name, string path)
            {
                Name = name;
                Path = path;
            }

            public string Name { get; }
            public string Path { get; }
            public string ModelType { get; set; } = string.Empty;
            public string TubPath { get; set; } = string.Empty;
            public int CurrentFrameIndex { get; set; }
            public bool IsLoaded { get; set; }
            public DonkeyAsyncWorker.PilotCardState? CardState { get; set; }
            public List<DonkeyAsyncWorker.PilotFrameData> Frames { get; set; } =
                new List<DonkeyAsyncWorker.PilotFrameData>();
        }

        #endregion
    }
}
