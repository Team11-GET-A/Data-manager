using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace Data_Manager
{
    public partial class Form2 : Form
    {
        private int pilotCardCount = 0;

        public Form2()
        {
            InitializeComponent();
        }

        private void BtnCardAdder_Click(object sender, EventArgs e)
        {
            if (flowLayoutPanel1.Controls.Count >= 4)
            {
                MessageBox.Show("파일럿 카드는 최대 4개까지만 추가할 수 있습니다.", "추가 제한", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 새 PilotCardControl 생성
            PilotCardControl card = new PilotCardControl();
            pilotCardCount++;

            // 샘플 데이터 셋업 (카드 개수에 따라 조금씩 변화)
            card.SetModelName($"테스트 모델 {pilotCardCount}");
            card.SetAngles(-15.2 + pilotCardCount * 2, -12.6 + pilotCardCount * 1.5);
            card.SetThrottles(0.72 - (pilotCardCount * 0.1), 0.65 - (pilotCardCount * 0.05));

            // 이벤트 구독
            card.RemoveRequested += Card_RemoveRequested;
            card.ModelSelectRequested += Card_ModelSelectRequested;

            flowLayoutPanel1.Controls.Add(card);
        }

        private void Card_RemoveRequested(PilotCardControl card)
        {
            flowLayoutPanel1.Controls.Remove(card);
            card.Dispose(); // 메모리 해제
        }

        private void Card_ModelSelectRequested(PilotCardControl card)
        {
            // 나중에 모델 선택 로직 구현
            MessageBox.Show("추후 모델 선택 로직이 연동될 예정입니다.", "모델 선택", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void materialLabel1_Click(object sender, EventArgs e)
        {

        }
    }
}
