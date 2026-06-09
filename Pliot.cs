using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
        private double _playbackSpeed = 1.0;

        public Pliot()
        {
            InitializeComponent();
            InitializePilotUi();
        }

        #region Initialization

        private void InitializePilotUi()
        {
            // Designer 파일은 배치만 담당하고, 이벤트 연결과 초기 화면 상태는 여기서 모읍니다.
            cmbSpeed.SelectedIndex = 1;
            ApplyOverlayStyles();
            ConfigurePlaybackTimer();
            ApplyPilotDesign();

            // Keep event wiring in one place so Designer files stay focused on layout only.
            btnModelLoad.Text = "모델 폴더 선택";
            btnModelLoad.Click += BtnModelLoad_Click;
            lvModelList.SelectedIndexChanged += LvModelList_SelectedIndexChanged;
            lvModelList.SizeChanged += (s, e) => ResizeModelColumns();

            btnTubInput.Click += BtnTubInput_Click;
            btnGenerateJudement.Click += BtnGenerateJudement_Click;
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

            pnlImageHost.Resize += (s, e) => PositionImageOverlays();
            pliotAngleIndicator.Resize += (s, e) => ConfigureAngleOverlayLayout();
            FormClosed += Pliot_FormClosed;
            SharedModelRegistry.ModelsChanged +=
                SharedModelRegistry_ModelsChanged;

            ResizeModelColumns();
            SyncModelsFromSharedRegistry();
            ConfigureLocationTrackBar();
            ClearModelLabels();
            ConfigurePilotValueControls();
            DrawTubRequiredMessage();
            ConfigureAngleOverlayLayout();
            EnsureImageOverlayParent();
            PositionImageOverlays();
            picPilotImage.SendToBack();
            pnlImageIndexOverlay.BringToFront();
            pliotAiThrottleGauge.BringToFront();
            pliotTubThrottleGauge.BringToFront();
            pliotAngleIndicator.BringToFront();
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

        private void ApplyPilotDesign()
        {
            Font = new Font("맑은 고딕", 10.5F, FontStyle.Regular);
            Text = "파일럿";
            BackColor = PilotBackColor;

            splitMain.BackColor = PilotBackColor;
            pnlLeft.BackColor = PilotBackColor;
            pnlRight.BackColor = PilotBackColor;
            pnlPilotCard.BackColor = PilotPanelColor;
            pnlPilotCard.BorderStyle = BorderStyle.None;
            pnlPilotHeader.BackColor = PilotPanelColor;
            pnlModelLoad.BackColor = PilotBackColor;
            pnlPlaybackControls.BackColor = PilotPanelColor;
            pnlTrackBar.BackColor = PilotPanelColor;
            pnlImageHost.BackColor = PilotBackColor;
            picPilotImage.BackColor = PilotBackColor;

            grpSelectedModel.Text = "선택한 모델 정보";
            grpSelectedModel.ForeColor = PilotTextColor;
            grpSelectedModel.BackColor = PilotPanelColor;
            grpSelectedModel.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);

            lblModelListTitle.Text = "모델 리스트";
            lblModelListTitle.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            lblModelListTitle.ForeColor = PilotTextColor;

            lblSelectedModelNameTitle.Text = "모델명";
            lblSelectedModelPathTitle.Text = "파일 경로";
            lblSelectedModelTypeTitle.Text = "타입";
            lblSelectedTubPathTitle.Text = "Tubs 경로";

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

            StyleModelList();
            StyleComboBox();
            StylePilotButton(btnModelLoad, PilotBlueColor, Color.White);
            StylePilotButton(btnTubInput, PilotCyanColor, Color.FromArgb(10, 24, 32));
            StylePilotButton(btnPilotChart, PilotGreenColor, Color.FromArgb(9, 30, 20));
            StylePilotButton(btnGenerateJudement, PilotOrangeColor, Color.FromArgb(34, 20, 6));
            StylePlaybackButton(btnJumpPrev5, "<< 5");
            StylePlaybackButton(btnPrevImage, "<");
            StylePlaybackButton(btnPlayPause, "▶");
            StylePlaybackButton(btnReversePlay, "◀");
            StylePlaybackButton(btnNextImage, ">");
            StylePlaybackButton(btnJumpNext5, "5 >>");
        }

        private void StyleModelList()
        {
            lvModelList.BackColor = PilotSurfaceColor;
            lvModelList.ForeColor = PilotTextColor;
            lvModelList.Font = new Font("맑은 고딕", 9.5F, FontStyle.Regular);
            lvModelList.BorderStyle = BorderStyle.None;
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

        private void StylePlaybackButton(Button button, string text)
        {
            button.Text = text;
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

        private static async Task<string> GetModelFolderInitialDirectoryAsync()
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
            foreach (string file in files)
            {
                if (AddModel(file))
                {
                    addedCount++;
                }
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
            pliotAngleIndicator.Anchor = AnchorStyles.Bottom;
        }

        private void EnsureImageOverlayParent()
        {
            // PictureBox child controls can show the current image through transparent overlay backgrounds.
            MoveOverlayToPictureBox(pnlImageIndexOverlay);
            MoveOverlayToPictureBox(pliotAiThrottleGauge);
            MoveOverlayToPictureBox(pliotTubThrottleGauge);
            MoveOverlayToPictureBox(pliotAngleIndicator);
        }

        private void MoveOverlayToPictureBox(Control overlay)
        {
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

            BeginInvoke(
                new Action(
                    () =>
                    {
                        if (!IsDisposed)
                        {
                            SyncModelsFromSharedRegistry();
                        }
                    }));
        }

        private void SyncModelsFromSharedRegistry()
        {
            List<SharedModelRegistryEntry> sharedModels =
                SharedModelRegistry.Load();

            HashSet<string> sharedPaths =
                sharedModels
                    .Select(model => model.WindowsPath)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

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
            ResizeModelColumns();
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
            if (!TryGetSelectedModel(out ModelListItem? model))
            {
                return;
            }

            await SelectModelAsync(model!);
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
            _cardState.WslDistroName = await DonkeyAsyncWorker.GetPreferredWslDistroNameAsync(token);

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
                    progressForm.MarkCompleted("모델 정보 연결 완료, tub 데이터는 별도 입력이 필요합니다.");
                    return;
                }

                SetTubPathLabels(tubPaths);
                await LoadTubFramesAsync(tubPaths, progress, token);
                await LoadAndMergeJudementAsync(progress, token);
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
            // 사용자가 선택한 tub 폴더를 WSL 경로로 저장하고,
            // catalog/record/image 정보를 파싱해 프레임 리스트에 올립니다.
            if (_cardState == null)
            {
                if (_selectedModel == null)
                {
                    MessageBox.Show("먼저 모델을 선택해 주세요.", "TUB 입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            dialog.Description = "tub 폴더 선택";
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
                MessageBox.Show("선택한 폴더에서 tub 데이터를 찾지 못했습니다.", "TUB 입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            progressForm.SetTitle("tub 데이터 연결 중...");
            progressForm.SetIndeterminate(true);
            progressForm.Show(this);

            IProgress<DonkeyAsyncWorker.ProgressReport> progress = CreateProgress(progressForm);

            try
            {
                await LoadTubFramesAsync(selectedPathWsls, progress, CancellationToken.None);
                await LoadAndMergeJudementAsync(progress, CancellationToken.None);
                ConfigureLocationTrackBar();
                MoveToFrame(0);
                CacheCurrentModelFrames();
                progressForm.MarkCompleted("tub 데이터 연결 완료");
            }
            catch (Exception ex)
            {
                progressForm.MarkFailed($"오류: {ex.Message}");
                MessageBox.Show(ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnGenerateJudement_Click(object? sender, EventArgs e)
        {
            // 선택 모델과 tub를 Python 추론 스크립트에 넘겨 judement 결과 JSON을 생성하거나 로드합니다.
            // 이후 사용자 주행값과 AI 예측값을 같은 프레임 인덱스로 병합합니다.
            if (_cardState == null || _selectedModel == null)
            {
                MessageBox.Show("먼저 모델을 선택해 주세요.", "AI 판단 생성", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_cardState.TrainingTubPaths == null || _cardState.TrainingTubPaths.Count == 0)
            {
                MessageBox.Show("먼저 TUB 데이터를 연결해 주세요.", "AI 판단 생성", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using ProgressStatusForm progressForm = new ProgressStatusForm();
            progressForm.SetTitle("AI 판단 데이터 생성 중...");
            progressForm.SetIndeterminate(true);
            progressForm.Show(this);

            IProgress<DonkeyAsyncWorker.ProgressReport> progress = CreateProgress(progressForm);

            try
            {
                DonkeyAsyncWorker.OperationResult<List<DonkeyAsyncWorker.JudementRecord>> result =
                    await DonkeyAsyncWorker.GenerateJudementAsync(
                        _cardState,
                        progress,
                        CancellationToken.None);

                if (!result.Success || result.Data == null)
                {
                    progressForm.MarkFailed(result.ErrorMessage);
                    MessageBox.Show(result.ErrorMessage, "AI 판단 생성", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _cardState.JudementRecords = result.Data;
                MergeJudementRecords(result.Data);
                ShowCurrentFrame();
                CacheCurrentModelFrames();
                progressForm.MarkCompleted("AI 판단 데이터 생성 완료");
            }
            catch (Exception ex)
            {
                progressForm.MarkFailed($"오류: {ex.Message}");
                MessageBox.Show(ex.Message, "AI 판단 생성", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("먼저 TUB 데이터를 연결해 주세요.", "그래프", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            CacheCurrentModelFrames();
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
            PositionImageOverlays();
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
            DisposeCurrentImage();

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                DrawImageMissingMessage();
                return;
            }

            string distroName = _cardState?.WslDistroName ?? "Ubuntu-22.04";
            string windowsPath = DonkeyAsyncWorker.ToWindowsPathFromWslPath(imagePath, distroName);
            if (!File.Exists(windowsPath))
            {
                DrawImageMissingMessage();
                return;
            }

            using FileStream stream = new FileStream(windowsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using Image image = Image.FromStream(stream);
            picPilotImage.Image = new Bitmap(image);
            picPilotImage.SendToBack();
        }

        private void DrawTubRequiredMessage()
        {
            DrawMessageImage("tub 데이터 필요");
        }

        private void DrawImageMissingMessage()
        {
            DrawMessageImage("이미지 없음");
        }

        private void DrawMessageImage(string text)
        {
            DisposeCurrentImage();

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
            picPilotImage.SendToBack();
        }

        private void DisposeCurrentImage()
        {
            if (picPilotImage.Image != null)
            {
                Image oldImage = picPilotImage.Image;
                picPilotImage.Image = null;
                oldImage.Dispose();
            }
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
            btnPlayPause.Text = "Ⅱ";
            btnReversePlay.Text = "◀";
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
            btnReversePlay.Text = "Ⅱ";
            btnPlayPause.Text = "▶";
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
            btnPlayPause.Text = "▶";
            btnReversePlay.Text = "◀";
        }

        private int GetPlaybackInterval()
        {
            return AD_AI_LearningData_Editor.frmMain.GetPlaybackIntervalForSpeed(_playbackSpeed);
        }

        #endregion

        #region Overlay Rendering

        private void PositionImageOverlays()
        {
            EnsureImageOverlayParent();

            int hostWidth = picPilotImage.ClientSize.Width;
            int hostHeight = picPilotImage.ClientSize.Height;
            int visibleHostHeight = GetVisibleImageHostHeight(hostHeight);

            ConfigureAngleOverlayLayout();
            UpdatePilotOverlaySizes(hostWidth, visibleHostHeight);
            pnlImageIndexOverlay.Location = new Point(12, 12);
            int margin = Math.Max(12, (int)(hostWidth * 0.016));
            int throttleX = margin;
            int throttleGap = Math.Max(6, (int)(10 * GetOverlayScale(hostWidth, visibleHostHeight)));
            int aiThrottleY =
                Math.Max(82, visibleHostHeight - pliotAiThrottleGauge.Height - margin);
            int tubThrottleY =
                Math.Max(82, aiThrottleY - pliotTubThrottleGauge.Height - throttleGap);
            int angleX = (hostWidth - pliotAngleIndicator.Width) / 2;

            pliotTubThrottleGauge.Location = new Point(
                throttleX,
                tubThrottleY);
            pliotAiThrottleGauge.Location = new Point(
                throttleX,
                aiThrottleY);

            pliotAngleIndicator.Location = new Point(
                Math.Min(
                    Math.Max(margin, angleX),
                    Math.Max(margin, hostWidth - pliotAngleIndicator.Width - margin)),
                Math.Max(12, visibleHostHeight - pliotAngleIndicator.Height - margin));

            picPilotImage.SendToBack();
            pnlImageIndexOverlay.BringToFront();
            pliotAiThrottleGauge.BringToFront();
            pliotTubThrottleGauge.BringToFront();
            pliotAngleIndicator.BringToFront();
            pliotAiThrottleGauge.Invalidate();
            pliotTubThrottleGauge.Invalidate();
            pliotAngleIndicator.Invalidate();
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
            double scale = Math.Min(widthScale, heightScale);
            scale = Math.Min(scale, angleFitScale);
            return Math.Max(0.62, Math.Min(1.35, scale));
        }

        private static int ClampInt(int value, int min, int max)
        {
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
