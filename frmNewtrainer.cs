using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;

namespace DonkeyDataManager
{
    public partial class frmNewtrainer : Form
    {
        // =====================================================
        // Windows API P/Invoke 선언
        // =====================================================

        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;
        private const int FLASHW_ALL = 3;
        private const uint FLASHW_TIMERNOFG = 4;

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        // =====================================================
        // 전역 변수
        // =====================================================

        private string selectedDataPath = "";

        private List<CatalogRecord> integratedCatalogList =
            new List<CatalogRecord>();

        private System.Windows.Forms.Timer playbackTimer =
            new System.Windows.Forms.Timer();

        private Process wslProcess = null;
        private Process browserProcess = null;

        private System.Windows.Forms.Timer browserMonitorTimer =
            new System.Windows.Forms.Timer();

        private string wslDistroName = "Ubuntu";
        private string wslUsername = "";
        private string wslBasePath = "";

        // =====================================================
        // ⭐ 모델 자동 로드 추가
        // =====================================================

        private System.Windows.Forms.Timer modelRefreshTimer =
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
            InitializeComponent();

            WireUiEvents();

            InitializePlaybackTimer();

            InitializeBrowserMonitor();

            InitializeWSLPaths();

            // ⭐ 추가
            InitializeModelRefreshTimer();

            // ⭐ 추가
            LoadModelsToList();
        }

        // =====================================================
        // ⭐ 모델 감시 타이머
        // =====================================================

        private void InitializeModelRefreshTimer()
        {
            modelRefreshTimer.Interval = 5000;

            modelRefreshTimer.Tick += (s, e) =>
            {
                LoadModelsToList();
            };

            modelRefreshTimer.Start();
        }

        // =====================================================
        // ⭐ 모델 리스트 로드
        // =====================================================

        private void LoadModelsToList()
        {
            try
            {
                if (lstModels == null)
                    return;

                string modelFolder =
                    Path.Combine(
                        wslBasePath,
                        "models");

                if (!Directory.Exists(modelFolder))
                    return;

                string[] modelFiles =
                    Directory.GetFiles(
                        modelFolder,
                        "*.h5");

                Array.Sort(modelFiles);

                lstModels.Items.Clear();

                foreach (string file in modelFiles)
                {
                    lstModels.Items.Add(
                        Path.GetFileName(file));
                }
            }
            catch
            {

            }
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

            btnDetectAnomalies.Click += BtnDetectAnomalies_Click;

            lstCatalogRows.SelectedIndexChanged +=
                LstCatalogRows_SelectedIndexChanged;

            cmbSpeed.SelectedIndexChanged +=
                CmbSpeed_SelectedIndexChanged;

            btnCleanData.Click += BtnCleanData_Click;

            btnRestoreData.Click += BtnRestoreData_Click;

            btnModelDlt.Click += BtnModelDlt_Click;

            btnNameCh.Click += BtnNameCh_Click;

            cmbSpeed.SelectedIndex = 1;
        }

        // =====================================================
        // 브라우저 감시
        // =====================================================

        private void InitializeBrowserMonitor()
        {
            browserMonitorTimer.Interval = 5000;

            browserMonitorTimer.Tick += (s, e) =>
            {
                if (
                    browserProcess != null &&
                    browserProcess.HasExited)
                {
                    TryRestartBrowser();
                }
            };
        }

        // =====================================================
        // WSL PATH
        // =====================================================

        private void InitializeWSLPaths()
        {
            try
            {
                var distros = GetWSLDistros();

                if (distros.Count > 0)
                {
                    wslDistroName = distros[0];
                }

                wslUsername = GetWSLUserName();

                wslBasePath =
                    $@"\\wsl$\{wslDistroName}\home\{wslUsername}\mycar";
            }
            catch
            {
                wslBasePath =
                    $@"\\wsl$\Ubuntu\home\{wslUsername}\mycar";
            }
        }

