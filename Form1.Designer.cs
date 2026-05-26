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
            ListViewItem listViewItem1 = new ListViewItem(new string[] { "파일목록" }, -1, Color.Empty, Color.Empty, new Font("맑은 고딕", 12F, FontStyle.Regular, GraphicsUnit.Point, 129));
            ListViewItem listViewItem2 = new ListViewItem(new string[] { "휴지통" }, -1, Color.Empty, Color.Empty, new Font("맑은 고딕", 12F, FontStyle.Regular, GraphicsUnit.Point, 129));
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
            btnPrefrm = new MaterialSkin.Controls.MaterialButton();
            btnNextFrm = new MaterialSkin.Controls.MaterialButton();
            btnFolder = new Button();
            pnlFolderList = new Panel();
            lstviewMain = new ListView();
            pnlProperty.SuspendLayout();
            crdProperty.SuspendLayout();
            pnlFolderList.SuspendLayout();
            SuspendLayout();
            // 
            // sdrSeekBar
            // 
            sdrSeekBar.Depth = 0;
            sdrSeekBar.ForeColor = Color.FromArgb(222, 0, 0, 0);
            sdrSeekBar.Location = new Point(21, 666);
            sdrSeekBar.MouseState = MaterialSkin.MouseState.HOVER;
            sdrSeekBar.Name = "sdrSeekBar";
            sdrSeekBar.ShowValue = false;
            sdrSeekBar.Size = new Size(1085, 40);
            sdrSeekBar.TabIndex = 0;
            sdrSeekBar.Text = "4567/10000";
            sdrSeekBar.Click += materialSlider1_Click;
            // 
            // pnlVideo
            // 
            pnlVideo.BackColor = Color.Black;
            pnlVideo.ForeColor = Color.Coral;
            pnlVideo.Location = new Point(21, 88);
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
            btnContrastProperty.Location = new Point(9, 11);
            btnContrastProperty.Margin = new Padding(4, 6, 4, 6);
            btnContrastProperty.MouseState = MaterialSkin.MouseState.HOVER;
            btnContrastProperty.Name = "btnContrastProperty";
            btnContrastProperty.NoAccentTextColor = Color.Empty;
            btnContrastProperty.Size = new Size(150, 50);
            btnContrastProperty.TabIndex = 5;
            btnContrastProperty.Text = "Contrast";
            btnContrastProperty.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnContrastProperty.UseAccentColor = false;
            btnContrastProperty.UseVisualStyleBackColor = true;
            // 
            // btnROI
            // 
            btnROI.AutoSize = false;
            btnROI.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnROI.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnROI.Depth = 0;
            btnROI.HighEmphasis = true;
            btnROI.Icon = null;
            btnROI.Location = new Point(9, 124);
            btnROI.Margin = new Padding(4, 6, 4, 6);
            btnROI.MouseState = MaterialSkin.MouseState.HOVER;
            btnROI.Name = "btnROI";
            btnROI.NoAccentTextColor = Color.Empty;
            btnROI.Size = new Size(150, 50);
            btnROI.TabIndex = 4;
            btnROI.Text = "ROI";
            btnROI.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnROI.UseAccentColor = false;
            btnROI.UseVisualStyleBackColor = true;
            // 
            // btnColorProperty
            // 
            btnColorProperty.AutoSize = false;
            btnColorProperty.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnColorProperty.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnColorProperty.Depth = 0;
            btnColorProperty.HighEmphasis = true;
            btnColorProperty.Icon = null;
            btnColorProperty.Location = new Point(167, 11);
            btnColorProperty.Margin = new Padding(4, 6, 4, 6);
            btnColorProperty.MouseState = MaterialSkin.MouseState.HOVER;
            btnColorProperty.Name = "btnColorProperty";
            btnColorProperty.NoAccentTextColor = Color.Empty;
            btnColorProperty.Size = new Size(150, 50);
            btnColorProperty.TabIndex = 3;
            btnColorProperty.Text = "Color";
            btnColorProperty.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnColorProperty.UseAccentColor = false;
            btnColorProperty.UseVisualStyleBackColor = true;
            // 
            // btnMirror
            // 
            btnMirror.AutoSize = false;
            btnMirror.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnMirror.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnMirror.Depth = 0;
            btnMirror.HighEmphasis = true;
            btnMirror.Icon = null;
            btnMirror.Location = new Point(167, 124);
            btnMirror.Margin = new Padding(4, 6, 4, 6);
            btnMirror.MouseState = MaterialSkin.MouseState.HOVER;
            btnMirror.Name = "btnMirror";
            btnMirror.NoAccentTextColor = Color.Empty;
            btnMirror.Size = new Size(150, 50);
            btnMirror.TabIndex = 2;
            btnMirror.Text = "Mirror";
            btnMirror.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnMirror.UseAccentColor = false;
            btnMirror.UseVisualStyleBackColor = true;
            btnMirror.Click += materialButton2_Click;
            // 
            // btnNoise
            // 
            btnNoise.AutoSize = false;
            btnNoise.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnNoise.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnNoise.Depth = 0;
            btnNoise.HighEmphasis = true;
            btnNoise.Icon = null;
            btnNoise.Location = new Point(325, 11);
            btnNoise.Margin = new Padding(4, 6, 4, 6);
            btnNoise.MouseState = MaterialSkin.MouseState.HOVER;
            btnNoise.Name = "btnNoise";
            btnNoise.NoAccentTextColor = Color.Empty;
            btnNoise.Size = new Size(150, 50);
            btnNoise.TabIndex = 1;
            btnNoise.Text = "noise";
            btnNoise.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnNoise.UseAccentColor = false;
            btnNoise.UseVisualStyleBackColor = true;
            // 
            // btnDel
            // 
            btnDel.AutoSize = false;
            btnDel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDel.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnDel.Depth = 0;
            btnDel.HighEmphasis = true;
            btnDel.Icon = null;
            btnDel.Location = new Point(954, 771);
            btnDel.Margin = new Padding(4, 6, 4, 6);
            btnDel.MouseState = MaterialSkin.MouseState.HOVER;
            btnDel.Name = "btnDel";
            btnDel.NoAccentTextColor = Color.Empty;
            btnDel.Size = new Size(150, 120);
            btnDel.TabIndex = 0;
            btnDel.Text = "Delete";
            btnDel.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnDel.UseAccentColor = true;
            btnDel.UseVisualStyleBackColor = true;
            // 
            // pnlProperty
            // 
            pnlProperty.BackColor = SystemColors.ControlDark;
            pnlProperty.BorderStyle = BorderStyle.Fixed3D;
            pnlProperty.Controls.Add(crdProperty);
            pnlProperty.Location = new Point(1111, 649);
            pnlProperty.Name = "pnlProperty";
            pnlProperty.Size = new Size(483, 245);
            pnlProperty.TabIndex = 6;
            // 
            // crdProperty
            // 
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
            textBox1.Location = new Point(6, 772);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(424, 122);
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
            btnSave.Location = new Point(793, 771);
            btnSave.Margin = new Padding(4, 6, 4, 6);
            btnSave.MouseState = MaterialSkin.MouseState.HOVER;
            btnSave.Name = "btnSave";
            btnSave.NoAccentTextColor = Color.Empty;
            btnSave.Size = new Size(150, 120);
            btnSave.TabIndex = 8;
            btnSave.Text = "Delete";
            btnSave.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnSave.UseAccentColor = true;
            btnSave.UseVisualStyleBackColor = true;
            // 
            // btnPrefrm
            // 
            btnPrefrm.AutoSize = false;
            btnPrefrm.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnPrefrm.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnPrefrm.Depth = 0;
            btnPrefrm.HighEmphasis = true;
            btnPrefrm.Icon = null;
            btnPrefrm.Location = new Point(442, 782);
            btnPrefrm.Margin = new Padding(4, 6, 4, 6);
            btnPrefrm.MouseState = MaterialSkin.MouseState.HOVER;
            btnPrefrm.Name = "btnPrefrm";
            btnPrefrm.NoAccentTextColor = Color.Empty;
            btnPrefrm.Size = new Size(160, 50);
            btnPrefrm.TabIndex = 9;
            btnPrefrm.Text = "Contrast";
            btnPrefrm.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnPrefrm.UseAccentColor = false;
            btnPrefrm.UseVisualStyleBackColor = true;
            // 
            // btnNextFrm
            // 
            btnNextFrm.AutoSize = false;
            btnNextFrm.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnNextFrm.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnNextFrm.Depth = 0;
            btnNextFrm.HighEmphasis = true;
            btnNextFrm.Icon = null;
            btnNextFrm.Location = new Point(614, 782);
            btnNextFrm.Margin = new Padding(4, 6, 4, 6);
            btnNextFrm.MouseState = MaterialSkin.MouseState.HOVER;
            btnNextFrm.Name = "btnNextFrm";
            btnNextFrm.NoAccentTextColor = Color.Empty;
            btnNextFrm.Size = new Size(160, 50);
            btnNextFrm.TabIndex = 10;
            btnNextFrm.Text = "Contrast";
            btnNextFrm.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnNextFrm.UseAccentColor = false;
            btnNextFrm.UseVisualStyleBackColor = true;
            // 
            // btnFolder
            // 
            btnFolder.Location = new Point(442, 5);
            btnFolder.Name = "btnFolder";
            btnFolder.Size = new Size(35, 35);
            btnFolder.TabIndex = 11;
            btnFolder.UseVisualStyleBackColor = true;
            // 
            // pnlFolderList
            // 
            pnlFolderList.BorderStyle = BorderStyle.Fixed3D;
            pnlFolderList.Controls.Add(lstviewMain);
            pnlFolderList.Controls.Add(btnFolder);
            pnlFolderList.Location = new Point(1111, 88);
            pnlFolderList.Name = "pnlFolderList";
            pnlFolderList.Size = new Size(483, 554);
            pnlFolderList.TabIndex = 12;
            // 
            // lstviewMain
            // 
            lstviewMain.Items.AddRange(new ListViewItem[] { listViewItem1, listViewItem2 });
            lstviewMain.Location = new Point(-1, 44);
            lstviewMain.Name = "lstviewMain";
            lstviewMain.Size = new Size(483, 509);
            lstviewMain.TabIndex = 12;
            lstviewMain.UseCompatibleStateImageBehavior = false;
            lstviewMain.View = View.List;
            lstviewMain.SelectedIndexChanged += listView1_SelectedIndexChanged;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(1600, 900);
            Controls.Add(pnlFolderList);
            Controls.Add(btnNextFrm);
            Controls.Add(btnPrefrm);
            Controls.Add(btnSave);
            Controls.Add(textBox1);
            Controls.Add(btnDel);
            Controls.Add(pnlProperty);
            Controls.Add(pnlVideo);
            Controls.Add(sdrSeekBar);
            Name = "frmMain";
            Text = "11팀 UI 프로토타입";
            pnlProperty.ResumeLayout(false);
            crdProperty.ResumeLayout(false);
            pnlFolderList.ResumeLayout(false);
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
        private MaterialSkin.Controls.MaterialButton btnPrefrm;
        private MaterialSkin.Controls.MaterialButton btnNextFrm;
        private Button btnFolder;
        private Panel pnlFolderList;
        private ListView lstviewMain;
    }
}
