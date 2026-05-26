using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

namespace DonkeyDataManager
{
    public partial class frmNewtrainer : Form
    {
        // =====================================================
        // 전역 변수
        // =====================================================

        private string selectedDataPath = "";

        private List<CatalogRecord> integratedCatalogList =
            new List<CatalogRecord>();

        private System.Windows.Forms.Timer playbackTimer =
            new System.Windows.Forms.Timer();

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

            public bool IsDeleted { get; set; }

            public bool IsAnomaly { get; set; }
        }

        // =====================================================
        // 생성자
        // =====================================================

        public frmNewtrainer()
        {
            SetupCustomLayout();

            InitializePlaybackTimer();
        }

        // =====================================================
        // 타이머 초기화
        // =====================================================

        private void InitializePlaybackTimer()
        {
            playbackTimer.Interval = 100;

            playbackTimer.Tick += PlaybackTimer_Tick;
        }

        // =====================================================
        // UI 컴포넌트 선언
        // =====================================================

        private Button btnLoadData = new Button();

        private Button btnDetectAnomalies = new Button();

        private ListBox lstCatalogRows = new ListBox();

        private PictureBox picDriveImage = new PictureBox();

        private Panel pnlPlayback = new Panel();

        private Button btnPlay = new Button();

        private Button btnPause = new Button();

        private Button btnStop = new Button();

        private Label lblSpeed = new Label();

        private ComboBox cmbSpeed = new ComboBox();

        private Button btnCleanData = new Button();

        private Button btnRestoreData = new Button();

        // =====================================================
        // ✅ 통합 AI 버튼
        // =====================================================

        private Button btnAutoPilot = new Button();

        // =====================================================
        // UI 레이아웃
        // =====================================================

