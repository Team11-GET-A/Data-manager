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
        private const int MaxRetries = 60;
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

                lblStatus.Text = "Donkey Car 서버에 연결 중...";
                Application.DoEvents();

                // 로컬호스트 대기
                if (await WaitForServerReadyAsync())
                {
                    webView.Source = new Uri(LocalHostUrl);
                    lblStatus.Text = "연결됨: " + LocalHostUrl;
                }
                else
                {
                    lblStatus.Text = "서버 연결 실패";
                    MessageBox.Show(
                        "Donkey Car 서버가 응답하지 않습니다.\n\n자율주행이 제대로 실행되었는지 확인하세요.",
                        "서버 연결 실패",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "오류: " + ex.Message;
                MessageBox.Show(
                    $"WebView2 초기화 실패:\\n{ex.Message}",
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
                        var response = await client.GetAsync(LocalHostUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                    // 서버가 준비되지 않음, 재시도
                }

                await Task.Delay(RetryDelayMs);
                lblStatus.Text = $"서버 대기 중... ({i + 1}/{MaxRetries})";
                Application.DoEvents();
            }

            return false;
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (webView != null)
            {
                webView.Reload();
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
