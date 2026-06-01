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
            panel2 = new Panel();
            btnChart = new Button();
            btnCardAdder = new Button();
            materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel2
            // 
            panel2.Controls.Add(btnChart);
            panel2.Controls.Add(btnCardAdder);
            panel2.Controls.Add(materialLabel1);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(2, 2);
            panel2.Margin = new Padding(4);
            panel2.Name = "panel2";
            panel2.Size = new Size(1580, 106);
            panel2.TabIndex = 1;
            // 
            // btnChart
            // 
            btnChart.Location = new Point(1156, 20);
            btnChart.Name = "btnChart";
            btnChart.Size = new Size(191, 61);
            btnChart.TabIndex = 0;
            btnChart.Text = "차트 열기";
            btnChart.UseVisualStyleBackColor = true;
            // 
            // btnCardAdder
            // 
            btnCardAdder.Location = new Point(1354, 20);
            btnCardAdder.Margin = new Padding(4);
            btnCardAdder.Name = "btnCardAdder";
            btnCardAdder.Size = new Size(206, 61);
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
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.BackColor = Color.LightGray;
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(2, 108);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(10);
            flowLayoutPanel1.Size = new Size(1580, 751);
            flowLayoutPanel1.TabIndex = 2;
            flowLayoutPanel1.WrapContents = false;
            // 
            // Pliot
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1584, 861);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(panel2);
            Font = new Font("맑은 고딕", 11.25F);
            Margin = new Padding(2);
            Name = "Pliot";
            Padding = new Padding(2);
            Text = "파일럿";
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel panel2;
        private FlowLayoutPanel flowLayoutPanel1;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private Button btnCardAdder;
        private Button btnChart;
    }
}