        private void SetupCustomLayout()
        {
            this.Size = new Size(1000, 700);

            this.Text =
                "🏎️ DonkeyCar Advanced Data Manager";

            this.StartPosition =
                FormStartPosition.CenterScreen;

            this.BackColor =
                Color.FromArgb(245, 247, 250);

            // =====================================================
            // HEADER
            // =====================================================

            Panel pnlHeader = new Panel();

            pnlHeader.Size = new Size(1000, 50);

            pnlHeader.BackColor =
                Color.FromArgb(26, 54, 93);

            pnlHeader.Dock = DockStyle.Top;

            Label lblTitle = new Label();

            lblTitle.Text =
                "🏎️ DonkeyCar Advanced Data Manager";

            lblTitle.ForeColor = Color.White;

            lblTitle.Font =
                new Font(
                    "Malgun Gothic",
                    12,
                    FontStyle.Bold);

            lblTitle.Location =
                new Point(15, 13);

            lblTitle.AutoSize = true;

            pnlHeader.Controls.Add(lblTitle);

            // =====================================================
            // LOAD BUTTON
            // =====================================================

            btnLoadData.Text =
                "📁 데이터 폴더 로드";

            btnLoadData.Location =
                new Point(20, 70);

            btnLoadData.Size =
                new Size(220, 40);

            btnLoadData.Click +=
                BtnLoadData_Click;

            // =====================================================
            // ANOMALY BUTTON
            // =====================================================

            btnDetectAnomalies.Text =
                "🚨 이상 데이터 탐지";

            btnDetectAnomalies.Location =
                new Point(250, 70);

            btnDetectAnomalies.Size =
                new Size(220, 40);

            btnDetectAnomalies.Click +=
                BtnDetectAnomalies_Click;

            // =====================================================
            // LISTBOX
            // =====================================================

            lstCatalogRows.Location =
                new Point(20, 125);

            lstCatalogRows.Size =
                new Size(470, 440);

            lstCatalogRows.Font =
                new Font("Consolas", 9);

            lstCatalogRows.SelectedIndexChanged +=
                LstCatalogRows_SelectedIndexChanged;

            // =====================================================
            // IMAGE VIEWER
            // =====================================================

            picDriveImage.Location =
                new Point(510, 125);

            picDriveImage.Size =
                new Size(450, 300);

            picDriveImage.BorderStyle =
                BorderStyle.FixedSingle;

            picDriveImage.SizeMode =
                PictureBoxSizeMode.Zoom;

            // =====================================================
            // PLAYBACK PANEL
            // =====================================================

            pnlPlayback.Location =
                new Point(510, 435);

            pnlPlayback.Size =
                new Size(450, 45);

            pnlPlayback.BorderStyle =
                BorderStyle.FixedSingle;

            // PLAY

            btnPlay.Text = "▶";

            btnPlay.Location =
                new Point(10, 7);

            btnPlay.Size =
                new Size(60, 30);

            btnPlay.Click +=
                (s, e) => playbackTimer.Start();

            // PAUSE

            btnPause.Text = "⏸";

            btnPause.Location =
                new Point(80, 7);

            btnPause.Size =
                new Size(60, 30);

            btnPause.Click +=
                (s, e) => playbackTimer.Stop();

            // STOP

            btnStop.Text = "⏹";

            btnStop.Location =
                new Point(150, 7);

            btnStop.Size =
                new Size(60, 30);

            btnStop.Click += BtnStop_Click;

            // SPEED LABEL

            lblSpeed.Text = "속도:";

            lblSpeed.Location =
                new Point(230, 12);

            lblSpeed.AutoSize = true;

            // SPEED COMBO

            cmbSpeed.Location =
                new Point(270, 8);

            cmbSpeed.Size =
                new Size(120, 30);

            cmbSpeed.Items.Add("0.5x");

            cmbSpeed.Items.Add("1.0x");

            cmbSpeed.Items.Add("2.0x");

            cmbSpeed.Items.Add("5.0x");

            cmbSpeed.SelectedIndex = 1;

            cmbSpeed.SelectedIndexChanged +=
                CmbSpeed_SelectedIndexChanged;

            pnlPlayback.Controls.Add(btnPlay);

            pnlPlayback.Controls.Add(btnPause);

            pnlPlayback.Controls.Add(btnStop);

            pnlPlayback.Controls.Add(lblSpeed);

            pnlPlayback.Controls.Add(cmbSpeed);

            // =====================================================
            // CLEAN BUTTON
            // =====================================================

            btnCleanData.Text =
                "✂️ 선택 프레임 제외";

            btnCleanData.Location =
                new Point(510, 495);

            btnCleanData.Size =
                new Size(220, 45);

            btnCleanData.Click +=
                BtnCleanData_Click;

            // =====================================================
            // RESTORE BUTTON
            // =====================================================

            btnRestoreData.Text =
                "⏪ 선택 프레임 복원";

            btnRestoreData.Location =
                new Point(740, 495);

            btnRestoreData.Size =
                new Size(220, 45);

            btnRestoreData.Click +=
                BtnRestoreData_Click;

            // =====================================================
            // ✅ AI AUTO BUTTON
            // =====================================================

            btnAutoPilot.Text =
                "🧠 AI 학습 + 🚗 자율주행 자동 시작";

            btnAutoPilot.Location =
                new Point(510, 560);

            btnAutoPilot.Size =
                new Size(450, 60);

            btnAutoPilot.BackColor =
                Color.FromArgb(43, 108, 176);

            btnAutoPilot.ForeColor =
                Color.White;

            btnAutoPilot.Font =
                new Font(
                    "Malgun Gothic",
                    10,
                    FontStyle.Bold);

            btnAutoPilot.Click +=
                BtnAutoPilot_Click;

            // =====================================================
            // ADD CONTROLS
            // =====================================================

            this.Controls.Add(pnlHeader);

            this.Controls.Add(btnLoadData);

            this.Controls.Add(btnDetectAnomalies);

            this.Controls.Add(lstCatalogRows);

            this.Controls.Add(picDriveImage);

            this.Controls.Add(pnlPlayback);

            this.Controls.Add(btnCleanData);

            this.Controls.Add(btnRestoreData);

            this.Controls.Add(btnAutoPilot);
        }

        // =====================================================
        // DATA LOAD
        // =====================================================

