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
            panelTop = new Panel();
            btnModelLoad = new Button();
            lblModelListTitle = new Label();
            lvModelList = new ListView();
            colNo = new ColumnHeader();
            colName = new ColumnHeader();
            colPath = new ColumnHeader();
            panelBottom = new Panel();
            btnResetFilter = new Button();
            btnModelFliter = new Button();
            txtModelFilter = new TextBox();
            panelTop.SuspendLayout();
            panelBottom.SuspendLayout();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.Controls.Add(btnModelLoad);
            panelTop.Controls.Add(lblModelListTitle);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Padding = new Padding(12);
            panelTop.Size = new Size(760, 64);
            panelTop.TabIndex = 0;
            // 
            // btnModelLoad
            // 
            btnModelLoad.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnModelLoad.Location = new Point(620, 14);
            btnModelLoad.Name = "btnModelLoad";
            btnModelLoad.Size = new Size(128, 36);
            btnModelLoad.TabIndex = 1;
            btnModelLoad.Text = "불러오기";
            btnModelLoad.UseVisualStyleBackColor = true;
            // 
            // lblModelListTitle
            // 
            lblModelListTitle.AutoSize = true;
            lblModelListTitle.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            lblModelListTitle.Location = new Point(14, 20);
            lblModelListTitle.Name = "lblModelListTitle";
            lblModelListTitle.Size = new Size(138, 21);
            lblModelListTitle.TabIndex = 0;
            lblModelListTitle.Text = "모델 파일 선택";
            // 
            // lvModelList
            // 
            lvModelList.Columns.AddRange(new ColumnHeader[] { colNo, colName, colPath });
            lvModelList.Dock = DockStyle.Fill;
            lvModelList.FullRowSelect = true;
            lvModelList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvModelList.Location = new Point(0, 64);
            lvModelList.MultiSelect = false;
            lvModelList.Name = "lvModelList";
            lvModelList.Size = new Size(760, 529);
            lvModelList.TabIndex = 1;
            lvModelList.UseCompatibleStateImageBehavior = false;
            lvModelList.View = View.Details;
            // 
            // colNo
            // 
            colNo.Text = "번호";
            colNo.Width = 70;
            // 
            // colName
            // 
            colName.Text = "모델 이름";
            colName.Width = 260;
            // 
            // colPath
            // 
            colPath.Text = "경로";
            colPath.Width = 420;
            // 
            // panelBottom
            // 
            panelBottom.Controls.Add(btnResetFilter);
            panelBottom.Controls.Add(btnModelFliter);
            panelBottom.Controls.Add(txtModelFilter);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(0, 593);
            panelBottom.Name = "panelBottom";
            panelBottom.Padding = new Padding(12);
            panelBottom.Size = new Size(760, 68);
            panelBottom.TabIndex = 2;
            // 
            // btnResetFilter
            // 
            btnResetFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnResetFilter.Location = new Point(548, 16);
            btnResetFilter.Name = "btnResetFilter";
            btnResetFilter.Size = new Size(96, 36);
            btnResetFilter.TabIndex = 1;
            btnResetFilter.Text = "초기화";
            btnResetFilter.UseVisualStyleBackColor = true;
            // 
            // btnModelFliter
            // 
            btnModelFliter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnModelFliter.Location = new Point(652, 16);
            btnModelFliter.Name = "btnModelFliter";
            btnModelFliter.Size = new Size(96, 36);
            btnModelFliter.TabIndex = 2;
            btnModelFliter.Text = "검색";
            btnModelFliter.UseVisualStyleBackColor = true;
            // 
            // txtModelFilter
            // 
            txtModelFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtModelFilter.Font = new Font("맑은 고딕", 13F);
            txtModelFilter.Location = new Point(12, 18);
            txtModelFilter.Name = "txtModelFilter";
            txtModelFilter.Size = new Size(528, 31);
            txtModelFilter.TabIndex = 0;
            // 
            // PliotModelList
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(760, 661);
            Controls.Add(lvModelList);
            Controls.Add(panelBottom);
            Controls.Add(panelTop);
            Font = new Font("맑은 고딕", 11.25F);
            MinimumSize = new Size(640, 520);
            Name = "PliotModelList";
            StartPosition = FormStartPosition.CenterParent;
            Text = "모델선택";
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelBottom.ResumeLayout(false);
            panelBottom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelTop;
        private Button btnModelLoad;
        private Label lblModelListTitle;
        private ListView lvModelList;
        private ColumnHeader colNo;
        private ColumnHeader colName;
        private ColumnHeader colPath;
        private Panel panelBottom;
        private Button btnResetFilter;
        private Button btnModelFliter;
        private TextBox txtModelFilter;
    }
}
