using System;
using System.Windows.Forms;

namespace DonkeyDataManager
{
    public partial class TrainerStatus : Form
    {
        public event EventHandler? CancelRequested;

        private bool trainingActive = true;

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
                });
        }

        public void MarkFinished(string status)
        {
            RunOnUiThread(
                () =>
                {
                    trainingActive = false;
                    lblStatus.Text = status;
                    btnCancel.Enabled = false;
                    btnCancel.Text = "완료";
                });
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (!trainingActive)
            {
                return;
            }

            btnCancel.Enabled = false;
            btnCancel.Text = "취소 중...";
            lblStatus.Text = "학습 취소 중";

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
