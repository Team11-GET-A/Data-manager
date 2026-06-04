using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DonkeyDataManager
{
    // 학습 진행 상태창입니다.
    // train.py 표준 출력/오류 로그를 보여주고, 취소/중단 요청을 frmNewtrainer에 전달합니다.
    public partial class TrainerStatus : Form
    {
        public event EventHandler? CancelRequested;
        public event EventHandler? StopTrainingRequested;

        private bool trainingActive = true;
        private bool stopRequested;

        public TrainerStatus()
        {
            InitializeComponent();
        }

        public void SetStatus(
            string status,
            string dataPath,
            string modelPath,
            string logPath)
        {
            RunOnUiThread(
                () =>
                {
                    lblStatus.Text = status;
                    txtDataPath.Text = dataPath;
                    txtModelPath.Text = modelPath;
                    txtLogPath.Text = logPath;
                });
        }

        public void AppendLog(string line)
        {
            RunOnUiThread(
                () =>
                {
                    txtLog.AppendText(
                        line +
                        Environment.NewLine);
                    UpdateProgressFromLogLine(line);
                });
        }

        public void MarkFinished(string status)
        {
            RunOnUiThread(
                () =>
                {
                    trainingActive = false;
                    lblStatus.Text = status;
                    if (status.Contains("완료"))
                    {
                        progressBar.Value = progressBar.Maximum;
                    }

                    btnStopTraining.Enabled = false;
                    btnCancel.Enabled = false;
                    btnCancel.Text = "완료";
                });
        }

        private void BtnStopTraining_Click(object sender, EventArgs e)
        {
            if (!trainingActive || stopRequested)
            {
                return;
            }

            stopRequested = true;
            btnStopTraining.Enabled = false;
            btnStopTraining.Text = "중단 요청됨";
            lblStatus.Text = "학습 중단 요청 중... 모델 저장을 기다립니다.";

            StopTrainingRequested?.Invoke(this, EventArgs.Empty);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (!trainingActive)
            {
                return;
            }

            btnCancel.Enabled = false;
            btnStopTraining.Enabled = false;
            btnCancel.Text = "취소 중...";
            lblStatus.Text = "학습 취소 중...";

            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void TrainerStatus_FormClosing(
            object sender,
            FormClosingEventArgs e)
        {
            if (!trainingActive)
            {
                return;
            }

            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                BtnCancel_Click(this, EventArgs.Empty);
            }
        }

        private void UpdateProgressFromLogLine(string line)
        {
            Match match = Regex.Match(
                line,
                @"\bEpoch\s+(\d+)\s*/\s*(\d+)\b",
                RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return;
            }

            if (!int.TryParse(match.Groups[1].Value, out int current) ||
                !int.TryParse(match.Groups[2].Value, out int total) ||
                total <= 0)
            {
                return;
            }

            int percent = Math.Max(
                0,
                Math.Min(
                    progressBar.Maximum,
                    (int)Math.Round(current * progressBar.Maximum / (double)total)));

            progressBar.Value = percent;
        }

        private void RunOnUiThread(Action action)
        {
            try
            {
                if (IsDisposed)
                {
                    return;
                }

                if (InvokeRequired)
                {
                    BeginInvoke(action);
                    return;
                }

                action();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
