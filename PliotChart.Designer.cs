namespace Data_Manager
{
    partial class PliotChart
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
            pnlHeader = new Panel();
            lblSummary = new Label();
            lblTitle = new Label();
            pnlChart = new Panel();
            pnlHeader.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(30, 34, 39);
            pnlHeader.Controls.Add(lblSummary);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(18, 12, 18, 10);
            pnlHeader.Size = new Size(1040, 78);
            pnlHeader.TabIndex = 0;
            // 
            // lblSummary
            // 
            lblSummary.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            lblSummary.ForeColor = Color.FromArgb(190, 198, 206);
            lblSummary.Location = new Point(20, 43);
            lblSummary.Name = "lblSummary";
            lblSummary.Size = new Size(1000, 22);
            lblSummary.TabIndex = 1;
            lblSummary.Text = "Frames 0 | AI Angle 0 | AI Throttle 0";
            lblSummary.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTitle
            // 
            lblTitle.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            lblTitle.Font = new Font("맑은 고딕", 13F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(18, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(1002, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Pilot Chart";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlChart
            // 
            pnlChart.BackColor = Color.FromArgb(25, 29, 34);
            pnlChart.Dock = DockStyle.Fill;
            pnlChart.Location = new Point(0, 78);
            pnlChart.Name = "pnlChart";
            pnlChart.Padding = new Padding(14);
            pnlChart.Size = new Size(1040, 602);
            pnlChart.TabIndex = 1;
            // 
            // PliotChart
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(18, 21, 25);
            ClientSize = new Size(1040, 680);
            Controls.Add(pnlChart);
            Controls.Add(pnlHeader);
            Font = new Font("맑은 고딕", 11F);
            MinimumSize = new Size(820, 520);
            Name = "PliotChart";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Pilot Chart";
            pnlHeader.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label lblSummary;
        private Label lblTitle;
        private Panel pnlChart;
    }
}
