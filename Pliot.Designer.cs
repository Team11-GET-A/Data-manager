namespace Data_Manager
{
    partial class Pliot
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitMain = new SplitContainer();
            pnlLeft = new Panel();
            grpSelectedModel = new GroupBox();
            tblSelectedModel = new TableLayoutPanel();
            lblSelectedModelNameTitle = new Label();
            lblSelectedModelPathTitle = new Label();
            lblSelectedModelTypeTitle = new Label();
            lblSelectedTubPathTitle = new Label();
            lblSelectedModelName = new Label();
            lblSelectedModelPath = new Label();
            lblSelectedModelType = new Label();
            lblSelectedTubPath = new Label();
            lblPilotShortcutGuide = new Label();
            lvModelList = new ListView();
            colModelNo = new ColumnHeader();
            colModelName = new ColumnHeader();
            colModelPath = new ColumnHeader();
            btnImportModel = new Button();
            pnlRight = new Panel();
            pnlPilotCard = new Panel();
            pnlTrackBar = new Panel();
            trbLocation = new TrackBar();
            pnlPlaybackControls = new Panel();
            btnJumpPrev5 = new Button();
            btnPrevImage = new Button();
            btnPlayPause = new Button();
            cmbSpeed = new ComboBox();
            btnReversePlay = new Button();
            btnNextImage = new Button();
            btnJumpNext5 = new Button();
            pnlImageHost = new Panel();
            picPilotImage = new PictureBox();
            pliotAngleIndicator = new pliotAngleDicatoer();
            pliotAiThrottleGauge = new pliotThrottleGauge();
            pliotTubThrottleGauge = new pliotThrottleGauge();
            pnlImageIndexOverlay = new Panel();
            lblImageIndexOverlay = new Label();
            pnlPilotHeader = new Panel();
            btnPilotChart = new Button();
            btnTubInput = new Button();
            lblTubPathValue = new Label();
            lblTubPathTitle = new Label();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            pnlLeft.SuspendLayout();
            grpSelectedModel.SuspendLayout();
            tblSelectedModel.SuspendLayout();
            pnlRight.SuspendLayout();
            pnlPilotCard.SuspendLayout();
            pnlTrackBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trbLocation).BeginInit();
            pnlPlaybackControls.SuspendLayout();
            pnlImageHost.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPilotImage).BeginInit();
            picPilotImage.SuspendLayout();
            pnlImageIndexOverlay.SuspendLayout();
            pnlPilotHeader.SuspendLayout();
            SuspendLayout();
            // 
            // splitMain
            // 
            splitMain.Anchor = AnchorStyles.None;
            splitMain.BackColor = Color.White;
            splitMain.Location = new Point(8, 8);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(pnlLeft);
            splitMain.Panel1MinSize = 1;
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(pnlRight);
            splitMain.Panel2MinSize = 1;
            splitMain.Size = new Size(1584, 884);
            splitMain.SplitterDistance = 404;
            splitMain.TabIndex = 0;
            // 
            // pnlLeft
            // 
            pnlLeft.Anchor = AnchorStyles.None;
            pnlLeft.BackColor = Color.White;
            pnlLeft.BorderStyle = BorderStyle.FixedSingle;
            pnlLeft.Controls.Add(grpSelectedModel);
            pnlLeft.Controls.Add(lblPilotShortcutGuide);
            pnlLeft.Controls.Add(lvModelList);
            pnlLeft.Controls.Add(btnImportModel);
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Padding = new Padding(8);
            pnlLeft.Size = new Size(404, 884);
            pnlLeft.TabIndex = 0;
            // 
            // grpSelectedModel
            // 
            grpSelectedModel.Anchor = AnchorStyles.None;
            grpSelectedModel.BackColor = Color.White;
            grpSelectedModel.Controls.Add(tblSelectedModel);
            grpSelectedModel.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            grpSelectedModel.ForeColor = Color.FromArgb(30, 39, 50);
            grpSelectedModel.Location = new Point(7, 675);
            grpSelectedModel.Name = "grpSelectedModel";
            grpSelectedModel.Padding = new Padding(10, 8, 10, 10);
            grpSelectedModel.Size = new Size(388, 200);
            grpSelectedModel.TabIndex = 2;
            grpSelectedModel.TabStop = false;
            grpSelectedModel.Text = "선택한 모델 정보";
            // 
            // tblSelectedModel
            // 
            tblSelectedModel.ColumnCount = 2;
            tblSelectedModel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            tblSelectedModel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblSelectedModel.Controls.Add(lblSelectedModelNameTitle, 0, 0);
            tblSelectedModel.Controls.Add(lblSelectedModelPathTitle, 0, 1);
            tblSelectedModel.Controls.Add(lblSelectedModelTypeTitle, 0, 2);
            tblSelectedModel.Controls.Add(lblSelectedTubPathTitle, 0, 3);
            tblSelectedModel.Controls.Add(lblSelectedModelName, 1, 0);
            tblSelectedModel.Controls.Add(lblSelectedModelPath, 1, 1);
            tblSelectedModel.Controls.Add(lblSelectedModelType, 1, 2);
            tblSelectedModel.Controls.Add(lblSelectedTubPath, 1, 3);
            tblSelectedModel.Dock = DockStyle.Fill;
            tblSelectedModel.Location = new Point(10, 26);
            tblSelectedModel.Name = "tblSelectedModel";
            tblSelectedModel.RowCount = 4;
            tblSelectedModel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tblSelectedModel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tblSelectedModel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tblSelectedModel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tblSelectedModel.Size = new Size(368, 164);
            tblSelectedModel.TabIndex = 0;
            // 
            // lblSelectedModelNameTitle
            // 
            lblSelectedModelNameTitle.Dock = DockStyle.Fill;
            lblSelectedModelNameTitle.Font = new Font("맑은 고딕", 9.5F, FontStyle.Bold);
            lblSelectedModelNameTitle.ForeColor = Color.FromArgb(92, 105, 122);
            lblSelectedModelNameTitle.Location = new Point(3, 0);
            lblSelectedModelNameTitle.Name = "lblSelectedModelNameTitle";
            lblSelectedModelNameTitle.Size = new Size(84, 41);
            lblSelectedModelNameTitle.TabIndex = 0;
            lblSelectedModelNameTitle.Text = "모델명";
            lblSelectedModelNameTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSelectedModelPathTitle
            // 
            lblSelectedModelPathTitle.Dock = DockStyle.Fill;
            lblSelectedModelPathTitle.Font = new Font("맑은 고딕", 9.5F, FontStyle.Bold);
            lblSelectedModelPathTitle.ForeColor = Color.FromArgb(92, 105, 122);
            lblSelectedModelPathTitle.Location = new Point(3, 41);
            lblSelectedModelPathTitle.Name = "lblSelectedModelPathTitle";
            lblSelectedModelPathTitle.Size = new Size(84, 41);
            lblSelectedModelPathTitle.TabIndex = 1;
            lblSelectedModelPathTitle.Text = "파일 경로";
            lblSelectedModelPathTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSelectedModelTypeTitle
            // 
            lblSelectedModelTypeTitle.Dock = DockStyle.Fill;
            lblSelectedModelTypeTitle.Font = new Font("맑은 고딕", 9.5F, FontStyle.Bold);
            lblSelectedModelTypeTitle.ForeColor = Color.FromArgb(92, 105, 122);
            lblSelectedModelTypeTitle.Location = new Point(3, 82);
            lblSelectedModelTypeTitle.Name = "lblSelectedModelTypeTitle";
            lblSelectedModelTypeTitle.Size = new Size(84, 41);
            lblSelectedModelTypeTitle.TabIndex = 2;
            lblSelectedModelTypeTitle.Text = "타입";
            lblSelectedModelTypeTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSelectedTubPathTitle
            // 
            lblSelectedTubPathTitle.Dock = DockStyle.Fill;
            lblSelectedTubPathTitle.Font = new Font("맑은 고딕", 9.5F, FontStyle.Bold);
            lblSelectedTubPathTitle.ForeColor = Color.FromArgb(92, 105, 122);
            lblSelectedTubPathTitle.Location = new Point(3, 123);
            lblSelectedTubPathTitle.Name = "lblSelectedTubPathTitle";
            lblSelectedTubPathTitle.Size = new Size(84, 41);
            lblSelectedTubPathTitle.TabIndex = 3;
            lblSelectedTubPathTitle.Text = "주행데이터 경로";
            lblSelectedTubPathTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSelectedModelName
            // 
            lblSelectedModelName.Dock = DockStyle.Fill;
            lblSelectedModelName.Font = new Font("맑은 고딕", 9.5F);
            lblSelectedModelName.ForeColor = Color.FromArgb(30, 39, 50);
            lblSelectedModelName.Location = new Point(93, 0);
            lblSelectedModelName.Name = "lblSelectedModelName";
            lblSelectedModelName.Size = new Size(272, 41);
            lblSelectedModelName.TabIndex = 4;
            lblSelectedModelName.Text = "model_20260531_001";
            lblSelectedModelName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSelectedModelPath
            // 
            lblSelectedModelPath.Dock = DockStyle.Fill;
            lblSelectedModelPath.Font = new Font("맑은 고딕", 9.5F);
            lblSelectedModelPath.ForeColor = Color.FromArgb(30, 39, 50);
            lblSelectedModelPath.Location = new Point(93, 41);
            lblSelectedModelPath.Name = "lblSelectedModelPath";
            lblSelectedModelPath.Size = new Size(272, 41);
            lblSelectedModelPath.TabIndex = 5;
            lblSelectedModelPath.Text = "C:\\data\\model_20260531_001.h5";
            lblSelectedModelPath.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSelectedModelType
            // 
            lblSelectedModelType.Dock = DockStyle.Fill;
            lblSelectedModelType.Font = new Font("맑은 고딕", 9.5F);
            lblSelectedModelType.ForeColor = Color.FromArgb(30, 39, 50);
            lblSelectedModelType.Location = new Point(93, 82);
            lblSelectedModelType.Name = "lblSelectedModelType";
            lblSelectedModelType.Size = new Size(272, 41);
            lblSelectedModelType.TabIndex = 6;
            lblSelectedModelType.Text = "linear";
            lblSelectedModelType.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSelectedTubPath
            // 
            lblSelectedTubPath.Dock = DockStyle.Fill;
            lblSelectedTubPath.Font = new Font("맑은 고딕", 9.5F);
            lblSelectedTubPath.ForeColor = Color.FromArgb(30, 39, 50);
            lblSelectedTubPath.Location = new Point(93, 123);
            lblSelectedTubPath.Name = "lblSelectedTubPath";
            lblSelectedTubPath.Size = new Size(272, 41);
            lblSelectedTubPath.TabIndex = 7;
            lblSelectedTubPath.Text = "/mnt/c/Users/cheon/.../data";
            lblSelectedTubPath.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblPilotShortcutGuide
            // 
            lblPilotShortcutGuide.Anchor = AnchorStyles.None;
            lblPilotShortcutGuide.Font = new Font("맑은 고딕", 8.5F);
            lblPilotShortcutGuide.ForeColor = Color.FromArgb(92, 105, 122);
            lblPilotShortcutGuide.Location = new Point(7, 587);
            lblPilotShortcutGuide.Name = "lblPilotShortcutGuide";
            lblPilotShortcutGuide.Padding = new Padding(8, 4, 8, 4);
            lblPilotShortcutGuide.Size = new Size(388, 88);
            lblPilotShortcutGuide.TabIndex = 3;
            lblPilotShortcutGuide.Text = "Space 재생/일시정지 | Esc 정지\r\n←/→ 1프레임 | Shift+←/→ 5프레임\r\nEnter 모델 로드 | Ctrl+I 모델 가져오기\r\nCtrl+T 데이터 로드 | Ctrl+G 그래프";
            lblPilotShortcutGuide.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lvModelList
            // 
            lvModelList.Anchor = AnchorStyles.None;
            lvModelList.BackColor = Color.White;
            lvModelList.BorderStyle = BorderStyle.FixedSingle;
            lvModelList.Columns.AddRange(new ColumnHeader[] { colModelNo, colModelName, colModelPath });
            lvModelList.Font = new Font("맑은 고딕", 9.5F);
            lvModelList.ForeColor = Color.FromArgb(30, 39, 50);
            lvModelList.FullRowSelect = true;
            lvModelList.GridLines = true;
            lvModelList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvModelList.Location = new Point(-1, -1);
            lvModelList.MultiSelect = false;
            lvModelList.Name = "lvModelList";
            lvModelList.Size = new Size(396, 684);
            lvModelList.TabIndex = 1;
            lvModelList.UseCompatibleStateImageBehavior = false;
            lvModelList.View = View.Details;
            // 
            // colModelNo
            // 
            colModelNo.Text = "번호";
            colModelNo.Width = 50;
            // 
            // colModelName
            // 
            colModelName.Text = "모델 이름";
            colModelName.Width = 150;
            // 
            // colModelPath
            // 
            colModelPath.Text = "경로";
            colModelPath.Width = 200;
            // 
            // btnImportModel
            // 
            btnImportModel.Anchor = AnchorStyles.None;
            btnImportModel.BackColor = Color.FromArgb(62, 150, 255);
            btnImportModel.Cursor = Cursors.Hand;
            btnImportModel.FlatAppearance.BorderColor = Color.FromArgb(96, 172, 255);
            btnImportModel.FlatStyle = FlatStyle.Flat;
            btnImportModel.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            btnImportModel.ForeColor = Color.White;
            btnImportModel.Location = new Point(-1, -1);
            btnImportModel.Name = "btnImportModel";
            btnImportModel.Size = new Size(396, 48);
            btnImportModel.TabIndex = 0;
            btnImportModel.Text = "모델 가져오기";
            btnImportModel.UseVisualStyleBackColor = false;
            // 
            // pnlRight
            // 
            pnlRight.Anchor = AnchorStyles.None;
            pnlRight.BackColor = Color.White;
            pnlRight.Controls.Add(pnlPilotCard);
            pnlRight.Location = new Point(0, 0);
            pnlRight.Name = "pnlRight";
            pnlRight.Padding = new Padding(8, 0, 0, 0);
            pnlRight.Size = new Size(1176, 884);
            pnlRight.TabIndex = 0;
            // 
            // pnlPilotCard
            // 
            pnlPilotCard.Anchor = AnchorStyles.None;
            pnlPilotCard.BackColor = Color.White;
            pnlPilotCard.BorderStyle = BorderStyle.FixedSingle;
            pnlPilotCard.Controls.Add(pnlTrackBar);
            pnlPilotCard.Controls.Add(pnlPlaybackControls);
            pnlPilotCard.Controls.Add(pnlImageHost);
            pnlPilotCard.Controls.Add(pnlPilotHeader);
            pnlPilotCard.Location = new Point(8, 0);
            pnlPilotCard.Name = "pnlPilotCard";
            pnlPilotCard.Padding = new Padding(14);
            pnlPilotCard.Size = new Size(1168, 884);
            pnlPilotCard.TabIndex = 0;
            // 
            // pnlTrackBar
            // 
            pnlTrackBar.BackColor = Color.White;
            pnlTrackBar.BorderStyle = BorderStyle.FixedSingle;
            pnlTrackBar.Controls.Add(trbLocation);
            pnlTrackBar.Location = new Point(14, 732);
            pnlTrackBar.Name = "pnlTrackBar";
            pnlTrackBar.Padding = new Padding(8, 4, 8, 4);
            pnlTrackBar.Size = new Size(1138, 57);
            pnlTrackBar.TabIndex = 2;
            // 
            // trbLocation
            // 
            trbLocation.Anchor = AnchorStyles.None;
            trbLocation.BackColor = Color.White;
            trbLocation.Enabled = false;
            trbLocation.Location = new Point(8, 7);
            trbLocation.Maximum = 0;
            trbLocation.Name = "trbLocation";
            trbLocation.Size = new Size(1122, 45);
            trbLocation.TabIndex = 1;
            trbLocation.TickStyle = TickStyle.Both;
            // 
            // pnlPlaybackControls
            // 
            pnlPlaybackControls.BackColor = Color.White;
            pnlPlaybackControls.BorderStyle = BorderStyle.FixedSingle;
            pnlPlaybackControls.Controls.Add(btnJumpPrev5);
            pnlPlaybackControls.Controls.Add(btnPrevImage);
            pnlPlaybackControls.Controls.Add(btnPlayPause);
            pnlPlaybackControls.Controls.Add(cmbSpeed);
            pnlPlaybackControls.Controls.Add(btnReversePlay);
            pnlPlaybackControls.Controls.Add(btnNextImage);
            pnlPlaybackControls.Controls.Add(btnJumpNext5);
            pnlPlaybackControls.Location = new Point(14, 797);
            pnlPlaybackControls.Name = "pnlPlaybackControls";
            pnlPlaybackControls.Padding = new Padding(8, 4, 8, 4);
            pnlPlaybackControls.Size = new Size(1138, 73);
            pnlPlaybackControls.TabIndex = 3;
            // 
            // btnJumpPrev5
            // 
            btnJumpPrev5.Anchor = AnchorStyles.None;
            btnJumpPrev5.BackColor = Color.FromArgb(245, 247, 250);
            btnJumpPrev5.BackgroundImage = Properties.Resources.arrow5_left;
            btnJumpPrev5.BackgroundImageLayout = ImageLayout.Zoom;
            btnJumpPrev5.FlatAppearance.BorderColor = Color.FromArgb(205, 214, 225);
            btnJumpPrev5.FlatStyle = FlatStyle.Flat;
            btnJumpPrev5.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            btnJumpPrev5.ForeColor = Color.FromArgb(30, 39, 50);
            btnJumpPrev5.Location = new Point(118, 20);
            btnJumpPrev5.Name = "btnJumpPrev5";
            btnJumpPrev5.Size = new Size(116, 36);
            btnJumpPrev5.TabIndex = 1;
            btnJumpPrev5.UseVisualStyleBackColor = false;
            // 
            // btnPrevImage
            // 
            btnPrevImage.Anchor = AnchorStyles.None;
            btnPrevImage.BackColor = Color.FromArgb(245, 247, 250);
            btnPrevImage.BackgroundImage = Properties.Resources.arrow1_left;
            btnPrevImage.BackgroundImageLayout = ImageLayout.Zoom;
            btnPrevImage.FlatAppearance.BorderColor = Color.FromArgb(205, 214, 225);
            btnPrevImage.FlatStyle = FlatStyle.Flat;
            btnPrevImage.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            btnPrevImage.ForeColor = Color.FromArgb(30, 39, 50);
            btnPrevImage.Location = new Point(246, 20);
            btnPrevImage.Name = "btnPrevImage";
            btnPrevImage.Size = new Size(116, 36);
            btnPrevImage.TabIndex = 2;
            btnPrevImage.UseVisualStyleBackColor = false;
            // 
            // btnPlayPause
            // 
            btnPlayPause.Anchor = AnchorStyles.None;
            btnPlayPause.BackColor = Color.FromArgb(245, 247, 250);
            btnPlayPause.BackgroundImage = Properties.Resources.PlaySlide4655096;
            btnPlayPause.BackgroundImageLayout = ImageLayout.Zoom;
            btnPlayPause.FlatAppearance.BorderColor = Color.FromArgb(205, 214, 225);
            btnPlayPause.FlatStyle = FlatStyle.Flat;
            btnPlayPause.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            btnPlayPause.ForeColor = Color.FromArgb(30, 39, 50);
            btnPlayPause.Location = new Point(374, 20);
            btnPlayPause.Name = "btnPlayPause";
            btnPlayPause.Size = new Size(116, 36);
            btnPlayPause.TabIndex = 3;
            btnPlayPause.UseVisualStyleBackColor = false;
            // 
            // cmbSpeed
            // 
            cmbSpeed.Anchor = AnchorStyles.None;
            cmbSpeed.BackColor = Color.FromArgb(245, 247, 250);
            cmbSpeed.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSpeed.FlatStyle = FlatStyle.Flat;
            cmbSpeed.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            cmbSpeed.ForeColor = Color.FromArgb(30, 39, 50);
            cmbSpeed.FormattingEnabled = true;
            cmbSpeed.Items.AddRange(new object[] { "0.5x", "1.0x", "2.0x", "3.0x" });
            cmbSpeed.Location = new Point(502, 23);
            cmbSpeed.Name = "cmbSpeed";
            cmbSpeed.Size = new Size(126, 28);
            cmbSpeed.TabIndex = 4;
            // 
            // btnReversePlay
            // 
            btnReversePlay.Anchor = AnchorStyles.None;
            btnReversePlay.BackColor = Color.FromArgb(245, 247, 250);
            btnReversePlay.BackgroundImage = Properties.Resources.UTurnArrow12262463;
            btnReversePlay.BackgroundImageLayout = ImageLayout.Zoom;
            btnReversePlay.FlatAppearance.BorderColor = Color.FromArgb(205, 214, 225);
            btnReversePlay.FlatStyle = FlatStyle.Flat;
            btnReversePlay.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            btnReversePlay.ForeColor = Color.FromArgb(30, 39, 50);
            btnReversePlay.Location = new Point(640, 20);
            btnReversePlay.Name = "btnReversePlay";
            btnReversePlay.Size = new Size(116, 36);
            btnReversePlay.TabIndex = 5;
            btnReversePlay.UseVisualStyleBackColor = false;
            // 
            // btnNextImage
            // 
            btnNextImage.Anchor = AnchorStyles.None;
            btnNextImage.BackColor = Color.FromArgb(245, 247, 250);
            btnNextImage.BackgroundImage = Properties.Resources.arrow1_right;
            btnNextImage.BackgroundImageLayout = ImageLayout.Zoom;
            btnNextImage.FlatAppearance.BorderColor = Color.FromArgb(205, 214, 225);
            btnNextImage.FlatStyle = FlatStyle.Flat;
            btnNextImage.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            btnNextImage.ForeColor = Color.FromArgb(30, 39, 50);
            btnNextImage.Location = new Point(768, 20);
            btnNextImage.Name = "btnNextImage";
            btnNextImage.Size = new Size(116, 36);
            btnNextImage.TabIndex = 6;
            btnNextImage.UseVisualStyleBackColor = false;
            // 
            // btnJumpNext5
            // 
            btnJumpNext5.Anchor = AnchorStyles.None;
            btnJumpNext5.BackColor = Color.FromArgb(245, 247, 250);
            btnJumpNext5.BackgroundImage = Properties.Resources.arrow5_right;
            btnJumpNext5.BackgroundImageLayout = ImageLayout.Zoom;
            btnJumpNext5.FlatAppearance.BorderColor = Color.FromArgb(205, 214, 225);
            btnJumpNext5.FlatStyle = FlatStyle.Flat;
            btnJumpNext5.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            btnJumpNext5.ForeColor = Color.FromArgb(30, 39, 50);
            btnJumpNext5.Location = new Point(896, 20);
            btnJumpNext5.Name = "btnJumpNext5";
            btnJumpNext5.Size = new Size(116, 36);
            btnJumpNext5.TabIndex = 7;
            btnJumpNext5.UseVisualStyleBackColor = false;
            // 
            // pnlImageHost
            // 
            pnlImageHost.BackColor = Color.White;
            pnlImageHost.BorderStyle = BorderStyle.FixedSingle;
            pnlImageHost.Controls.Add(picPilotImage);
            pnlImageHost.Location = new Point(14, 74);
            pnlImageHost.Name = "pnlImageHost";
            pnlImageHost.Padding = new Padding(4);
            pnlImageHost.Size = new Size(1138, 650);
            pnlImageHost.TabIndex = 1;
            // 
            // picPilotImage
            // 
            picPilotImage.BackColor = Color.White;
            picPilotImage.Controls.Add(pliotAngleIndicator);
            picPilotImage.Controls.Add(pliotAiThrottleGauge);
            picPilotImage.Controls.Add(pliotTubThrottleGauge);
            picPilotImage.Controls.Add(pnlImageIndexOverlay);
            picPilotImage.Dock = DockStyle.Fill;
            picPilotImage.Location = new Point(4, 4);
            picPilotImage.Name = "picPilotImage";
            picPilotImage.Size = new Size(1128, 640);
            picPilotImage.SizeMode = PictureBoxSizeMode.Zoom;
            picPilotImage.TabIndex = 0;
            picPilotImage.TabStop = false;
            // 
            // pliotAngleIndicator
            // 
            pliotAngleIndicator.BackColor = Color.Transparent;
            pliotAngleIndicator.ForeColor = Color.White;
            pliotAngleIndicator.Location = new Point(355, 585);
            pliotAngleIndicator.Name = "pliotAngleIndicator";
            pliotAngleIndicator.Size = new Size(420, 164);
            pliotAngleIndicator.TabIndex = 3;
            // 
            // pliotAiThrottleGauge
            // 
            pliotAiThrottleGauge.BackColor = Color.Transparent;
            pliotAiThrottleGauge.ForeColor = Color.White;
            pliotAiThrottleGauge.GaugeTitle = "AI";
            pliotAiThrottleGauge.Location = new Point(18, 629);
            pliotAiThrottleGauge.Name = "pliotAiThrottleGauge";
            pliotAiThrottleGauge.Size = new Size(240, 120);
            pliotAiThrottleGauge.TabIndex = 2;
            // 
            // pliotTubThrottleGauge
            // 
            pliotTubThrottleGauge.BackColor = Color.Transparent;
            pliotTubThrottleGauge.ForeColor = Color.White;
            pliotTubThrottleGauge.GaugeTitle = "사람";
            pliotTubThrottleGauge.Location = new Point(18, 499);
            pliotTubThrottleGauge.Name = "pliotTubThrottleGauge";
            pliotTubThrottleGauge.Size = new Size(240, 120);
            pliotTubThrottleGauge.TabIndex = 4;
            // 
            // pnlImageIndexOverlay
            // 
            pnlImageIndexOverlay.BackColor = Color.FromArgb(120, 22, 26, 32);
            pnlImageIndexOverlay.Controls.Add(lblImageIndexOverlay);
            pnlImageIndexOverlay.Location = new Point(12, 12);
            pnlImageIndexOverlay.Name = "pnlImageIndexOverlay";
            pnlImageIndexOverlay.Size = new Size(190, 58);
            pnlImageIndexOverlay.TabIndex = 1;
            // 
            // lblImageIndexOverlay
            // 
            lblImageIndexOverlay.Dock = DockStyle.Fill;
            lblImageIndexOverlay.Font = new Font("맑은 고딕", 18F, FontStyle.Bold);
            lblImageIndexOverlay.ForeColor = Color.White;
            lblImageIndexOverlay.Location = new Point(0, 0);
            lblImageIndexOverlay.Name = "lblImageIndexOverlay";
            lblImageIndexOverlay.Size = new Size(190, 58);
            lblImageIndexOverlay.TabIndex = 0;
            lblImageIndexOverlay.Text = "123 / 3456";
            lblImageIndexOverlay.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlPilotHeader
            // 
            pnlPilotHeader.BackColor = Color.White;
            pnlPilotHeader.Controls.Add(btnPilotChart);
            pnlPilotHeader.Controls.Add(btnTubInput);
            pnlPilotHeader.Controls.Add(lblTubPathValue);
            pnlPilotHeader.Controls.Add(lblTubPathTitle);
            pnlPilotHeader.Location = new Point(14, 14);
            pnlPilotHeader.Name = "pnlPilotHeader";
            pnlPilotHeader.Size = new Size(1142, 52);
            pnlPilotHeader.TabIndex = 0;
            //
            // btnPilotChart
            // 
            btnPilotChart.Anchor = AnchorStyles.None;
            btnPilotChart.BackColor = Color.FromArgb(65, 190, 125);
            btnPilotChart.FlatAppearance.BorderColor = Color.FromArgb(99, 224, 159);
            btnPilotChart.FlatStyle = FlatStyle.Flat;
            btnPilotChart.ForeColor = Color.FromArgb(9, 30, 20);
            btnPilotChart.Location = new Point(748, 8);
            btnPilotChart.Name = "btnPilotChart";
            btnPilotChart.Size = new Size(126, 36);
            btnPilotChart.TabIndex = 4;
            btnPilotChart.Text = "그래프";
            btnPilotChart.UseVisualStyleBackColor = false;
            // 
            // btnTubInput
            // 
            btnTubInput.Anchor = AnchorStyles.None;
            btnTubInput.BackColor = Color.FromArgb(44, 205, 220);
            btnTubInput.FlatAppearance.BorderColor = Color.FromArgb(78, 239, 254);
            btnTubInput.FlatStyle = FlatStyle.Flat;
            btnTubInput.ForeColor = Color.FromArgb(10, 24, 32);
            btnTubInput.Location = new Point(1012, 8);
            btnTubInput.Name = "btnTubInput";
            btnTubInput.Size = new Size(126, 36);
            btnTubInput.TabIndex = 2;
            btnTubInput.Text = "데이터 로드";
            btnTubInput.UseVisualStyleBackColor = false;
            // 
            // lblTubPathValue
            // 
            lblTubPathValue.Anchor = AnchorStyles.None;
            lblTubPathValue.Font = new Font("맑은 고딕", 9.5F);
            lblTubPathValue.ForeColor = Color.FromArgb(30, 39, 50);
            lblTubPathValue.Location = new Point(90, 12);
            lblTubPathValue.Name = "lblTubPathValue";
            lblTubPathValue.Size = new Size(652, 28);
            lblTubPathValue.TabIndex = 1;
            lblTubPathValue.Text = "model_20260531_001";
            lblTubPathValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTubPathTitle
            // 
            lblTubPathTitle.AutoSize = true;
            lblTubPathTitle.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            lblTubPathTitle.ForeColor = Color.FromArgb(92, 105, 122);
            lblTubPathTitle.Location = new Point(0, 14);
            lblTubPathTitle.Name = "lblTubPathTitle";
            lblTubPathTitle.Size = new Size(74, 20);
            lblTubPathTitle.TabIndex = 0;
            lblTubPathTitle.Text = "선택 모델";
            // 
            // Pliot
            // 
            AutoScaleDimensions = new SizeF(8F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1600, 900);
            Controls.Add(splitMain);
            Font = new Font("맑은 고딕", 10.5F);
            Margin = new Padding(2);
            MinimumSize = new Size(900, 520);
            Name = "Pliot";
            Padding = new Padding(8);
            Text = "파일럿";
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            pnlLeft.ResumeLayout(false);
            grpSelectedModel.ResumeLayout(false);
            tblSelectedModel.ResumeLayout(false);
            pnlRight.ResumeLayout(false);
            pnlPilotCard.ResumeLayout(false);
            pnlTrackBar.ResumeLayout(false);
            pnlTrackBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trbLocation).EndInit();
            pnlPlaybackControls.ResumeLayout(false);
            pnlImageHost.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picPilotImage).EndInit();
            picPilotImage.ResumeLayout(false);
            pnlImageIndexOverlay.ResumeLayout(false);
            pnlPilotHeader.ResumeLayout(false);
            pnlPilotHeader.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitMain;
        private Panel pnlLeft;
        private ListView lvModelList;
        private Button btnImportModel;
        private Label lblPilotShortcutGuide;
        private ColumnHeader colModelNo;
        private ColumnHeader colModelName;
        private ColumnHeader colModelPath;
        private GroupBox grpSelectedModel;
        private TableLayoutPanel tblSelectedModel;
        private Label lblSelectedModelNameTitle;
        private Label lblSelectedModelPathTitle;
        private Label lblSelectedModelTypeTitle;
        private Label lblSelectedTubPathTitle;
        private Label lblSelectedModelName;
        private Label lblSelectedModelPath;
        private Label lblSelectedModelType;
        private Label lblSelectedTubPath;
        private Panel pnlRight;
        private Panel pnlPilotCard;
        private Panel pnlPilotHeader;
        private Button btnPilotChart;
        private Button btnTubInput;
        private Label lblTubPathValue;
        private Label lblTubPathTitle;
        private Panel pnlImageHost;
        private PictureBox picPilotImage;
        private Panel pnlImageIndexOverlay;
        private Label lblImageIndexOverlay;
        private pliotAngleDicatoer pliotAngleIndicator;
        private pliotThrottleGauge pliotAiThrottleGauge;
        private pliotThrottleGauge pliotTubThrottleGauge;
        private Panel pnlTrackBar;
        private TrackBar trbLocation;
        private Panel pnlPlaybackControls;
        private Button btnJumpPrev5;
        private Button btnPrevImage;
        private Button btnPlayPause;
        private ComboBox cmbSpeed;
        private Button btnReversePlay;
        private Button btnNextImage;
        private Button btnJumpNext5;
    }
}
