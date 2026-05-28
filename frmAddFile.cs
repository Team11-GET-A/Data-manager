using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AD_AI_LearningData_Editor;

namespace Data_Manager
{
    public partial class frmAddFile : Form
    {
        private frmMain _mainForm;

        // =====================================================
        // 선택된 프레임 데이터 저장
        // =====================================================

        private List<string> selectedFrameLines =
            new List<string>();

        private List<string> copyTargetPaths =
            new List<string>();

        // =====================================================
        // 생성자
        // =====================================================

        public frmAddFile()
        {
            InitializeComponent();

            InitListViewStyles();

            RegisterEvents();
        }

        public frmAddFile(frmMain mainForm)
            : this()
        {
            _mainForm = mainForm;
        }

        // =====================================================
        // ListView 스타일
        // =====================================================

        private void InitListViewStyles()
        {
            lstviewCopyFile.View =
                View.Details;

            lstviewCopyFile.FullRowSelect =
                true;

            lstviewCopyFile.HeaderStyle =
                ColumnHeaderStyle.None;

            lstviewCopyFile.Columns.Add(
                "Frame Data",
                lstviewCopyFile.Width - 25);

            lstviewAddFile.View =
                View.Details;

            lstviewAddFile.FullRowSelect =
                true;

            lstviewAddFile.HeaderStyle =
                ColumnHeaderStyle.None;

            lstviewAddFile.Columns.Add(
                "Copy Data",
                lstviewAddFile.Width - 25);
        }

        // =====================================================
        // 이벤트 등록
        // =====================================================

        private void RegisterEvents()
        {
            btnSelctFile.Click +=
                btnSelctFile_Click;

            btnCopyFile.Click +=
                btnCopyFile_Click;

            btnAddFile.Click +=
                btnAddFile_Click;
        }

        // =====================================================
        // WSL mycar/data 자동 탐색
        // =====================================================

