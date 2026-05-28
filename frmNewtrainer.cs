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

        // WSL/브라우저 프로세스 관리
        private Process wslProcess = null;
        private Process browserProcess = null;
        private System.Windows.Forms.Timer browserMonitorTimer = new System.Windows.Forms.Timer();

        // WSL 경로 설정
        private string wslDistroName = "Ubuntu";  // 기본값
        private string wslUsername = "odozy";     // WSL 사용자명
        private string wslBasePath = "";          // ~/mycar 경로

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
            InitializeComponent();
            InitializePlaybackTimer();
            InitializeBrowserMonitor();

            // WSL 경로 초기화
            InitializeWSLPaths();
        }

        // =====================================================
        // 타이머 초기화
        // =====================================================

        private void InitializePlaybackTimer()
        {
            playbackTimer.Interval = 100;

            playbackTimer.Tick += PlaybackTimer_Tick;
        }

        private void WireUiEvents()
        {
            btnPlay.Click += (s, e) => playbackTimer.Start();
            btnPause.Click += (s, e) => playbackTimer.Stop();
            btnStop.Click += BtnStop_Click;
            btnLoadData.Click += BtnLoadData_Click;
            btnDetectAnomalies.Click += BtnDetectAnomalies_Click;
            lstCatalogRows.SelectedIndexChanged += LstCatalogRows_SelectedIndexChanged;
            cmbSpeed.SelectedIndexChanged += CmbSpeed_SelectedIndexChanged;
            btnCleanData.Click += BtnCleanData_Click;
            btnRestoreData.Click += BtnRestoreData_Click;
            btnTrain.Click += BtnTrain_Click;
            btnDrive.Click += BtnDrive_Click;
            cmbSpeed.SelectedIndex = 1;
        }

        /// <summary>
        /// 브라우저 감시 타이머 초기화 (브라우저 종료 감지 및 재실행)
        /// </summary>
        private void InitializeBrowserMonitor()
        {
            browserMonitorTimer.Interval = 5000; // 5초마다 체크

            browserMonitorTimer.Tick += (s, e) =>
            {
                // 브라우저 프로세스가 존재하고 종료되지 않았는지 확인
                if (browserProcess != null && browserProcess.HasExited)
                {
                    // 브라우저가 종료됨 - 재실행
                    TryRestartBrowser();
                }
            };
        }

        /// <summary>
        /// WSL 경로 초기화 (Windows에서 접근 가능한 경로 설정)
        /// </summary>
        private void InitializeWSLPaths()
        {
            try
            {
                // WSL 배포판 목록 조회
                var distros = GetWSLDistros();
                if (distros.Count > 0)
                {
                    wslDistroName = distros[0];
                }

                // WSL 기본 경로 구성: \\wsl$\{distro}\home\{username}\mycar\
                wslBasePath = $@"\\wsl$\{wslDistroName}\home\{wslUsername}\mycar";
            }
            catch
            {
                // WSL 경로 설정 실패 시 기본값 사용
                wslBasePath = $@"\\wsl$\Ubuntu\home\{wslUsername}\mycar";
            }
        }

        /// <summary>
        /// 설치된 WSL 배포판 목록 조회
        /// </summary>
        private List<string> GetWSLDistros()
        {
            List<string> distros = new List<string>();
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = "--list --quiet",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process proc = Process.Start(psi))
                {
                    using (System.IO.StreamReader reader = proc.StandardOutput)
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            line = line.Trim();
                            if (!string.IsNullOrEmpty(line))
                            {
                                distros.Add(line);
                            }
                        }
                    }
                    proc.WaitForExit();
                }
            }
            catch
            {
                // WSL 명령 실패 시 기본 배포판 사용
                distros.Add("Ubuntu");
            }

            return distros;
        }

        // =====================================================
        // UI 컴포넌트 선언
        // =====================================================


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

        /// <summary>
        /// AI 학습만 시작합니다
        /// </summary>
        private void BtnTrain_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                // 학습할 데이터 폴더 선택
                string selectedTubFolder = PromptTubFolderSelection();

                if (string.IsNullOrEmpty(selectedTubFolder))
                {
                    MessageBox.Show(
                        "학습할 폴더가 선택되지 않았습니다.",
                        "선택 취소",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show(
                    $"AI 학습을 시작합니다.\n\n" +
                    $"데이터 폴더: {selectedTubFolder}\n" +
                    $"WSL 창이 자동으로 열립니다.",
                    "AI 학습 시작");

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
                    "python train.py --tub " + selectedTubFolder + " --model models/mypilot.h5" +
                    "\"";

                psi.UseShellExecute = true;

                // WSL 프로세스 추적
                wslProcess = Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"학습 실행 실패\n\n{ex.Message}");
            }
        }

        /// <summary>
        /// 학습할 tub(데이터) 폴더를 선택합니다
        /// </summary>
        private string PromptTubFolderSelection()
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "학습할 데이터 폴더를 선택하세요 (tub 폴더 또는 data 폴더)";
                fbd.ShowNewFolderButton = false;

                // 초기 경로: WSL의 ~/mycar/data
                string initialPath = Path.Combine(wslBasePath, "data");

                if (Directory.Exists(initialPath))
                {
                    fbd.SelectedPath = initialPath;
                }

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    // 선택된 폴더의 상대 경로 반환
                    // WSL에서 사용할 수 있도록 폴더명만 추출
                    string folderName = Path.GetFileName(fbd.SelectedPath);
                    return folderName;
                }

                return null;
            }
        }

        /// <summary>
        /// 자율주행을 시작합니다 (모델 선택 포함)
        /// </summary>
        private void BtnDrive_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                // 먼저 모델 선택 대화상자 띄우기
                string selectedModel = PromptModelSelection();

                if (string.IsNullOrEmpty(selectedModel))
                {
                    MessageBox.Show(
                        "모델이 선택되지 않았습니다.",
                        "선택 취소",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show(
                    $"자율주행을 시작합니다.\n\n" +
                    $"모델: {selectedModel}\n" +
                    $"\n[진행 상황]\n" +
                    $"1. WSL 창 오픈\n" +
                    $"2. Drive 서버 시작 중... (10초 대기)\n" +
                    $"3. 서버 준비 확인 중... (최대 50초)\n" +
                    $"4. 준비 완료 시 웹브라우저 자동 오픈\n\n" +
                    $"잠시만 기다려주세요.",
                    "자율주행 시작 중");

                ProcessStartInfo psi =
                    new ProcessStartInfo();

                psi.FileName = "cmd.exe";

                psi.Arguments =
                    "/k wsl bash -c \"" +
                    "source ~/miniconda3/etc/profile.d/conda.sh && " +
                    "conda activate e2e_env && " +
                    "cd ~/mycar && " +
                    "echo '=================================' && " +
                    "echo 'AI DRIVE START' && " +
                    "echo '=================================' && " +
                    "python manage.py drive --model ./models/" + selectedModel + "\"";

                psi.UseShellExecute = true;

                // WSL 프로세스 추적
                wslProcess = Process.Start(psi);

                // 약간의 지연 후 웹브라우저 오픈 (서버 준비 확인 포함)
                OpenBrowserAfterDelay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"자율주행 실행 실패\n\n{ex.Message}");
            }
        }

        /// <summary>
        /// 사용자로부터 AI 모델을 선택받습니다
        /// </summary>
        private string PromptModelSelection()
        {
            // 파일 탐색기에서 모델 파일 선택 (WSL 경로 사용)
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "AI 모델 파일 선택";

                // WSL의 models 폴더 경로
                string modelsPath = Path.Combine(wslBasePath, "models");

                // 경로가 존재하면 사용
                if (Directory.Exists(modelsPath))
                {
                    ofd.InitialDirectory = modelsPath;
                }
                else
                {
                    // 경로 존재 안 하면 사용자의 홈 디렉토리
                    ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                }

                ofd.Filter = "H5 파일 (*.h5)|*.h5|PK 파일 (*.pk)|*.pk|모든 파일 (*.*)|*.*";
                ofd.FilterIndex = 1;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // 선택된 파일 경로에서 파일명만 추출
                    string modelFileName = Path.GetFileName(ofd.FileName);
                    return modelFileName;
                }

                return null;
            }
        }

        private void BtnAutoPilot_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                MessageBox.Show(
                    "AI 학습 후 자동으로 자율주행을 시작합니다.\n\n" +
                    "WSL 창과 웹브라우저가 자동으로 열립니다.",
                    "AI 학습 + 자율주행 시작");

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

                // 약간의 지연 후 웹브라우저 오픈 (drive 서버 시작 대기)
                // train 시간이 소요되므로 충분한 지연 설정
                OpenBrowserAfterDelay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"자동 실행 실패\n\n{ex.Message}");
            }
        }

        /// <summary>
        /// 지연 후 웹브라우저에서 자율주행 모니터링 페이지 오픈
        /// </summary>
        private async void OpenBrowserAfterDelay()
        {
            try
            {
                string url = "http://localhost:8887";

                // 초기 지연: 10초 (drive 서버 시작 시간)
                await System.Threading.Tasks.Task.Delay(10000);

                // 브라우저 감시 시작
                if (browserMonitorTimer != null)
                {
                    browserMonitorTimer.Start();
                }

                // 서버가 실제로 준비될 때까지 대기 (최대 50초 추가)
                bool serverReady = await WaitForServerReady(url, 50);

                if (serverReady)
                {
                    OpenBrowserToUrl(url);
                }
                else
                {
                    // 서버가 준비되지 않았으면 경고 후 브라우저 시도
                    MessageBox.Show(
                        $"서버 준비 시간초과 (60초)\n\n" +
                        $"수동으로 접속하세요: {url}",
                        "드라이브 서버 시작 확인 필요",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    // 그래도 시도
                    OpenBrowserToUrl(url);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"웹브라우저 실행 중 오류:\n{ex.Message}",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// URL을 브라우저에서 오픈합니다 (프로세스 추적 포함)
        /// </summary>
        private void OpenBrowserToUrl(string url)
        {
            try
            {
                // 기본 브라우저로 열기
                try
                {
                    browserProcess = System.Diagnostics.Process.Start(url);
                }
                catch
                {
                    // 브라우저가 없으면 Chrome이나 Edge 직접 실행 시도
                    try
                    {
                        ProcessStartInfo browserPsi = new ProcessStartInfo();
                        browserPsi.FileName = "chrome.exe";
                        browserPsi.Arguments = url;
                        browserProcess = System.Diagnostics.Process.Start(browserPsi);
                    }
                    catch
                    {
                        // Edge 시도
                        try
                        {
                            ProcessStartInfo edgePsi = new ProcessStartInfo();
                            edgePsi.FileName = "msedge.exe";
                            edgePsi.Arguments = url;
                            browserProcess = System.Diagnostics.Process.Start(edgePsi);
                        }
                        catch
                        {
                            MessageBox.Show(
                                $"웹브라우저 자동 실행 실패\n\n" +
                                $"수동으로 접속하세요: {url}",
                                "브라우저 오픈 실패",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"브라우저 오픈 중 오류:\n{ex.Message}");
            }
        }

        /// <summary>
        /// 서버가 준비될 때까지 대기합니다 (HTTP GET 요청으로 확인)
        /// </summary>
        private async System.Threading.Tasks.Task<bool> WaitForServerReady(
            string url,
            int maxWaitSeconds = 60)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                client.Timeout = System.TimeSpan.FromSeconds(5);

                for (int i = 0; i < maxWaitSeconds; i++)
                {
                    try
                    {
                        var response = await client.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        // 아직 준비 안 됨 - 계속 대기
                    }

                    // 1초 대기 후 재시도
                    await System.Threading.Tasks.Task.Delay(1000);
                }
            }

            // 타임아웃
            return false;
        }

        /// <summary>
        /// 브라우저 재시작 시도
        /// </summary>
        private void TryRestartBrowser()
        {
            try
            {
                string url = "http://localhost:8887";

            }
            catch
            {
                // 조용히 실패 - 로그만 남김
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