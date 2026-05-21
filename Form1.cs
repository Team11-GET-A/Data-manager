using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace AD_AI_LearningData_Editor
{
    public partial class frmMain : MaterialForm
    {
        public frmMain()
        {
            InitializeComponent();

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            //materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;

            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.BlueGrey800, Primary.BlueGrey900,
                Primary.BlueGrey500, Accent.Red400,
                TextShade.WHITE
            );

            SetupTabs();

            // lstviewMain의 더블클릭 이벤트를 코드로 명시적 연결합니다.
            this.lstviewMain.MouseDoubleClick += lstviewMain_MouseDoubleClick;
        }

        private void SetupTabs()
        {
            MaterialTabControl tabControl = new MaterialTabControl();
            tabControl.Dock = DockStyle.Fill;

            TabPage tabManager = new TabPage("매니저");
            TabPage tabTrainer = new TabPage("트레이너");
            TabPage tabPilot = new TabPage("파일럿");

            tabControl.Controls.Add(tabManager);
            tabControl.Controls.Add(tabTrainer);
            tabControl.Controls.Add(tabPilot);

            // 기존 Form1의 컨트롤들을 '매니저' 탭으로 이동시킵니다.
            var controlsToMove = new System.Collections.Generic.List<Control>();
            foreach (Control c in this.Controls)
            {
                if (c != tabControl && c.Name != "DrawerTabControl")
                {
                    controlsToMove.Add(c);
                }
            }
            foreach (Control c in controlsToMove)
            {
                tabManager.Controls.Add(c);
            }

            this.Controls.Add(tabControl);

            // Form3을 '트레이너' 탭에 추가
            Data_Manager.Form3 form3 = new Data_Manager.Form3();
            form3.TopLevel = false;
            form3.FormBorderStyle = FormBorderStyle.None;
            form3.Dock = DockStyle.Fill;
            tabTrainer.Controls.Add(form3);
            form3.Show();

            // Form2를 '파일럿' 탭에 추가
            Data_Manager.Form2 form2 = new Data_Manager.Form2();
            form2.TopLevel = false;
            form2.FormBorderStyle = FormBorderStyle.None;
            form2.Dock = DockStyle.Fill;
            tabPilot.Controls.Add(form2);
            form2.Show();

            // MaterialSkin의 Form 상단 탭이나 사이드메뉴에 연결하려면 아래 속성을 설정합니다.
            this.DrawerTabControl = tabControl;
        }

        private void materialSlider1_Click(object sender, EventArgs e)
        {

        }

        private void materialButton2_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// btnOpnFolderList 버튼 클릭 시 실행되는 메서드입니다.
        /// 패널 내의 모든 리스트뷰를 숨긴 후 lstviewMain만 노출합니다.
        /// </summary>
        private void btnOpnFolderList1_Click(object sender, EventArgs e)
        {
            lstviewFileList.Visible = false;
            lstviewTrash.Visible = false;
            lstviewMain.Visible = true;
        }

        /// <summary>
        /// lstviewMain의 아이템을 더블클릭했을 때 실행되는 메서드입니다.
        /// </summary>
        private void lstviewMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstviewMain.SelectedItems.Count > 0)
            {
                // 텍스트가 아니라 '태그'에 적힌 글자를 가져옵니다.
                string itemTag = lstviewMain.SelectedItems[0].Tag?.ToString();

                // 이제 태그를 비교하므로, 화면에 보이는 텍스트가 바뀌어도 이 조건문은 항상 참이 됩니다.
                if (itemTag == "파일목록")
                {
                    lstviewMain.Visible = false;
                    lstviewFileList.Visible = true;
                    lstviewTrash.Visible = false;
                }
                else if (itemTag == "휴지통")
                {
                    lstviewMain.Visible = false;
                    lstviewFileList.Visible = false;
                    lstviewTrash.Visible = true;
                }
            }
        }

        private void lstviewFileList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}