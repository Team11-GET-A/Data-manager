namespace AD_AI_LearningData_Editor
{
    partial class frmMain
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
            components = new System.ComponentModel.Container();
            ListViewItem listViewItem1 = new ListViewItem(new string[] { "[파일추가]" }, 1, Color.Empty, Color.Empty, new Font("맑은 고딕", 12F));
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            ListViewItem listViewItem2 = new ListViewItem(new string[] { "[파일목록]" }, 0, Color.Empty, Color.Empty, new Font("맑은 고딕", 12F, FontStyle.Regular, GraphicsUnit.Point, 129));
            ListViewItem listViewItem3 = new ListViewItem(new string[] { "[휴지통]" }, 2, Color.Empty, Color.Empty, new Font("맑은 고딕", 12F, FontStyle.Regular, GraphicsUnit.Point, 129));
            sdrSeekBar = new MaterialSkin.Controls.MaterialSlider();
            pnlVideo = new Panel();
            btnContrastProperty = new MaterialSkin.Controls.MaterialButton();
            btnROI = new MaterialSkin.Controls.MaterialButton();
            btnColorProperty = new MaterialSkin.Controls.MaterialButton();
            btnMirror = new MaterialSkin.Controls.MaterialButton();
            btnNoise = new MaterialSkin.Controls.MaterialButton();
            btnDel = new MaterialSkin.Controls.MaterialButton();
            pnlProperty = new Panel();
            crdProperty = new MaterialSkin.Controls.MaterialCard();
            textBox1 = new TextBox();
            btnSave = new MaterialSkin.Controls.MaterialButton();
            btnPre5F = new MaterialSkin.Controls.MaterialButton();
            btnOpnFolderList = new Button();
            pnlFolderList = new Panel();
            btnRestoration = new Button();
            lblLstVwName = new Label();
            btnOpnFileExplrr = new Button();
            lblFolderList = new Label();
            lstviewFileList = new ListView();
            imglst1 = new ImageList(components);
            lstviewTrash = new ListView();
            lstviewMain = new ListView();
            lstviewFileListD = new ListView();
            pnlCtrl = new Panel();
            btnSpeedPopup = new MaterialSkin.Controls.MaterialButton();
            btnPre1F = new MaterialSkin.Controls.MaterialButton();
            btnPlayStop = new MaterialSkin.Controls.MaterialButton();
            btnNxt5F = new MaterialSkin.Controls.MaterialButton();
            btnNxt1F = new MaterialSkin.Controls.MaterialButton();
            pnlSpeedPopup = new Panel();
            btnSpeedMinus = new MaterialSkin.Controls.MaterialButton();
            btnSpeedPlus = new MaterialSkin.Controls.MaterialButton();
            sdrSpeedController = new MaterialSkin.Controls.MaterialSlider();
            lblSpeedText = new Label();
            btnOpnFolderList2 = new Button();
            pnlProperty.SuspendLayout();
            crdProperty.SuspendLayout();
            pnlFolderList.SuspendLayout();
            pnlCtrl.SuspendLayout();
            pnlSpeedPopup.SuspendLayout();
            SuspendLayout();
            // 
            // sdrSeekBar
            // 
            sdrSeekBar.Depth = 0;
            sdrSeekBar.ForeColor = Color.FromArgb(222, 0, 0, 0);
            sdrSeekBar.Location = new Point(21, 589);
            sdrSeekBar.MouseState = MaterialSkin.MouseState.HOVER;
            sdrSeekBar.Name = "sdrSeekBar";
            sdrSeekBar.ShowValue = false;
            sdrSeekBar.Size = new Size(1085, 40);
            sdrSeekBar.TabIndex = 0;
            sdrSeekBar.Text = "n/m";
            sdrSeekBar.Click += materialSlider1_Click;
            // 
            // pnlVideo
            // 
            pnlVideo.BackColor = Color.Black;
            pnlVideo.ForeColor = Color.Coral;
            pnlVideo.Location = new Point(21, 11);
            pnlVideo.Name = "pnlVideo";
            pnlVideo.Size = new Size(1085, 552);
            pnlVideo.TabIndex = 1;
            // 
            // btnContrastProperty
            // 
            btnContrastProperty.AutoSize = false;
            btnContrastProperty.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnContrastProperty.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnContrastProperty.Depth = 0;
            btnContrastProperty.HighEmphasis = true;
            btnContrastProperty.Icon = null;
            btnContrastProperty.Location = new Point(9, 16);
            btnContrastProperty.Margin = new Padding(4, 6, 4, 6);
            btnContrastProperty.MouseState = MaterialSkin.MouseState.HOVER;
            btnContrastProperty.Name = "btnContrastProperty";
            btnContrastProperty.NoAccentTextColor = Color.Empty;
            btnContrastProperty.Size = new Size(150, 100);
            btnContrastProperty.TabIndex = 5;
            btnContrastProperty.Text = "명암";
            btnContrastProperty.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnContrastProperty.UseAccentColor = false;
            btnContrastProperty.UseVisualStyleBackColor = true;
            // 
            // btnROI
            // 
            btnROI.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnROI.AutoSize = false;
            btnROI.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnROI.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnROI.Depth = 0;
            btnROI.HighEmphasis = true;
            btnROI.Icon = null;
            btnROI.Location = new Point(9, 129);
            btnROI.Margin = new Padding(4, 6, 4, 6);
            btnROI.MouseState = MaterialSkin.MouseState.HOVER;
            btnROI.Name = "btnROI";
            btnROI.NoAccentTextColor = Color.Empty;
            btnROI.Size = new Size(150, 100);
            btnROI.TabIndex = 4;
            btnROI.Text = "ROI";
            btnROI.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnROI.UseAccentColor = false;
            btnROI.UseVisualStyleBackColor = true;
            // 
            // btnColorProperty
            // 
            btnColorProperty.Anchor = AnchorStyles.Top;
            btnColorProperty.AutoSize = false;
            btnColorProperty.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnColorProperty.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnColorProperty.Depth = 0;
            btnColorProperty.HighEmphasis = true;
            btnColorProperty.Icon = null;
            btnColorProperty.Location = new Point(167, 16);
            btnColorProperty.Margin = new Padding(4, 6, 4, 6);
            btnColorProperty.MouseState = MaterialSkin.MouseState.HOVER;
            btnColorProperty.Name = "btnColorProperty";
            btnColorProperty.NoAccentTextColor = Color.Empty;
            btnColorProperty.Size = new Size(150, 100);
            btnColorProperty.TabIndex = 3;
            btnColorProperty.Text = "색 필터";
            btnColorProperty.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnColorProperty.UseAccentColor = false;
            btnColorProperty.UseVisualStyleBackColor = true;
            // 
            // btnMirror
            // 
            btnMirror.Anchor = AnchorStyles.Bottom;
            btnMirror.AutoSize = false;
            btnMirror.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnMirror.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnMirror.Depth = 0;
            btnMirror.HighEmphasis = true;
            btnMirror.Icon = null;
            btnMirror.Location = new Point(167, 129);
            btnMirror.Margin = new Padding(4, 6, 4, 6);
            btnMirror.MouseState = MaterialSkin.MouseState.HOVER;
            btnMirror.Name = "btnMirror";
            btnMirror.NoAccentTextColor = Color.Empty;
            btnMirror.Size = new Size(150, 100);
            btnMirror.TabIndex = 2;
            btnMirror.Text = "좌우 반전";
            btnMirror.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnMirror.UseAccentColor = false;
            btnMirror.UseVisualStyleBackColor = true;
            btnMirror.Click += materialButton2_Click;
            // 
            // btnNoise
            // 
            btnNoise.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNoise.AutoSize = false;
            btnNoise.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnNoise.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnNoise.Depth = 0;
            btnNoise.HighEmphasis = true;
            btnNoise.Icon = null;
            btnNoise.Location = new Point(325, 16);
            btnNoise.Margin = new Padding(4, 6, 4, 6);
            btnNoise.MouseState = MaterialSkin.MouseState.HOVER;
            btnNoise.Name = "btnNoise";
            btnNoise.NoAccentTextColor = Color.Empty;
            btnNoise.Size = new Size(150, 100);
            btnNoise.TabIndex = 1;
            btnNoise.Text = "화질 열화";
            btnNoise.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnNoise.UseAccentColor = false;
            btnNoise.UseVisualStyleBackColor = true;
            // 
            // btnDel
            // 
            btnDel.AutoSize = false;
            btnDel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDel.BackColor = SystemColors.Control;
            btnDel.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnDel.Depth = 0;
            btnDel.HighEmphasis = true;
            btnDel.Icon = null;
            btnDel.Location = new Point(552, 10);
            btnDel.Margin = new Padding(4, 6, 4, 6);
            btnDel.MouseState = MaterialSkin.MouseState.HOVER;
            btnDel.Name = "btnDel";
            btnDel.NoAccentTextColor = Color.Empty;
            btnDel.Size = new Size(110, 100);
            btnDel.TabIndex = 0;
            btnDel.Text = "삭제";
            btnDel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnDel.UseAccentColor = true;
            btnDel.UseVisualStyleBackColor = false;
            // 
            // pnlProperty
            // 
            pnlProperty.BackColor = SystemColors.ControlDark;
            pnlProperty.BorderStyle = BorderStyle.Fixed3D;
            pnlProperty.Controls.Add(crdProperty);
            pnlProperty.Location = new Point(1111, 572);
            pnlProperty.Name = "pnlProperty";
            pnlProperty.Size = new Size(483, 245);
            pnlProperty.TabIndex = 6;
            // 
            // crdProperty
            // 
            crdProperty.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            crdProperty.BackColor = Color.FromArgb(255, 255, 255);
            crdProperty.Controls.Add(btnContrastProperty);
            crdProperty.Controls.Add(btnMirror);
            crdProperty.Controls.Add(btnColorProperty);
            crdProperty.Controls.Add(btnROI);
            crdProperty.Controls.Add(btnNoise);
            crdProperty.Depth = 0;
            crdProperty.ForeColor = Color.FromArgb(222, 0, 0, 0);
            crdProperty.Location = new Point(-2, -5);
            crdProperty.Margin = new Padding(14);
            crdProperty.MouseState = MaterialSkin.MouseState.HOVER;
            crdProperty.Name = "crdProperty";
            crdProperty.Padding = new Padding(14);
            crdProperty.Size = new Size(483, 245);
            crdProperty.TabIndex = 0;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.Lime;
            textBox1.Location = new Point(6, 664);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(424, 153);
            textBox1.TabIndex = 7;
            // 
            // btnSave
            // 
            btnSave.AutoSize = false;
            btnSave.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSave.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnSave.Depth = 0;
            btnSave.HighEmphasis = true;
            btnSave.Icon = null;
            btnSave.Location = new Point(432, 10);
            btnSave.Margin = new Padding(4, 6, 4, 6);
            btnSave.MouseState = MaterialSkin.MouseState.HOVER;
            btnSave.Name = "btnSave";
            btnSave.NoAccentTextColor = Color.Empty;
            btnSave.Size = new Size(110, 100);
            btnSave.TabIndex = 8;
            btnSave.Text = "저장";
            btnSave.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnSave.UseAccentColor = false;
            btnSave.UseVisualStyleBackColor = true;
            // 
            // btnPre5F
            // 
            btnPre5F.AutoSize = false;
            btnPre5F.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnPre5F.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnPre5F.Depth = 0;
            btnPre5F.HighEmphasis = true;
            btnPre5F.Icon = null;
            btnPre5F.Location = new Point(4, 16);
            btnPre5F.Margin = new Padding(4, 6, 4, 6);
            btnPre5F.MouseState = MaterialSkin.MouseState.HOVER;
            btnPre5F.Name = "btnPre5F";
            btnPre5F.NoAccentTextColor = Color.Empty;
            btnPre5F.Size = new Size(60, 50);
            btnPre5F.TabIndex = 10;
            btnPre5F.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnPre5F.UseAccentColor = false;
            btnPre5F.UseVisualStyleBackColor = true;
            // 
            // btnOpnFolderList
            // 
            btnOpnFolderList.Location = new Point(3, 5);
            btnOpnFolderList.Name = "btnOpnFolderList";
            btnOpnFolderList.Size = new Size(35, 35);
            btnOpnFolderList.TabIndex = 11;
            btnOpnFolderList.UseVisualStyleBackColor = true;
            btnOpnFolderList.Click += btnOpnFolderList1_Click;
            // 
            // pnlFolderList
            // 
            pnlFolderList.BorderStyle = BorderStyle.Fixed3D;
            pnlFolderList.Controls.Add(btnRestoration);
            pnlFolderList.Controls.Add(lblLstVwName);
            pnlFolderList.Controls.Add(btnOpnFileExplrr);
            pnlFolderList.Controls.Add(lblFolderList);
            pnlFolderList.Controls.Add(btnOpnFolderList);
            pnlFolderList.Controls.Add(lstviewFileList);
            pnlFolderList.Controls.Add(lstviewTrash);
            pnlFolderList.Controls.Add(lstviewMain);
            pnlFolderList.Controls.Add(lstviewFileListD);
            pnlFolderList.Location = new Point(1111, 11);
            pnlFolderList.Name = "pnlFolderList";
            pnlFolderList.Size = new Size(483, 554);
            pnlFolderList.TabIndex = 12;
            // 
            // btnRestoration
            // 
            btnRestoration.Location = new Point(397, 5);
            btnRestoration.Name = "btnRestoration";
            btnRestoration.Size = new Size(35, 35);
            btnRestoration.TabIndex = 19;
            btnRestoration.UseVisualStyleBackColor = true;
            btnRestoration.Visible = false;
            // 
            // lblLstVwName
            // 
            lblLstVwName.Font = new Font("맑은 고딕", 18F);
            lblLstVwName.Location = new Point(44, 8);
            lblLstVwName.Name = "lblLstVwName";
            lblLstVwName.Size = new Size(314, 32);
            lblLstVwName.TabIndex = 18;
            // 
            // btnOpnFileExplrr
            // 
            btnOpnFileExplrr.Location = new Point(438, 5);
            btnOpnFileExplrr.Name = "btnOpnFileExplrr";
            btnOpnFileExplrr.Size = new Size(35, 35);
            btnOpnFileExplrr.TabIndex = 17;
            btnOpnFileExplrr.UseVisualStyleBackColor = true;
            // 
            // lblFolderList
            // 
            lblFolderList.AutoSize = true;
            lblFolderList.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            lblFolderList.Location = new Point(3, 1);
            lblFolderList.Name = "lblFolderList";
            lblFolderList.Padding = new Padding(0, 0, 0, 3);
            lblFolderList.Size = new Size(0, 40);
            lblFolderList.TabIndex = 15;
            // 
            // lstviewFileList
            // 
            lstviewFileList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewItem1.Tag = "파일추가";
            lstviewFileList.Items.AddRange(new ListViewItem[] { listViewItem1 });
            lstviewFileList.Location = new Point(0, 44);
            lstviewFileList.Name = "lstviewFileList";
            lstviewFileList.Size = new Size(483, 25);
            lstviewFileList.SmallImageList = imglst1;
            lstviewFileList.TabIndex = 13;
            lstviewFileList.UseCompatibleStateImageBehavior = false;
            lstviewFileList.View = View.List;
            lstviewFileList.Visible = false;
            lstviewFileList.SelectedIndexChanged += lstviewFileList_SelectedIndexChanged;
            // 
            // imglst1
            // 
            imglst1.ColorDepth = ColorDepth.Depth32Bit;
            imglst1.ImageStream = (ImageListStreamer)resources.GetObject("imglst1.ImageStream");
            imglst1.TransparentColor = Color.Transparent;
            imglst1.Images.SetKeyName(0, "노란 폴더2.png");
            imglst1.Images.SetKeyName(1, "파란 폴더2.png");
            imglst1.Images.SetKeyName(2, "휴지통2.png");
            // 
            // lstviewTrash
            // 
            lstviewTrash.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstviewTrash.Location = new Point(-1, 44);
            lstviewTrash.Name = "lstviewTrash";
            lstviewTrash.Size = new Size(483, 509);
            lstviewTrash.TabIndex = 14;
            lstviewTrash.UseCompatibleStateImageBehavior = false;
            lstviewTrash.View = View.List;
            lstviewTrash.Visible = false;
            // 
            // lstviewMain
            // 
            lstviewMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewItem2.Tag = "파일목록";
            listViewItem3.Tag = "휴지통";
            lstviewMain.Items.AddRange(new ListViewItem[] { listViewItem2, listViewItem3 });
            lstviewMain.Location = new Point(-1, 44);
            lstviewMain.Name = "lstviewMain";
            lstviewMain.Size = new Size(483, 509);
            lstviewMain.SmallImageList = imglst1;
            lstviewMain.TabIndex = 12;
            lstviewMain.UseCompatibleStateImageBehavior = false;
            lstviewMain.View = View.List;
            lstviewMain.SelectedIndexChanged += listView1_SelectedIndexChanged;
            // 
            // lstviewFileListD
            // 
            lstviewFileListD.Location = new Point(0, 69);
            lstviewFileListD.Name = "lstviewFileListD";
            lstviewFileListD.Size = new Size(483, 489);
            lstviewFileListD.TabIndex = 16;
            lstviewFileListD.UseCompatibleStateImageBehavior = false;
            lstviewFileListD.View = View.List;
            lstviewFileListD.Visible = false;
            // 
            // pnlCtrl
            // 
            pnlCtrl.Controls.Add(btnSpeedPopup);
            pnlCtrl.Controls.Add(btnPre1F);
            pnlCtrl.Controls.Add(btnPlayStop);
            pnlCtrl.Controls.Add(btnNxt5F);
            pnlCtrl.Controls.Add(btnNxt1F);
            pnlCtrl.Controls.Add(btnPre5F);
            pnlCtrl.Controls.Add(btnSave);
            pnlCtrl.Controls.Add(btnDel);
            pnlCtrl.Location = new Point(436, 685);
            pnlCtrl.Name = "pnlCtrl";
            pnlCtrl.Size = new Size(670, 132);
            pnlCtrl.TabIndex = 13;
            // 
            // btnSpeedPopup
            // 
            btnSpeedPopup.AutoSize = false;
            btnSpeedPopup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSpeedPopup.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnSpeedPopup.Depth = 0;
            btnSpeedPopup.HighEmphasis = true;
            btnSpeedPopup.Icon = null;
            btnSpeedPopup.Location = new Point(4, 78);
            btnSpeedPopup.Margin = new Padding(4, 6, 4, 6);
            btnSpeedPopup.MouseState = MaterialSkin.MouseState.HOVER;
            btnSpeedPopup.Name = "btnSpeedPopup";
            btnSpeedPopup.NoAccentTextColor = Color.Empty;
            btnSpeedPopup.Size = new Size(418, 28);
            btnSpeedPopup.TabIndex = 16;
            btnSpeedPopup.Text = "배속";
            btnSpeedPopup.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnSpeedPopup.UseAccentColor = false;
            btnSpeedPopup.UseVisualStyleBackColor = true;
            // 
            // btnPre1F
            // 
            btnPre1F.AutoSize = false;
            btnPre1F.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnPre1F.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnPre1F.Depth = 0;
            btnPre1F.HighEmphasis = true;
            btnPre1F.Icon = null;
            btnPre1F.Location = new Point(72, 16);
            btnPre1F.Margin = new Padding(4, 6, 4, 6);
            btnPre1F.MouseState = MaterialSkin.MouseState.HOVER;
            btnPre1F.Name = "btnPre1F";
            btnPre1F.NoAccentTextColor = Color.Empty;
            btnPre1F.Size = new Size(60, 50);
            btnPre1F.TabIndex = 14;
            btnPre1F.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnPre1F.UseAccentColor = false;
            btnPre1F.UseVisualStyleBackColor = true;
            // 
            // btnPlayStop
            // 
            btnPlayStop.AutoSize = false;
            btnPlayStop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnPlayStop.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnPlayStop.Depth = 0;
            btnPlayStop.HighEmphasis = true;
            btnPlayStop.Icon = null;
            btnPlayStop.Location = new Point(140, 16);
            btnPlayStop.Margin = new Padding(4, 6, 4, 6);
            btnPlayStop.MouseState = MaterialSkin.MouseState.HOVER;
            btnPlayStop.Name = "btnPlayStop";
            btnPlayStop.NoAccentTextColor = Color.Empty;
            btnPlayStop.Size = new Size(147, 50);
            btnPlayStop.TabIndex = 13;
            btnPlayStop.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnPlayStop.UseAccentColor = false;
            btnPlayStop.UseVisualStyleBackColor = true;
            btnPlayStop.Click += btnPlayStop_Click;
            // 
            // btnNxt5F
            // 
            btnNxt5F.AutoSize = false;
            btnNxt5F.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnNxt5F.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnNxt5F.Depth = 0;
            btnNxt5F.HighEmphasis = true;
            btnNxt5F.Icon = null;
            btnNxt5F.Location = new Point(362, 16);
            btnNxt5F.Margin = new Padding(4, 6, 4, 6);
            btnNxt5F.MouseState = MaterialSkin.MouseState.HOVER;
            btnNxt5F.Name = "btnNxt5F";
            btnNxt5F.NoAccentTextColor = Color.Empty;
            btnNxt5F.Size = new Size(60, 50);
            btnNxt5F.TabIndex = 12;
            btnNxt5F.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnNxt5F.UseAccentColor = false;
            btnNxt5F.UseVisualStyleBackColor = true;
            // 
            // btnNxt1F
            // 
            btnNxt1F.AutoSize = false;
            btnNxt1F.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnNxt1F.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnNxt1F.Depth = 0;
            btnNxt1F.HighEmphasis = true;
            btnNxt1F.Icon = null;
            btnNxt1F.Location = new Point(295, 16);
            btnNxt1F.Margin = new Padding(4, 6, 4, 6);
            btnNxt1F.MouseState = MaterialSkin.MouseState.HOVER;
            btnNxt1F.Name = "btnNxt1F";
            btnNxt1F.NoAccentTextColor = Color.Empty;
            btnNxt1F.Size = new Size(60, 50);
            btnNxt1F.TabIndex = 11;
            btnNxt1F.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnNxt1F.UseAccentColor = false;
            btnNxt1F.UseVisualStyleBackColor = true;
            // 
            // pnlSpeedPopup
            // 
            pnlSpeedPopup.BackColor = Color.Gray;
            pnlSpeedPopup.Controls.Add(btnSpeedMinus);
            pnlSpeedPopup.Controls.Add(btnSpeedPlus);
            pnlSpeedPopup.Controls.Add(sdrSpeedController);
            pnlSpeedPopup.Controls.Add(lblSpeedText);
            pnlSpeedPopup.Location = new Point(440, 679);
            pnlSpeedPopup.Name = "pnlSpeedPopup";
            pnlSpeedPopup.Size = new Size(418, 84);
            pnlSpeedPopup.TabIndex = 15;
            pnlSpeedPopup.Visible = false;
            // 
            // btnSpeedMinus
            // 
            btnSpeedMinus.AutoSize = false;
            btnSpeedMinus.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSpeedMinus.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnSpeedMinus.Depth = 0;
            btnSpeedMinus.HighEmphasis = true;
            btnSpeedMinus.Icon = null;
            btnSpeedMinus.Location = new Point(1, 1);
            btnSpeedMinus.Margin = new Padding(4, 6, 4, 6);
            btnSpeedMinus.MouseState = MaterialSkin.MouseState.HOVER;
            btnSpeedMinus.Name = "btnSpeedMinus";
            btnSpeedMinus.NoAccentTextColor = Color.Empty;
            btnSpeedMinus.Size = new Size(31, 82);
            btnSpeedMinus.TabIndex = 2;
            btnSpeedMinus.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnSpeedMinus.UseAccentColor = false;
            btnSpeedMinus.UseVisualStyleBackColor = true;
            // 
            // btnSpeedPlus
            // 
            btnSpeedPlus.AutoSize = false;
            btnSpeedPlus.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSpeedPlus.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnSpeedPlus.Depth = 0;
            btnSpeedPlus.HighEmphasis = true;
            btnSpeedPlus.Icon = null;
            btnSpeedPlus.Location = new Point(386, 1);
            btnSpeedPlus.Margin = new Padding(4, 6, 4, 6);
            btnSpeedPlus.MouseState = MaterialSkin.MouseState.HOVER;
            btnSpeedPlus.Name = "btnSpeedPlus";
            btnSpeedPlus.NoAccentTextColor = Color.Empty;
            btnSpeedPlus.Size = new Size(31, 82);
            btnSpeedPlus.TabIndex = 1;
            btnSpeedPlus.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnSpeedPlus.UseAccentColor = false;
            btnSpeedPlus.UseVisualStyleBackColor = true;
            // 
            // sdrSpeedController
            // 
            sdrSpeedController.Depth = 0;
            sdrSpeedController.ForeColor = Color.Gray;
            sdrSpeedController.Location = new Point(38, 6);
            sdrSpeedController.MouseState = MaterialSkin.MouseState.HOVER;
            sdrSpeedController.Name = "sdrSpeedController";
            sdrSpeedController.ShowValue = false;
            sdrSpeedController.Size = new Size(340, 40);
            sdrSpeedController.TabIndex = 0;
            sdrSpeedController.Text = "";
            // 
            // lblSpeedText
            // 
            lblSpeedText.ForeColor = Color.Black;
            lblSpeedText.Location = new Point(180, 51);
            lblSpeedText.Name = "lblSpeedText";
            lblSpeedText.Size = new Size(58, 28);
            lblSpeedText.TabIndex = 3;
            lblSpeedText.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnOpnFolderList2
            // 
            btnOpnFolderList2.Location = new Point(442, 5);
            btnOpnFolderList2.Name = "btnOpnFolderList2";
            btnOpnFolderList2.Size = new Size(35, 35);
            btnOpnFolderList2.TabIndex = 11;
            btnOpnFolderList2.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(1600, 900);
            Controls.Add(pnlSpeedPopup);
            Controls.Add(pnlCtrl);
            Controls.Add(pnlFolderList);
            Controls.Add(textBox1);
            Controls.Add(pnlProperty);
            Controls.Add(pnlVideo);
            Controls.Add(sdrSeekBar);
            Name = "frmMain";
            Padding = new Padding(0);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "11팀";
            pnlProperty.ResumeLayout(false);
            crdProperty.ResumeLayout(false);
            pnlFolderList.ResumeLayout(false);
            pnlFolderList.PerformLayout();
            pnlCtrl.ResumeLayout(false);
            pnlSpeedPopup.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaterialSkin.Controls.MaterialSlider sdrSeekBar;
        private Panel pnlVideo;
        private MaterialSkin.Controls.MaterialButton btnDel;
        private MaterialSkin.Controls.MaterialButton btnContrastProperty;
        private MaterialSkin.Controls.MaterialButton btnROI;
        private MaterialSkin.Controls.MaterialButton btnColorProperty;
        private MaterialSkin.Controls.MaterialButton btnMirror;
        private MaterialSkin.Controls.MaterialButton btnNoise;
        private Panel pnlProperty;
        private MaterialSkin.Controls.MaterialCard crdProperty;
        private TextBox textBox1;
        private MaterialSkin.Controls.MaterialButton btnSave;
        private MaterialSkin.Controls.MaterialButton btnPre5F;
        private Button btnOpnFolderList;
        private Panel pnlFolderList;
        private ListView lstviewMain;
        private Panel pnlCtrl;
        private MaterialSkin.Controls.MaterialButton btnPre1F;
        private MaterialSkin.Controls.MaterialButton btnPlayStop;
        private MaterialSkin.Controls.MaterialButton btnNxt5F;
        private MaterialSkin.Controls.MaterialButton btnNxt1F;
        private Button btnOpnFolderList2;
        private ListView lstviewTrash;
        public ListView lstviewFileList;
        private Label lblFolderList;
        private ImageList imglst1;
        private ListView lstviewFileListD;
        private Button btnOpnFileExplrr;
        private Label lblLstVwName;
        private Button btnRestoration;
        private MaterialSkin.Controls.MaterialButton btnSpeedPopup;
        private Panel pnlSpeedPopup;
        private MaterialSkin.Controls.MaterialSlider sdrSpeedController;
        private MaterialSkin.Controls.MaterialButton btnSpeedMinus;
        private MaterialSkin.Controls.MaterialButton btnSpeedPlus;
        private Label lblSpeedText;
    }
}