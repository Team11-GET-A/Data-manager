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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmNewtrainer));
            lstTubFolders = new CheckedListBox();
            btnAddTubFolder = new Button();
            btnRemoveTubFolder = new Button();
            lstCatalogRows = new ListBox();
            lblCatalogShortcutGuide = new Label();
            picDriveImage = new PictureBox();
            pnlPlayback = new Panel();
            btnLeft = new Button();
            btnPlay = new Button();
            btnRight = new Button();
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
            lstModelTrash = new ListView();
            colTrashNo = new ColumnHeader();
            colTrashName = new ColumnHeader();
            colTrashPath = new ColumnHeader();
            btnImportModel = new Button();
            btnModelDlt = new Button();
            btnNameCh = new Button();
            btnModelRestore = new Button();
            lblTrainerShortcutGuide = new Label();
            ((System.ComponentModel.ISupportInitialize)picDriveImage).BeginInit();
            pnlPlayback.SuspendLayout();
            SuspendLayout();
            // 
            // lstTubFolders
            // 
            lstTubFolders.CheckOnClick = true;
            lstTubFolders.Font = new Font("맑은 고딕", 9F);
            lstTubFolders.FormattingEnabled = true;
            lstTubFolders.Location = new Point(20, 20);
            lstTubFolders.Name = "lstTubFolders";
            lstTubFolders.Size = new Size(500, 330);
            lstTubFolders.TabIndex = 2;
            lstTubFolders.SelectedIndexChanged += LstTubFolders_SelectedIndexChanged;
            // 
            // btnAddTubFolder
            // 
            btnAddTubFolder.BackColor = Color.FromArgb(56, 118, 198);
            btnAddTubFolder.Cursor = Cursors.Hand;
            btnAddTubFolder.FlatAppearance.BorderSize = 0;
            btnAddTubFolder.FlatStyle = FlatStyle.Flat;
            btnAddTubFolder.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            btnAddTubFolder.ForeColor = Color.White;
            btnAddTubFolder.Location = new Point(20, 365);
            btnAddTubFolder.Name = "btnAddTubFolder";
            btnAddTubFolder.Size = new Size(245, 48);
            btnAddTubFolder.TabIndex = 13;
            btnAddTubFolder.Text = "학습 tub 추가";
            btnAddTubFolder.UseVisualStyleBackColor = false;
            btnAddTubFolder.Click += BtnAddTubFolder_Click;
            // 
            // btnRemoveTubFolder
            // 
            btnRemoveTubFolder.BackColor = Color.FromArgb(83, 105, 136);
            btnRemoveTubFolder.Cursor = Cursors.Hand;
            btnRemoveTubFolder.FlatAppearance.BorderSize = 0;
            btnRemoveTubFolder.FlatStyle = FlatStyle.Flat;
            btnRemoveTubFolder.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            btnRemoveTubFolder.ForeColor = Color.White;
            btnRemoveTubFolder.Location = new Point(275, 365);
            btnRemoveTubFolder.Name = "btnRemoveTubFolder";
            btnRemoveTubFolder.Size = new Size(245, 48);
            btnRemoveTubFolder.TabIndex = 14;
            btnRemoveTubFolder.Text = "선택 tub 제거";
            btnRemoveTubFolder.UseVisualStyleBackColor = false;
            btnRemoveTubFolder.Click += BtnRemoveTubFolder_Click;
            // 
            // lstCatalogRows
            // 
            lstCatalogRows.Font = new Font("Consolas", 9F);
            lstCatalogRows.FormattingEnabled = true;
            lstCatalogRows.Location = new Point(20, 430);
            lstCatalogRows.Name = "lstCatalogRows";
            lstCatalogRows.SelectionMode = SelectionMode.MultiExtended;
            lstCatalogRows.Size = new Size(500, 380);
            lstCatalogRows.TabIndex = 3;
            lstCatalogRows.SelectedIndexChanged += LstCatalogRows_SelectedIndexChanged;
            // 
            // lblCatalogShortcutGuide
            // 
            lblCatalogShortcutGuide.Font = new Font("맑은 고딕", 9F);
            lblCatalogShortcutGuide.ForeColor = Color.FromArgb(64, 64, 64);
            lblCatalogShortcutGuide.Location = new Point(20, 820);
            lblCatalogShortcutGuide.Name = "lblCatalogShortcutGuide";
            lblCatalogShortcutGuide.Size = new Size(500, 60);
            lblCatalogShortcutGuide.TabIndex = 17;
            lblCatalogShortcutGuide.Text = "Catalog 단축키: Ctrl+D 제외 / Ctrl+R 제외 취소";
            lblCatalogShortcutGuide.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // picDriveImage
            // 
            picDriveImage.BorderStyle = BorderStyle.FixedSingle;
            picDriveImage.Location = new Point(550, 20);
            picDriveImage.Name = "picDriveImage";
            picDriveImage.Size = new Size(500, 430);
            picDriveImage.SizeMode = PictureBoxSizeMode.Zoom;
            picDriveImage.TabIndex = 4;
            picDriveImage.TabStop = false;
            // 
            // pnlPlayback
            // 
            pnlPlayback.BorderStyle = BorderStyle.FixedSingle;
            pnlPlayback.Controls.Add(btnLeft);
            pnlPlayback.Controls.Add(btnPlay);
            pnlPlayback.Controls.Add(btnRight);
            pnlPlayback.Controls.Add(lblSpeed);
            pnlPlayback.Controls.Add(cmbSpeed);
            pnlPlayback.Location = new Point(550, 465);
            pnlPlayback.Name = "pnlPlayback";
            pnlPlayback.Size = new Size(500, 48);
            pnlPlayback.TabIndex = 5;
            // 
            // btnLeft
            // 
            btnLeft.BackColor = Color.FromArgb(232, 238, 247);
            btnLeft.Cursor = Cursors.Hand;
            btnLeft.FlatAppearance.BorderSize = 0;
            btnLeft.FlatStyle = FlatStyle.Flat;
            btnLeft.ForeColor = Color.FromArgb(26, 54, 93);
            btnLeft.Image = (Image)resources.GetObject("btnLeft.Image");
            btnLeft.Location = new Point(18, 8);
            btnLeft.Name = "btnLeft";
            btnLeft.Size = new Size(60, 30);
            btnLeft.TabIndex = 0;
            btnLeft.UseVisualStyleBackColor = false;
            btnLeft.Click += BtnLeft_Click;
            // 
            // btnPlay
            // 
            btnPlay.BackColor = Color.FromArgb(232, 238, 247);
            btnPlay.Cursor = Cursors.Hand;
            btnPlay.FlatAppearance.BorderSize = 0;
            btnPlay.FlatStyle = FlatStyle.Flat;
            btnPlay.ForeColor = Color.FromArgb(26, 54, 93);
            btnPlay.Image = (Image)resources.GetObject("btnPlay.Image");
            btnPlay.Location = new Point(88, 8);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(60, 30);
            btnPlay.TabIndex = 1;
            btnPlay.UseVisualStyleBackColor = false;
            // 
            // btnRight
            // 
            btnRight.BackColor = Color.FromArgb(232, 238, 247);
            btnRight.Cursor = Cursors.Hand;
            btnRight.FlatAppearance.BorderSize = 0;
            btnRight.FlatStyle = FlatStyle.Flat;
            btnRight.ForeColor = Color.FromArgb(26, 54, 93);
            btnRight.Image = (Image)resources.GetObject("btnRight.Image");
            btnRight.Location = new Point(158, 8);
            btnRight.Name = "btnRight";
            btnRight.Size = new Size(60, 30);
            btnRight.TabIndex = 2;
            btnRight.UseVisualStyleBackColor = false;
            btnRight.Click += BtnRight_Click;
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
            cmbSpeed.Items.AddRange(new object[] { "0.5x", "1.0x", "2.0x", "3.0x" });
            cmbSpeed.Location = new Point(350, 10);
            cmbSpeed.Name = "cmbSpeed";
            cmbSpeed.Size = new Size(120, 23);
            cmbSpeed.TabIndex = 4;
            cmbSpeed.SelectedIndexChanged += CmbSpeed_SelectedIndexChanged;
            // 
            // btnCleanData
            // 
            btnCleanData.BackColor = Color.FromArgb(204, 91, 84);
            btnCleanData.Cursor = Cursors.Hand;
            btnCleanData.FlatAppearance.BorderSize = 0;
            btnCleanData.FlatStyle = FlatStyle.Flat;
            btnCleanData.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            btnCleanData.ForeColor = Color.White;
            btnCleanData.Location = new Point(550, 530);
            btnCleanData.Name = "btnCleanData";
            btnCleanData.Size = new Size(245, 48);
            btnCleanData.TabIndex = 6;
            btnCleanData.Text = "✂️ 선택 프레임 제외";
            btnCleanData.UseVisualStyleBackColor = false;
            btnCleanData.Click += BtnCleanData_Click;
            // 
            // btnRestoreData
            // 
            btnRestoreData.BackColor = Color.FromArgb(75, 143, 112);
            btnRestoreData.Cursor = Cursors.Hand;
            btnRestoreData.FlatAppearance.BorderSize = 0;
            btnRestoreData.FlatStyle = FlatStyle.Flat;
            btnRestoreData.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            btnRestoreData.ForeColor = Color.White;
            btnRestoreData.Location = new Point(805, 530);
            btnRestoreData.Name = "btnRestoreData";
            btnRestoreData.Size = new Size(245, 48);
            btnRestoreData.TabIndex = 7;
            btnRestoreData.Text = "⏪ 선택 프레임 복원";
            btnRestoreData.UseVisualStyleBackColor = false;
            btnRestoreData.Click += BtnRestoreData_Click;
            // 
            // btnTrain
            // 
            btnTrain.BackColor = Color.FromArgb(43, 108, 176);
            btnTrain.Cursor = Cursors.Hand;
            btnTrain.FlatAppearance.BorderSize = 0;
            btnTrain.FlatStyle = FlatStyle.Flat;
            btnTrain.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            btnTrain.ForeColor = Color.White;
            btnTrain.Location = new Point(550, 595);
            btnTrain.Name = "btnTrain";
            btnTrain.Size = new Size(245, 68);
            btnTrain.TabIndex = 8;
            btnTrain.Text = "\U0001f9e0 AI 학습 시작";
            btnTrain.UseVisualStyleBackColor = false;
            btnTrain.Click += BtnTrain_Click;
            // 
            // btnDrive
            // 
            btnDrive.BackColor = Color.FromArgb(75, 143, 112);
            btnDrive.Cursor = Cursors.Hand;
            btnDrive.FlatAppearance.BorderSize = 0;
            btnDrive.FlatStyle = FlatStyle.Flat;
            btnDrive.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            btnDrive.ForeColor = Color.White;
            btnDrive.Location = new Point(805, 595);
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
            lstModels.Location = new Point(1080, 20);
            lstModels.Name = "lstModels";
            lstModels.Size = new Size(500, 790);
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
            // lstModelTrash
            // 
            lstModelTrash.Columns.AddRange(new ColumnHeader[] { colTrashNo, colTrashName, colTrashPath });
            lstModelTrash.Font = new Font("Consolas", 9F);
            lstModelTrash.FullRowSelect = true;
            lstModelTrash.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lstModelTrash.Location = new Point(1080, 430);
            lstModelTrash.Name = "lstModelTrash";
            lstModelTrash.Size = new Size(500, 380);
            lstModelTrash.TabIndex = 15;
            lstModelTrash.UseCompatibleStateImageBehavior = false;
            lstModelTrash.View = View.Details;
            // 
            // colTrashNo
            // 
            colTrashNo.Text = "번호";
            colTrashNo.Width = 55;
            // 
            // colTrashName
            // 
            colTrashName.Text = "제외한 모델";
            colTrashName.Width = 170;
            // 
            // colTrashPath
            // 
            colTrashPath.Text = "경로";
            colTrashPath.Width = 285;
            // 
            // btnImportModel
            // 
            btnImportModel.BackColor = Color.FromArgb(56, 118, 198);
            btnImportModel.Cursor = Cursors.Hand;
            btnImportModel.FlatAppearance.BorderSize = 0;
            btnImportModel.FlatStyle = FlatStyle.Flat;
            btnImportModel.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnImportModel.ForeColor = Color.White;
            btnImportModel.Location = new Point(1080, 365);
            btnImportModel.Margin = new Padding(2);
            btnImportModel.Name = "btnImportModel";
            btnImportModel.Size = new Size(255, 48);
            btnImportModel.TabIndex = 19;
            btnImportModel.Text = "모델 가져오기";
            btnImportModel.UseVisualStyleBackColor = false;
            // 
            // btnModelDlt
            // 
            btnModelDlt.BackColor = Color.FromArgb(204, 91, 84);
            btnModelDlt.Cursor = Cursors.Hand;
            btnModelDlt.FlatAppearance.BorderSize = 0;
            btnModelDlt.FlatStyle = FlatStyle.Flat;
            btnModelDlt.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnModelDlt.ForeColor = Color.White;
            btnModelDlt.Location = new Point(1250, 825);
            btnModelDlt.Margin = new Padding(2);
            btnModelDlt.Name = "btnModelDlt";
            btnModelDlt.Size = new Size(160, 52);
            btnModelDlt.TabIndex = 11;
            btnModelDlt.Text = "모델 삭제";
            btnModelDlt.UseVisualStyleBackColor = false;
            // 
            // btnNameCh
            // 
            btnNameCh.BackColor = Color.FromArgb(75, 143, 112);
            btnNameCh.Cursor = Cursors.Hand;
            btnNameCh.FlatAppearance.BorderSize = 0;
            btnNameCh.FlatStyle = FlatStyle.Flat;
            btnNameCh.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnNameCh.ForeColor = Color.White;
            btnNameCh.Location = new Point(1080, 825);
            btnNameCh.Margin = new Padding(2);
            btnNameCh.Name = "btnNameCh";
            btnNameCh.Size = new Size(160, 52);
            btnNameCh.TabIndex = 12;
            btnNameCh.Text = "이름 변경";
            btnNameCh.UseVisualStyleBackColor = false;
            // 
            // btnModelRestore
            // 
            btnModelRestore.BackColor = Color.FromArgb(56, 118, 198);
            btnModelRestore.Cursor = Cursors.Hand;
            btnModelRestore.FlatAppearance.BorderSize = 0;
            btnModelRestore.FlatStyle = FlatStyle.Flat;
            btnModelRestore.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnModelRestore.ForeColor = Color.White;
            btnModelRestore.Location = new Point(1420, 825);
            btnModelRestore.Margin = new Padding(2);
            btnModelRestore.Name = "btnModelRestore";
            btnModelRestore.Size = new Size(160, 52);
            btnModelRestore.TabIndex = 16;
            btnModelRestore.Text = "모델 복원";
            btnModelRestore.UseVisualStyleBackColor = false;
            // 
            // lblTrainerShortcutGuide
            // 
            lblTrainerShortcutGuide.Font = new Font("맑은 고딕", 9F);
            lblTrainerShortcutGuide.ForeColor = Color.FromArgb(64, 64, 64);
            lblTrainerShortcutGuide.Location = new Point(550, 680);
            lblTrainerShortcutGuide.Name = "lblTrainerShortcutGuide";
            lblTrainerShortcutGuide.Size = new Size(500, 130);
            lblTrainerShortcutGuide.TabIndex = 18;
            lblTrainerShortcutGuide.Text = "단축키: tub 선택 후 Enter 학습 시작\r\n모델 선택 후 Delete 제외 / Ctrl+N 이름 변경";
            lblTrainerShortcutGuide.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // frmNewtrainer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(1600, 900);
            Controls.Add(lblTrainerShortcutGuide);
            Controls.Add(lblCatalogShortcutGuide);
            Controls.Add(btnModelRestore);
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
            Controls.Add(btnRemoveTubFolder);
            Controls.Add(btnAddTubFolder);
            Controls.Add(lstTubFolders);
            Name = "frmNewtrainer";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "🏎️ DonkeyCar Advanced Data Manager";
            ((System.ComponentModel.ISupportInitialize)picDriveImage).EndInit();
            pnlPlayback.ResumeLayout(false);
            pnlPlayback.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private CheckedListBox lstTubFolders;
        private Button btnAddTubFolder;
        private Button btnRemoveTubFolder;
        private ListBox lstCatalogRows;
        private Label lblCatalogShortcutGuide;
        private PictureBox picDriveImage;
        private Panel pnlPlayback;
        private Button btnPlay;
        private Button btnLeft;
        private Button btnRight;
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
        private ListView lstModelTrash;
        private ColumnHeader colTrashNo;
        private ColumnHeader colTrashName;
        private ColumnHeader colTrashPath;
        private Button btnImportModel;
        private Button btnModelDlt;
        private Button btnNameCh;
        private Button btnModelRestore;
        private Label lblTrainerShortcutGuide;
    }
}
