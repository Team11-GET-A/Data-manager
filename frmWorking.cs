using System;
using System.ComponentModel; // DesignerSerializationVisibility를 사용하기 위해 추가
using System.Threading;
using System.Windows.Forms;

namespace Data_Manager
{
    public partial class frmWoking : Form
    {
        // 윈폼 디자이너가 이 속성을 직렬화하지 않도록 숨김 처리
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CancellationTokenSource Cts { get; set; }

        public frmWoking()
        {
            InitializeComponent();

            // 초기 버튼 상태 설정
            btnDone.Visible = false;
            btnCancle.Visible = true;

            // 이벤트 연결
            btnCancle.Click += btnCancle_Click;
            btnDone.Click += btnDone_Click;
        }

        // 1. 프로그레스바 진행도 갱신
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

        // 3. 작업 완료 시 버튼 상태 전환
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

        // 2. 취소 버튼 클릭 이벤트 (CS8622 해결을 위해 object?, EventArgs? 사용 및 소문자 메서드명 적용)
        private void btnCancle_Click(object? sender, EventArgs? e)
        {
            if (Cts != null && !Cts.IsCancellationRequested)
            {
                btnCancle.Enabled = false; // 중복 클릭 방지
                Cts.Cancel();
            }
        }

        // 4. 완료 버튼 클릭 이벤트
        private void btnDone_Click(object? sender, EventArgs? e)
        {
            this.Close();
        }
    }
}