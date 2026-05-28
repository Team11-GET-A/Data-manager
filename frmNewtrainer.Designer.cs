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
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1584, 50);
            pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(15, 13);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(321, 21);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🏎️ DonkeyCar Advanced Data Manager";
            // 
            // btnLoadData
            // 
            btnLoadData.Location = new Point(20, 70);
            btnLoadData.Name = "btnLoadData";
            btnLoadData.Size = new Size(220, 40);
            btnLoadData.TabIndex = 1;
            btnLoadData.Text = "📁 데이터 폴더 로드";
            btnLoadData.UseVisualStyleBackColor = true;
            btnLoadData.Click += BtnLoadData_Click;
            // 
            // btnDetectAnomalies
            // 
            btnDetectAnomalies.Location = new Point(250, 70);
            btnDetectAnomalies.Name = "btnDetectAnomalies";
            btnDetectAnomalies.Size = new Size(220, 40);
            btnDetectAnomalies.TabIndex = 2;
            btnDetectAnomalies.Text = "🚨 이상 데이터 탐지";
            btnDetectAnomalies.UseVisualStyleBackColor = true;
            btnDetectAnomalies.Click += BtnDetectAnomalies_Click;
            // 
            // lstCatalogRows
            // 
            lstCatalogRows.Font = new Font("Consolas", 9F);
            lstCatalogRows.FormattingEnabled = true;
            lstCatalogRows.Location = new Point(20, 125);
            lstCatalogRows.Name = "lstCatalogRows";
            lstCatalogRows.Size = new Size(470, 438);
            lstCatalogRows.TabIndex = 3;
            lstCatalogRows.SelectedIndexChanged += LstCatalogRows_SelectedIndexChanged;
            // 
            // picDriveImage
            // 
            picDriveImage.BorderStyle = BorderStyle.FixedSingle;
            picDriveImage.Location = new Point(510, 125);
            picDriveImage.Name = "picDriveImage";
            picDriveImage.Size = new Size(450, 300);
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
            pnlPlayback.Location = new Point(510, 435);
            pnlPlayback.Name = "pnlPlayback";
            pnlPlayback.Size = new Size(450, 45);
            pnlPlayback.TabIndex = 5;
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(10, 7);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(60, 30);
            btnPlay.TabIndex = 0;
            btnPlay.Text = "▶";
            btnPlay.UseVisualStyleBackColor = true;
            // 
            // btnPause
            // 
            btnPause.Location = new Point(80, 7);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(60, 30);
            btnPause.TabIndex = 1;
            btnPause.Text = "⏸";
            btnPause.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            btnStop.Location = new Point(150, 7);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(60, 30);
            btnStop.TabIndex = 2;
            btnStop.Text = "⏹";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += BtnStop_Click;
            // 
            // lblSpeed
            // 
            lblSpeed.AutoSize = true;
            lblSpeed.Location = new Point(230, 12);
            lblSpeed.Name = "lblSpeed";
            lblSpeed.Size = new Size(34, 15);
            lblSpeed.TabIndex = 3;
            lblSpeed.Text = "속도:";
            // 
            // cmbSpeed
            // 
            cmbSpeed.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSpeed.FormattingEnabled = true;
            cmbSpeed.Items.AddRange(new object[] { "0.5x", "1.0x", "2.0x", "5.0x" });
            cmbSpeed.Location = new Point(270, 8);
            cmbSpeed.Name = "cmbSpeed";
            cmbSpeed.Size = new Size(120, 23);
            cmbSpeed.TabIndex = 4;
            cmbSpeed.SelectedIndexChanged += CmbSpeed_SelectedIndexChanged;
            // 
            // btnCleanData
            // 
            btnCleanData.Location = new Point(510, 495);
            btnCleanData.Name = "btnCleanData";
            btnCleanData.Size = new Size(220, 45);
            btnCleanData.TabIndex = 6;
            btnCleanData.Text = "✂️ 선택 프레임 제외";
            btnCleanData.UseVisualStyleBackColor = true;
            btnCleanData.Click += BtnCleanData_Click;
            // 
            // btnRestoreData
            // 
            btnRestoreData.Location = new Point(740, 495);
            btnRestoreData.Name = "btnRestoreData";
            btnRestoreData.Size = new Size(220, 45);
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
            btnTrain.Location = new Point(510, 560);
            btnTrain.Name = "btnTrain";
            btnTrain.Size = new Size(220, 60);
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
            btnDrive.Location = new Point(740, 560);
            btnDrive.Name = "btnDrive";
            btnDrive.Size = new Size(220, 60);
            btnDrive.TabIndex = 9;
            btnDrive.Text = "🚗 자율주행 시작";
            btnDrive.UseVisualStyleBackColor = false;
            btnDrive.Click += BtnDrive_Click;
            // 
            // frmNewtrainer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(1584, 861);
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
    }
}
