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
            button3 = new Button();
            button1 = new Button();
            button2 = new Button();
            comboBox1 = new ComboBox();
            panel2 = new Panel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            materialSlider1 = new MaterialSkin.Controls.MaterialSlider();
            button6 = new Button();
            button7 = new Button();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.Control;
            panel1.Controls.Add(button7);
            panel1.Controls.Add(button6);
            panel1.Controls.Add(materialSlider1);
            panel1.Controls.Add(button5);
            panel1.Controls.Add(button4);
            panel1.Controls.Add(button3);
            panel1.Controls.Add(button1);
            panel1.Controls.Add(button2);
            panel1.Controls.Add(comboBox1);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(2, 689);
            panel1.Margin = new Padding(2);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(10);
            panel1.Size = new Size(1596, 209);
            panel1.TabIndex = 0;
            // 
            // button5
            // 
            button5.Location = new Point(1056, 74);
            button5.Margin = new Padding(4);
            button5.Name = "button5";
            button5.Size = new Size(160, 90);
            button5.TabIndex = 7;
            button5.Text = ">>";
            button5.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Location = new Point(888, 74);
            button4.Margin = new Padding(4);
            button4.Name = "button4";
            button4.Size = new Size(160, 90);
            button4.TabIndex = 6;
            button4.Text = ">";
            button4.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(384, 74);
            button3.Margin = new Padding(4);
            button3.Name = "button3";
            button3.Size = new Size(160, 90);
            button3.TabIndex = 5;
            button3.Text = "<<";
            button3.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Location = new Point(552, 74);
            button1.Margin = new Padding(4);
            button1.Name = "button1";
            button1.Size = new Size(160, 90);
            button1.TabIndex = 4;
            button1.Text = "<";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(720, 74);
            button2.Margin = new Padding(4);
            button2.Name = "button2";
            button2.Size = new Size(160, 90);
            button2.TabIndex = 3;
            button2.Text = "play";
            button2.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "1배속", "2배속", "3배속", "5배속" });
            comboBox1.Location = new Point(1392, 74);
            comboBox1.Margin = new Padding(4);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(168, 28);
            comboBox1.TabIndex = 2;
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
            flowLayoutPanel1.Size = new Size(1596, 502);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // materialSlider1
            // 
            materialSlider1.Depth = 0;
            materialSlider1.ForeColor = Color.FromArgb(222, 0, 0, 0);
            materialSlider1.Location = new Point(13, 13);
            materialSlider1.MouseState = MaterialSkin.MouseState.HOVER;
            materialSlider1.Name = "materialSlider1";
            materialSlider1.ShowValue = false;
            materialSlider1.Size = new Size(1570, 40);
            materialSlider1.TabIndex = 8;
            materialSlider1.Text = "count";
            // 
            // button6
            // 
            button6.Location = new Point(1480, 119);
            button6.Margin = new Padding(4);
            button6.Name = "button6";
            button6.Size = new Size(80, 45);
            button6.TabIndex = 9;
            button6.Text = "배속";
            button6.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            button7.Location = new Point(1392, 119);
            button7.Margin = new Padding(4);
            button7.Name = "button7";
            button7.Size = new Size(80, 45);
            button7.TabIndex = 10;
            button7.Text = ">>";
            button7.UseVisualStyleBackColor = true;
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
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panel2;
        private ComboBox comboBox1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button button2;
        private Button button5;
        private Button button4;
        private Button button3;
        private Button button1;
        private Button button6;
        private MaterialSkin.Controls.MaterialSlider materialSlider1;
        private Button button7;
    }
}