namespace Data_Manager
{
    partial class ProgressStatusForm
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblCurrentStep = new Label();
            progressBar = new ProgressBar();
            txtLog = new TextBox();
            btnCancel = new Button();
            btnClose = new Button();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTitle.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            lblTitle.Location = new Point(12, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(560, 24);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "진행 상황";
            // 
            // lblCurrentStep
            // 
            lblCurrentStep.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblCurrentStep.Location = new Point(12, 42);
            lblCurrentStep.Name = "lblCurrentStep";
            lblCurrentStep.Size = new Size(560, 20);
            lblCurrentStep.TabIndex = 1;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(12, 70);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(560, 18);
            progressBar.TabIndex = 2;
            // 
            // txtLog
            // 
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.Location = new Point(12, 100);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(560, 230);
            txtLog.TabIndex = 3;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCancel.Location = new Point(12, 340);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 28);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "취소";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Enabled = false;
            btnClose.Location = new Point(492, 340);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(80, 28);
            btnClose.TabIndex = 5;
            btnClose.Text = "닫기";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // ProgressStatusForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(584, 381);
            Controls.Add(btnClose);
            Controls.Add(btnCancel);
            Controls.Add(txtLog);
            Controls.Add(progressBar);
            Controls.Add(lblCurrentStep);
            Controls.Add(lblTitle);
            Font = new Font("맑은 고딕", 11F);
            MinimumSize = new Size(520, 340);
            Name = "ProgressStatusForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "진행 상황";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;
        private Label lblCurrentStep;
        private ProgressBar progressBar;
        private TextBox txtLog;
        private Button btnCancel;
        private Button btnClose;
    }
}
