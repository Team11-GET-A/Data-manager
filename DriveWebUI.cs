using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using System.Threading.Tasks;

namespace DonkeyDataManager
{
    public partial class DriveWebUI : Form
    {
        private WebView2 webView;
        private const string LocalHostUrl = "http://localhost:8887";
        private const string DrivePageUrl = "http://localhost:8887/drive";
        private const int MaxRetries = 120;  // 2분간 재시도
        private const int RetryDelayMs = 1000;

        public DriveWebUI()
        {
            InitializeComponent();
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                webView = new WebView2
                {
                    Dock = DockStyle.Fill
                };

                // WebView 정보 표시
                lblStatus.Text = "WebView2 초기화 중...";
                Application.DoEvents();

                // WebView2 런타임 초기화
                await webView.EnsureCoreWebView2Async(null);

                // 웹뷰를 폼에 추가 (Designer에서 생성한 패널 아래에)
                pnlWebViewContainer.Controls.Clear();
                pnlWebViewContainer.Controls.Add(webView);

                lblStatus.Text = "Donkey Car 서버에 연결 중... (localhost:8887)";
                Application.DoEvents();

                // 로컬호스트 대기
                bool serverReady = await WaitForServerReadyAsync();

                if (serverReady)
                {
                    lblStatus.Text = "서버 연결 중: " + DrivePageUrl;
                    Application.DoEvents();

                    // /drive 페이지로 네비게이트
                    webView.Source = new Uri(DrivePageUrl);
                    lblStatus.Text = "연결됨: " + DrivePageUrl;
                }
                else
                {
                    lblStatus.Text = "❌ 서버 연결 실패 (localhost:8887)";

                    MessageBox.Show(
                        "Donkey Car 서버가 응답하지 않습니다.\n\n" +
                        "다음을 확인하세요:\n" +
                        "1. WSL에서 'python manage.py drive' 실행 중인가?\n" +
                        "2. 포트 8887이 사용 가능한가?\n" +
                        "3. 네트워크 연결이 정상인가?\n\n" +
                        "WSL 터미널 로그를 확인하세요.",
                        "서버 연결 실패",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    // 실패해도 localhost:8887을 시도
                    webView.Source = new Uri(LocalHostUrl);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ 오류: " + ex.Message;
                MessageBox.Show(
                    $"WebView2 초기화 실패:\n{ex.Message}\n\n" +
                    "WebView2 런타임이 설치되어 있는지 확인하세요.\n" +
                    "https://developer.microsoft.com/en-us/microsoft-edge/webview2/",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task<bool> WaitForServerReadyAsync()
        {
            for (int i = 0; i < MaxRetries; i++)
            {
                try
                {
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(2);

                        // 먼저 /drive 페이지 확인
                        try
                        {
                            var response = await client.GetAsync(DrivePageUrl);
                            if (response.IsSuccessStatusCode)
                            {
                                return true;
                            }
                        }
                        catch
                        {
                            // /drive 페이지 실패, 기본 경로 시도
                        }

                        // 기본 localhost:8887 확인
                        try
                        {
                            var response = await client.GetAsync(LocalHostUrl);
                            if (response.IsSuccessStatusCode)
                            {
                                return true;
                            }
                        }
                        catch
                        {
                            // 계속 재시도
                        }
                    }
                }
                catch
                {
                    // 서버가 준비되지 않음, 재시도
                }

                await Task.Delay(RetryDelayMs);
                int seconds = (i + 1);
                lblStatus.Text = $"🔄 서버 대기 중... ({seconds}초)";
                Application.DoEvents();
            }

            return false;
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (webView != null)
            {
                lblStatus.Text = "🔄 새로고침 중...";
                webView.Reload();
                lblStatus.Text = "연결됨: " + DrivePageUrl;
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (webView?.CanGoBack == true)
            {
                webView.GoBack();
            }
        }

        private void BtnForward_Click(object sender, EventArgs e)
        {
            if (webView?.CanGoForward == true)
            {
                webView.GoForward();
            }
        }

        private void DriveWebUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            webView?.Dispose();
        }
    }
}
