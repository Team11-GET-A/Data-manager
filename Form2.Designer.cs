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
            button5 = new Button();
            button4 = new Button();
            btnPlay = new Button();
            button2 = new Button();
            comboBox1 = new ComboBox();
            button1 = new Button();
            trackBar1 = new TrackBar();
            panel2 = new Panel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.Control;
            panel1.Controls.Add(button5);
            panel1.Controls.Add(button4);
            panel1.Controls.Add(btnPlay);
            panel1.Controls.Add(button2);
            panel1.Controls.Add(comboBox1);
            panel1.Controls.Add(button1);
            panel1.Controls.Add(trackBar1);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(2, 674);
            panel1.Margin = new Padding(2);
            panel1.Name = "panel1";
            panel1.Size = new Size(1596, 224);
            panel1.TabIndex = 0;
            // 
            // button5
            // 
            button5.Location = new Point(520, 116);
            button5.Margin = new Padding(4);
            button5.Name = "button5";
            button5.Size = new Size(110, 62);
            button5.TabIndex = 6;
            button5.Text = ">>>";
            button5.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Location = new Point(403, 116);
            button4.Margin = new Padding(4);
            button4.Name = "button4";
            button4.Size = new Size(110, 62);
            button4.TabIndex = 5;
            button4.Text = ">";
            button4.UseVisualStyleBackColor = true;
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(286, 116);
            btnPlay.Margin = new Padding(4);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(110, 62);
            btnPlay.TabIndex = 4;
            btnPlay.Text = "play";
            btnPlay.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(169, 116);
            button2.Margin = new Padding(4);
            button2.Name = "button2";
            button2.Size = new Size(110, 62);
            button2.TabIndex = 3;
            button2.Text = "<";
            button2.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "1배속", "2배속", "3배속", "5배속" });
            comboBox1.Location = new Point(661, 34);
            comboBox1.Margin = new Padding(4);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(155, 28);
            comboBox1.TabIndex = 2;
            // 
            // button1
            // 
            button1.Location = new Point(52, 116);
            button1.Margin = new Padding(4);
            button1.Name = "button1";
            button1.Size = new Size(110, 62);
            button1.TabIndex = 1;
            button1.Text = "<<<";
            button1.UseVisualStyleBackColor = true;
            // 
            // trackBar1
            // 
            trackBar1.Location = new Point(52, 21);
            trackBar1.Margin = new Padding(2);
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new Size(578, 45);
            trackBar1.TabIndex = 0;
            trackBar1.TickStyle = TickStyle.Both;
            // 
            // panel2
            // 
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(2, 85);
            panel2.Margin = new Padding(4);
            panel2.Name = "panel2";
            panel2.Size = new Size(1596, 102);
            panel2.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(2, 187);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(1596, 487);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1600, 900);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Font = new Font("맑은 고딕", 11.25F);
            Margin = new Padding(2);
            Name = "Form2";
            Padding = new Padding(2, 85, 2, 2);
            Text = "파일럿";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private TrackBar trackBar1;
        private Panel panel2;
        private ComboBox comboBox1;
        private Button button1;
        private Button button5;
        private Button button4;
        private Button btnPlay;
        private Button button2;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}