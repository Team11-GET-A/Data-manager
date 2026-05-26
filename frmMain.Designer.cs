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
            pnlROI = new Panel();
            btnROIR = new Button();
            btnROIL = new Button();
            btnROID = new Button();
            btnROIU = new Button();
            btnROIRU = new Button();
            btnROIRD = new Button();
            btnROILD = new Button();
            btnROILU = new Button();
            btnROICenter = new Button();
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
            lstviewFileListD = new ListView();
            lstviewTrash = new ListView();
            lstviewMain = new ListView();
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
            sdrSeekBar.Location = new Point(30, 982);
            sdrSeekBar.Margin = new Padding(4, 5, 4, 5);
            sdrSeekBar.MouseState = MaterialSkin.MouseState.HOVER;
            sdrSeekBar.Name = "sdrSeekBar";
            sdrSeekBar.ShowValue = false;
            sdrSeekBar.Size = new Size(1550, 40);
            sdrSeekBar.TabIndex = 0;
            sdrSeekBar.Text = "n/m";
            sdrSeekBar.Click += materialSlider1_Click;
            // 
            // pnlVideo
            // 
            pnlVideo.BackColor = Color.Black;
            pnlVideo.ForeColor = Color.Coral;
            pnlVideo.Location = new Point(30, 18);
            pnlVideo.Margin = new Padding(4, 5, 4, 5);
            pnlVideo.Name = "pnlVideo";
            pnlVideo.Size = new Size(1550, 920);
            pnlVideo.TabIndex = 1;
            // 
            // pnlROI
            // 
            pnlROI.BackColor = SystemColors.ControlDark;
            pnlROI.Controls.Add(btnROIR);
            pnlROI.Controls.Add(btnROIL);
            pnlROI.Controls.Add(btnROID);
            pnlROI.Controls.Add(btnROIU);
            pnlROI.Controls.Add(btnROIRU);
            pnlROI.Controls.Add(btnROIRD);
            pnlROI.Controls.Add(btnROILD);
            pnlROI.Controls.Add(btnROILU);
            pnlROI.Controls.Add(btnROICenter);
            pnlROI.Location = new Point(1587, 953);
            pnlROI.Margin = new Padding(4, 5, 4, 5);
            pnlROI.Name = "pnlROI";
            pnlROI.Size = new Size(690, 408);
            pnlROI.TabIndex = 16;
            pnlROI.Visible = false;
            // 
            // btnROIR
            // 
            btnROIR.Location = new Point(521, 140);
            btnROIR.Margin = new Padding(4, 5, 4, 5);
            btnROIR.Name = "btnROIR";
            btnROIR.Size = new Size(163, 130);
            btnROIR.TabIndex = 15;
            btnROIR.UseVisualStyleBackColor = true;
            // 
            // btnROIL
            // 
            btnROIL.Location = new Point(6, 140);
            btnROIL.Margin = new Padding(4, 5, 4, 5);
            btnROIL.Name = "btnROIL";
            btnROIL.Size = new Size(163, 130);
            btnROIL.TabIndex = 14;
            btnROIL.UseVisualStyleBackColor = true;
            // 
            // btnROID
            // 
            btnROID.Location = new Point(170, 273);
            btnROID.Margin = new Padding(4, 5, 4, 5);
            btnROID.Name = "btnROID";
            btnROID.Size = new Size(350, 130);
            btnROID.TabIndex = 13;
            btnROID.Text = "button4";
            btnROID.UseVisualStyleBackColor = true;
            // 
            // btnROIU
            // 
            btnROIU.Location = new Point(170, 8);
            btnROIU.Margin = new Padding(4, 5, 4, 5);
            btnROIU.Name = "btnROIU";
            btnROIU.Size = new Size(350, 130);
            btnROIU.TabIndex = 12;
            btnROIU.Text = "button3";
            btnROIU.UseVisualStyleBackColor = true;
            // 
            // btnROIRU
            // 
            btnROIRU.Location = new Point(521, 8);
            btnROIRU.Margin = new Padding(4, 5, 4, 5);
            btnROIRU.Name = "btnROIRU";
            btnROIRU.Size = new Size(163, 130);
            btnROIRU.TabIndex = 11;
            btnROIRU.UseVisualStyleBackColor = true;
            // 
            // btnROIRD
            // 
            btnROIRD.Location = new Point(521, 273);
            btnROIRD.Margin = new Padding(4, 5, 4, 5);
            btnROIRD.Name = "btnROIRD";
            btnROIRD.Size = new Size(163, 130);
            btnROIRD.TabIndex = 10;
            btnROIRD.UseVisualStyleBackColor = true;
            // 
            // btnROILD
            // 
            btnROILD.Location = new Point(6, 273);
            btnROILD.Margin = new Padding(4, 5, 4, 5);
            btnROILD.Name = "btnROILD";
            btnROILD.Size = new Size(163, 130);
            btnROILD.TabIndex = 9;
            btnROILD.UseVisualStyleBackColor = true;
            // 
            // btnROILU
            // 
            btnROILU.Location = new Point(6, 7);
            btnROILU.Margin = new Padding(4, 5, 4, 5);
            btnROILU.Name = "btnROILU";
            btnROILU.Size = new Size(163, 130);
            btnROILU.TabIndex = 8;
            btnROILU.UseVisualStyleBackColor = true;
            // 
            // btnROICenter
            // 
            btnROICenter.Location = new Point(170, 140);
            btnROICenter.Margin = new Padding(4, 5, 4, 5);
            btnROICenter.Name = "btnROICenter";
            btnROICenter.Size = new Size(350, 130);
            btnROICenter.TabIndex = 0;
            btnROICenter.Text = "button2";
            btnROICenter.UseVisualStyleBackColor = true;
            // 
            // pnlContrastProperty
            // 
            pnlContrastProperty.BackColor = SystemColors.ControlDark;
            pnlContrastProperty.Controls.Add(trcbrContrastProperty);
            pnlContrastProperty.Location = new Point(1587, 953);
            pnlContrastProperty.Margin = new Padding(4, 5, 4, 5);
            pnlContrastProperty.Name = "pnlContrastProperty";
            pnlContrastProperty.Size = new Size(690, 128);
            pnlContrastProperty.TabIndex = 17;
            pnlContrastProperty.Visible = false;
            pnlContrastProperty.Paint += pnlContrastProperty_Paint;
            // 
            // trcbrContrastProperty
            // 
            trcbrContrastProperty.AutoSize = false;
            trcbrContrastProperty.BackColor = SystemColors.ControlDark;
            trcbrContrastProperty.Location = new Point(64, 43);
            trcbrContrastProperty.Margin = new Padding(4, 5, 4, 5);
            trcbrContrastProperty.Name = "trcbrContrastProperty";
            trcbrContrastProperty.Size = new Size(560, 50);
            trcbrContrastProperty.TabIndex = 3;
            trcbrContrastProperty.TickStyle = TickStyle.None;
            // 
            // pnlColorProperty
            // 
            pnlColorProperty.BackColor = SystemColors.ControlDark;
            pnlColorProperty.BorderStyle = BorderStyle.Fixed3D;
            pnlColorProperty.Controls.Add(GBPalete);
            pnlColorProperty.Location = new Point(1587, 953);
            pnlColorProperty.Margin = new Padding(4, 5, 4, 5);
            pnlColorProperty.Name = "pnlColorProperty";
            pnlColorProperty.Size = new Size(688, 161);
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
            GBPalete.Font = new Font("새굴림", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            GBPalete.Location = new Point(7, 3);
            GBPalete.Margin = new Padding(4, 5, 4, 5);
            GBPalete.Name = "GBPalete";
            GBPalete.Padding = new Padding(4, 5, 4, 5);
            GBPalete.Size = new Size(664, 147);
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
            btnColorCancle.Location = new Point(467, 52);
            btnColorCancle.Margin = new Padding(6, 10, 6, 10);
            btnColorCancle.MouseState = MaterialSkin.MouseState.HOVER;
            btnColorCancle.Name = "btnColorCancle";
            btnColorCancle.NoAccentTextColor = Color.Empty;
            btnColorCancle.Size = new Size(90, 70);
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
            btnColorCfm.Location = new Point(563, 52);
            btnColorCfm.Margin = new Padding(6, 10, 6, 10);
            btnColorCfm.MouseState = MaterialSkin.MouseState.HOVER;
            btnColorCfm.Name = "btnColorCfm";
            btnColorCfm.NoAccentTextColor = Color.Empty;
            btnColorCfm.Size = new Size(90, 70);
            btnColorCfm.TabIndex = 11;
            btnColorCfm.Text = "적용";
            btnColorCfm.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnColorCfm.UseAccentColor = false;
            btnColorCfm.UseVisualStyleBackColor = true;
            // 
            // btnPalette4
            // 
            btnPalette4.BackColor = Color.White;
            btnPalette4.Location = new Point(284, 37);
            btnPalette4.Margin = new Padding(4, 5, 4, 5);
            btnPalette4.Name = "btnPalette4";
            btnPalette4.Size = new Size(86, 100);
            btnPalette4.TabIndex = 9;
            btnPalette4.UseVisualStyleBackColor = false;
            // 
            // btnPalette5
            // 
            btnPalette5.BackColor = Color.White;
            btnPalette5.Location = new Point(374, 37);
            btnPalette5.Margin = new Padding(4, 5, 4, 5);
            btnPalette5.Name = "btnPalette5";
            btnPalette5.Size = new Size(86, 100);
            btnPalette5.TabIndex = 6;
            btnPalette5.UseVisualStyleBackColor = false;
            // 
            // btnPalette3
            // 
            btnPalette3.BackColor = Color.White;
            btnPalette3.Location = new Point(193, 37);
            btnPalette3.Margin = new Padding(4, 5, 4, 5);
            btnPalette3.Name = "btnPalette3";
            btnPalette3.Size = new Size(86, 100);
            btnPalette3.TabIndex = 10;
            btnPalette3.UseVisualStyleBackColor = false;
            // 
            // btnPalette1
            // 
            btnPalette1.BackColor = Color.White;
            btnPalette1.Location = new Point(10, 37);
            btnPalette1.Margin = new Padding(4, 5, 4, 5);
            btnPalette1.Name = "btnPalette1";
            btnPalette1.Size = new Size(86, 100);
            btnPalette1.TabIndex = 5;
            btnPalette1.UseVisualStyleBackColor = false;
            // 
            // btnPalette2
            // 
            btnPalette2.BackColor = Color.White;
            btnPalette2.Location = new Point(101, 37);
            btnPalette2.Margin = new Padding(4, 5, 4, 5);
            btnPalette2.Name = "btnPalette2";
            btnPalette2.Size = new Size(86, 100);
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
            btnContrastProperty.Icon = null;
            btnContrastProperty.Location = new Point(13, 27);
            btnContrastProperty.Margin = new Padding(6, 10, 6, 10);
            btnContrastProperty.MouseState = MaterialSkin.MouseState.HOVER;
            btnContrastProperty.Name = "btnContrastProperty";
            btnContrastProperty.NoAccentTextColor = Color.Empty;
            btnContrastProperty.Size = new Size(214, 167);
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
            btnROI.Location = new Point(464, 27);
            btnROI.Margin = new Padding(6, 10, 6, 10);
            btnROI.MouseState = MaterialSkin.MouseState.HOVER;
            btnROI.Name = "btnROI";
            btnROI.NoAccentTextColor = Color.Empty;
            btnROI.Size = new Size(214, 167);
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
            btnColorProperty.Location = new Point(239, 27);
            btnColorProperty.Margin = new Padding(6, 10, 6, 10);
            btnColorProperty.MouseState = MaterialSkin.MouseState.HOVER;
            btnColorProperty.Name = "btnColorProperty";
            btnColorProperty.NoAccentTextColor = Color.Empty;
            btnColorProperty.Size = new Size(214, 167);
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
            btnMirror.Location = new Point(239, 213);
            btnMirror.Margin = new Padding(6, 10, 6, 10);
            btnMirror.MouseState = MaterialSkin.MouseState.HOVER;
            btnMirror.Name = "btnMirror";
            btnMirror.NoAccentTextColor = Color.Empty;
            btnMirror.Size = new Size(214, 167);
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
            btnNoise.Location = new Point(13, 213);
            btnNoise.Margin = new Padding(6, 10, 6, 10);
            btnNoise.MouseState = MaterialSkin.MouseState.HOVER;
            btnNoise.Name = "btnNoise";
            btnNoise.NoAccentTextColor = Color.Empty;
            btnNoise.Size = new Size(214, 167);
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
            btnDel.Location = new Point(789, 18);
            btnDel.Margin = new Padding(6, 10, 6, 10);
            btnDel.MouseState = MaterialSkin.MouseState.HOVER;
            btnDel.Name = "btnDel";
            btnDel.NoAccentTextColor = Color.Empty;
            btnDel.Size = new Size(157, 167);
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
            pnlProperty.Location = new Point(1587, 953);
            pnlProperty.Margin = new Padding(4, 5, 4, 5);
            pnlProperty.Name = "pnlProperty";
            pnlProperty.Size = new Size(688, 406);
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
            crdProperty.Location = new Point(0, 0);
            crdProperty.Margin = new Padding(20, 23, 20, 23);
            crdProperty.MouseState = MaterialSkin.MouseState.HOVER;
            crdProperty.Name = "crdProperty";
            crdProperty.Padding = new Padding(20, 23, 20, 23);
            crdProperty.Size = new Size(690, 408);
            crdProperty.TabIndex = 0;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.Lime;
            textBox1.Location = new Point(9, 1107);
            textBox1.Margin = new Padding(4, 5, 4, 5);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(604, 252);
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
            btnSave.Location = new Point(617, 18);
            btnSave.Margin = new Padding(6, 10, 6, 10);
            btnSave.MouseState = MaterialSkin.MouseState.HOVER;
            btnSave.Name = "btnSave";
            btnSave.NoAccentTextColor = Color.Empty;
            btnSave.Size = new Size(157, 167);
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
            btnPre5F.Location = new Point(6, 27);
            btnPre5F.Margin = new Padding(6, 10, 6, 10);
            btnPre5F.MouseState = MaterialSkin.MouseState.HOVER;
            btnPre5F.Name = "btnPre5F";
            btnPre5F.NoAccentTextColor = Color.Empty;
            btnPre5F.Size = new Size(86, 83);
            btnPre5F.TabIndex = 10;
            btnPre5F.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnPre5F.UseAccentColor = false;
            btnPre5F.UseVisualStyleBackColor = true;
            // 
            // btnOpnFolderList
            // 
            btnOpnFolderList.Location = new Point(4, 8);
            btnOpnFolderList.Margin = new Padding(4, 5, 4, 5);
            btnOpnFolderList.Name = "btnOpnFolderList";
            btnOpnFolderList.Size = new Size(50, 58);
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
            pnlFolderList.Controls.Add(lstviewFileListD);
            pnlFolderList.Controls.Add(lstviewTrash);
            pnlFolderList.Controls.Add(lstviewMain);
            pnlFolderList.Location = new Point(1587, 18);
            pnlFolderList.Margin = new Padding(4, 5, 4, 5);
            pnlFolderList.Name = "pnlFolderList";
            pnlFolderList.Size = new Size(688, 921);
            pnlFolderList.TabIndex = 12;
            // 
            // btnRestoration
            // 
            btnRestoration.Location = new Point(567, 8);
            btnRestoration.Margin = new Padding(4, 5, 4, 5);
            btnRestoration.Name = "btnRestoration";
            btnRestoration.Size = new Size(50, 58);
            btnRestoration.TabIndex = 19;
            btnRestoration.UseVisualStyleBackColor = true;
            btnRestoration.Visible = false;
            // 
            // lblLstVwName
            // 
            lblLstVwName.Font = new Font("맑은 고딕", 18F);
            lblLstVwName.Location = new Point(63, 13);
            lblLstVwName.Margin = new Padding(4, 0, 4, 0);
            lblLstVwName.Name = "lblLstVwName";
            lblLstVwName.Size = new Size(449, 53);
            lblLstVwName.TabIndex = 18;
            // 
            // btnOpnFileExplrr
            // 
            btnOpnFileExplrr.Location = new Point(626, 8);
            btnOpnFileExplrr.Margin = new Padding(4, 5, 4, 5);
            btnOpnFileExplrr.Name = "btnOpnFileExplrr";
            btnOpnFileExplrr.Size = new Size(50, 58);
            btnOpnFileExplrr.TabIndex = 17;
            btnOpnFileExplrr.UseVisualStyleBackColor = true;
            // 
            // lblFolderList
            // 
            lblFolderList.AutoSize = true;
            lblFolderList.Font = new Font("맑은 고딕", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            lblFolderList.Location = new Point(4, 2);
            lblFolderList.Margin = new Padding(4, 0, 4, 0);
            lblFolderList.Name = "lblFolderList";
            lblFolderList.Padding = new Padding(0, 0, 0, 5);
            lblFolderList.Size = new Size(0, 60);
            lblFolderList.TabIndex = 15;
            // 
            // lstviewFileList
            // 
            lstviewFileList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewItem1.Tag = "파일추가";
            lstviewFileList.Items.AddRange(new ListViewItem[] { listViewItem1 });
            lstviewFileList.Location = new Point(0, 73);
            lstviewFileList.Margin = new Padding(4, 5, 4, 5);
            lstviewFileList.Name = "lstviewFileList";
            lstviewFileList.Size = new Size(688, 39);
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
            // lstviewFileListD
            // 
            lstviewFileListD.Location = new Point(0, 115);
            lstviewFileListD.Margin = new Padding(4, 5, 4, 5);
            lstviewFileListD.Name = "lstviewFileListD";
            lstviewFileListD.Size = new Size(688, 812);
            lstviewFileListD.TabIndex = 16;
            lstviewFileListD.UseCompatibleStateImageBehavior = false;
            lstviewFileListD.View = View.List;
            lstviewFileListD.Visible = false;
            // 
            // lstviewTrash
            // 
            lstviewTrash.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstviewTrash.Location = new Point(-1, 73);
            lstviewTrash.Margin = new Padding(4, 5, 4, 5);
            lstviewTrash.Name = "lstviewTrash";
            lstviewTrash.Size = new Size(688, 846);
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
            lstviewMain.Location = new Point(-1, 73);
            lstviewMain.Margin = new Padding(4, 5, 4, 5);
            lstviewMain.Name = "lstviewMain";
            lstviewMain.Size = new Size(688, 846);
            lstviewMain.SmallImageList = imglst1;
            lstviewMain.TabIndex = 12;
            lstviewMain.UseCompatibleStateImageBehavior = false;
            lstviewMain.View = View.List;
            lstviewMain.SelectedIndexChanged += listView1_SelectedIndexChanged;
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
            pnlCtrl.Location = new Point(623, 1142);
            pnlCtrl.Margin = new Padding(4, 5, 4, 5);
            pnlCtrl.Name = "pnlCtrl";
            pnlCtrl.Size = new Size(957, 220);
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
            btnSpeedPopup.Location = new Point(6, 130);
            btnSpeedPopup.Margin = new Padding(6, 10, 6, 10);
            btnSpeedPopup.MouseState = MaterialSkin.MouseState.HOVER;
            btnSpeedPopup.Name = "btnSpeedPopup";
            btnSpeedPopup.NoAccentTextColor = Color.Empty;
            btnSpeedPopup.Size = new Size(597, 47);
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
            btnPre1F.Location = new Point(103, 27);
            btnPre1F.Margin = new Padding(6, 10, 6, 10);
            btnPre1F.MouseState = MaterialSkin.MouseState.HOVER;
            btnPre1F.Name = "btnPre1F";
            btnPre1F.NoAccentTextColor = Color.Empty;
            btnPre1F.Size = new Size(86, 83);
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
            btnPlayStop.Location = new Point(200, 27);
            btnPlayStop.Margin = new Padding(6, 10, 6, 10);
            btnPlayStop.MouseState = MaterialSkin.MouseState.HOVER;
            btnPlayStop.Name = "btnPlayStop";
            btnPlayStop.NoAccentTextColor = Color.Empty;
            btnPlayStop.Size = new Size(210, 83);
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
            btnNxt5F.Location = new Point(517, 27);
            btnNxt5F.Margin = new Padding(6, 10, 6, 10);
            btnNxt5F.MouseState = MaterialSkin.MouseState.HOVER;
            btnNxt5F.Name = "btnNxt5F";
            btnNxt5F.NoAccentTextColor = Color.Empty;
            btnNxt5F.Size = new Size(86, 83);
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
            btnNxt1F.Location = new Point(421, 27);
            btnNxt1F.Margin = new Padding(6, 10, 6, 10);
            btnNxt1F.MouseState = MaterialSkin.MouseState.HOVER;
            btnNxt1F.Name = "btnNxt1F";
            btnNxt1F.NoAccentTextColor = Color.Empty;
            btnNxt1F.Size = new Size(86, 83);
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
            pnlSpeedPopup.Location = new Point(629, 1132);
            pnlSpeedPopup.Margin = new Padding(4, 5, 4, 5);
            pnlSpeedPopup.Name = "pnlSpeedPopup";
            pnlSpeedPopup.Size = new Size(597, 140);
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
            btnSpeedMinus.Location = new Point(1, 2);
            btnSpeedMinus.Margin = new Padding(6, 10, 6, 10);
            btnSpeedMinus.MouseState = MaterialSkin.MouseState.HOVER;
            btnSpeedMinus.Name = "btnSpeedMinus";
            btnSpeedMinus.NoAccentTextColor = Color.Empty;
            btnSpeedMinus.Size = new Size(44, 137);
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
            btnSpeedPlus.Location = new Point(551, 2);
            btnSpeedPlus.Margin = new Padding(6, 10, 6, 10);
            btnSpeedPlus.MouseState = MaterialSkin.MouseState.HOVER;
            btnSpeedPlus.Name = "btnSpeedPlus";
            btnSpeedPlus.NoAccentTextColor = Color.Empty;
            btnSpeedPlus.Size = new Size(44, 137);
            btnSpeedPlus.TabIndex = 1;
            btnSpeedPlus.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnSpeedPlus.UseAccentColor = false;
            btnSpeedPlus.UseVisualStyleBackColor = true;
            // 
            // sdrSpeedController
            // 
            sdrSpeedController.Depth = 0;
            sdrSpeedController.ForeColor = Color.Gray;
            sdrSpeedController.Location = new Point(54, 10);
            sdrSpeedController.Margin = new Padding(4, 5, 4, 5);
            sdrSpeedController.MouseState = MaterialSkin.MouseState.HOVER;
            sdrSpeedController.Name = "sdrSpeedController";
            sdrSpeedController.ShowValue = false;
            sdrSpeedController.Size = new Size(486, 40);
            sdrSpeedController.TabIndex = 0;
            sdrSpeedController.Text = "";
            // 
            // lblSpeedText
            // 
            lblSpeedText.ForeColor = Color.Black;
            lblSpeedText.Location = new Point(257, 85);
            lblSpeedText.Margin = new Padding(4, 0, 4, 0);
            lblSpeedText.Name = "lblSpeedText";
            lblSpeedText.Size = new Size(83, 47);
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
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(2286, 1500);
            Controls.Add(pnlContrastProperty);
            Controls.Add(pnlColorProperty);
            Controls.Add(pnlROI);
            Controls.Add(pnlSpeedPopup);
            Controls.Add(pnlCtrl);
            Controls.Add(pnlFolderList);
            Controls.Add(textBox1);
            Controls.Add(pnlProperty);
            Controls.Add(pnlVideo);
            Controls.Add(sdrSeekBar);
            Margin = new Padding(4, 5, 4, 5);
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
    }
}