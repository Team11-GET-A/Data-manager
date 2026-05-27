using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace Data_Manager
{
    public partial class frmWoking : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CancellationTokenSource Cts { get; set; }

        public frmWoking()
        {
            InitializeComponent();

            btnDone.Visible = false;
            btnCancle.Visible = true;

            btnCancle.Click += btnCancle_Click;
            btnDone.Click += btnDone_Click;
        }

        public void UpdateProgress(int percent)
        {
            if (percent < pgbarWorking.Minimum)
            {
                percent = pgbarWorking.Minimum;
            }

            if (percent > pgbarWorking.Maximum)
            {
                percent = pgbarWorking.Maximum;
            }

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => pgbarWorking.Value = percent));
            }
            else
            {
                pgbarWorking.Value = percent;
            }
        }

        public void ShowDone()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ShowDone));
                return;
            }

            btnCancle.Visible = false;
            btnDone.Visible = true;
            txtbWait.Text = "작업이 완료되었습니다.";
        }

        private void btnCancle_Click(object sender, EventArgs e)
        {
            if (Cts != null && !Cts.IsCancellationRequested)
            {
                btnCancle.Enabled = false;
                Cts.Cancel();
            }
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Form ownerForm = this.Owner;
            this.Close();

            if (ownerForm != null && !ownerForm.IsDisposed)
            {
                ownerForm.Close();
            }
        }
    }
}
