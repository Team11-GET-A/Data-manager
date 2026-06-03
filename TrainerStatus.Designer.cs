namespace DonkeyDataManager
{
    partial class TrainerStatus
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblStatus = new Label();
            lblDataPath = new Label();
            txtDataPath = new TextBox();
            lblModelPath = new Label();
            txtModelPath = new TextBox();
            lblLogPath = new Label();
            txtLogPath = new TextBox();
            txtLog = new TextBox();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            lblStatus.Location = new Point(18, 16);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(956, 34);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "대기 중";
            // 
            // lblDataPath
            // 
            lblDataPath.AutoSize = true;
            lblDataPath.Location = new Point(20, 66);
            lblDataPath.Name = "lblDataPath";
            lblDataPath.Size = new Size(90, 25);
            lblDataPath.TabIndex = 1;
            lblDataPath.Text = "데이터 폴더";
            // 
            // txtDataPath
            // 
            txtDataPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDataPath.Location = new Point(136, 62);
            txtDataPath.Name = "txtDataPath";
            txtDataPath.ReadOnly = true;
            txtDataPath.Size = new Size(838, 31);
            txtDataPath.TabIndex = 2;
            // 
            // lblModelPath
            // 
            lblModelPath.AutoSize = true;
            lblModelPath.Location = new Point(20, 105);
            lblModelPath.Name = "lblModelPath";
            lblModelPath.Size = new Size(74, 25);
            lblModelPath.TabIndex = 3;
            lblModelPath.Text = "모델 저장";
            // 
            // txtModelPath
            // 
            txtModelPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtModelPath.Location = new Point(136, 101);
            txtModelPath.Name = "txtModelPath";
            txtModelPath.ReadOnly = true;
            txtModelPath.Size = new Size(838, 31);
            txtModelPath.TabIndex = 4;
            // 
            // lblLogPath
            // 
            lblLogPath.AutoSize = true;
            lblLogPath.Location = new Point(20, 144);
            lblLogPath.Name = "lblLogPath";
            lblLogPath.Size = new Size(74, 25);
            lblLogPath.TabIndex = 5;
            lblLogPath.Text = "로그 파일";
            // 
            // txtLogPath
            // 
            txtLogPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtLogPath.Location = new Point(136, 140);
            txtLogPath.Name = "txtLogPath";
            txtLogPath.ReadOnly = true;
            txtLogPath.Size = new Size(838, 31);
            txtLogPath.TabIndex = 6;
            // 
            // txtLog
            // 
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.Font = new Font("Consolas", 9F);
            txtLog.Location = new Point(20, 190);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Both;
            txtLog.Size = new Size(954, 412);
            txtLog.TabIndex = 7;
            txtLog.WordWrap = false;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.BackColor = Color.FromArgb(192, 64, 64);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(834, 618);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(140, 42);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "학습 취소";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += BtnCancel_Click;
            // 
            // TrainerStatus
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(994, 676);
            Controls.Add(btnCancel);
            Controls.Add(txtLog);
            Controls.Add(txtLogPath);
            Controls.Add(lblLogPath);
            Controls.Add(txtModelPath);
            Controls.Add(lblModelPath);
            Controls.Add(txtDataPath);
            Controls.Add(lblDataPath);
            Controls.Add(lblStatus);
            MinimumSize = new Size(860, 560);
            Name = "TrainerStatus";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AI 학습 로그";
            FormClosing += TrainerStatus_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblStatus;
        private Label lblDataPath;
        private TextBox txtDataPath;
        private Label lblModelPath;
        private TextBox txtModelPath;
        private Label lblLogPath;
        private TextBox txtLogPath;
        private TextBox txtLog;
        private Button btnCancel;
    }
}
