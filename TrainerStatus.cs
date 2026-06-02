using System;
using System.Drawing;
using System.Windows.Forms;

namespace DonkeyDataManager
{
    public class TrainerStatus : Form
    {
        private readonly Label lblStatus;
        private readonly TextBox txtDataPath;
        private readonly TextBox txtWslPath;
        private readonly TextBox txtModelPath;
        private readonly TextBox txtLogPath;
        private readonly TextBox txtLog;
        private readonly Button btnClose;

        public TrainerStatus()
        {
            Text = "TrainerStatus";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(920, 680);
            MinimumSize = new Size(760, 520);

            TableLayoutPanel root =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 3,
                    Padding = new Padding(14)
                };

            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            lblStatus =
                new Label
                {
                    AutoSize = true,
                    Font = new Font("맑은 고딕", 12F, FontStyle.Bold),
                    Text = "대기 중"
                };

            TableLayoutPanel fields =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    ColumnCount = 2,
                    RowCount = 4,
                    AutoSize = true,
                    Padding = new Padding(0, 12, 0, 12)
                };

            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtDataPath = CreateReadOnlyTextBox();
            txtWslPath = CreateReadOnlyTextBox();
            txtModelPath = CreateReadOnlyTextBox();
            txtLogPath = CreateReadOnlyTextBox();

            AddField(fields, 0, "데이터 폴더", txtDataPath);
            AddField(fields, 1, "WSL 경로", txtWslPath);
            AddField(fields, 2, "모델 저장", txtModelPath);
            AddField(fields, 3, "로그 파일", txtLogPath);

            TableLayoutPanel topPanel =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    ColumnCount = 1,
                    RowCount = 2
                };

            topPanel.RowStyles.Add(
                new RowStyle(SizeType.AutoSize));

            topPanel.RowStyles.Add(
                new RowStyle(SizeType.AutoSize));

            topPanel.Controls.Add(lblStatus, 0, 0);
            topPanel.Controls.Add(fields, 0, 1);

            txtLog =
                new TextBox
                {
                    Dock = DockStyle.Fill,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Both,
                    WordWrap = false,
                    Font = new Font("Consolas", 9F)
                };

            btnClose =
                new Button
                {
                    Anchor = AnchorStyles.Right,
                    Text = "닫기",
                    Width = 110,
                    Height = 36
                };

            btnClose.Click += (s, e) => Close();

            FlowLayoutPanel bottom =
                new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    AutoSize = true
                };

            bottom.Controls.Add(btnClose);

            root.Controls.Add(topPanel, 0, 0);
            root.Controls.Add(txtLog, 0, 1);
            root.Controls.Add(bottom, 0, 2);

            Controls.Add(root);
        }

        public void SetStatus(
            string status,
            string dataPath,
            string wslPath,
            string modelPath,
            string logPath)
        {
            RunOnUiThread(
                () =>
                {
                    lblStatus.Text = status;
                    txtDataPath.Text = dataPath;
                    txtWslPath.Text = wslPath;
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

        private TextBox CreateReadOnlyTextBox()
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true
            };
        }

        private void AddField(
            TableLayoutPanel fields,
            int row,
            string labelText,
            TextBox textBox)
        {
            Label label =
                new Label
                {
                    AutoSize = true,
                    Text = labelText,
                    Anchor = AnchorStyles.Left,
                    Padding = new Padding(0, 6, 8, 0)
                };

            fields.RowStyles.Add(
                new RowStyle(SizeType.AutoSize));

            fields.Controls.Add(label, 0, row);
            fields.Controls.Add(textBox, 1, row);
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