        private string FindWslMycarDataPath()
        {
            string[] wslRoots =
            {
                @"\\wsl.localhost\Ubuntu\home",
                @"\\wsl.localhost\Ubuntu-22.04\home",
                @"\\wsl.localhost\Ubuntu22.04\home"
            };

            foreach (string root in wslRoots)
            {
                try
                {
                    if (!Directory.Exists(root))
                    {
                        continue;
                    }

                    foreach (string userDir
                        in Directory.GetDirectories(root))
                    {
                        string candidate =
                            Path.Combine(
                                userDir,
                                "mycar",
                                "data");

                        if (Directory.Exists(candidate))
                        {
                            return candidate;
                        }
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        // =====================================================
        // 데이터 폴더 선택
        // =====================================================

        private void btnSelctFile_Click(
            object sender,
            EventArgs e)
        {
            lstviewCopyFile.Items.Clear();

            selectedFrameLines.Clear();

            txtbSelctFile.Clear();

            using (FolderBrowserDialog fbd =
                new FolderBrowserDialog())
            {
                fbd.Description =
                    "mycar/data 폴더 선택";

                string initialPath =
                    FindWslMycarDataPath();

                if (!string.IsNullOrWhiteSpace(initialPath)
                    &&
                    Directory.Exists(initialPath))
                {
                    fbd.SelectedPath =
                        initialPath;
                }

                if (fbd.ShowDialog() ==
                    DialogResult.OK)
                {
                    string selectedDataPath =
                        fbd.SelectedPath;

                    txtbSelctFile.Text =
                        selectedDataPath;

                    // =====================================================
                    // catalog 파일 가져오기
                    // =====================================================

                    string[] catalogFiles =
                        Directory.GetFiles(
                            selectedDataPath,
                            "catalog_*.catalog");

                    Array.Sort(catalogFiles);

                    int totalFrames = 0;

                    // =====================================================
                    // catalog 안의 프레임 데이터 읽기
                    // =====================================================

                    foreach (string catalogPath
                        in catalogFiles)
                    {
                        string[] lines =
                            File.ReadAllLines(
                                catalogPath);

                        foreach (string line
                            in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line))
                            {
                                continue;
                            }

                            selectedFrameLines.Add(line);

                            totalFrames++;

                            // =====================================================
                            // 리스트뷰에는 프레임 번호 표시
                            // =====================================================

                            string frameIndex =
                                ExtractJsonValue(
                                    line,
                                    "_index");

                            string angle =
                                ExtractJsonValue(
                                    line,
                                    "user/angle");

                            string throttle =
                                ExtractJsonValue(
                                    line,
                                    "user/throttle");

                            string imageName =
                                ExtractJsonValue(
                                    line,
                                    "cam/image_array");

                            string displayText =
                                $"Frame {frameIndex} | " +
                                $"Angle:{angle} | " +
                                $"Throttle:{throttle} | " +
                                $"{imageName}";

                            lstviewCopyFile.Items.Add(
                                new ListViewItem(displayText));
                        }
                    }

                    MessageBox.Show(
                        $"총 {catalogFiles.Length}개의 catalog 파일\n" +
                        $"총 {totalFrames}개 프레임 로드 완료",
                        "로드 완료",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }

        // =====================================================
        // COPY 버튼
        // =====================================================

        private void btnCopyFile_Click(
            object sender,
            EventArgs e)
        {
            lstviewAddFile.Items.Clear();

            copyTargetPaths.Clear();

            if (selectedFrameLines.Count == 0)
            {
                MessageBox.Show(
                    "먼저 데이터를 로드하세요.");

                return;
            }

            for (int i = 0;
                i < selectedFrameLines.Count;
                i++)
            {
                string line =
                    selectedFrameLines[i];

                string frameIndex =
                    ExtractJsonValue(
                        line,
                        "_index");

                lstviewAddFile.Items.Add(
                    new ListViewItem(
                        $"Frame_{frameIndex}_Copy"));

                copyTargetPaths.Add(line);
            }

            MessageBox.Show(
                $"총 {copyTargetPaths.Count}개 프레임 복사 준비 완료");
        }

        // =====================================================
        // ADD 버튼
        // =====================================================

        private async void btnAddFile_Click(
            object sender,
            EventArgs e)
        {
            if (copyTargetPaths.Count == 0)
            {
                MessageBox.Show(
                    "복사할 프레임이 없습니다.");

                return;
            }

            string uploadedFolder =
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "UploadedFrames");

            if (!Directory.Exists(uploadedFolder))
            {
                Directory.CreateDirectory(
                    uploadedFolder);
            }

            frmWoking popup =
                new frmWoking();

            popup.StartPosition =
                FormStartPosition.CenterParent;

            popup.Show(this);

            this.Enabled = false;

            await Task.Run(() =>
            {
                for (int i = 0;
                    i < copyTargetPaths.Count;
                    i++)
                {
                    string frameData =
                        copyTargetPaths[i];

                    string savePath =
                        Path.Combine(
                            uploadedFolder,
                            $"frame_{i}.txt");

                    File.WriteAllText(
                        savePath,
                        frameData);

                    int progress =
                        (int)(((double)(i + 1)
                        /
                        copyTargetPaths.Count) * 100);

                    popup.UpdateProgress(progress);

                    Thread.Sleep(1);
                }
            });

            popup.ShowDone();

            this.Enabled = true;

            MessageBox.Show(
                $"총 {copyTargetPaths.Count}개 프레임 저장 완료");

            if (_mainForm != null)
            {
                _mainForm.Invoke(
                    new Action(() =>
                    {
                        _mainForm.LoadUploadedFilesToD();
                    }));
            }
        }

        // =====================================================
        // JSON VALUE 추출
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
                {
                    return "";
                }

                startIdx +=
                    searchKey.Length;

                while (
                    startIdx < json.Length
                    &&
                    json[startIdx] == ' ')
                {
                    startIdx++;
                }

                // 문자열
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
                // 숫자
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

        // =====================================================
        // 닫기
        // =====================================================

        private void btnClose_Click(
            object sender,
            EventArgs e)
        {
            this.Close();
        }

        // =====================================================
        // FORM LOAD
        // =====================================================

        private void frmAddFile_Load(
            object sender,
            EventArgs e)
        {

        }
    }
}