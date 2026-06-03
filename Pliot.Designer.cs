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
            lvModelList = new ListView();
            colModelNo = new ColumnHeader();
            colModelName = new ColumnHeader();
            colModelPath = new ColumnHeader();
            pnlModelLoad = new Panel();
            btnModelLoad = new Button();
            lblModelListTitle = new Label();
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
            pnlAngleOverlay = new Panel();
            pnlAngleCenterLine = new Panel();
            lblUserAngleValue = new Label();
            lblPilotAngleValue = new Label();
            pnlThrottleOverlay = new Panel();
            lblUserThrottleTitle = new Label();
            lblUserThrottleValue = new Label();
            lblPilotThrottleTitle = new Label();
            lblPilotThrottleValue = new Label();
            pnlImageIndexOverlay = new Panel();
            lblImageIndexOverlay = new Label();
            picPilotImage = new PictureBox();
            pnlPilotHeader = new Panel();
            btnGenerateJudement = new Button();
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
            pnlModelLoad.SuspendLayout();
            pnlRight.SuspendLayout();
            pnlPilotCard.SuspendLayout();
            pnlTrackBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trbLocation).BeginInit();
            pnlPlaybackControls.SuspendLayout();
            pnlImageHost.SuspendLayout();
            pnlAngleOverlay.SuspendLayout();
            pnlThrottleOverlay.SuspendLayout();
            pnlImageIndexOverlay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPilotImage).BeginInit();
            pnlPilotHeader.SuspendLayout();
            SuspendLayout();
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.FixedPanel = FixedPanel.Panel1;
            splitMain.Location = new Point(8, 8);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(pnlLeft);
            splitMain.Panel1MinSize = 400;
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(pnlRight);
            splitMain.Panel2MinSize = 600;
            splitMain.Size = new Size(1568, 845);
            splitMain.SplitterDistance = 400;
            splitMain.TabIndex = 0;
            // 
            // pnlLeft
            // 
            pnlLeft.Controls.Add(grpSelectedModel);
            pnlLeft.Controls.Add(lvModelList);
            pnlLeft.Controls.Add(pnlModelLoad);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Padding = new Padding(0, 0, 8, 0);
            pnlLeft.Size = new Size(400, 845);
            pnlLeft.TabIndex = 0;
            // 
            // grpSelectedModel
            // 
            grpSelectedModel.Controls.Add(tblSelectedModel);
            grpSelectedModel.Dock = DockStyle.Bottom;
            grpSelectedModel.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            grpSelectedModel.Location = new Point(0, 645);
            grpSelectedModel.Name = "grpSelectedModel";
            grpSelectedModel.Padding = new Padding(10, 8, 10, 10);
            grpSelectedModel.Size = new Size(392, 200);
            grpSelectedModel.TabIndex = 2;
            grpSelectedModel.TabStop = false;
            grpSelectedModel.Text = "선택된 모델 정보";
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
            tblSelectedModel.Size = new Size(372, 164);
            tblSelectedModel.TabIndex = 0;
            // 
            // lblSelectedModelNameTitle
            // 
            lblSelectedModelNameTitle.Dock = DockStyle.Fill;
            lblSelectedModelNameTitle.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
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
            lblSelectedModelPathTitle.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
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
            lblSelectedModelTypeTitle.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
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
            lblSelectedTubPathTitle.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            lblSelectedTubPathTitle.Location = new Point(3, 123);
            lblSelectedTubPathTitle.Name = "lblSelectedTubPathTitle";
            lblSelectedTubPathTitle.Size = new Size(84, 41);
            lblSelectedTubPathTitle.TabIndex = 3;
            lblSelectedTubPathTitle.Text = "Tubs 경로";
            lblSelectedTubPathTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSelectedModelName
            // 
            lblSelectedModelName.Dock = DockStyle.Fill;
            lblSelectedModelName.Font = new Font("맑은 고딕", 9F);
            lblSelectedModelName.Location = new Point(93, 0);
            lblSelectedModelName.Name = "lblSelectedModelName";
            lblSelectedModelName.Size = new Size(276, 41);
            lblSelectedModelName.TabIndex = 4;
            lblSelectedModelName.Text = "model_20260531_001";
            lblSelectedModelName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSelectedModelPath
            // 
            lblSelectedModelPath.Dock = DockStyle.Fill;
            lblSelectedModelPath.Font = new Font("맑은 고딕", 9F);
            lblSelectedModelPath.Location = new Point(93, 41);
            lblSelectedModelPath.Name = "lblSelectedModelPath";
            lblSelectedModelPath.Size = new Size(276, 41);
            lblSelectedModelPath.TabIndex = 5;
            lblSelectedModelPath.Text = "C:\\data\\model_20260531_001.h5";
            lblSelectedModelPath.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSelectedModelType
            // 
            lblSelectedModelType.Dock = DockStyle.Fill;
            lblSelectedModelType.Font = new Font("맑은 고딕", 9F);
            lblSelectedModelType.Location = new Point(93, 82);
            lblSelectedModelType.Name = "lblSelectedModelType";
            lblSelectedModelType.Size = new Size(276, 41);
            lblSelectedModelType.TabIndex = 6;
            lblSelectedModelType.Text = "linear";
            lblSelectedModelType.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSelectedTubPath
            // 
            lblSelectedTubPath.Dock = DockStyle.Fill;
            lblSelectedTubPath.Font = new Font("맑은 고딕", 9F);
            lblSelectedTubPath.Location = new Point(93, 123);
            lblSelectedTubPath.Name = "lblSelectedTubPath";
            lblSelectedTubPath.Size = new Size(276, 41);
            lblSelectedTubPath.TabIndex = 7;
            lblSelectedTubPath.Text = "/mnt/c/Users/cheon/.../data";
            lblSelectedTubPath.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lvModelList
            // 
            lvModelList.Columns.AddRange(new ColumnHeader[] { colModelNo, colModelName, colModelPath });
            lvModelList.Dock = DockStyle.Fill;
            lvModelList.FullRowSelect = true;
            lvModelList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvModelList.Location = new Point(0, 92);
            lvModelList.MultiSelect = false;
            lvModelList.Name = "lvModelList";
            lvModelList.Size = new Size(392, 753);
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
            // pnlModelLoad
            // 
            pnlModelLoad.Controls.Add(btnModelLoad);
            pnlModelLoad.Controls.Add(lblModelListTitle);
            pnlModelLoad.Dock = DockStyle.Top;
            pnlModelLoad.Location = new Point(0, 0);
            pnlModelLoad.Name = "pnlModelLoad";
            pnlModelLoad.Padding = new Padding(8);
            pnlModelLoad.Size = new Size(392, 92);
            pnlModelLoad.TabIndex = 0;
            // 
            // btnModelLoad
            // 
            btnModelLoad.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnModelLoad.Location = new Point(12, 38);
            btnModelLoad.Name = "btnModelLoad";
            btnModelLoad.Size = new Size(366, 38);
            btnModelLoad.TabIndex = 1;
            btnModelLoad.Text = "모델 파일 선택";
            btnModelLoad.UseVisualStyleBackColor = true;
            // 
            // lblModelListTitle
            // 
            lblModelListTitle.AutoSize = true;
            lblModelListTitle.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            lblModelListTitle.Location = new Point(12, 10);
            lblModelListTitle.Name = "lblModelListTitle";
            lblModelListTitle.Size = new Size(84, 19);
            lblModelListTitle.TabIndex = 0;
            lblModelListTitle.Text = "모델 리스트";
            // 
            // pnlRight
            // 
            pnlRight.Controls.Add(pnlPilotCard);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Location = new Point(0, 0);
            pnlRight.Name = "pnlRight";
            pnlRight.Padding = new Padding(8, 0, 0, 0);
            pnlRight.Size = new Size(1164, 845);
            pnlRight.TabIndex = 0;
            // 
            // pnlPilotCard
            // 
            pnlPilotCard.BorderStyle = BorderStyle.FixedSingle;
            pnlPilotCard.Controls.Add(pnlTrackBar);
            pnlPilotCard.Controls.Add(pnlPlaybackControls);
            pnlPilotCard.Controls.Add(pnlImageHost);
            pnlPilotCard.Controls.Add(pnlPilotHeader);
            pnlPilotCard.Dock = DockStyle.Fill;
            pnlPilotCard.Location = new Point(8, 0);
            pnlPilotCard.Name = "pnlPilotCard";
            pnlPilotCard.Padding = new Padding(12);
            pnlPilotCard.Size = new Size(1156, 845);
            pnlPilotCard.TabIndex = 0;
            // 
            // pnlTrackBar
            // 
            pnlTrackBar.Controls.Add(trbLocation);
            pnlTrackBar.Dock = DockStyle.Bottom;
            pnlTrackBar.Location = new Point(12, 701);
            pnlTrackBar.Name = "pnlTrackBar";
            pnlTrackBar.Size = new Size(1130, 57);
            pnlTrackBar.TabIndex = 2;
            // 
            // trbLocation
            // 
            trbLocation.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trbLocation.Enabled = false;
            trbLocation.Location = new Point(0, 7);
            trbLocation.Maximum = 0;
            trbLocation.Name = "trbLocation";
            trbLocation.Size = new Size(1130, 45);
            trbLocation.TabIndex = 1;
            // 
            // pnlPlaybackControls
            // 
            pnlPlaybackControls.Controls.Add(btnJumpPrev5);
            pnlPlaybackControls.Controls.Add(btnPrevImage);
            pnlPlaybackControls.Controls.Add(btnPlayPause);
            pnlPlaybackControls.Controls.Add(cmbSpeed);
            pnlPlaybackControls.Controls.Add(btnReversePlay);
            pnlPlaybackControls.Controls.Add(btnNextImage);
            pnlPlaybackControls.Controls.Add(btnJumpNext5);
            pnlPlaybackControls.Dock = DockStyle.Bottom;
            pnlPlaybackControls.Location = new Point(12, 758);
            pnlPlaybackControls.Name = "pnlPlaybackControls";
            pnlPlaybackControls.Size = new Size(1130, 73);
            pnlPlaybackControls.TabIndex = 3;
            // 
            // btnJumpPrev5
            // 
            btnJumpPrev5.Anchor = AnchorStyles.Top;
            btnJumpPrev5.Font = new Font("맑은 고딕", 13F, FontStyle.Bold);
            btnJumpPrev5.Location = new Point(118, 20);
            btnJumpPrev5.Name = "btnJumpPrev5";
            btnJumpPrev5.Size = new Size(116, 36);
            btnJumpPrev5.TabIndex = 1;
            btnJumpPrev5.Text = "<< 5";
            btnJumpPrev5.UseVisualStyleBackColor = true;
            // 
            // btnPrevImage
            // 
            btnPrevImage.Anchor = AnchorStyles.Top;
            btnPrevImage.Font = new Font("맑은 고딕", 13F, FontStyle.Bold);
            btnPrevImage.Location = new Point(246, 20);
            btnPrevImage.Name = "btnPrevImage";
            btnPrevImage.Size = new Size(116, 36);
            btnPrevImage.TabIndex = 2;
            btnPrevImage.Text = "<";
            btnPrevImage.UseVisualStyleBackColor = true;
            // 
            // btnPlayPause
            // 
            btnPlayPause.Anchor = AnchorStyles.Top;
            btnPlayPause.Font = new Font("맑은 고딕", 13F, FontStyle.Bold);
            btnPlayPause.Location = new Point(374, 20);
            btnPlayPause.Name = "btnPlayPause";
            btnPlayPause.Size = new Size(116, 36);
            btnPlayPause.TabIndex = 3;
            btnPlayPause.Text = "▶";
            btnPlayPause.UseVisualStyleBackColor = true;
            // 
            // cmbSpeed
            // 
            cmbSpeed.Anchor = AnchorStyles.Top;
            cmbSpeed.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSpeed.Font = new Font("맑은 고딕", 12F);
            cmbSpeed.FormattingEnabled = true;
            cmbSpeed.Items.AddRange(new object[] { "0.5x", "1.0x", "2.0x", "3.0x" });
            cmbSpeed.Location = new Point(502, 23);
            cmbSpeed.Name = "cmbSpeed";
            cmbSpeed.Size = new Size(126, 29);
            cmbSpeed.TabIndex = 4;
            // 
            // btnReversePlay
            // 
            btnReversePlay.Anchor = AnchorStyles.Top;
            btnReversePlay.Font = new Font("맑은 고딕", 13F, FontStyle.Bold);
            btnReversePlay.Location = new Point(640, 20);
            btnReversePlay.Name = "btnReversePlay";
            btnReversePlay.Size = new Size(116, 36);
            btnReversePlay.TabIndex = 5;
            btnReversePlay.Text = "◀";
            btnReversePlay.UseVisualStyleBackColor = true;
            // 
            // btnNextImage
            // 
            btnNextImage.Anchor = AnchorStyles.Top;
            btnNextImage.Font = new Font("맑은 고딕", 13F, FontStyle.Bold);
            btnNextImage.Location = new Point(768, 20);
            btnNextImage.Name = "btnNextImage";
            btnNextImage.Size = new Size(116, 36);
            btnNextImage.TabIndex = 6;
            btnNextImage.Text = ">";
            btnNextImage.UseVisualStyleBackColor = true;
            // 
            // btnJumpNext5
            // 
            btnJumpNext5.Anchor = AnchorStyles.Top;
            btnJumpNext5.Font = new Font("맑은 고딕", 13F, FontStyle.Bold);
            btnJumpNext5.Location = new Point(896, 20);
            btnJumpNext5.Name = "btnJumpNext5";
            btnJumpNext5.Size = new Size(116, 36);
            btnJumpNext5.TabIndex = 7;
            btnJumpNext5.Text = "5 >>";
            btnJumpNext5.UseVisualStyleBackColor = true;
            // 
            // pnlImageHost
            // 
            pnlImageHost.BackColor = Color.FromArgb(28, 32, 36);
            pnlImageHost.Controls.Add(picPilotImage);
            pnlImageHost.Dock = DockStyle.Fill;
            pnlImageHost.Location = new Point(12, 64);
            pnlImageHost.Name = "pnlImageHost";
            pnlImageHost.Size = new Size(1130, 767);
            pnlImageHost.TabIndex = 1;
            // 
            // pnlAngleOverlay
            // 
            pnlAngleOverlay.BackColor = Color.FromArgb(120, 22, 26, 32);
            pnlAngleOverlay.Controls.Add(pnlAngleCenterLine);
            pnlAngleOverlay.Controls.Add(lblUserAngleValue);
            pnlAngleOverlay.Controls.Add(lblPilotAngleValue);
            pnlAngleOverlay.Location = new Point(374, 233);
            pnlAngleOverlay.Name = "pnlAngleOverlay";
            pnlAngleOverlay.Size = new Size(420, 130);
            pnlAngleOverlay.TabIndex = 3;
            // 
            // pnlAngleCenterLine
            // 
            pnlAngleCenterLine.BackColor = Color.Gainsboro;
            pnlAngleCenterLine.Location = new Point(209, 18);
            pnlAngleCenterLine.Name = "pnlAngleCenterLine";
            pnlAngleCenterLine.Size = new Size(3, 74);
            pnlAngleCenterLine.TabIndex = 1;
            pnlAngleCenterLine.Visible = false;
            // 
            // lblUserAngleValue
            // 
            lblUserAngleValue.Font = new Font("맑은 고딕", 14F, FontStyle.Bold);
            lblUserAngleValue.ForeColor = Color.Lime;
            lblUserAngleValue.Location = new Point(14, 92);
            lblUserAngleValue.Name = "lblUserAngleValue";
            lblUserAngleValue.Size = new Size(132, 30);
            lblUserAngleValue.TabIndex = 0;
            lblUserAngleValue.Text = "-0.25";
            lblUserAngleValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblPilotAngleValue
            // 
            lblPilotAngleValue.Font = new Font("맑은 고딕", 14F, FontStyle.Bold);
            lblPilotAngleValue.ForeColor = Color.DeepSkyBlue;
            lblPilotAngleValue.Location = new Point(274, 92);
            lblPilotAngleValue.Name = "lblPilotAngleValue";
            lblPilotAngleValue.Size = new Size(132, 30);
            lblPilotAngleValue.TabIndex = 2;
            lblPilotAngleValue.Text = "-0.23";
            lblPilotAngleValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // pnlThrottleOverlay
            // 
            pnlThrottleOverlay.BackColor = Color.FromArgb(120, 22, 26, 32);
            pnlThrottleOverlay.Controls.Add(lblUserThrottleTitle);
            pnlThrottleOverlay.Controls.Add(lblUserThrottleValue);
            pnlThrottleOverlay.Controls.Add(lblPilotThrottleTitle);
            pnlThrottleOverlay.Controls.Add(lblPilotThrottleValue);
            pnlThrottleOverlay.Location = new Point(12, 420);
            pnlThrottleOverlay.Name = "pnlThrottleOverlay";
            pnlThrottleOverlay.Size = new Size(220, 92);
            pnlThrottleOverlay.TabIndex = 2;
            // 
            // lblUserThrottleTitle
            // 
            lblUserThrottleTitle.AutoSize = true;
            lblUserThrottleTitle.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            lblUserThrottleTitle.ForeColor = Color.White;
            lblUserThrottleTitle.Location = new Point(14, 18);
            lblUserThrottleTitle.Name = "lblUserThrottleTitle";
            lblUserThrottleTitle.Size = new Size(97, 19);
            lblUserThrottleTitle.TabIndex = 0;
            lblUserThrottleTitle.Text = "사용자 속력";
            // 
            // lblUserThrottleValue
            // 
            lblUserThrottleValue.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            lblUserThrottleValue.ForeColor = Color.Lime;
            lblUserThrottleValue.Location = new Point(128, 18);
            lblUserThrottleValue.Name = "lblUserThrottleValue";
            lblUserThrottleValue.Size = new Size(74, 19);
            lblUserThrottleValue.TabIndex = 1;
            lblUserThrottleValue.Text = "0.42";
            lblUserThrottleValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblPilotThrottleTitle
            // 
            lblPilotThrottleTitle.AutoSize = true;
            lblPilotThrottleTitle.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            lblPilotThrottleTitle.ForeColor = Color.White;
            lblPilotThrottleTitle.Location = new Point(14, 54);
            lblPilotThrottleTitle.Name = "lblPilotThrottleTitle";
            lblPilotThrottleTitle.Size = new Size(81, 19);
            lblPilotThrottleTitle.TabIndex = 2;
            lblPilotThrottleTitle.Text = "AI 속력";
            // 
            // lblPilotThrottleValue
            // 
            lblPilotThrottleValue.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            lblPilotThrottleValue.ForeColor = Color.DeepSkyBlue;
            lblPilotThrottleValue.Location = new Point(128, 54);
            lblPilotThrottleValue.Name = "lblPilotThrottleValue";
            lblPilotThrottleValue.Size = new Size(74, 19);
            lblPilotThrottleValue.TabIndex = 3;
            lblPilotThrottleValue.Text = "0.41";
            lblPilotThrottleValue.TextAlign = ContentAlignment.MiddleRight;
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
            // picPilotImage
            // 
            picPilotImage.BackColor = Color.FromArgb(35, 39, 44);
            picPilotImage.Controls.Add(pnlAngleOverlay);
            picPilotImage.Controls.Add(pnlThrottleOverlay);
            picPilotImage.Controls.Add(pnlImageIndexOverlay);
            picPilotImage.Dock = DockStyle.Fill;
            picPilotImage.Location = new Point(0, 0);
            picPilotImage.Name = "picPilotImage";
            picPilotImage.Size = new Size(1130, 767);
            picPilotImage.SizeMode = PictureBoxSizeMode.Zoom;
            picPilotImage.TabIndex = 0;
            picPilotImage.TabStop = false;
            // 
            // pnlPilotHeader
            // 
            pnlPilotHeader.Controls.Add(btnGenerateJudement);
            pnlPilotHeader.Controls.Add(btnPilotChart);
            pnlPilotHeader.Controls.Add(btnTubInput);
            pnlPilotHeader.Controls.Add(lblTubPathValue);
            pnlPilotHeader.Controls.Add(lblTubPathTitle);
            pnlPilotHeader.Dock = DockStyle.Top;
            pnlPilotHeader.Location = new Point(12, 12);
            pnlPilotHeader.Name = "pnlPilotHeader";
            pnlPilotHeader.Size = new Size(1130, 52);
            pnlPilotHeader.TabIndex = 0;
            // 
            // btnGenerateJudement
            // 
            btnGenerateJudement.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGenerateJudement.Location = new Point(868, 8);
            btnGenerateJudement.Name = "btnGenerateJudement";
            btnGenerateJudement.Size = new Size(126, 36);
            btnGenerateJudement.TabIndex = 3;
            btnGenerateJudement.Text = "AI 판단 생성";
            btnGenerateJudement.UseVisualStyleBackColor = true;
            // 
            // btnPilotChart
            // 
            btnPilotChart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPilotChart.Location = new Point(736, 8);
            btnPilotChart.Name = "btnPilotChart";
            btnPilotChart.Size = new Size(126, 36);
            btnPilotChart.TabIndex = 4;
            btnPilotChart.Text = "그래프";
            btnPilotChart.UseVisualStyleBackColor = true;
            // 
            // btnTubInput
            // 
            btnTubInput.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTubInput.Location = new Point(1000, 8);
            btnTubInput.Name = "btnTubInput";
            btnTubInput.Size = new Size(126, 36);
            btnTubInput.TabIndex = 2;
            btnTubInput.Text = "TUB 입력";
            btnTubInput.UseVisualStyleBackColor = true;
            // 
            // lblTubPathValue
            // 
            lblTubPathValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTubPathValue.Font = new Font("맑은 고딕", 11F);
            lblTubPathValue.Location = new Point(90, 12);
            lblTubPathValue.Name = "lblTubPathValue";
            lblTubPathValue.Size = new Size(640, 28);
            lblTubPathValue.TabIndex = 1;
            lblTubPathValue.Text = "model_20260531_001";
            lblTubPathValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTubPathTitle
            // 
            lblTubPathTitle.AutoSize = true;
            lblTubPathTitle.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            lblTubPathTitle.Location = new Point(0, 14);
            lblTubPathTitle.Name = "lblTubPathTitle";
            lblTubPathTitle.Size = new Size(64, 20);
            lblTubPathTitle.TabIndex = 0;
            lblTubPathTitle.Text = "모델명:";
            // 
            // Pliot
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1584, 861);
            Controls.Add(splitMain);
            Font = new Font("맑은 고딕", 11.25F);
            Margin = new Padding(2);
            MinimumSize = new Size(1100, 700);
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
            pnlModelLoad.ResumeLayout(false);
            pnlModelLoad.PerformLayout();
            pnlRight.ResumeLayout(false);
            pnlPilotCard.ResumeLayout(false);
            pnlTrackBar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)trbLocation).EndInit();
            pnlPlaybackControls.ResumeLayout(false);
            pnlImageHost.ResumeLayout(false);
            pnlAngleOverlay.ResumeLayout(false);
            pnlThrottleOverlay.ResumeLayout(false);
            pnlThrottleOverlay.PerformLayout();
            pnlImageIndexOverlay.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picPilotImage).EndInit();
            pnlPilotHeader.ResumeLayout(false);
            pnlPilotHeader.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitMain;
        private Panel pnlLeft;
        private Panel pnlModelLoad;
        private Button btnModelLoad;
        private Label lblModelListTitle;
        private ListView lvModelList;
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
        private Button btnGenerateJudement;
        private Button btnPilotChart;
        private Button btnTubInput;
        private Label lblTubPathValue;
        private Label lblTubPathTitle;
        private Panel pnlImageHost;
        private PictureBox picPilotImage;
        private Panel pnlImageIndexOverlay;
        private Label lblImageIndexOverlay;
        private Panel pnlThrottleOverlay;
        private Label lblUserThrottleTitle;
        private Label lblUserThrottleValue;
        private Label lblPilotThrottleTitle;
        private Label lblPilotThrottleValue;
        private Panel pnlAngleOverlay;
        private Label lblUserAngleValue;
        private Panel pnlAngleCenterLine;
        private Label lblPilotAngleValue;
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
