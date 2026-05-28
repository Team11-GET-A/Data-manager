namespace Data_Manager
{
    partial class frmAddFile
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
            pnlCopyFile = new Panel();
            lstviewCopyFile = new ListView();
            pnlAddFile = new Panel();
            lstviewAddFile = new ListView();
            txtbSelctFile = new TextBox();
            btnAddFile = new Button();
            btnSelctFile = new Button();
            pnlCopyFile.SuspendLayout();
            pnlAddFile.SuspendLayout();
            SuspendLayout();
            // 
            // pnlCopyFile
            // 
            pnlCopyFile.BorderStyle = BorderStyle.Fixed3D;
            pnlCopyFile.Controls.Add(lstviewCopyFile);
            pnlCopyFile.Location = new Point(14, 88);
            pnlCopyFile.Margin = new Padding(4, 4, 4, 4);
            pnlCopyFile.Name = "pnlCopyFile";
            pnlCopyFile.Size = new Size(702, 629);
            pnlCopyFile.TabIndex = 0;
            // 
            // lstviewCopyFile
            // 
            lstviewCopyFile.Location = new Point(-4, -1);
            lstviewCopyFile.Margin = new Padding(4, 4, 4, 4);
            lstviewCopyFile.Name = "lstviewCopyFile";
            lstviewCopyFile.Size = new Size(705, 629);
            lstviewCopyFile.TabIndex = 5;
            lstviewCopyFile.UseCompatibleStateImageBehavior = false;
            // 
            // pnlAddFile
            // 
            pnlAddFile.BorderStyle = BorderStyle.Fixed3D;
            pnlAddFile.Controls.Add(lstviewAddFile);
            pnlAddFile.Location = new Point(739, 88);
            pnlAddFile.Margin = new Padding(4, 4, 4, 4);
            pnlAddFile.Name = "pnlAddFile";
            pnlAddFile.Size = new Size(706, 631);
            pnlAddFile.TabIndex = 1;
            // 
            // lstviewAddFile
            // 
            lstviewAddFile.Location = new Point(-1, -3);
            lstviewAddFile.Margin = new Padding(4, 4, 4, 4);
            lstviewAddFile.Name = "lstviewAddFile";
            lstviewAddFile.Size = new Size(705, 629);
            lstviewAddFile.TabIndex = 4;
            lstviewAddFile.UseCompatibleStateImageBehavior = false;
            // 
            // txtbSelctFile
            // 
            txtbSelctFile.Font = new Font("맑은 고딕", 9F);
            txtbSelctFile.Location = new Point(17, 35);
            txtbSelctFile.Margin = new Padding(4, 4, 4, 4);
            txtbSelctFile.Multiline = true;
            txtbSelctFile.Name = "txtbSelctFile";
            txtbSelctFile.Size = new Size(617, 40);
            txtbSelctFile.TabIndex = 2;
            // 
            // btnAddFile
            // 
            btnAddFile.Location = new Point(1318, 728);
            btnAddFile.Margin = new Padding(4, 4, 4, 4);
            btnAddFile.Name = "btnAddFile";
            btnAddFile.Size = new Size(116, 80);
            btnAddFile.TabIndex = 5;
            btnAddFile.UseVisualStyleBackColor = true;
            // 
            // btnSelctFile
            // 
            btnSelctFile.ImageAlign = ContentAlignment.BottomCenter;
            btnSelctFile.Location = new Point(643, 24);
            btnSelctFile.Margin = new Padding(4, 4, 4, 4);
            btnSelctFile.Name = "btnSelctFile";
            btnSelctFile.Size = new Size(71, 56);
            btnSelctFile.TabIndex = 6;
            btnSelctFile.TextAlign = ContentAlignment.BottomCenter;
            btnSelctFile.UseVisualStyleBackColor = true;
            // 
            // frmAddFile
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1462, 825);
            Controls.Add(btnSelctFile);
            Controls.Add(btnAddFile);
            Controls.Add(pnlAddFile);
            Controls.Add(txtbSelctFile);
            Controls.Add(pnlCopyFile);
            Location = new Point(200, 200);
            Margin = new Padding(4, 4, 4, 4);
            Name = "frmAddFile";
            StartPosition = FormStartPosition.CenterParent;
            Text = "로컬 파일을 프로그램에 복사하여 업로드";
            Load += frmAddFile_Load;
            pnlCopyFile.ResumeLayout(false);
            pnlAddFile.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlCopyFile;
        private Panel pnlAddFile;
        private TextBox txtbSelctFile;
        private Button btnAddFile;
        private ListView lstviewAddFile;
        private ListView lstviewCopyFile;
        private Button btnSelctFile;
    }
}