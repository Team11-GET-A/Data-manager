namespace Data_Manager
{
    partial class trainer
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
            materialButton1 = new MaterialSkin.Controls.MaterialButton();
            lblTrainer = new MaterialSkin.Controls.MaterialLabel();
            panel3 = new Panel();
            libModelList = new MaterialSkin.Controls.MaterialListBox();
            panel1.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(materialButton1);
            panel1.Controls.Add(lblTrainer);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1584, 132);
            panel1.TabIndex = 0;
            // 
            // materialButton1
            // 
            materialButton1.AutoSize = false;
            materialButton1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            materialButton1.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            materialButton1.Depth = 0;
            materialButton1.HighEmphasis = true;
            materialButton1.Icon = null;
            materialButton1.Location = new Point(1375, 9);
            materialButton1.Margin = new Padding(4, 6, 4, 6);
            materialButton1.MouseState = MaterialSkin.MouseState.HOVER;
            materialButton1.Name = "materialButton1";
            materialButton1.NoAccentTextColor = Color.Empty;
            materialButton1.Size = new Size(160, 90);
            materialButton1.TabIndex = 1;
            materialButton1.Text = "materialButton1";
            materialButton1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            materialButton1.UseAccentColor = false;
            materialButton1.UseVisualStyleBackColor = true;
            // 
            // lblTrainer
            // 
            lblTrainer.Depth = 0;
            lblTrainer.Font = new Font("Roboto Light", 60F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblTrainer.FontType = MaterialSkin.MaterialSkinManager.fontType.H2;
            lblTrainer.Location = new Point(12, 9);
            lblTrainer.MouseState = MaterialSkin.MouseState.HOVER;
            lblTrainer.Name = "lblTrainer";
            lblTrainer.Size = new Size(234, 108);
            lblTrainer.TabIndex = 0;
            lblTrainer.Text = "트레이너";
            lblTrainer.TextAlign = ContentAlignment.MiddleCenter;
            lblTrainer.UseAccent = true;
            // 
            // panel3
            // 
            panel3.Controls.Add(libModelList);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(0, 132);
            panel3.Name = "panel3";
            panel3.Padding = new Padding(10, 0, 10, 10);
            panel3.Size = new Size(1584, 729);
            panel3.TabIndex = 2;
            // 
            // libModelList
            // 
            libModelList.BackColor = Color.White;
            libModelList.BorderColor = Color.LightGray;
            libModelList.Depth = 0;
            libModelList.Dock = DockStyle.Fill;
            libModelList.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            libModelList.Location = new Point(10, 0);
            libModelList.MouseState = MaterialSkin.MouseState.HOVER;
            libModelList.Name = "libModelList";
            libModelList.SelectedIndex = -1;
            libModelList.SelectedItem = null;
            libModelList.Size = new Size(1564, 719);
            libModelList.TabIndex = 0;
            // 
            // Form3
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1584, 861);
            Controls.Add(panel3);
            Controls.Add(panel1);
            Name = "Form3";
            Text = "트레이너";
            panel1.ResumeLayout(false);
            panel3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panel3;
        private MaterialSkin.Controls.MaterialLabel lblTrainer;
        private MaterialSkin.Controls.MaterialListBox libModelList;
        private MaterialSkin.Controls.MaterialButton materialButton1;
    }
}