using System;
using System.Drawing;
using System.Windows.Forms;

namespace Data_Manager
{
    // 진행 상황을 표시하는 전용 폼입니다.
    public class ProgressStatusForm : Form
    {
        private readonly Label lblTitle;
        private readonly Label lblCurrentStep;
        private readonly ProgressBar progressBar;
        private readonly TextBox txtLog;
        private readonly Button btnCancel;
        private readonly Button btnClose;

        public event Action? CancelRequested;

        public ProgressStatusForm()
        {
            Text = "진행 상황";
            Size = new Size(600, 420);
            StartPosition = FormStartPosition.CenterParent;

            lblTitle = new Label
            {
                Location = new Point(12, 12),
                Size = new Size(560, 24),
                Font = new Font("맑은 고딕", 11, FontStyle.Bold)
            };

            lblCurrentStep = new Label
            {
                Location = new Point(12, 42),
                Size = new Size(560, 20)
            };

            progressBar = new ProgressBar
            {
                Location = new Point(12, 70),
                Size = new Size(560, 18)
            };

            txtLog = new TextBox
            {
                Location = new Point(12, 100),
                Size = new Size(560, 230),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };

            btnCancel = new Button
            {
                Location = new Point(12, 340),
                Size = new Size(80, 28),
                Text = "취소"
            };
            btnCancel.Click += (s, e) => CancelRequested?.Invoke();

            btnClose = new Button
            {
                Location = new Point(492, 340),
                Size = new Size(80, 28),
                Text = "닫기",
                Enabled = false
            };
            btnClose.Click += (s, e) => Close();

            Controls.Add(lblTitle);
            Controls.Add(lblCurrentStep);
            Controls.Add(progressBar);
            Controls.Add(txtLog);
            Controls.Add(btnCancel);
            Controls.Add(btnClose);
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