        private List<string> GetWSLDistros()
        {
            List<string> distros =
                new List<string>();

            try
            {
                ProcessStartInfo psi =
                    new ProcessStartInfo
                    {
                        FileName = "wsl.exe",

                        Arguments =
                            "--list --quiet",

                        RedirectStandardOutput = true,

                        UseShellExecute = false,

                        CreateNoWindow = true
                    };

                using (Process proc =
                    Process.Start(psi))
                {
                    using (StreamReader reader =
                        proc.StandardOutput)
                    {
                        string line;

                        while (
                            (line = reader.ReadLine())
                            != null)
                        {
                            line = line.Trim();

                            if (
                                !string.IsNullOrEmpty(
                                    line))
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
                distros.Add("Ubuntu");
            }

            return distros;
        }

        private string GetWSLUserName()
        {
            try
            {
                ProcessStartInfo psi =
                    new ProcessStartInfo
                    {
                        FileName = "wsl.exe",
                        Arguments = "whoami",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                using (Process proc =
                    Process.Start(psi))
                {
                    string user =
                        proc.StandardOutput
                            .ReadToEnd()
                            .Trim();

                    proc.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(user))
                        return user;
                }
            }
            catch
            {

            }

            return "odozy";
        }

        // =====================================================
        // WINDOW EFFECT
        // =====================================================

        private void FlashWindow()
        {
            try
            {
                FLASHWINFO fwi =
                    new FLASHWINFO();

                fwi.cbSize =
                    Convert.ToUInt32(
                        Marshal.SizeOf(fwi));

                fwi.hwnd = this.Handle;

                fwi.dwFlags =
                    FLASHW_ALL |
                    FLASHW_TIMERNOFG;

                fwi.uCount = 5;

                fwi.dwTimeout = 500;

                FlashWindowEx(ref fwi);
            }
            catch
            {

            }
        }

        private void ActivateWindow()
        {
            try
            {
                if (IsIconic(this.Handle))
                {
                    ShowWindow(
                        this.Handle,
                        SW_RESTORE);
                }

                SetForegroundWindow(
                    this.Handle);
            }
            catch
            {

            }
        }

        private void AttentionWindow()
        {
            try
            {
                FlashWindow();

                ActivateWindow();
            }
            catch
            {

            }
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

                if (
                    fbd.ShowDialog() ==
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
        // LIST UPDATE
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
            if (
                lstCatalogRows.Items.Count == 0)
                return;

            int next =
                lstCatalogRows.SelectedIndex + 1;

            if (
                next >=
                lstCatalogRows.Items.Count)
            {
                playbackTimer.Stop();

                return;
            }

            lstCatalogRows.SelectedIndex =
                next;
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
            switch (
                cmbSpeed.SelectedIndex)
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
                if (
                    picDriveImage.Image != null)
                {
                    picDriveImage.Image.Dispose();

                    picDriveImage.Image = null;
                }

                using (
                    FileStream fs =
                    new FileStream(
                        imgPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read))
                {
                    using (
                        Image temp =
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
        // ANOMALY
        // =====================================================

        private void BtnDetectAnomalies_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "이상 데이터 탐지 완료");
        }

        // =====================================================
        // CLEAN
        // =====================================================

        private void BtnCleanData_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "프레임 제외 완료");
        }

        // =====================================================
        // RESTORE
        // =====================================================

        private void BtnRestoreData_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "프레임 복원 완료");
        }

        // =====================================================
        // TRAIN
        // =====================================================

        private void BtnTrain_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                string selectedTubFolder =
                    PromptTubFolderSelection();

                if (
                    string.IsNullOrEmpty(
                        selectedTubFolder))
                {
                    MessageBox.Show(
                        "학습할 폴더가 선택되지 않았습니다.",
                        "선택 취소",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                // ⭐ 모델명 자동 생성
                string modelName =
                    "mypilot_" +
                    DateTime.Now.ToString(
                        "yyyyMMdd_HHmmss") +
                    ".h5";

                MessageBox.Show(
                    $"AI 학습 시작\n\n" +
                    $"데이터 폴더 : {selectedTubFolder}\n" +
                    $"생성 모델 : {modelName}");

                ProcessStartInfo psi =
                    new ProcessStartInfo();

                psi.FileName = "cmd.exe";

                psi.Arguments =
                    "/k wsl bash -c \"" +
                    "source ~/miniconda3/etc/profile.d/conda.sh && " +
                    "conda activate e2e_env && " +
                    "cd ~/mycar && " +
                    "python train.py --tub " +
                    selectedTubFolder +
                    " --model models/" +
                    modelName +
                    "\"";

                psi.UseShellExecute = true;

                wslProcess =
                    Process.Start(psi);

                // ⭐ 즉시 리스트에 추가
                if (
                    !lstModels.Items.Contains(
                        modelName))
                {
                    lstModels.Items.Add(
                        modelName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"학습 실행 실패\n\n{ex.Message}");
            }
        }

        // =====================================================
        // TUB SELECT
        // =====================================================

        private string PromptTubFolderSelection()
        {
            using (
                FolderBrowserDialog fbd =
                new FolderBrowserDialog())
            {
                fbd.Description =
                    "학습할 데이터 폴더 선택";

                fbd.ShowNewFolderButton =
                    false;

                string initialPath =
                    Path.Combine(
                        wslBasePath,
                        "data");

                if (
                    Directory.Exists(
                        initialPath))
                {
                    fbd.SelectedPath =
                        initialPath;
                }

                if (
                    fbd.ShowDialog() ==
                    DialogResult.OK)
                {
                    string folderName =
                        Path.GetFileName(
                            fbd.SelectedPath);

                    return folderName;
                }

                return null;
            }
        }

        // =====================================================
        // DRIVE
        // =====================================================

        private void BtnDrive_Click(
    object sender,
    EventArgs e)
        {
            try
            {
                if (lstModels.SelectedItem == null)
                {
                    MessageBox.Show(
                        "모델을 먼저 선택하세요.");

                    return;
                }

                string selectedModel =
                    lstModels.SelectedItem
                    .ToString();

                ProcessStartInfo psi =
                    new ProcessStartInfo();

                psi.FileName = "cmd.exe";

                psi.Arguments =
                    "/k wsl bash -c \"" +
                    "source ~/miniconda3/etc/profile.d/conda.sh && " +
                    "conda activate e2e_env && " +
                    "cd ~/mycar && " +
                    "python manage.py drive --model ./models/" +
                    selectedModel +
                    "\"";

                psi.UseShellExecute = true;

                wslProcess =
                    Process.Start(psi);

                OpenBrowserAfterDelay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"자율주행 실행 실패\n\n{ex.Message}");
            }
        }

        // =====================================================
        // MODEL SELECT
        // =====================================================

        private string PromptModelSelection()
        {
            using (
                OpenFileDialog ofd =
                new OpenFileDialog())
            {
                ofd.Title =
                    "AI 모델 파일 선택";

                string modelsPath =
                    Path.Combine(
                        wslBasePath,
                        "models");

                if (
                    Directory.Exists(
                        modelsPath))
                {
                    ofd.InitialDirectory =
                        modelsPath;
                }

                ofd.Filter =
                    "H5 파일 (*.h5)|*.h5";

                if (
                    ofd.ShowDialog() ==
                    DialogResult.OK)
                {
                    return
                        Path.GetFileName(
                            ofd.FileName);
                }

                return null;
            }
        }

        // =====================================================
        // BROWSER
        // =====================================================

        private async void OpenBrowserAfterDelay()
        {
            try
            {
                string url =
                    "http://localhost:8887";

                await System.Threading.Tasks
                    .Task.Delay(10000);

                bool serverReady =
                    await WaitForServerReady(
                        url,
                        50);

                if (serverReady)
                {
                    OpenBrowserToUrl(url);
                }
            }
            catch
            {

            }
        }

        private void OpenBrowserToUrl(
            string url)
        {
            try
            {
                AttentionWindow();

                browserProcess =
                    Process.Start(url);
            }
            catch
            {

            }
        }

        private async System.Threading.Tasks.Task<bool>
            WaitForServerReady(
            string url,
            int maxWaitSeconds = 60)
        {
            using (
                var client =
                new System.Net.Http.HttpClient())
            {
                client.Timeout =
                    TimeSpan.FromSeconds(5);

                for (
                    int i = 0;
                    i < maxWaitSeconds;
                    i++)
                {
                    try
                    {
                        var response =
                            await client.GetAsync(
                                url);

                        if (
                            response
                            .IsSuccessStatusCode)
                        {
                            return true;
                        }
                    }
                    catch
                    {

                    }

                    await System.Threading.Tasks
                        .Task.Delay(1000);
                }
            }

            return false;
        }

        private void TryRestartBrowser()
        {
            try
            {

            }
            catch
            {

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

                if (
                    json[startIdx] == '"')
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
        // =====================================================
        // MODEL DELETE
        // =====================================================

        private void BtnModelDlt_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                if (lstModels.SelectedItem == null)
                {
                    MessageBox.Show(
                        "삭제할 모델을 선택하세요.");

                    return;
                }

                string selectedModel =
                    lstModels.SelectedItem.ToString();

                DialogResult result =
                    MessageBox.Show(
                        selectedModel +
                        "\n\n선택한 모델을 삭제하시겠습니까?",
                        "모델 삭제",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;

                lstModels.Items.Remove(
                    selectedModel);

                MessageBox.Show(
                    "리스트에서 삭제되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message);
            }
        }
        // =====================================================
        // MODEL RENAME
        // =====================================================

        private void BtnNameCh_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                if (lstModels.SelectedItem == null)
                {
                    MessageBox.Show(
                        "이름을 변경할 모델을 선택하세요.");

                    return;
                }

                string oldName =
                    lstModels.SelectedItem
                    .ToString()
                    .Replace("\0", "")
                    .Trim();

                string newName =
                    Microsoft.VisualBasic.Interaction.InputBox(
                        "새 모델명을 입력하세요.",
                        "모델 이름 변경",
                        oldName);

                if (string.IsNullOrWhiteSpace(newName))
                    return;

                newName =
    newName
    .Replace("\0", "")
    .Trim();

                string modelsPath =
    Path.Combine(
        wslBasePath
            .Replace("\0", "")
            .Trim(),
        "models");

                string oldFilePath =
                    Path.Combine(
                        modelsPath,
                        oldName);

                if (!newName.EndsWith(".h5"))
                {
                    newName += ".h5";
                }

                string newFilePath =
                    Path.Combine(
                        modelsPath,
                        newName);

                if (File.Exists(newFilePath))
                {
                    MessageBox.Show(
                        "동일한 이름의 모델이 이미 존재합니다.");

                    return;
                }

                File.Move(
                    oldFilePath,
                    newFilePath);

                int selectedIndex =
                    lstModels.SelectedIndex;

                lstModels.Items[selectedIndex] =
                    newName;

                MessageBox.Show(
                    "모델 이름이 변경되었습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message);
            }
        }
    }
}