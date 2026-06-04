namespace Data_Manager
{
    // 프로그램 시작점입니다.
    // WinForms 기본 설정을 초기화한 뒤 메인 데이터 관리 화면(frmMain)을 실행합니다.
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new AD_AI_LearningData_Editor.frmMain());
        }
    }
}
