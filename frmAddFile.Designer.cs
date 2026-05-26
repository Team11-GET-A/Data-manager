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
            btnCopyFile = new Button();
            btnAddFile = new Button();
            btnSelctFile = new Button();
            btnClose = new Button();
            pnlCopyFile.SuspendLayout();
            pnlAddFile.SuspendLayout();
            SuspendLayout();
            // 
            // pnlCopyFile
            // 
            pnlCopyFile.BorderStyle = BorderStyle.Fixed3D;
            pnlCopyFile.Controls.Add(lstviewCopyFile);
            pnlCopyFile.Location = new Point(11, 66);
            pnlCopyFile.Name = "pnlCopyFile";
            pnlCopyFile.Size = new Size(547, 473);
            pnlCopyFile.TabIndex = 0;
            // 
            // lstviewCopyFile
            // 
            lstviewCopyFile.Location = new Point(-3, -1);
            lstviewCopyFile.Name = "lstviewCopyFile";
            lstviewCopyFile.Size = new Size(549, 473);
            lstviewCopyFile.TabIndex = 5;
            lstviewCopyFile.UseCompatibleStateImageBehavior = false;
            // 
            // pnlAddFile
            // 
            pnlAddFile.BorderStyle = BorderStyle.Fixed3D;
            pnlAddFile.Controls.Add(lstviewAddFile);
            pnlAddFile.Location = new Point(575, 66);
            pnlAddFile.Name = "pnlAddFile";
            pnlAddFile.Size = new Size(550, 474);
            pnlAddFile.TabIndex = 1;
            // 
            // lstviewAddFile
            // 
            lstviewAddFile.Location = new Point(-1, -2);
            lstviewAddFile.Name = "lstviewAddFile";
            lstviewAddFile.Size = new Size(549, 473);
            lstviewAddFile.TabIndex = 4;
            lstviewAddFile.UseCompatibleStateImageBehavior = false;
            // 
            // txtbSelctFile
            // 
            txtbSelctFile.Font = new Font("맑은 고딕", 9F);
            txtbSelctFile.Location = new Point(13, 23);
            txtbSelctFile.Multiline = true;
            txtbSelctFile.Name = "txtbSelctFile";
            txtbSelctFile.Size = new Size(449, 31);
            txtbSelctFile.TabIndex = 2;
            // 
            // btnCopyFile
            // 
            btnCopyFile.Location = new Point(451, 550);
            btnCopyFile.Name = "btnCopyFile";
            btnCopyFile.Size = new Size(105, 57);
            btnCopyFile.TabIndex = 4;
            btnCopyFile.Text = " 복사";
            btnCopyFile.UseVisualStyleBackColor = true;
            // 
            // btnAddFile
            // 
            btnAddFile.Location = new Point(910, 545);
            btnAddFile.Name = "btnAddFile";
            btnAddFile.Size = new Size(105, 57);
            btnAddFile.TabIndex = 5;
            btnAddFile.Text = "파일추가";
            btnAddFile.UseVisualStyleBackColor = true;
            // 
            // btnSelctFile
            // 
            btnSelctFile.Location = new Point(468, 14);
            btnSelctFile.Name = "btnSelctFile";
            btnSelctFile.Size = new Size(87, 47);
            btnSelctFile.TabIndex = 6;
            btnSelctFile.Text = "파일선택";
            btnSelctFile.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(1019, 545);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(105, 57);
            btnClose.TabIndex = 7;
            btnClose.Text = "닫기";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // frmAddFile
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1137, 619);
            Controls.Add(btnClose);
            Controls.Add(btnSelctFile);
            Controls.Add(btnAddFile);
            Controls.Add(pnlAddFile);
            Controls.Add(btnCopyFile);
            Controls.Add(txtbSelctFile);
            Controls.Add(pnlCopyFile);
            Location = new Point(200, 200);
            Name = "frmAddFile";
            StartPosition = FormStartPosition.CenterParent;
            Text = "로컬 파일을 프로그램에 업로드";
            pnlCopyFile.ResumeLayout(false);
            pnlAddFile.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlCopyFile;
        private Panel pnlAddFile;
        private Button btnCopyFile;
        private TextBox txtbSelctFile;
        private Button btnAddFile;
        private ListView lstviewAddFile;
        private ListView lstviewCopyFile;
        private Button btnSelctFile;
        private Button btnClose;
    }
}