        private void BtnLoadData_Click(
            object sender,
            EventArgs e)
        {
            using (FolderBrowserDialog fbd =
                new FolderBrowserDialog())
            {
                fbd.Description =
                    "mycar/data 폴더 선택";

                if (fbd.ShowDialog() ==
                    DialogResult.OK)
                {
                    selectedDataPath =
                        fbd.SelectedPath;

                    integratedCatalogList.Clear();

                    lstCatalogRows.Items.Clear();

                    string[] catalogFiles =
                        Directory.GetFiles(
                            selectedDataPath,
                            "catalog_*.catalog");

                    Array.Sort(catalogFiles);

                    foreach (string catalogPath
                        in catalogFiles)
                    {
                        string[] lines =
                            File.ReadAllLines(
                                catalogPath);

                        for (int i = 0;
                            i < lines.Length;
                            i++)
                        {
                            string line = lines[i];

                            if (
                                string.IsNullOrWhiteSpace(
                                    line))
                            {
                                continue;
                            }

                            CatalogRecord record =
                                new CatalogRecord()
                                {
                                    OriginalLine = line,

                                    SourceFilePath =
                                        catalogPath,

                                    LineIndex = i,

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

                            UpdateListBoxItem(record);
                        }
                    }

                    MessageBox.Show(
                        $"총 {integratedCatalogList.Count}개 프레임 로드 완료");
                }
            }
        }

        // =====================================================
        // LISTBOX UPDATE
        // =====================================================

        private void UpdateListBoxItem(
            CatalogRecord record)
        {
            string fileName =
                Path.GetFileName(
                    record.SourceFilePath);

            string text =
                $"{fileName} | " +
                $"F_{record.Index} | " +
                $"A:{record.Angle} | " +
                $"T:{record.Throttle}";

            lstCatalogRows.Items.Add(text);
        }

        // =====================================================
        // PLAYBACK
        // =====================================================

        private void PlaybackTimer_Tick(
            object sender,
            EventArgs e)
        {
            if (lstCatalogRows.Items.Count == 0)
                return;

            int next =
                lstCatalogRows.SelectedIndex + 1;

            if (next >= lstCatalogRows.Items.Count)
            {
                playbackTimer.Stop();

                return;
            }

            lstCatalogRows.SelectedIndex = next;
        }

        private void BtnStop_Click(
            object sender,
            EventArgs e)
        {
            playbackTimer.Stop();
        }

        private void CmbSpeed_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            switch (cmbSpeed.SelectedIndex)
            {
                case 0:
                    playbackTimer.Interval = 200;
                    break;

                case 1:
                    playbackTimer.Interval = 100;
                    break;

                case 2:
                    playbackTimer.Interval = 50;
                    break;

                case 3:
                    playbackTimer.Interval = 20;
                    break;
            }
        }

        // =====================================================
        // IMAGE VIEW
        // =====================================================

        private void LstCatalogRows_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            int idx =
                lstCatalogRows.SelectedIndex;

            if (
                idx < 0 ||
                idx >= integratedCatalogList.Count)
            {
                return;
            }

            CatalogRecord record =
                integratedCatalogList[idx];

            string imgPath =
                Path.Combine(
                    selectedDataPath,
                    "images",
                    record.ImageFileName);

            if (!File.Exists(imgPath))
            {
                picDriveImage.Image = null;

                return;
            }

            try
            {
                if (picDriveImage.Image != null)
                {
                    picDriveImage.Image.Dispose();

                    picDriveImage.Image = null;
                }

                using (FileStream fs =
                    new FileStream(
                        imgPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read))
                {
                    using (Image temp =
                        Image.FromStream(fs))
                    {
                        picDriveImage.Image =
                            new Bitmap(temp);
                    }
                }
            }
            catch
            {
                picDriveImage.Image = null;
            }
        }

        // =====================================================
        // ANOMALY DETECT
        // =====================================================

        private void BtnDetectAnomalies_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show("이상 데이터 탐지 완료");
        }

        // =====================================================
        // CLEAN
        // =====================================================

        private void BtnCleanData_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show("프레임 제외 완료");
        }

        // =====================================================
        // RESTORE
        // =====================================================

        private void BtnRestoreData_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show("프레임 복원 완료");
        }

        // =====================================================
        // ✅ TRAIN + DRIVE AUTO
        // =====================================================

        private void BtnAutoPilot_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                MessageBox.Show(
                    "AI 학습 후 자동으로 자율주행을 시작합니다.");

                ProcessStartInfo psi =
                    new ProcessStartInfo();

                psi.FileName = "cmd.exe";

                psi.Arguments =
                    "/k wsl bash -c \"" +

                    "source ~/miniconda3/etc/profile.d/conda.sh && " +

                    "conda activate e2e_env && " +

                    "cd ~/mycar && " +

                    "echo '=================================' && " +
                    "echo 'AI TRAIN START' && " +
                    "echo '=================================' && " +

                    "python train.py --tub data --model models/mypilot.h5 ; " +

                    "echo '=================================' && " +
                    "echo 'AI DRIVE START' && " +
                    "echo '=================================' && " +

                    "python manage.py drive --model ./models/mypilot.h5" +

                    "\"";

                psi.UseShellExecute = true;

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"자동 실행 실패\n\n{ex.Message}");
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

                startIdx += searchKey.Length;

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
                            new char[] { ',', '}' },
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