using System;
using System.Windows.Forms;

namespace Data_Manager
{
    // Long-running model/tub jobs report their current step through this reusable dialog.
    // Pilot 화면의 tub 파싱, 모델 로드, 추론 생성처럼 시간이 걸리는 작업에서 공통으로 사용합니다.
    public partial class ProgressStatusForm : Form
    {
        public event Action? CancelRequested;

        public ProgressStatusForm()
        {
            InitializeComponent();
            btnCancel.Click += (s, e) => CancelRequested?.Invoke();
            btnClose.Click += (s, e) => Close();
        }

        public void SetTitle(string title)
        {
            SafeInvoke(() => lblTitle.Text = title);
        }

        public void SetStep(string step)
        {
            SafeInvoke(() => lblCurrentStep.Text = step);
        }

        public void SetProgress(int percent)
        {
            SafeInvoke(() =>
            {
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = Math.Max(0, Math.Min(100, percent));
            });
        }

        public void SetIndeterminate(bool isIndeterminate)
        {
            SafeInvoke(() =>
            {
                progressBar.Style = isIndeterminate
                    ? ProgressBarStyle.Marquee
                    : ProgressBarStyle.Blocks;
            });
        }

        public void AppendLog(string message)
        {
            SafeInvoke(() =>
            {
                if (IsDisposed || txtLog.IsDisposed)
                {
                    return;
                }

                string time = DateTime.Now.ToString("HH:mm:ss");
                txtLog.AppendText($"[{time}] {message}{Environment.NewLine}");
            });
        }

        public void MarkCompleted(string message)
        {
            SafeInvoke(() =>
            {
                AppendLog(message);
                btnClose.Enabled = true;
                btnCancel.Enabled = false;
            });
        }

        public void MarkFailed(string message)
        {
            SafeInvoke(() =>
            {
                AppendLog(message);
                btnClose.Enabled = true;
                btnCancel.Enabled = false;
            });
        }

        public void MarkCanceled(string message)
        {
            SafeInvoke(() =>
            {
                AppendLog(message);
                btnClose.Enabled = true;
                btnCancel.Enabled = false;
            });
        }

        private void SafeInvoke(Action action)
        {
            if (InvokeRequired)
            {
                Invoke(action);
                return;
            }

            action();
        }
    }
}
