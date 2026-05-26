using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Data_Manager
{
    public class PilotCardControl : UserControl
    {
        // UI Controls
        private Label lblModelNameTitle;
        private Label lblModelName;
        private Button btnModelSelect;
        private Button btnDelete;
        private PictureBox picDriveView;
        private Panel pnlHeader;
        private Panel pnlThrottle;

        private Label lblDriveThrottleTitle;
        private ProgressBar pbDriveThrottle;
        private Label lblDriveThrottleVal;
        private Label lblDriveThrottleDir;

        private Label lblModelThrottleTitle;
        private ProgressBar pbModelThrottle;
        private Label lblModelThrottleVal;
        private Label lblModelThrottleDir;

        // Data Storage
        private double driveAngle = 0;
        private double modelAngle = 0;
        private double driveThrottle = 0;
        private double modelThrottle = 0;

        // Events
        public event Action<PilotCardControl> RemoveRequested;
        public event Action<PilotCardControl> ModelSelectRequested;

        public PilotCardControl()
        {
            BuildUi();
        }

        private void BuildUi()
        {
            this.Size = new Size(386, 542);
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Margin = new Padding(0);

            // 1. 헤더 영역 구성
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = Color.WhiteSmoke
            };

            lblModelNameTitle = new Label { Text = "모델 이름:", Location = new Point(10, 12), AutoSize = true, Font = new Font("맑은 고딕", 9, FontStyle.Bold) };
            lblModelName = new Label { Text = "선택 안 됨", Location = new Point(80, 12), AutoSize = true, Font = new Font("맑은 고딕", 9) };

            btnModelSelect = new Button { Text = "모델 선택", Size = new Size(95, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnModelSelect.Click += (s, e) => ModelSelectRequested?.Invoke(this);

            btnDelete = new Button { Text = "X", ForeColor = Color.Red, Size = new Size(30, 25), FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += (s, e) => RemoveRequested?.Invoke(this);

            pnlHeader.Controls.Add(lblModelNameTitle);
            pnlHeader.Controls.Add(lblModelName);
            pnlHeader.Controls.Add(btnModelSelect);
            pnlHeader.Controls.Add(btnDelete);

            // 2. PictureBox (주행 뷰) 영역 구성
            picDriveView = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 270,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.LightGray
            };
            // Paint 이벤트 연동 (방향 선 및 텍스트 그리기)
            picDriveView.Paint += PicDriveView_Paint;

            // 3. Throttle (스로틀 비교) 영역 구성
            pnlThrottle = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // 첫 번째 줄: 주행데이터 Throttle
            lblDriveThrottleTitle = new Label { Text = "주행값 Throttle", Location = new Point(10, 15), Size = new Size(120, 20) };
            pbDriveThrottle = new ProgressBar { Location = new Point(130, 15), Size = new Size(120, 20), Maximum = 100 };
            lblDriveThrottleVal = new Label { Text = "0.00", Location = new Point(260, 15), Size = new Size(50, 20) };
            lblDriveThrottleDir = new Label { Text = "-", Location = new Point(320, 15), Size = new Size(50, 20) };

            // 두 번째 줄: 모델판단 Throttle
            lblModelThrottleTitle = new Label { Text = "학습값 Throttle", Location = new Point(10, 55), Size = new Size(120, 20) };
            pbModelThrottle = new ProgressBar { Location = new Point(130, 55), Size = new Size(120, 20), Maximum = 100 };
            lblModelThrottleVal = new Label { Text = "0.00", Location = new Point(260, 55), Size = new Size(50, 20) };
            lblModelThrottleDir = new Label { Text = "-", Location = new Point(320, 55), Size = new Size(50, 20) };

            pnlThrottle.Controls.AddRange(new Control[] {
                lblDriveThrottleTitle, pbDriveThrottle, lblDriveThrottleVal, lblDriveThrottleDir,
                lblModelThrottleTitle, pbModelThrottle, lblModelThrottleVal, lblModelThrottleDir
            });

            this.Controls.Add(pnlThrottle);
            this.Controls.Add(picDriveView);
            this.Controls.Add(pnlHeader);

            UpdateLayoutSizes();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateLayoutSizes();
        }

        private void UpdateLayoutSizes()
        {
            int width = ClientSize.Width;
            int height = ClientSize.Height;

            if (width <= 0)
            {
                return;
            }

            if (btnDelete == null || btnModelSelect == null || lblDriveThrottleTitle == null || pbDriveThrottle == null
                || lblDriveThrottleVal == null || lblDriveThrottleDir == null || lblModelThrottleTitle == null
                || pbModelThrottle == null || lblModelThrottleVal == null || lblModelThrottleDir == null)
            {
                return;
            }

            int headerPadding = 10;
            btnDelete.Location = new Point(width - btnDelete.Width - headerPadding, 13);
            btnModelSelect.Location = new Point(btnDelete.Left - btnModelSelect.Width - 15, 13);

            int headerHeight = Math.Max(50, (int)Math.Round(height * 0.1));
            int pictureHeight = Math.Max(230, (int)Math.Round(height * 0.56));
            int throttleHeight = Math.Max(140, height - headerHeight - pictureHeight);

            pnlHeader.Height = headerHeight;
            picDriveView.Height = pictureHeight;
            pnlThrottle.Height = throttleHeight;

            int padding = 10;
            int titleWidth = Math.Max(110, (int)Math.Round(width * 0.28));
            int valueWidth = Math.Max(50, (int)Math.Round(width * 0.12));
            int dirWidth = Math.Max(50, (int)Math.Round(width * 0.12));
            int progressWidth = Math.Max(80, width - (padding * 2 + titleWidth + valueWidth + dirWidth + 10));

            int xTitle = padding;
            int xProgress = xTitle + titleWidth;
            int xValue = xProgress + progressWidth + 5;
            int xDir = xValue + valueWidth + 5;

            lblDriveThrottleTitle.Location = new Point(xTitle, 25);
            lblDriveThrottleTitle.Size = new Size(titleWidth, 20);
            pbDriveThrottle.Location = new Point(xProgress, 25);
            pbDriveThrottle.Size = new Size(progressWidth, 20);
            lblDriveThrottleVal.Location = new Point(xValue, 25);
            lblDriveThrottleVal.Size = new Size(valueWidth, 20);
            lblDriveThrottleDir.Location = new Point(xDir, 25);
            lblDriveThrottleDir.Size = new Size(dirWidth, 20);

            lblModelThrottleTitle.Location = new Point(xTitle, 75);
            lblModelThrottleTitle.Size = new Size(titleWidth, 20);
            pbModelThrottle.Location = new Point(xProgress, 75);
            pbModelThrottle.Size = new Size(progressWidth, 20);
            lblModelThrottleVal.Location = new Point(xValue, 75);
            lblModelThrottleVal.Size = new Size(valueWidth, 20);
            lblModelThrottleDir.Location = new Point(xDir, 75);
            lblModelThrottleDir.Size = new Size(dirWidth, 20);
        }

        private void PicDriveView_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = picDriveView.Width;
            int h = picDriveView.Height;

            // 이미지가 없을 때 기본 텍스트
            if (picDriveView.Image == null)
            {
                string txt = "이미지 없음";
                using (Font font = new Font("맑은 고딕", 14, FontStyle.Bold))
                {
                    var size = g.MeasureString(txt, font);
                    g.DrawString(txt, font, Brushes.Gray, (w - size.Width) / 2, (h - size.Height) / 2);
                }
            }

            // 하단 정중앙 기준점
            PointF origin = new PointF(w / 2f, h);
            float length = h * 0.7f;

            // 선 그리기 (겹치지 않게 Drive를 먼저, Model을 나중에 얇게)
            DrawAngleLine(g, origin, length, driveAngle, Color.White, 4);
            DrawAngleLine(g, origin, length, modelAngle, Color.Yellow, 2); 

            // 오버레이 텍스트 반투명 박스 그리기
            string dTxt = $"주행값 Angle: {driveAngle:F1}";
            string mTxt = $"학습값 Angle: {modelAngle:F1}";

            using (Font f = new Font("맑은 고딕", 9, FontStyle.Bold))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
            {
                SizeF dSize = g.MeasureString(dTxt, f);
                SizeF mSize = g.MeasureString(mTxt, f);

                g.FillRectangle(bg, 5, h - dSize.Height - 10, dSize.Width + 10, dSize.Height + 5);
                g.FillRectangle(bg, w - mSize.Width - 15, h - mSize.Height - 10, mSize.Width + 10, mSize.Height + 5);

                g.DrawString(dTxt, f, Brushes.White, 10, h - dSize.Height - 7);
                g.DrawString(mTxt, f, Brushes.White, w - mSize.Width - 10, h - mSize.Height - 7);
            }
        }

        private void DrawAngleLine(Graphics g, PointF origin, float length, double angle, Color color, float thickness)
        {
            // 상단 방향을 기준으로 하기 위해 -90도 보정
            double rad = (angle - 90) * Math.PI / 180.0;
            float x = origin.X + (float)(Math.Cos(rad) * length);
            float y = origin.Y + (float)(Math.Sin(rad) * length);

            using (Pen p = new Pen(color, thickness))
            {
                g.DrawLine(p, origin.X, origin.Y, x, y);
            }
        }

        // --- 외부 접근용 public 메서드 ---

        public void SetModelName(string modelName)
        {
            lblModelName.Text = modelName;
        }

        public void SetAngles(double driveAng, double modelAng)
        {
            this.driveAngle = driveAng;
            this.modelAngle = modelAng;
            picDriveView.Invalidate(); // 변경 후 재도장
        }

        public void SetThrottles(double driveThr, double modelThr)
        {
            this.driveThrottle = driveThr;
            this.modelThrottle = modelThr;

            pbDriveThrottle.Value = Math.Min(100, (int)(Math.Abs(driveThr) * 100));
            lblDriveThrottleVal.Text = driveThr.ToString("F2");
            lblDriveThrottleDir.Text = driveThr >= 0 ? "전진" : "후진";

            pbModelThrottle.Value = Math.Min(100, (int)(Math.Abs(modelThr) * 100));
            lblModelThrottleVal.Text = modelThr.ToString("F2");
            lblModelThrottleDir.Text = modelThr >= 0 ? "전진" : "후진";
        }

        public void SetDriveImage(Image image)
        {
            picDriveView.Image = image;
        }

        public void ClearDriveImage()
        {
            picDriveView.Image = null;
        }
    }
}