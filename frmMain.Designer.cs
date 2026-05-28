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
            ListViewItem listViewItem1 = new ListViewItem(new string[] { "[파일목록]" }, 0, Color.Empty, Color.Empty, new Font("맑은 고딕", 12F, FontStyle.Regular, GraphicsUnit.Point, 129));
            ListViewItem listViewItem2 = new ListViewItem(new string[] { "[파일추가]" }, 3, Color.Empty, Color.Empty, new Font("맑은 고딕", 12F));
            ListViewItem listViewItem3 = new ListViewItem(new string[] { "[휴지통]" }, 2, Color.Empty, Color.Empty, new Font("맑은 고딕", 12F, FontStyle.Regular, GraphicsUnit.Point, 129));
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            sdrSeekBar = new MaterialSkin.Controls.MaterialSlider();
            pnlVideo = new Panel();
            pnlROI = new Panel();
            btnROIR = new Button();
            btnROID = new Button();
            btnROIU = new Button();
            btnROIRU = new Button();
            btnROIRD = new Button();
            btnROILD = new Button();
            btnROILU = new Button();
            btnROICenter = new Button();
            btnROIL = new Button();
            pnlContrastProperty = new Panel();
            trcbrContrastProperty = new TrackBar();
            pnlColorProperty = new Panel();
            GBPalete = new GroupBox();
            btnColorCancle = new MaterialSkin.Controls.MaterialButton();
            btnColorCfm = new MaterialSkin.Controls.MaterialButton();
            btnPalette4 = new Button();
            btnPalette5 = new Button();
            btnPalette3 = new Button();
            btnPalette1 = new Button();
            btnPalette2 = new Button();
            btnContrastProperty = new MaterialSkin.Controls.MaterialButton();
            btnROI = new MaterialSkin.Controls.MaterialButton();
            btnColorProperty = new MaterialSkin.Controls.MaterialButton();
            btnMirror = new MaterialSkin.Controls.MaterialButton();
            btnNoise = new MaterialSkin.Controls.MaterialButton();
            btnDel = new MaterialSkin.Controls.MaterialButton();
            pnlProperty = new Panel();
            crdProperty = new MaterialSkin.Controls.MaterialCard();
            btnMirrorY = new MaterialSkin.Controls.MaterialButton();
            textBox1 = new TextBox();
            btnSave = new MaterialSkin.Controls.MaterialButton();
            btnPre5F = new MaterialSkin.Controls.MaterialButton();
            btnOpnFolderList = new Button();
            pnlFolderList = new Panel();
            btnRemove = new Button();
            btnRestoration = new Button();
            lblLstVwName = new Label();
            btnOpnFileExplrr = new Button();
            lblFolderList = new Label();
            lstviewMain = new ListView();
            imglst1 = new ImageList(components);
            lstviewTrash = new ListView();
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
            pnlROI.SuspendLayout();
            pnlContrastProperty.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trcbrContrastProperty).BeginInit();
            pnlColorProperty.SuspendLayout();
            GBPalete.SuspendLayout();
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
            sdrSeekBar.Location = new Point(27, 785);
            sdrSeekBar.Margin = new Padding(4);
            sdrSeekBar.MouseState = MaterialSkin.MouseState.HOVER;
            sdrSeekBar.Name = "sdrSeekBar";
            sdrSeekBar.ShowValue = false;
            sdrSeekBar.Size = new Size(1395, 40);
            sdrSeekBar.TabIndex = 0;
            sdrSeekBar.Text = "n/m";
            sdrSeekBar.Click += materialSlider1_Click;
            // 
            // pnlVideo
            // 
            pnlVideo.BackColor = Color.Black;
            pnlVideo.ForeColor = Color.Coral;
            pnlVideo.Location = new Point(27, 15);
            pnlVideo.Margin = new Padding(4);
            pnlVideo.Name = "pnlVideo";
            pnlVideo.Size = new Size(1395, 736);
            pnlVideo.TabIndex = 1;
            // 
            // pnlROI
            // 
            pnlROI.BackColor = SystemColors.ControlDark;
            pnlROI.Controls.Add(btnROIR);
            pnlROI.Controls.Add(btnROID);
            pnlROI.Controls.Add(btnROIU);
            pnlROI.Controls.Add(btnROIRU);
            pnlROI.Controls.Add(btnROIRD);
            pnlROI.Controls.Add(btnROILD);
            pnlROI.Controls.Add(btnROILU);
            pnlROI.Controls.Add(btnROICenter);
            pnlROI.Controls.Add(btnROIL);
            pnlROI.Location = new Point(1428, 763);
            pnlROI.Margin = new Padding(4);
            pnlROI.Name = "pnlROI";
            pnlROI.Size = new Size(621, 327);
            pnlROI.TabIndex = 16;
            pnlROI.Visible = false;
            // 
            // btnROIR
            // 
            btnROIR.Location = new Point(469, 112);
            btnROIR.Margin = new Padding(4);
            btnROIR.Name = "btnROIR";
            btnROIR.Size = new Size(147, 104);
            btnROIR.TabIndex = 15;
            btnROIR.UseVisualStyleBackColor = true;
            // 
            // btnROID
            // 
            btnROID.Location = new Point(153, 219);
            btnROID.Margin = new Padding(4);
            btnROID.Name = "btnROID";
            btnROID.Size = new Size(315, 104);
            btnROID.TabIndex = 13;
            btnROID.UseVisualStyleBackColor = true;
            // 
            // btnROIU
            // 
            btnROIU.Location = new Point(153, 7);
            btnROIU.Margin = new Padding(4);
            btnROIU.Name = "btnROIU";
            btnROIU.Size = new Size(315, 104);
            btnROIU.TabIndex = 12;
            btnROIU.UseVisualStyleBackColor = true;
            // 
            // btnROIRU
            // 
            btnROIRU.Location = new Point(469, 7);
            btnROIRU.Margin = new Padding(4);
            btnROIRU.Name = "btnROIRU";
            btnROIRU.Size = new Size(147, 104);
            btnROIRU.TabIndex = 11;
            btnROIRU.UseVisualStyleBackColor = true;
            // 
            // btnROIRD
            // 
            btnROIRD.Location = new Point(469, 219);
            btnROIRD.Margin = new Padding(4);
            btnROIRD.Name = "btnROIRD";
            btnROIRD.Size = new Size(147, 104);
            btnROIRD.TabIndex = 10;
            btnROIRD.UseVisualStyleBackColor = true;
            // 
            // btnROILD
            // 
            btnROILD.Location = new Point(5, 219);
            btnROILD.Margin = new Padding(4);
            btnROILD.Name = "btnROILD";
            btnROILD.Size = new Size(147, 104);
            btnROILD.TabIndex = 9;
            btnROILD.UseVisualStyleBackColor = true;
            // 
            // btnROILU
            // 
            btnROILU.Location = new Point(5, 5);
            btnROILU.Margin = new Padding(4);
            btnROILU.Name = "btnROILU";
            btnROILU.Size = new Size(147, 104);
            btnROILU.TabIndex = 8;
            btnROILU.UseVisualStyleBackColor = true;
            // 
            // btnROICenter
            // 
            btnROICenter.Location = new Point(153, 112);
            btnROICenter.Margin = new Padding(4);
            btnROICenter.Name = "btnROICenter";
            btnROICenter.Size = new Size(315, 104);
            btnROICenter.TabIndex = 0;
            btnROICenter.UseVisualStyleBackColor = true;
            // 
            // btnROIL
            // 
            btnROIL.Location = new Point(5, 112);
            btnROIL.Margin = new Padding(4);
            btnROIL.Name = "btnROIL";
            btnROIL.Size = new Size(147, 104);
            btnROIL.TabIndex = 14;
            btnROIL.UseVisualStyleBackColor = true;
            // 
            // pnlContrastProperty
            // 
            pnlContrastProperty.BackColor = SystemColors.ControlDark;
            pnlContrastProperty.Controls.Add(trcbrContrastProperty);
            pnlContrastProperty.Location = new Point(1428, 763);
            pnlContrastProperty.Margin = new Padding(4);
            pnlContrastProperty.Name = "pnlContrastProperty";
            pnlContrastProperty.Size = new Size(621, 103);
            pnlContrastProperty.TabIndex = 17;
            pnlContrastProperty.Visible = false;
            pnlContrastProperty.Paint += pnlContrastProperty_Paint;
            // 
            // trcbrContrastProperty
            // 
            trcbrContrastProperty.AutoSize = false;
            trcbrContrastProperty.BackColor = SystemColors.ControlDark;
            trcbrContrastProperty.Location = new Point(58, 39);
            trcbrContrastProperty.Margin = new Padding(4);
            trcbrContrastProperty.Name = "trcbrContrastProperty";
            trcbrContrastProperty.Size = new Size(504, 40);
            trcbrContrastProperty.TabIndex = 3;
            trcbrContrastProperty.TickStyle = TickStyle.None;
            // 
            // pnlColorProperty
            // 
            pnlColorProperty.BackColor = SystemColors.ControlDark;
            pnlColorProperty.BorderStyle = BorderStyle.Fixed3D;
            pnlColorProperty.Controls.Add(GBPalete);
            pnlColorProperty.Location = new Point(1428, 763);
            pnlColorProperty.Margin = new Padding(4);
            pnlColorProperty.Name = "pnlColorProperty";
            pnlColorProperty.Size = new Size(620, 129);
            pnlColorProperty.TabIndex = 17;
            pnlColorProperty.Visible = false;
            // 
            // GBPalete
            // 
            GBPalete.Controls.Add(btnColorCancle);
            GBPalete.Controls.Add(btnColorCfm);
            GBPalete.Controls.Add(btnPalette4);
            GBPalete.Controls.Add(btnPalette5);
            GBPalete.Controls.Add(btnPalette3);
            GBPalete.Controls.Add(btnPalette1);
            GBPalete.Controls.Add(btnPalette2);
            GBPalete.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            GBPalete.Location = new Point(6, 3);
            GBPalete.Margin = new Padding(4);
            GBPalete.Name = "GBPalete";
            GBPalete.Padding = new Padding(4);
            GBPalete.Size = new Size(598, 117);
            GBPalete.TabIndex = 11;
            GBPalete.TabStop = false;
            GBPalete.Text = "필터";
            GBPalete.Enter += GBPalete_Enter;
            // 
            // btnColorCancle
            // 
            btnColorCancle.AutoSize = false;
            btnColorCancle.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnColorCancle.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnColorCancle.Depth = 0;
            btnColorCancle.HighEmphasis = true;
            btnColorCancle.Icon = null;
            btnColorCancle.Location = new Point(420, 41);
            btnColorCancle.Margin = new Padding(5, 8, 5, 8);
            btnColorCancle.MouseState = MaterialSkin.MouseState.HOVER;
            btnColorCancle.Name = "btnColorCancle";
            btnColorCancle.NoAccentTextColor = Color.Empty;
            btnColorCancle.Size = new Size(81, 56);
            btnColorCancle.TabIndex = 12;
            btnColorCancle.Text = "취소";
            btnColorCancle.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnColorCancle.UseAccentColor = false;
            btnColorCancle.UseVisualStyleBackColor = true;
            // 
            // btnColorCfm
            // 
            btnColorCfm.AutoSize = false;
            btnColorCfm.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnColorCfm.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnColorCfm.Depth = 0;
            btnColorCfm.HighEmphasis = true;
            btnColorCfm.Icon = null;
            btnColorCfm.Location = new Point(507, 41);
            btnColorCfm.Margin = new Padding(5, 8, 5, 8);
            btnColorCfm.MouseState = MaterialSkin.MouseState.HOVER;
            btnColorCfm.Name = "btnColorCfm";
            btnColorCfm.NoAccentTextColor = Color.Empty;
            btnColorCfm.Size = new Size(81, 56);
            btnColorCfm.TabIndex = 11;
            btnColorCfm.Text = "적용";
            btnColorCfm.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnColorCfm.UseAccentColor = false;
            btnColorCfm.UseVisualStyleBackColor = true;
            // 
            // btnPalette4
            // 
            btnPalette4.BackColor = Color.White;
            btnPalette4.FlatStyle = FlatStyle.Popup;
            btnPalette4.Location = new Point(256, 29);
            btnPalette4.Margin = new Padding(4);
            btnPalette4.Name = "btnPalette4";
            btnPalette4.Size = new Size(77, 80);
            btnPalette4.TabIndex = 9;
            btnPalette4.UseVisualStyleBackColor = false;
            // 
            // btnPalette5
            // 
            btnPalette5.BackColor = Color.White;
            btnPalette5.FlatStyle = FlatStyle.Popup;
            btnPalette5.Location = new Point(337, 29);
            btnPalette5.Margin = new Padding(4);
            btnPalette5.Name = "btnPalette5";
            btnPalette5.Size = new Size(77, 80);
            btnPalette5.TabIndex = 6;
            btnPalette5.UseVisualStyleBackColor = false;
            // 
            // btnPalette3
            // 
            btnPalette3.BackColor = Color.White;
            btnPalette3.FlatStyle = FlatStyle.Popup;
            btnPalette3.Location = new Point(174, 29);
            btnPalette3.Margin = new Padding(4);
            btnPalette3.Name = "btnPalette3";
            btnPalette3.Size = new Size(77, 80);
            btnPalette3.TabIndex = 10;
            btnPalette3.UseVisualStyleBackColor = false;
            // 
            // btnPalette1
            // 
            btnPalette1.BackColor = Color.White;
            btnPalette1.FlatStyle = FlatStyle.Popup;
            btnPalette1.Location = new Point(9, 29);
            btnPalette1.Margin = new Padding(4);
            btnPalette1.Name = "btnPalette1";
            btnPalette1.Size = new Size(77, 80);
            btnPalette1.TabIndex = 5;
            btnPalette1.UseVisualStyleBackColor = false;
            // 
            // btnPalette2
            // 
            btnPalette2.BackColor = Color.White;
            btnPalette2.FlatStyle = FlatStyle.Popup;
            btnPalette2.Location = new Point(91, 29);
            btnPalette2.Margin = new Padding(4);
            btnPalette2.Name = "btnPalette2";
            btnPalette2.Size = new Size(77, 80);
            btnPalette2.TabIndex = 8;
            btnPalette2.UseVisualStyleBackColor = false;
            // 
            // btnContrastProperty
            // 
            btnContrastProperty.AutoSize = false;
            btnContrastProperty.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnContrastProperty.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnContrastProperty.Depth = 0;
            btnContrastProperty.HighEmphasis = true;
            btnContrastProperty.Icon = Data_Manager.Properties.Resources.P_Contrast;
            btnContrastProperty.Location = new Point(12, 21);
            btnContrastProperty.Margin = new Padding(5, 8, 5, 8);
            btnContrastProperty.MouseState = MaterialSkin.MouseState.HOVER;
            btnContrastProperty.Name = "btnContrastProperty";
            btnContrastProperty.NoAccentTextColor = Color.Empty;
            btnContrastProperty.Size = new Size(193, 133);
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
            btnROI.Icon = Data_Manager.Properties.Resources.P_brush;
            btnROI.Location = new Point(418, 21);
            btnROI.Margin = new Padding(5, 8, 5, 8);
            btnROI.MouseState = MaterialSkin.MouseState.HOVER;
            btnROI.Name = "btnROI";
            btnROI.NoAccentTextColor = Color.Empty;
            btnROI.Size = new Size(193, 133);
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
            btnColorProperty.Icon = Data_Manager.Properties.Resources.P_palette;
            btnColorProperty.Location = new Point(215, 21);
            btnColorProperty.Margin = new Padding(5, 8, 5, 8);
            btnColorProperty.MouseState = MaterialSkin.MouseState.HOVER;
            btnColorProperty.Name = "btnColorProperty";
            btnColorProperty.NoAccentTextColor = Color.Empty;
            btnColorProperty.Size = new Size(193, 133);
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
            btnMirror.Icon = Data_Manager.Properties.Resources.P_Mirror;
            btnMirror.Location = new Point(215, 171);
            btnMirror.Margin = new Padding(5, 8, 5, 8);
            btnMirror.MouseState = MaterialSkin.MouseState.HOVER;
            btnMirror.Name = "btnMirror";
            btnMirror.NoAccentTextColor = Color.Empty;
            btnMirror.Size = new Size(193, 133);
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
            btnNoise.Icon = Data_Manager.Properties.Resources.P_mosaic;
            btnNoise.Location = new Point(12, 171);
            btnNoise.Margin = new Padding(5, 8, 5, 8);
            btnNoise.MouseState = MaterialSkin.MouseState.HOVER;
            btnNoise.Name = "btnNoise";
            btnNoise.NoAccentTextColor = Color.Empty;
            btnNoise.Size = new Size(193, 133);
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
            btnDel.Icon = Data_Manager.Properties.Resources.TrashCan11538270;
            btnDel.Location = new Point(710, 15);
            btnDel.Margin = new Padding(5, 8, 5, 8);
            btnDel.MouseState = MaterialSkin.MouseState.HOVER;
            btnDel.Name = "btnDel";
            btnDel.NoAccentTextColor = Color.Empty;
            btnDel.Size = new Size(141, 133);
            btnDel.TabIndex = 0;
            btnDel.Text = "삭제";
            btnDel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnDel.UseAccentColor = true;
            btnDel.UseVisualStyleBackColor = false;
            // 
            // pnlProperty
            // 
            pnlProperty.BackColor = SystemColors.Control;
            pnlProperty.BorderStyle = BorderStyle.Fixed3D;
            pnlProperty.Controls.Add(crdProperty);
            pnlProperty.Location = new Point(1428, 763);
            pnlProperty.Margin = new Padding(4);
            pnlProperty.Name = "pnlProperty";
            pnlProperty.Size = new Size(620, 325);
            pnlProperty.TabIndex = 6;
            // 
            // crdProperty
            // 
            crdProperty.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            crdProperty.BackColor = Color.FromArgb(255, 255, 255);
            crdProperty.Controls.Add(btnMirrorY);
            crdProperty.Controls.Add(btnContrastProperty);
            crdProperty.Controls.Add(btnMirror);
            crdProperty.Controls.Add(btnColorProperty);
            crdProperty.Controls.Add(btnROI);
            crdProperty.Controls.Add(btnNoise);
            crdProperty.Depth = 0;
            crdProperty.ForeColor = Color.FromArgb(222, 0, 0, 0);
            crdProperty.Location = new Point(0, 0);
            crdProperty.Margin = new Padding(18, 19, 18, 19);
            crdProperty.MouseState = MaterialSkin.MouseState.HOVER;
            crdProperty.Name = "crdProperty";
            crdProperty.Padding = new Padding(18, 19, 18, 19);
            crdProperty.Size = new Size(621, 327);
            crdProperty.TabIndex = 0;
            // 
            // btnMirrorY
            // 
            btnMirrorY.Anchor = AnchorStyles.Bottom;
            btnMirrorY.AutoSize = false;
            btnMirrorY.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnMirrorY.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnMirrorY.Depth = 0;
            btnMirrorY.HighEmphasis = true;
            btnMirrorY.Icon = Data_Manager.Properties.Resources.P_mirror_UpDown;
            btnMirrorY.Location = new Point(418, 171);
            btnMirrorY.Margin = new Padding(5, 8, 5, 8);
            btnMirrorY.MouseState = MaterialSkin.MouseState.HOVER;
            btnMirrorY.Name = "btnMirrorY";
            btnMirrorY.NoAccentTextColor = Color.Empty;
            btnMirrorY.Size = new Size(193, 133);
            btnMirrorY.TabIndex = 6;
            btnMirrorY.Text = "상하 반전";
            btnMirrorY.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnMirrorY.UseAccentColor = false;
            btnMirrorY.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.Lime;
            textBox1.Location = new Point(8, 885);
            textBox1.Margin = new Padding(4);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(544, 203);
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
            btnSave.Location = new Point(555, 15);
            btnSave.Margin = new Padding(5, 8, 5, 8);
            btnSave.MouseState = MaterialSkin.MouseState.HOVER;
            btnSave.Name = "btnSave";
            btnSave.NoAccentTextColor = Color.Empty;
            btnSave.Size = new Size(141, 133);
            btnSave.TabIndex = 8;
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
            btnPre5F.Location = new Point(5, 21);
            btnPre5F.Margin = new Padding(5, 8, 5, 8);
            btnPre5F.MouseState = MaterialSkin.MouseState.HOVER;
            btnPre5F.Name = "btnPre5F";
            btnPre5F.NoAccentTextColor = Color.Empty;
            btnPre5F.Size = new Size(77, 67);
            btnPre5F.TabIndex = 10;
            btnPre5F.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnPre5F.UseAccentColor = false;
            btnPre5F.UseVisualStyleBackColor = true;
            // 
            // btnOpnFolderList
            // 
            btnOpnFolderList.BackgroundImageLayout = ImageLayout.None;
            btnOpnFolderList.FlatStyle = FlatStyle.Popup;
            btnOpnFolderList.Location = new Point(4, 5);
            btnOpnFolderList.Margin = new Padding(4);
            btnOpnFolderList.Name = "btnOpnFolderList";
            btnOpnFolderList.Size = new Size(45, 47);
            btnOpnFolderList.TabIndex = 11;
            btnOpnFolderList.TabStop = false;
            btnOpnFolderList.UseVisualStyleBackColor = true;
            btnOpnFolderList.Click += btnOpnFolderList1_Click;
            // 
            // pnlFolderList
            // 
            pnlFolderList.BorderStyle = BorderStyle.Fixed3D;
            pnlFolderList.Controls.Add(btnRemove);
            pnlFolderList.Controls.Add(btnRestoration);
            pnlFolderList.Controls.Add(lblLstVwName);
            pnlFolderList.Controls.Add(btnOpnFileExplrr);
            pnlFolderList.Controls.Add(lblFolderList);
            pnlFolderList.Controls.Add(btnOpnFolderList);
            pnlFolderList.Controls.Add(lstviewMain);
            pnlFolderList.Controls.Add(lstviewTrash);
            pnlFolderList.Controls.Add(lstviewFileListD);
            pnlFolderList.Location = new Point(1428, 15);
            pnlFolderList.Margin = new Padding(4);
            pnlFolderList.Name = "pnlFolderList";
            pnlFolderList.Size = new Size(620, 737);
            pnlFolderList.TabIndex = 12;
            // 
            // btnRemove
            // 
            btnRemove.FlatStyle = FlatStyle.Popup;
            btnRemove.Location = new Point(468, 7);
            btnRemove.Margin = new Padding(4);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(45, 47);
            btnRemove.TabIndex = 20;
            btnRemove.TabStop = false;
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Visible = false;
            // 
            // btnRestoration
            // 
            btnRestoration.FlatStyle = FlatStyle.Popup;
            btnRestoration.Location = new Point(518, 7);
            btnRestoration.Margin = new Padding(4);
            btnRestoration.Name = "btnRestoration";
            btnRestoration.Size = new Size(45, 47);
            btnRestoration.TabIndex = 19;
            btnRestoration.TabStop = false;
            btnRestoration.UseVisualStyleBackColor = true;
            btnRestoration.Visible = false;
            // 
            // lblLstVwName
            // 
            lblLstVwName.Font = new Font("맑은 고딕", 18F);
            lblLstVwName.Location = new Point(57, 11);
            lblLstVwName.Margin = new Padding(4, 0, 4, 0);
            lblLstVwName.Name = "lblLstVwName";
            lblLstVwName.Size = new Size(404, 43);
            lblLstVwName.TabIndex = 18;
            // 
            // btnOpnFileExplrr
            // 
            btnOpnFileExplrr.FlatStyle = FlatStyle.Popup;
            btnOpnFileExplrr.Location = new Point(567, 7);
            btnOpnFileExplrr.Margin = new Padding(4);
            btnOpnFileExplrr.Name = "btnOpnFileExplrr";
            btnOpnFileExplrr.Size = new Size(45, 47);
            btnOpnFileExplrr.TabIndex = 17;
            btnOpnFileExplrr.TabStop = false;
            btnOpnFileExplrr.UseVisualStyleBackColor = true;
            // 
            // lblFolderList
            // 
            lblFolderList.AutoSize = true;
            lblFolderList.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            lblFolderList.Location = new Point(4, 1);
            lblFolderList.Margin = new Padding(4, 0, 4, 0);
            lblFolderList.Name = "lblFolderList";
            lblFolderList.Padding = new Padding(0, 0, 0, 4);
            lblFolderList.Size = new Size(0, 50);
            lblFolderList.TabIndex = 15;
            // 
            // lstviewMain
            // 
            lstviewMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewItem1.Tag = "파일목록";
            listViewItem2.Tag = "파일추가";
            listViewItem3.Tag = "휴지통";
            lstviewMain.Items.AddRange(new ListViewItem[] { listViewItem1, listViewItem2, listViewItem3 });
            lstviewMain.Location = new Point(-1, 59);
            lstviewMain.Margin = new Padding(4);
            lstviewMain.Name = "lstviewMain";
            lstviewMain.Size = new Size(620, 677);
            lstviewMain.SmallImageList = imglst1;
            lstviewMain.TabIndex = 12;
            lstviewMain.UseCompatibleStateImageBehavior = false;
            lstviewMain.View = View.List;
            lstviewMain.SelectedIndexChanged += listView1_SelectedIndexChanged;
            // 
            // imglst1
            // 
            imglst1.ColorDepth = ColorDepth.Depth32Bit;
            imglst1.ImageStream = (ImageListStreamer)resources.GetObject("imglst1.ImageStream");
            imglst1.TransparentColor = Color.Transparent;
            imglst1.Images.SetKeyName(0, "노란 폴더2.png");
            imglst1.Images.SetKeyName(1, "파란 폴더2.png");
            imglst1.Images.SetKeyName(2, "휴지통2.png");
            imglst1.Images.SetKeyName(3, "download-667443.png");
            // 
            // lstviewTrash
            // 
            lstviewTrash.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstviewTrash.Location = new Point(-1, 59);
            lstviewTrash.Margin = new Padding(4);
            lstviewTrash.Name = "lstviewTrash";
            lstviewTrash.Size = new Size(620, 677);
            lstviewTrash.TabIndex = 14;
            lstviewTrash.UseCompatibleStateImageBehavior = false;
            lstviewTrash.View = View.List;
            lstviewTrash.Visible = false;
            // 
            // lstviewFileListD
            // 
            lstviewFileListD.Location = new Point(-2, 57);
            lstviewFileListD.Margin = new Padding(4);
            lstviewFileListD.Name = "lstviewFileListD";
            lstviewFileListD.Size = new Size(620, 677);
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
            pnlCtrl.Location = new Point(561, 913);
            pnlCtrl.Margin = new Padding(4);
            pnlCtrl.Name = "pnlCtrl";
            pnlCtrl.Size = new Size(861, 176);
            pnlCtrl.TabIndex = 13;
            // 
            // btnSpeedPopup
            // 
            btnSpeedPopup.AutoSize = false;
            btnSpeedPopup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSpeedPopup.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnSpeedPopup.Depth = 0;
            btnSpeedPopup.HighEmphasis = true;
            btnSpeedPopup.Icon = Data_Manager.Properties.Resources.speedometer8017074;
            btnSpeedPopup.Location = new Point(5, 104);
            btnSpeedPopup.Margin = new Padding(5, 8, 5, 8);
            btnSpeedPopup.MouseState = MaterialSkin.MouseState.HOVER;
            btnSpeedPopup.Name = "btnSpeedPopup";
            btnSpeedPopup.NoAccentTextColor = Color.Empty;
            btnSpeedPopup.Size = new Size(537, 37);
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
            btnPre1F.Location = new Point(93, 21);
            btnPre1F.Margin = new Padding(5, 8, 5, 8);
            btnPre1F.MouseState = MaterialSkin.MouseState.HOVER;
            btnPre1F.Name = "btnPre1F";
            btnPre1F.NoAccentTextColor = Color.Empty;
            btnPre1F.Size = new Size(77, 67);
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
            btnPlayStop.Icon = Data_Manager.Properties.Resources.PlaySlide4655096;
            btnPlayStop.Location = new Point(180, 21);
            btnPlayStop.Margin = new Padding(5, 8, 5, 8);
            btnPlayStop.MouseState = MaterialSkin.MouseState.HOVER;
            btnPlayStop.Name = "btnPlayStop";
            btnPlayStop.NoAccentTextColor = Color.Empty;
            btnPlayStop.Size = new Size(189, 67);
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
            btnNxt5F.Location = new Point(465, 21);
            btnNxt5F.Margin = new Padding(5, 8, 5, 8);
            btnNxt5F.MouseState = MaterialSkin.MouseState.HOVER;
            btnNxt5F.Name = "btnNxt5F";
            btnNxt5F.NoAccentTextColor = Color.Empty;
            btnNxt5F.Size = new Size(77, 67);
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
            btnNxt1F.Location = new Point(379, 21);
            btnNxt1F.Margin = new Padding(5, 8, 5, 8);
            btnNxt1F.MouseState = MaterialSkin.MouseState.HOVER;
            btnNxt1F.Name = "btnNxt1F";
            btnNxt1F.NoAccentTextColor = Color.Empty;
            btnNxt1F.Size = new Size(77, 67);
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
            pnlSpeedPopup.Location = new Point(566, 905);
            pnlSpeedPopup.Margin = new Padding(4);
            pnlSpeedPopup.Name = "pnlSpeedPopup";
            pnlSpeedPopup.Size = new Size(537, 112);
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
            btnSpeedMinus.Margin = new Padding(5, 8, 5, 8);
            btnSpeedMinus.MouseState = MaterialSkin.MouseState.HOVER;
            btnSpeedMinus.Name = "btnSpeedMinus";
            btnSpeedMinus.NoAccentTextColor = Color.Empty;
            btnSpeedMinus.Size = new Size(40, 109);
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
            btnSpeedPlus.Location = new Point(496, 1);
            btnSpeedPlus.Margin = new Padding(5, 8, 5, 8);
            btnSpeedPlus.MouseState = MaterialSkin.MouseState.HOVER;
            btnSpeedPlus.Name = "btnSpeedPlus";
            btnSpeedPlus.NoAccentTextColor = Color.Empty;
            btnSpeedPlus.Size = new Size(40, 109);
            btnSpeedPlus.TabIndex = 1;
            btnSpeedPlus.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnSpeedPlus.UseAccentColor = false;
            btnSpeedPlus.UseVisualStyleBackColor = true;
            // 
            // sdrSpeedController
            // 
            sdrSpeedController.Depth = 0;
            sdrSpeedController.ForeColor = Color.Gray;
            sdrSpeedController.Location = new Point(49, 8);
            sdrSpeedController.Margin = new Padding(4);
            sdrSpeedController.MouseState = MaterialSkin.MouseState.HOVER;
            sdrSpeedController.Name = "sdrSpeedController";
            sdrSpeedController.ShowValue = false;
            sdrSpeedController.Size = new Size(437, 40);
            sdrSpeedController.TabIndex = 0;
            sdrSpeedController.Text = "";
            // 
            // lblSpeedText
            // 
            lblSpeedText.ForeColor = Color.Black;
            lblSpeedText.Location = new Point(231, 68);
            lblSpeedText.Margin = new Padding(4, 0, 4, 0);
            lblSpeedText.Name = "lblSpeedText";
            lblSpeedText.Size = new Size(75, 37);
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
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(1942, 1102);
            Controls.Add(pnlContrastProperty);
            Controls.Add(pnlFolderList);
            Controls.Add(textBox1);
            Controls.Add(pnlVideo);
            Controls.Add(sdrSeekBar);
            Controls.Add(pnlSpeedPopup);
            Controls.Add(pnlCtrl);
            Controls.Add(pnlColorProperty);
            Controls.Add(pnlROI);
            Controls.Add(pnlProperty);
            Margin = new Padding(4);
            Name = "frmMain";
            Padding = new Padding(0);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "11팀";
            pnlROI.ResumeLayout(false);
            pnlContrastProperty.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)trcbrContrastProperty).EndInit();
            pnlColorProperty.ResumeLayout(false);
            GBPalete.ResumeLayout(false);
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
        private Panel pnlColorProperty;
        private Button btnPalette1;
        private Button btnPalette3;
        private Button btnPalette4;
        private Button btnPalette2;
        private Button btnPalette5;
        private GroupBox GBPalete;
        private Panel pnlROI;
        private Panel pnlContrastProperty;
        private Button btnROID;
        private Button btnROIU;
        private Button btnROIRU;
        private Button btnROIRD;
        private Button btnROILD;
        private Button btnROILU;
        private Button btnROICenter;
        private Button btnROIR;
        private Button btnROIL;
        private TrackBar trcbrContrastProperty;
        private TrackBar trackBar3;
        private TrackBar trackBar2;
        private TrackBar trackBar1;
        private MaterialSkin.Controls.MaterialButton btnColorCfm;
        private MaterialSkin.Controls.MaterialButton btnColorCancle;
        private MaterialSkin.Controls.MaterialButton btnMirrorY;
        private Button btnRemove;
    }
}