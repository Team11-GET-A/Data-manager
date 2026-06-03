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
            lstModels = new ListView();
            colModelNo = new ColumnHeader();
            colModelName = new ColumnHeader();
            colModelPath = new ColumnHeader();
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
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1584, 56);
            pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(18, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(92, 21);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🏎️ Trainer";
            // 
            // btnLoadData
            // 
            btnLoadData.Location = new Point(12, 758);
            btnLoadData.Name = "btnLoadData";
            btnLoadData.Size = new Size(500, 46);
            btnLoadData.TabIndex = 1;
            btnLoadData.Text = "📁 데이터 폴더 로드";
            btnLoadData.UseVisualStyleBackColor = true;
            btnLoadData.Click += BtnLoadData_Click;
            // 
            // lstCatalogRows
            // 
            lstCatalogRows.Font = new Font("Consolas", 9F);
            lstCatalogRows.FormattingEnabled = true;
            lstCatalogRows.Location = new Point(12, 90);
            lstCatalogRows.Name = "lstCatalogRows";
            lstCatalogRows.Size = new Size(500, 648);
            lstCatalogRows.TabIndex = 3;
            lstCatalogRows.SelectedIndexChanged += LstCatalogRows_SelectedIndexChanged;
            // 
            // picDriveImage
            // 
            picDriveImage.BorderStyle = BorderStyle.FixedSingle;
            picDriveImage.Location = new Point(532, 90);
            picDriveImage.Name = "picDriveImage";
            picDriveImage.Size = new Size(500, 375);
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
            pnlPlayback.Location = new Point(532, 480);
            pnlPlayback.Name = "pnlPlayback";
            pnlPlayback.Size = new Size(500, 48);
            pnlPlayback.TabIndex = 5;
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(18, 8);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(60, 30);
            btnPlay.TabIndex = 0;
            btnPlay.Text = "▶";
            btnPlay.UseVisualStyleBackColor = true;
            // 
            // btnPause
            // 
            btnPause.Location = new Point(88, 8);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(60, 30);
            btnPause.TabIndex = 1;
            btnPause.Text = "⏸";
            btnPause.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            btnStop.Location = new Point(158, 8);
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
            lblSpeed.Location = new Point(306, 14);
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
            cmbSpeed.Location = new Point(350, 10);
            cmbSpeed.Name = "cmbSpeed";
            cmbSpeed.Size = new Size(120, 23);
            cmbSpeed.TabIndex = 4;
            cmbSpeed.SelectedIndexChanged += CmbSpeed_SelectedIndexChanged;
            // 
            // btnCleanData
            // 
            btnCleanData.Location = new Point(532, 550);
            btnCleanData.Name = "btnCleanData";
            btnCleanData.Size = new Size(245, 48);
            btnCleanData.TabIndex = 6;
            btnCleanData.Text = "✂️ 선택 프레임 제외";
            btnCleanData.UseVisualStyleBackColor = true;
            btnCleanData.Click += BtnCleanData_Click;
            // 
            // btnRestoreData
            // 
            btnRestoreData.Location = new Point(787, 550);
            btnRestoreData.Name = "btnRestoreData";
            btnRestoreData.Size = new Size(245, 48);
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
            btnTrain.Location = new Point(532, 620);
            btnTrain.Name = "btnTrain";
            btnTrain.Size = new Size(245, 68);
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
            btnDrive.Location = new Point(787, 620);
            btnDrive.Name = "btnDrive";
            btnDrive.Size = new Size(245, 68);
            btnDrive.TabIndex = 9;
            btnDrive.Text = "🚗 자율주행 시작";
            btnDrive.UseVisualStyleBackColor = false;
            btnDrive.Click += BtnDrive_Click;
            // 
            // lstModels
            // 
            lstModels.Columns.AddRange(new ColumnHeader[] { colModelNo, colModelName, colModelPath });
            lstModels.Font = new Font("Consolas", 9F);
            lstModels.FullRowSelect = true;
            lstModels.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lstModels.Location = new Point(1052, 90);
            lstModels.MultiSelect = false;
            lstModels.Name = "lstModels";
            lstModels.Size = new Size(520, 650);
            lstModels.TabIndex = 10;
            lstModels.UseCompatibleStateImageBehavior = false;
            lstModels.View = View.Details;
            // 
            // colModelNo
            // 
            colModelNo.Text = "번호";
            colModelNo.Width = 55;
            // 
            // colModelName
            // 
            colModelName.Text = "모델 이름";
            colModelName.Width = 170;
            // 
            // colModelPath
            // 
            colModelPath.Text = "경로";
            colModelPath.Width = 285;
            // 
            // btnModelDlt
            // 
            btnModelDlt.BackColor = Color.Red;
            btnModelDlt.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnModelDlt.ForeColor = Color.White;
            btnModelDlt.Location = new Point(1052, 758);
            btnModelDlt.Margin = new Padding(2);
            btnModelDlt.Name = "btnModelDlt";
            btnModelDlt.Size = new Size(160, 52);
            btnModelDlt.TabIndex = 11;
            btnModelDlt.Text = "모델 삭제";
            btnModelDlt.UseVisualStyleBackColor = false;
            // 
            // btnNameCh
            // 
            btnNameCh.BackColor = Color.FromArgb(0, 192, 0);
            btnNameCh.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnNameCh.ForeColor = Color.Black;
            btnNameCh.Location = new Point(1222, 758);
            btnNameCh.Margin = new Padding(2);
            btnNameCh.Name = "btnNameCh";
            btnNameCh.Size = new Size(160, 52);
            btnNameCh.TabIndex = 12;
            btnNameCh.Text = "이름 변경";
            btnNameCh.UseVisualStyleBackColor = false;
            // 
            // frmNewtrainer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(1584, 861);
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
        private ListView lstModels;
        private ColumnHeader colModelNo;
        private ColumnHeader colModelName;
        private ColumnHeader colModelPath;
        private Button btnModelDlt;
        private Button btnNameCh;
    }
}
