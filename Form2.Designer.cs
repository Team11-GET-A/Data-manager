namespace Data_Manager
{
    partial class Form2
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
            panel1 = new Panel();
            btnSpeedSet1 = new Button();
            btnReversePlay = new Button();
            trbImageLocation = new MaterialSkin.Controls.MaterialSlider();
            btnOverNext = new Button();
            btnNext = new Button();
            btnOverBack = new Button();
            btnBack = new Button();
            btnPlay = new Button();
            cbxFaster = new ComboBox();
            panel2 = new Panel();
            btnCardAdder = new Button();
            materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.Control;
            panel1.Controls.Add(btnSpeedSet1);
            panel1.Controls.Add(btnReversePlay);
            panel1.Controls.Add(trbImageLocation);
            panel1.Controls.Add(btnOverNext);
            panel1.Controls.Add(btnNext);
            panel1.Controls.Add(btnOverBack);
            panel1.Controls.Add(btnBack);
            panel1.Controls.Add(btnPlay);
            panel1.Controls.Add(cbxFaster);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(2, 674);
            panel1.Margin = new Padding(2);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(10);
            panel1.Size = new Size(1580, 189);
            panel1.TabIndex = 0;
            // 
            // btnSpeedSet1
            // 
            btnSpeedSet1.Location = new Point(1480, 119);
            btnSpeedSet1.Margin = new Padding(4);
            btnSpeedSet1.Name = "btnSpeedSet1";
            btnSpeedSet1.Size = new Size(80, 45);
            btnSpeedSet1.TabIndex = 10;
            btnSpeedSet1.Text = "X 1";
            btnSpeedSet1.UseVisualStyleBackColor = true;
            // 
            // btnReversePlay
            // 
            btnReversePlay.Location = new Point(1392, 119);
            btnReversePlay.Margin = new Padding(4);
            btnReversePlay.Name = "btnReversePlay";
            btnReversePlay.Size = new Size(80, 45);
            btnReversePlay.TabIndex = 9;
            btnReversePlay.Text = "역재생";
            btnReversePlay.UseVisualStyleBackColor = true;
            // 
            // trbImageLocation
            // 
            trbImageLocation.Depth = 0;
            trbImageLocation.ForeColor = Color.FromArgb(222, 0, 0, 0);
            trbImageLocation.Location = new Point(13, 13);
            trbImageLocation.MouseState = MaterialSkin.MouseState.HOVER;
            trbImageLocation.Name = "trbImageLocation";
            trbImageLocation.ShowValue = false;
            trbImageLocation.Size = new Size(1564, 40);
            trbImageLocation.TabIndex = 8;
            trbImageLocation.Text = "count";
            // 
            // btnOverNext
            // 
            btnOverNext.Location = new Point(1056, 74);
            btnOverNext.Margin = new Padding(4);
            btnOverNext.Name = "btnOverNext";
            btnOverNext.Size = new Size(160, 90);
            btnOverNext.TabIndex = 7;
            btnOverNext.Text = ">>";
            btnOverNext.UseVisualStyleBackColor = true;
            // 
            // btnNext
            // 
            btnNext.Location = new Point(888, 74);
            btnNext.Margin = new Padding(4);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(160, 90);
            btnNext.TabIndex = 6;
            btnNext.Text = ">";
            btnNext.UseVisualStyleBackColor = true;
            // 
            // btnOverBack
            // 
            btnOverBack.Location = new Point(384, 74);
            btnOverBack.Margin = new Padding(4);
            btnOverBack.Name = "btnOverBack";
            btnOverBack.Size = new Size(160, 90);
            btnOverBack.TabIndex = 5;
            btnOverBack.Text = "<<";
            btnOverBack.UseVisualStyleBackColor = true;
            // 
            // btnBack
            // 
            btnBack.Location = new Point(552, 74);
            btnBack.Margin = new Padding(4);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(160, 90);
            btnBack.TabIndex = 4;
            btnBack.Text = "<";
            btnBack.UseVisualStyleBackColor = true;
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(720, 74);
            btnPlay.Margin = new Padding(4);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(160, 90);
            btnPlay.TabIndex = 3;
            btnPlay.Text = "play";
            btnPlay.UseVisualStyleBackColor = true;
            // 
            // cbxFaster
            // 
            cbxFaster.FormattingEnabled = true;
            cbxFaster.Items.AddRange(new object[] { "1배속", "2배속", "3배속", "5배속" });
            cbxFaster.Location = new Point(1392, 74);
            cbxFaster.Margin = new Padding(4);
            cbxFaster.Name = "cbxFaster";
            cbxFaster.Size = new Size(168, 28);
            cbxFaster.TabIndex = 2;
            // 
            // panel2
            // 
            panel2.Controls.Add(btnCardAdder);
            panel2.Controls.Add(materialLabel1);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(2, 85);
            panel2.Margin = new Padding(4);
            panel2.Name = "panel2";
            panel2.Size = new Size(1580, 106);
            panel2.TabIndex = 1;
            // 
            // btnCardAdder
            // 
            btnCardAdder.Location = new Point(1354, 28);
            btnCardAdder.Margin = new Padding(4);
            btnCardAdder.Name = "btnCardAdder";
            btnCardAdder.Size = new Size(206, 45);
            btnCardAdder.TabIndex = 11;
            btnCardAdder.Text = "모델 추가하기";
            btnCardAdder.UseVisualStyleBackColor = true;
            btnCardAdder.Click += BtnCardAdder_Click;
            // 
            // materialLabel1
            // 
            materialLabel1.AutoSize = true;
            materialLabel1.Depth = 0;
            materialLabel1.Font = new Font("Roboto", 48F, FontStyle.Bold, GraphicsUnit.Pixel);
            materialLabel1.FontType = MaterialSkin.MaterialSkinManager.fontType.H3;
            materialLabel1.Location = new Point(23, 15);
            materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            materialLabel1.Name = "materialLabel1";
            materialLabel1.Size = new Size(124, 58);
            materialLabel1.TabIndex = 0;
            materialLabel1.Text = "파일럿";
            materialLabel1.Click += materialLabel1_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.BackColor = Color.LightGray;
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(2, 187);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(10);
            flowLayoutPanel1.Size = new Size(1580, 562);
            flowLayoutPanel1.TabIndex = 2;
            flowLayoutPanel1.WrapContents = false;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1584, 861);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Font = new Font("맑은 고딕", 11.25F);
            Margin = new Padding(2);
            Name = "Form2";
            Padding = new Padding(2);
            Text = "파일럿";
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private TrackBar trackBar1;
        private Panel panel2;
        private ComboBox cbxFaster;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button btnPlay;
        private Button btnOverNext;
        private Button btnNext;
        private Button btnOverBack;
        private Button btnBack;
        private Button btnReversePlay;
        private MaterialSkin.Controls.MaterialSlider trbImageLocation;
        private Button btnSpeedSet1;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private Button btnCardAdder;
    }
}