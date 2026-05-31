using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Data_Manager
{
    public partial class Pliot : Form
    {
        private int pilotCardCount = 0;

        private const int CardGap = 5;

        private const int MaxCards = 4;

        private const int CardHeight = 552;

        // =====================================================
        // 데이터 변수
        // =====================================================

        private string selectedDataPath = "";

        private List<CatalogRecord>
            integratedCatalogList =
            new List<CatalogRecord>();

        private PilotCardControl? pendingModelCard;
        private DonkeyAsyncWorker.PilotCardState? pendingModelState;
        private CancellationTokenSource? modelLoadCts;

        // =====================================================
        // 데이터 구조
        // =====================================================

        public class CatalogRecord
        {
            public string OriginalLine { get; set; }

            public string SourceFilePath { get; set; }

            public int LineIndex { get; set; }

            public string ImageFileName { get; set; }

            public string Angle { get; set; }

            public string Throttle { get; set; }

            public string Index { get; set; }
        }

        // =====================================================
        // 생성자
        // =====================================================

        public Pliot()
        {
            InitializeComponent();

            flowLayoutPanel1.SizeChanged +=
                (s, e) => UpdatePilotCardLayout();

            flowLayoutPanel1.Layout +=
                (s, e) => UpdatePilotCardLayout();

            Shown += Form2_Shown;

            Resize += Form2_Resize;
        }

        // =====================================================
        // SHOWN
        // =====================================================

        private void Form2_Shown(
            object sender,
            EventArgs e)
        {
            UpdatePilotCardLayout();
        }

        // =====================================================
        // RESIZE
        // =====================================================

        private void Form2_Resize(
            object sender,
            EventArgs e)
        {
            UpdatePilotCardLayout();
        }

        // =====================================================
        // CARD ADD
        // =====================================================

        private void BtnCardAdder_Click(
            object sender,
            EventArgs e)
        {
            if (
                flowLayoutPanel1.Controls.Count
                >= MaxCards)
            {
                MessageBox.Show(
                    "파일럿 카드는 최대 4개까지만 추가할 수 있습니다.");

                return;
            }

            PilotCardControl card =
                new PilotCardControl();

            pilotCardCount++;

            card.SetModelName(
                $"테스트 모델 {pilotCardCount}");

            card.SetAngles(
                -15.2 + pilotCardCount * 2,
                -12.6 + pilotCardCount * 1.5);

            card.SetThrottles(
                0.72 - (pilotCardCount * 0.1),
                0.65 - (pilotCardCount * 0.05));

            card.RemoveRequested +=
                Card_RemoveRequested;

            card.ModelSelectRequested +=
                Card_ModelSelectRequested;

            card.TubSelectRequested +=
                Card_TubSelectRequested;

            flowLayoutPanel1.Controls.Add(card);

            BeginInvoke(
                new Action(UpdatePilotCardLayout));
        }

        // =====================================================
        // REMOVE
        // =====================================================

        private void Card_RemoveRequested(
            PilotCardControl card)
        {
            flowLayoutPanel1.Controls.Remove(card);

            card.Dispose();

            UpdatePilotCardLayout();
        }

        // =====================================================
        // MODEL SELECT
        // =====================================================

        private void Card_ModelSelectRequested(
            PilotCardControl card)
        {
            pendingModelCard = card;

            PliotModelList modelList =
                new PliotModelList();

            // TODO: frmNewtrainer 폼의 리스트박스 존재 여부 확인 후 항목 복사
            // if (/* frmNewtrainer 리스트박스에 항목 있음 */)
            // {
            //     modelList.LoadFromTrainerList();
            // }

            modelList.ModelSelected +=
                (name, path) =>
                {
                    ApplySelectedModel(
                        name,
                        path,
                        modelList);
                };

            modelList.Show(this);
        }

        private void ApplySelectedModel(
            string modelName,
            string modelPath,
            Form modelList)
        {
            if (pendingModelCard == null)
            {
                return;
            }

            pendingModelCard.SetModelFilePath(modelPath);

            string? folderPath =
                Path.GetDirectoryName(modelPath);

            if (!string.IsNullOrWhiteSpace(folderPath))
            {
                pendingModelCard.SetModelFolderPath(folderPath);
            }

            pendingModelCard.SetModelName(modelName);

            DonkeyAsyncWorker.PilotCardState cardState = new DonkeyAsyncWorker.PilotCardState
            {
                ModelName = modelName,
                ModelPath = modelPath
            };

            pendingModelState = cardState;
            _ = ReceiveSelectedModelFromPilotModelListAsync(modelName, modelPath);

            pendingModelCard = null;

            modelList.Close();
        }

        private void Card_TubSelectRequested(PilotCardControl card)
        {
            _ = SelectTubFolderAndConnectAsync(card);
        }

        private Task SelectTubFolderAndConnectAsync(
            PilotCardControl card)
        {
            return SelectTubFolderAndConnectCoreAsync(card);
        }

        public async Task ReceiveSelectedModelFromPilotModelListAsync(
            string modelName,
            string modelPath)
        {
            if (pendingModelCard == null)
            {
                return;
            }

            modelLoadCts?.Cancel();
            modelLoadCts = new CancellationTokenSource();
            CancellationToken token = modelLoadCts.Token;

            using ProgressStatusForm progressForm = new ProgressStatusForm();
            progressForm.SetTitle("모델 데이터 연결 중...");
            progressForm.SetIndeterminate(true);
            progressForm.CancelRequested += () => modelLoadCts?.Cancel();
            progressForm.Show(this);

            var progress = new Progress<DonkeyAsyncWorker.ProgressReport>(report =>
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

            try
            {
                await LoadPilotCardFromSelectedModelAsync(
                    pendingModelCard,
                    modelName,
                    modelPath,
                    progress,
                    token);

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

        private async Task LoadPilotCardFromSelectedModelAsync(
            PilotCardControl card,
            string modelName,
            string modelPath,
            IProgress<DonkeyAsyncWorker.ProgressReport> progress,
            CancellationToken token)
        {
            DonkeyAsyncWorker.PilotCardState cardState = new DonkeyAsyncWorker.PilotCardState
            {
                ModelName = modelName,
                ModelPath = modelPath
            };

            progress.Report(new DonkeyAsyncWorker.ProgressReport
            {
                Step = "저장된 mycar 경로 확인 중...",
                Log = "설정 파일에서 mycar 경로를 확인합니다.",
                IsIndeterminate = true
            });

            DonkeyAsyncWorker.OperationResult<string> myCarResult =
                await DonkeyAsyncWorker.FindMyCarPathInWslAsync(
                    cardState.WslDistroName,
                    progress,
                    token);

            if (!myCarResult.Success || string.IsNullOrWhiteSpace(myCarResult.Data))
            {
                throw new InvalidOperationException(myCarResult.ErrorMessage);
            }

            cardState.MyCarPath = myCarResult.Data;

            DonkeyAsyncWorker.OperationResult<DonkeyAsyncWorker.PilotCardState> modelResult =
                await DonkeyAsyncWorker.LoadModelInfoFromDatabaseAsync(
                    cardState,
                    progress,
                    token);

            if (!modelResult.Success || modelResult.Data == null)
            {
                throw new InvalidOperationException(modelResult.ErrorMessage);
            }

            cardState = modelResult.Data;

            progress.Report(new DonkeyAsyncWorker.ProgressReport
            {
                Step = "tub 데이터 파싱 중...",
                Log = "학습 tub 데이터를 비동기로 파싱합니다.",
                IsIndeterminate = true
            });

            DonkeyAsyncWorker.OperationResult<List<DonkeyAsyncWorker.TubDrivingRecord>> tubResult =
                await DonkeyAsyncWorker.LoadTubDrivingRecordsAsync(
                    cardState.TrainingTubPaths,
                    cardState.WslDistroName,
                    progress,
                    token);

            cardState.TubRecords = tubResult.Data ?? new List<DonkeyAsyncWorker.TubDrivingRecord>();
            cardState.IsTubConnected = tubResult.Success && cardState.TubRecords.Count > 0;

            await InvokeAsync(() =>
            {
                if (!cardState.IsTubConnected)
                {
                    DrawTubRequiredMessage(card.GetDrivePictureBox());
                    BindTubDrivingRecordsToGrid(card.GetTubGrid(), cardState.TubRecords, progress);
                }
                else
                {
                    ShowImageInPictureBox(
                        card.GetDrivePictureBox(),
                        cardState.TubRecords.First().ImagePath,
                        cardState.WslDistroName);
                    BindTubDrivingRecordsToGrid(card.GetTubGrid(), cardState.TubRecords, progress);
                }
            });

            DonkeyAsyncWorker.OperationResult<List<DonkeyAsyncWorker.JudementRecord>> judementResult =
                await DonkeyAsyncWorker.CheckOrLoadJudementAsync(
                    cardState,
                    progress,
                    token);

            await InvokeAsync(() =>
            {
                if (judementResult.Success && judementResult.Data != null)
                {
                    BindJudementRecordsToGrid(card.GetTubGrid(), judementResult.Data, progress);
                }
                else
                {
                    progress.Report(new DonkeyAsyncWorker.ProgressReport
                    {
                        Log = "AI 판단 데이터가 아직 없습니다. 생성 버튼을 눌러 생성하세요."
                    });
                }
            });

            pendingModelState = cardState;
        }

        private async Task SelectTubFolderAndConnectCoreAsync(PilotCardControl card)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "tub 폴더 선택";

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (pendingModelState == null)
            {
                pendingModelState = new DonkeyAsyncWorker.PilotCardState
                {
                    ModelName = card.GetModelName(),
                    ModelPath = card.GetModelFilePath()
                };
            }

            pendingModelState.TrainingTubPaths = new List<string>
            {
                DonkeyAsyncWorker.ToWslPathFromWindowsPath(dialog.SelectedPath)
            };

            using ProgressStatusForm progressForm = new ProgressStatusForm();
            progressForm.SetTitle("tub 데이터 연결 중...");
            progressForm.SetIndeterminate(true);
            progressForm.Show(this);

            var progress = new Progress<DonkeyAsyncWorker.ProgressReport>(report =>
            {
                if (!string.IsNullOrWhiteSpace(report.Step))
                {
                    progressForm.SetStep(report.Step);
                }

                if (!string.IsNullOrWhiteSpace(report.Log))
                {
                    progressForm.AppendLog(report.Log);
                }

                progressForm.SetIndeterminate(report.IsIndeterminate);
            });

            DonkeyAsyncWorker.OperationResult<List<DonkeyAsyncWorker.TubDrivingRecord>> tubResult =
                await DonkeyAsyncWorker.LoadTubDrivingRecordsAsync(
                    pendingModelState.TrainingTubPaths,
                    pendingModelState.WslDistroName,
                    progress,
                    CancellationToken.None);

            pendingModelState.TubRecords = tubResult.Data ?? new List<DonkeyAsyncWorker.TubDrivingRecord>();
            pendingModelState.IsTubConnected = tubResult.Success && pendingModelState.TubRecords.Count > 0;

            if (!pendingModelState.IsTubConnected)
            {
                DrawTubRequiredMessage(card.GetDrivePictureBox());
                BindTubDrivingRecordsToGrid(card.GetTubGrid(), pendingModelState.TubRecords, progress);
                progressForm.MarkFailed("tub 데이터를 찾지 못했습니다.");
                return;
            }

            ShowImageInPictureBox(
                card.GetDrivePictureBox(),
                pendingModelState.TubRecords.First().ImagePath,
                pendingModelState.WslDistroName);
            BindTubDrivingRecordsToGrid(card.GetTubGrid(), pendingModelState.TubRecords, progress);
            card.SetTubFolderPath(pendingModelState.TrainingTubPaths.FirstOrDefault() ?? string.Empty);
            progressForm.MarkCompleted("tub 데이터 연결 완료");
        }

        private async Task InvokeAsync(Action action)
        {
            if (InvokeRequired)
            {
                await Task.Run(() => Invoke(action));
                return;
            }

            action();
        }

        private void ShowImageInPictureBox(
            PictureBox pictureBox,
            string imagePath,
            string distroName)
        {
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;

            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
                pictureBox.Image = null;
            }

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                DrawTubRequiredMessage(pictureBox);
                return;
            }

            string windowsPath = DonkeyAsyncWorker.ToWindowsPathFromWslPath(imagePath, distroName);
            if (!File.Exists(windowsPath))
            {
                DrawTubRequiredMessage(pictureBox);
                return;
            }

            using (var stream = new FileStream(windowsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var image = Image.FromStream(stream))
            {
                pictureBox.Image = new Bitmap(image);
            }
        }

        private void DrawTubRequiredMessage(PictureBox pictureBox)
        {
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
                pictureBox.Image = null;
            }

            Bitmap bmp = new Bitmap(pictureBox.Width, pictureBox.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            using (Font font = new Font("맑은 고딕", 16, FontStyle.Bold))
            using (Brush brush = new SolidBrush(Color.Gray))
            {
                g.Clear(Color.LightGray);
                string text = "tub 데이터 필요";
                SizeF size = g.MeasureString(text, font);
                float x = (bmp.Width - size.Width) / 2;
                float y = (bmp.Height - size.Height) / 2;
                g.DrawString(text, font, brush, x, y);
            }

            pictureBox.Image = bmp;
        }

        private void BindTubDrivingRecordsToGrid(
            DataGridView grid,
            List<DonkeyAsyncWorker.TubDrivingRecord> records,
            IProgress<DonkeyAsyncWorker.ProgressReport> progress)
        {
            var data = records.Take(1000).Select(record => new
            {
                record.Index,
                record.ImagePath,
                record.UserAngle,
                record.UserThrottle,
                record.Mode
            }).ToList();

            grid.DataSource = data;

            if (records.Count > 1000)
            {
                progress.Report(new DonkeyAsyncWorker.ProgressReport
                {
                    Log = $"tub 데이터 {records.Count}건 중 1000건만 표시합니다."
                });
            }
        }

        private void BindJudementRecordsToGrid(
            DataGridView grid,
            List<DonkeyAsyncWorker.JudementRecord> records,
            IProgress<DonkeyAsyncWorker.ProgressReport> progress)
        {
            var data = records.Take(1000).Select(record => new
            {
                record.Index,
                record.ImagePath,
                record.UserAngle,
                record.UserThrottle,
                record.PilotAngle,
                record.PilotThrottle,
                record.AngleError,
                record.ThrottleError,
                record.Mode
            }).ToList();

            grid.DataSource = data;

            if (records.Count > 1000)
            {
                progress.Report(new DonkeyAsyncWorker.ProgressReport
                {
                    Log = $"AI 판단 데이터 {records.Count}건 중 1000건만 표시합니다."
                });
            }
        }

        // =====================================================
        // PANEL CAPTURE
        // =====================================================

        // =====================================================
        // CARD LAYOUT
        // =====================================================

        private void UpdatePilotCardLayout()
        {
            int count =
                flowLayoutPanel1.Controls.Count;

            if (count == 0)
            {
                return;
            }

            int availableWidth =
                flowLayoutPanel1.ClientSize.Width -
                flowLayoutPanel1.Padding.Left -
                flowLayoutPanel1.Padding.Right;

            int totalGap =
                CardGap * (count - 1);

            int cardWidth =
                (availableWidth - totalGap) / count;

            int availableHeight =
                flowLayoutPanel1.ClientSize.Height -
                flowLayoutPanel1.Padding.Top -
                flowLayoutPanel1.Padding.Bottom;

            int cardHeight =
                availableHeight;

            foreach (Control control
                in flowLayoutPanel1.Controls)
            {
                control.Margin =
                    new Padding(0, 0, CardGap, 0);

                control.Size =
                    new Size(cardWidth, cardHeight);
            }

            if (
                flowLayoutPanel1.Controls.Count > 0)
            {
                flowLayoutPanel1.Controls[^1]
                    .Margin = new Padding(0);
            }
        }

        // =====================================================
        // LOAD DATA BUTTON
        // =====================================================

        private void btnLoadData1_Click(
            object sender,
            EventArgs e)
        {
            using (FolderBrowserDialog fbd =
                new FolderBrowserDialog())
            {
                fbd.Description =
                    "mycar/data 폴더 선택";

                if (
                    fbd.ShowDialog() ==
                    DialogResult.OK)
                {
                    selectedDataPath =
                        fbd.SelectedPath;

                    integratedCatalogList.Clear();

                    string[] catalogFiles =
                        Directory.GetFiles(
                            selectedDataPath,
                            "catalog_*.catalog");

                    Array.Sort(catalogFiles);

                    foreach (
                        string catalogPath
                        in catalogFiles)
                    {
                        string[] lines =
                            File.ReadAllLines(
                                catalogPath);

                        for (
                            int i = 0;
                            i < lines.Length;
                            i++)
                        {
                            string line =
                                lines[i];

                            if (
                                string.IsNullOrWhiteSpace(
                                    line))
                            {
                                continue;
                            }

                            CatalogRecord record =
                                new CatalogRecord()
                                {
                                    OriginalLine =
                                        line,

                                    SourceFilePath =
                                        catalogPath,

                                    LineIndex =
                                        i,

                                    ImageFileName =
                                        ExtractJsonValue(
                                            line,
                                            "cam/image_array"),

                                    Angle =
                                        ExtractJsonValue(
                                            line,
                                            "user/angle"),

                                    Throttle =
                                        ExtractJsonValue(
                                            line,
                                            "user/throttle"),

                                    Index =
                                        ExtractJsonValue(
                                            line,
                                            "_index")
                                };

                            integratedCatalogList
                                .Add(record);
                        }
                    }

                    MessageBox.Show(
                        $"총 {integratedCatalogList.Count}개 프레임 로드 완료");
                }
            }
        }

        // =====================================================
        // JSON VALUE
        // =====================================================

        private string ExtractJsonValue(
            string json,
            string key)
        {
            try
            {
                string searchKey =
                    $"\"{key}\":";

                int startIdx =
                    json.IndexOf(searchKey);

                if (startIdx == -1)
                    return "";

                startIdx +=
                    searchKey.Length;

                while (
                    startIdx < json.Length &&
                    json[startIdx] == ' ')
                {
                    startIdx++;
                }

                if (json[startIdx] == '"')
                {
                    startIdx++;

                    int endIdx =
                        json.IndexOf(
                            '"',
                            startIdx);

                    return json.Substring(
                        startIdx,
                        endIdx - startIdx);
                }
                else
                {
                    int endIdx =
                        json.IndexOfAny(
                            new char[]
                            {
                                ',',
                                '}'
                            },
                            startIdx);

                    return json.Substring(
                        startIdx,
                        endIdx - startIdx)
                        .Trim();
                }
            }
            catch
            {
                return "";
            }
        }
    }
}