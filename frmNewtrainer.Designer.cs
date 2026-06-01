namespace DonkeyDataManager
{
    partial class frmNewtrainer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            lblTitle = new Label();
            btnLoadData = new Button();
            btnDetectAnomalies = new Button();
            lstCatalogRows = new ListBox();
            picDriveImage = new PictureBox();
            pnlPlayback = new Panel();
            btnPlay = new Button();
            btnPause = new Button();
            btnStop = new Button();
            lblSpeed = new Label();
            cmbSpeed = new ComboBox();
            btnCleanData = new Button();
            btnRestoreData = new Button();
            btnTrain = new Button();
            btnDrive = new Button();
            lstModels = new ListBox();
            btnModelDlt = new Button();
            btnNameCh = new Button();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picDriveImage).BeginInit();
            pnlPlayback.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(26, 54, 93);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(4, 5, 4, 5);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(2263, 83);
            pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(21, 22);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(471, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🏎️ DonkeyCar Advanced Data Manager";
            // 
            // btnLoadData
            // 
            btnLoadData.Location = new Point(29, 117);
            btnLoadData.Margin = new Padding(4, 5, 4, 5);
            btnLoadData.Name = "btnLoadData";
            btnLoadData.Size = new Size(314, 67);
            btnLoadData.TabIndex = 1;
            btnLoadData.Text = "📁 데이터 폴더 로드";
            btnLoadData.UseVisualStyleBackColor = true;
            btnLoadData.Click += BtnLoadData_Click;
            // 
            // btnDetectAnomalies
            // 
            btnDetectAnomalies.Location = new Point(357, 117);
            btnDetectAnomalies.Margin = new Padding(4, 5, 4, 5);
            btnDetectAnomalies.Name = "btnDetectAnomalies";
            btnDetectAnomalies.Size = new Size(314, 67);
            btnDetectAnomalies.TabIndex = 2;
            btnDetectAnomalies.Text = "🚨 이상 데이터 탐지";
            btnDetectAnomalies.UseVisualStyleBackColor = true;
            btnDetectAnomalies.Click += BtnDetectAnomalies_Click;
            // 
            // lstCatalogRows
            // 
            lstCatalogRows.Font = new Font("Consolas", 9F);
            lstCatalogRows.FormattingEnabled = true;
            lstCatalogRows.Location = new Point(29, 208);
            lstCatalogRows.Margin = new Padding(4, 5, 4, 5);
            lstCatalogRows.Name = "lstCatalogRows";
            lstCatalogRows.Size = new Size(670, 576);
            lstCatalogRows.TabIndex = 3;
            lstCatalogRows.SelectedIndexChanged += LstCatalogRows_SelectedIndexChanged;
            // 
            // picDriveImage
            // 
            picDriveImage.BorderStyle = BorderStyle.FixedSingle;
            picDriveImage.Location = new Point(729, 208);
            picDriveImage.Margin = new Padding(4, 5, 4, 5);
            picDriveImage.Name = "picDriveImage";
            picDriveImage.Size = new Size(642, 499);
            picDriveImage.SizeMode = PictureBoxSizeMode.Zoom;
            picDriveImage.TabIndex = 4;
            picDriveImage.TabStop = false;
            // 
            // pnlPlayback
            // 
            pnlPlayback.BorderStyle = BorderStyle.FixedSingle;
            pnlPlayback.Controls.Add(btnPlay);
            pnlPlayback.Controls.Add(btnPause);
            pnlPlayback.Controls.Add(btnStop);
            pnlPlayback.Controls.Add(lblSpeed);
            pnlPlayback.Controls.Add(cmbSpeed);
            pnlPlayback.Location = new Point(729, 725);
            pnlPlayback.Margin = new Padding(4, 5, 4, 5);
            pnlPlayback.Name = "pnlPlayback";
            pnlPlayback.Size = new Size(642, 74);
            pnlPlayback.TabIndex = 5;
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(14, 12);
            btnPlay.Margin = new Padding(4, 5, 4, 5);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(86, 50);
            btnPlay.TabIndex = 0;
            btnPlay.Text = "▶";
            btnPlay.UseVisualStyleBackColor = true;
            // 
            // btnPause
            // 
            btnPause.Location = new Point(114, 12);
            btnPause.Margin = new Padding(4, 5, 4, 5);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(86, 50);
            btnPause.TabIndex = 1;
            btnPause.Text = "⏸";
            btnPause.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            btnStop.Location = new Point(214, 12);
            btnStop.Margin = new Padding(4, 5, 4, 5);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(86, 50);
            btnStop.TabIndex = 2;
            btnStop.Text = "⏹";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += BtnStop_Click;
            // 
            // lblSpeed
            // 
            lblSpeed.AutoSize = true;
            lblSpeed.Location = new Point(329, 20);
            lblSpeed.Margin = new Padding(4, 0, 4, 0);
            lblSpeed.Name = "lblSpeed";
            lblSpeed.Size = new Size(52, 25);
            lblSpeed.TabIndex = 3;
            lblSpeed.Text = "속도:";
            // 
            // cmbSpeed
            // 
            cmbSpeed.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSpeed.FormattingEnabled = true;
            cmbSpeed.Items.AddRange(new object[] { "0.5x", "1.0x", "2.0x", "5.0x" });
            cmbSpeed.Location = new Point(386, 13);
            cmbSpeed.Margin = new Padding(4, 5, 4, 5);
            cmbSpeed.Name = "cmbSpeed";
            cmbSpeed.Size = new Size(170, 33);
            cmbSpeed.TabIndex = 4;
            cmbSpeed.SelectedIndexChanged += CmbSpeed_SelectedIndexChanged;
            // 
            // btnCleanData
            // 
            btnCleanData.Location = new Point(729, 825);
            btnCleanData.Margin = new Padding(4, 5, 4, 5);
            btnCleanData.Name = "btnCleanData";
            btnCleanData.Size = new Size(314, 75);
            btnCleanData.TabIndex = 6;
            btnCleanData.Text = "✂️ 선택 프레임 제외";
            btnCleanData.UseVisualStyleBackColor = true;
            btnCleanData.Click += BtnCleanData_Click;
            // 
            // btnRestoreData
            // 
            btnRestoreData.Location = new Point(1057, 825);
            btnRestoreData.Margin = new Padding(4, 5, 4, 5);
            btnRestoreData.Name = "btnRestoreData";
            btnRestoreData.Size = new Size(314, 75);
            btnRestoreData.TabIndex = 7;
            btnRestoreData.Text = "⏪ 선택 프레임 복원";
            btnRestoreData.UseVisualStyleBackColor = true;
            btnRestoreData.Click += BtnRestoreData_Click;
            // 
            // btnTrain
            // 
            btnTrain.BackColor = Color.FromArgb(43, 108, 176);
            btnTrain.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            btnTrain.ForeColor = Color.White;
            btnTrain.Location = new Point(729, 933);
            btnTrain.Margin = new Padding(4, 5, 4, 5);
            btnTrain.Name = "btnTrain";
            btnTrain.Size = new Size(314, 100);
            btnTrain.TabIndex = 8;
            btnTrain.Text = "\U0001f9e0 AI 학습 시작";
            btnTrain.UseVisualStyleBackColor = false;
            btnTrain.Click += BtnTrain_Click;
            // 
            // btnDrive
            // 
            btnDrive.BackColor = Color.FromArgb(34, 139, 34);
            btnDrive.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            btnDrive.ForeColor = Color.White;
            btnDrive.Location = new Point(1057, 933);
            btnDrive.Margin = new Padding(4, 5, 4, 5);
            btnDrive.Name = "btnDrive";
            btnDrive.Size = new Size(314, 100);
            btnDrive.TabIndex = 9;
            btnDrive.Text = "🚗 자율주행 시작";
            btnDrive.UseVisualStyleBackColor = false;
            btnDrive.Click += BtnDrive_Click;
            // 
            // lstModels
            // 
            lstModels.Font = new Font("Consolas", 9F);
            lstModels.FormattingEnabled = true;
            lstModels.Location = new Point(29, 794);
            lstModels.Margin = new Padding(4, 5, 4, 5);
            lstModels.Name = "lstModels";
            lstModels.Size = new Size(670, 246);
            lstModels.TabIndex = 10;
            // 
            // btnModelDlt
            // 
            btnModelDlt.BackColor = Color.Red;
            btnModelDlt.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnModelDlt.ForeColor = Color.White;
            btnModelDlt.Location = new Point(29, 1048);
            btnModelDlt.Name = "btnModelDlt";
            btnModelDlt.Size = new Size(182, 80);
            btnModelDlt.TabIndex = 11;
            btnModelDlt.Text = "모델 삭제";
            btnModelDlt.UseVisualStyleBackColor = false;
            // 
            // btnNameCh
            // 
            btnNameCh.BackColor = Color.FromArgb(0, 192, 0);
            btnNameCh.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnNameCh.ForeColor = Color.Black;
            btnNameCh.Location = new Point(217, 1048);
            btnNameCh.Name = "btnNameCh";
            btnNameCh.Size = new Size(182, 80);
            btnNameCh.TabIndex = 12;
            btnNameCh.Text = "이름 변경";
            btnNameCh.UseVisualStyleBackColor = false;
            // 
            // frmNewtrainer
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(2263, 1435);
            Controls.Add(btnNameCh);
            Controls.Add(btnModelDlt);
            Controls.Add(lstModels);
            Controls.Add(btnDrive);
            Controls.Add(btnTrain);
            Controls.Add(btnRestoreData);
            Controls.Add(btnCleanData);
            Controls.Add(pnlPlayback);
            Controls.Add(picDriveImage);
            Controls.Add(lstCatalogRows);
            Controls.Add(btnDetectAnomalies);
            Controls.Add(btnLoadData);
            Controls.Add(pnlHeader);
            Margin = new Padding(4, 5, 4, 5);
            Name = "frmNewtrainer";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "🏎️ DonkeyCar Advanced Data Manager";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picDriveImage).EndInit();
            pnlPlayback.ResumeLayout(false);
            pnlPlayback.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Label lblTitle;
        private Button btnLoadData;
        private Button btnDetectAnomalies;
        private ListBox lstCatalogRows;
        private PictureBox picDriveImage;
        private Panel pnlPlayback;
        private Button btnPlay;
        private Button btnPause;
        private Button btnStop;
        private Label lblSpeed;
        private ComboBox cmbSpeed;
        private Button btnCleanData;
        private Button btnRestoreData;
        private Button btnTrain;
        private Button btnDrive;
        private ListBox lstModels;
        private Button btnModelDlt;
        private Button btnNameCh;
    }
}
