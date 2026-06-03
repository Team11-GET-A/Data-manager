using System.Windows.Forms;

namespace AD_AI_LearningData_Editor
{
    // 프로젝트 전체에서 동일한 지연 시간/표시 시간을 갖는 ToolTip을 만들기 위한 작은 헬퍼입니다.
    public static class propToolTip
    {
        public static ToolTip CreateDefaultToolTip()
        {
            ToolTip toolTip = new ToolTip();

            toolTip.AutoPopDelay = 5000;   // 표시 유지 시간
            toolTip.InitialDelay = 300;   // 마우스를 올린 뒤 표시까지 시간
            toolTip.ReshowDelay = 300;     // 다른 컨트롤로 이동했을 때 표시 지연
            toolTip.ShowAlways = true;

            return toolTip;
        }

        public static void Set(Control control, string text)
        {
            if (control == null) return;

            ToolTip toolTip = CreateDefaultToolTip();
            toolTip.SetToolTip(control, text);
        }

        public static void Set(ToolTip toolTip, Control control, string text)
        {
            if (toolTip == null || control == null) return;

            toolTip.SetToolTip(control, text);
        }
    }
}
