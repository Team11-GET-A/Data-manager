using System;
using System.ComponentModel; // DesignerSerializationVisibility를 사용하기 위해 추가
using System.Threading;
using System.Windows.Forms;

namespace Data_Manager
{
    public partial class frmWoking : Form
    {
        // 외부에서 전달된 작업 취소 토큰을 보관 (디자이너 직렬화 대상 제외)
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CancellationTokenSource Cts { get; set; }

        public frmWoking()
        {
            InitializeComponent();

            // 초기 버튼 상태 설정 (작업 진행 중)
            btnDone.Visible = false;
            btnCancle.Visible = true;

            // 버튼 이벤트 연결
            btnCancle.Click += btnCancle_Click;
            btnDone.Click += btnDone_Click;
        }

        // 진행률 표시 업데이트 (백그라운드 스레드 호출 대응)
        public void UpdateProgress(int percent)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => pgbarWorking.Value = percent));
            }
            else
            {
                pgbarWorking.Value = percent;
            }
        }

        // 작업 완료 시 버튼 표시 전환
        public void ShowDone()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ShowDone));
                return;
            }
            btnCancle.Visible = false;
            btnDone.Visible = true;
        }

        // 취소 요청 처리 (중복 클릭 방지 후 취소 토큰 호출)
        private void btnCancle_Click(object? sender, EventArgs? e)
        {
            if (Cts != null && !Cts.IsCancellationRequested)
            {
                btnCancle.Enabled = false; // 중복 클릭 방지
                Cts.Cancel();
            }
        }

        // 완료 버튼 클릭 시 팝업 닫기
        private void btnDone_Click(object? sender, EventArgs? e)
        {
            this.Close();
        }
    }
}