namespace Data_Manager
{
    partial class PliotModelList
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
            btnResetFilter = new Button();
            btnModelFliter = new Button();
            txtModelFilter = new TextBox();
            lstModelList = new MaterialSkin.Controls.MaterialListBox();
            panel2 = new Panel();
            btnModelLoad = new Button();
            panel3 = new Panel();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(btnResetFilter);
            panel1.Controls.Add(btnModelFliter);
            panel1.Controls.Add(txtModelFilter);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 695);
            panel1.Name = "panel1";
            panel1.Size = new Size(684, 66);
            panel1.TabIndex = 0;
            // 
            // btnResetFilter
            // 
            btnResetFilter.Location = new Point(470, 16);
            btnResetFilter.Name = "btnResetFilter";
            btnResetFilter.Size = new Size(98, 34);
            btnResetFilter.TabIndex = 2;
            btnResetFilter.Text = "초기화";
            btnResetFilter.UseVisualStyleBackColor = true;
            // 
            // btnModelFliter
            // 
            btnModelFliter.Location = new Point(574, 16);
            btnModelFliter.Name = "btnModelFliter";
            btnModelFliter.Size = new Size(98, 34);
            btnModelFliter.TabIndex = 1;
            btnModelFliter.Text = "검색";
            btnModelFliter.UseVisualStyleBackColor = true;
            // 
            // txtModelFilter
            // 
            txtModelFilter.Font = new Font("맑은 고딕", 15F);
            txtModelFilter.Location = new Point(12, 16);
            txtModelFilter.Name = "txtModelFilter";
            txtModelFilter.Size = new Size(452, 34);
            txtModelFilter.TabIndex = 0;
            // 
            // lstModelList
            // 
            lstModelList.BackColor = Color.White;
            lstModelList.BorderColor = Color.LightGray;
            lstModelList.Depth = 0;
            lstModelList.Dock = DockStyle.Fill;
            lstModelList.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            lstModelList.Location = new Point(0, 0);
            lstModelList.MouseState = MaterialSkin.MouseState.HOVER;
            lstModelList.Name = "lstModelList";
            lstModelList.SelectedIndex = -1;
            lstModelList.SelectedItem = null;
            lstModelList.Size = new Size(684, 653);
            lstModelList.TabIndex = 0;
            // 
            // panel2
            // 
            panel2.Controls.Add(btnModelLoad);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(684, 42);
            panel2.TabIndex = 1;
            // 
            // btnModelLoad
            // 
            btnModelLoad.Location = new Point(597, 8);
            btnModelLoad.Name = "btnModelLoad";
            btnModelLoad.Size = new Size(75, 23);
            btnModelLoad.TabIndex = 0;
            btnModelLoad.Text = "불러오기";
            btnModelLoad.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            panel3.Controls.Add(lstModelList);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(0, 42);
            panel3.Name = "panel3";
            panel3.Size = new Size(684, 653);
            panel3.TabIndex = 2;
            // 
            // PliotModelList
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(684, 761);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "PliotModelList";
            Text = "모델선택";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button btnResetFilter;
        private Button btnModelFliter;
        private TextBox txtModelFilter;
        private MaterialSkin.Controls.MaterialListBox lstModelList;
        private Panel panel2;
        private Button btnModelLoad;
        private Panel panel3;
    }
}