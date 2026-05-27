namespace Data_Manager
{
    partial class frmWoking
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
            txtbWait = new TextBox();
            pgbarWorking = new MaterialSkin.Controls.MaterialProgressBar();
            btnDone = new Button();
            btnCancle = new Button();
            SuspendLayout();
            // 
            // txtbWait
            // 
            txtbWait.BorderStyle = BorderStyle.None;
            txtbWait.Enabled = false;
            txtbWait.Font = new Font("맑은 고딕", 16F);
            txtbWait.Location = new Point(27, 24);
            txtbWait.Multiline = true;
            txtbWait.Name = "txtbWait";
            txtbWait.Size = new Size(262, 64);
            txtbWait.TabIndex = 0;
            txtbWait.TabStop = false;
            txtbWait.Text = "작업 중입니다.\r\n잠시만 기다려 주세요.";
            txtbWait.TextAlign = HorizontalAlignment.Center;
            // 
            // pgbarWorking
            // 
            pgbarWorking.Depth = 0;
            pgbarWorking.Location = new Point(21, 99);
            pgbarWorking.MouseState = MaterialSkin.MouseState.HOVER;
            pgbarWorking.Name = "pgbarWorking";
            pgbarWorking.Size = new Size(300, 5);
            pgbarWorking.TabIndex = 2;
            // 
            // btnDone
            // 
            btnDone.Font = new Font("맑은 고딕", 11F);
            btnDone.Location = new Point(131, 115);
            btnDone.Name = "btnDone";
            btnDone.Size = new Size(75, 29);
            btnDone.TabIndex = 3;
            btnDone.Text = "완료";
            btnDone.UseVisualStyleBackColor = true;
            btnDone.Visible = false;
            // 
            // btnCancle
            // 
            btnCancle.Font = new Font("맑은 고딕", 11F);
            btnCancle.Location = new Point(130, 115);
            btnCancle.Name = "btnCancle";
            btnCancle.Size = new Size(75, 29);
            btnCancle.TabIndex = 4;
            btnCancle.Text = "취소";
            btnCancle.UseVisualStyleBackColor = true;
            btnCancle.Visible = false;
            // 
            // frmWoking
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(337, 150);
            Controls.Add(pgbarWorking);
            Controls.Add(txtbWait);
            Controls.Add(btnDone);
            Controls.Add(btnCancle);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Location = new Point(500, 500);
            Name = "frmWoking";
            StartPosition = FormStartPosition.CenterParent;
            Text = "작업 중...";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtbWait;
        private MaterialSkin.Controls.MaterialProgressBar pgbarWorking;
        private Button btnDone;
        private Button btnCancle;
    }
}