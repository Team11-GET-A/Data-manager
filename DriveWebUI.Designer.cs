namespace DonkeyDataManager
{
    partial class DriveWebUI
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnForward;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel pnlWebViewContainer;

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
            pnlToolbar = new System.Windows.Forms.Panel();
            btnBack = new System.Windows.Forms.Button();
            btnForward = new System.Windows.Forms.Button();
            btnRefresh = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            pnlWebViewContainer = new System.Windows.Forms.Panel();

            // pnlToolbar
            pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            pnlToolbar.Controls.Add(btnBack);
            pnlToolbar.Controls.Add(btnForward);
            pnlToolbar.Controls.Add(btnRefresh);
            pnlToolbar.Controls.Add(lblStatus);
            pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            pnlToolbar.Height = 50;
            pnlToolbar.Padding = new System.Windows.Forms.Padding(5);

            // btnBack
            btnBack.BackColor = System.Drawing.Color.White;
            btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            btnBack.Location = new System.Drawing.Point(10, 10);
            btnBack.Name = "btnBack";
            btnBack.Size = new System.Drawing.Size(35, 30);
            btnBack.TabIndex = 0;
            btnBack.Text = "◀";
            btnBack.UseVisualStyleBackColor = false;
            btnBack.Click += BtnBack_Click;

            // btnForward
            btnForward.BackColor = System.Drawing.Color.White;
            btnForward.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnForward.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            btnForward.Location = new System.Drawing.Point(50, 10);
            btnForward.Name = "btnForward";
            btnForward.Size = new System.Drawing.Size(35, 30);
            btnForward.TabIndex = 1;
            btnForward.Text = "▶";
            btnForward.UseVisualStyleBackColor = false;
            btnForward.Click += BtnForward_Click;

            // btnRefresh
            btnRefresh.BackColor = System.Drawing.Color.White;
            btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRefresh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            btnRefresh.Location = new System.Drawing.Point(90, 10);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new System.Drawing.Size(35, 30);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "🔄";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += BtnRefresh_Click;

            // lblStatus
            lblStatus.AutoSize = true;
            lblStatus.ForeColor = System.Drawing.Color.White;
            lblStatus.Location = new System.Drawing.Point(130, 15);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(100, 15);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "초기화 중...";

            // pnlWebViewContainer
            pnlWebViewContainer.BackColor = System.Drawing.Color.White;
            pnlWebViewContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlWebViewContainer.Location = new System.Drawing.Point(0, 50);
            pnlWebViewContainer.Name = "pnlWebViewContainer";
            pnlWebViewContainer.Size = new System.Drawing.Size(800, 550);
            pnlWebViewContainer.TabIndex = 4;

            // DriveWebUI
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 600);
            Controls.Add(pnlWebViewContainer);
            Controls.Add(pnlToolbar);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Name = "DriveWebUI";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Donkey Car 자율주행 UI";
            WindowState = System.Windows.Forms.FormWindowState.Maximized;
            FormClosing += DriveWebUI_FormClosing;
            ResumeLayout(false);
        }
    }
}
