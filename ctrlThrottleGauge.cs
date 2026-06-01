using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AD_AI_LearningData_Editor
{
    public class ctrlThrottleGauge : Control
    {
        private double? rawThrottleValue;
        private double normalizedThrottle;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double? RawThrottleValue
        {
            get { return rawThrottleValue; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double NormalizedThrottle
        {
            get { return normalizedThrottle; }
        }

        public ctrlThrottleGauge()
        {
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true
            );

            this.BackColor = Color.Transparent;
            this.ForeColor = Color.White;
        }

        public void SetThrottleValue(double? value)
        {
            rawThrottleValue = value;

            if (!value.HasValue)
            {
                normalizedThrottle = 0.0;
                this.Invalidate();
                return;
            }

            normalizedThrottle = Clamp(value.Value, 0.0, 1.0);
            this.Invalidate();
        }

        private double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            PaintTransparentBackground(pevent);
        }

        private void PaintTransparentBackground(PaintEventArgs e)
        {
            if (this.Parent == null)
            {
                base.OnPaintBackground(e);
                return;
            }

            GraphicsState state = e.Graphics.Save();

            try
            {
                e.Graphics.TranslateTransform(-this.Left, -this.Top);

                Rectangle parentClip = new Rectangle(this.Left, this.Top, this.Width, this.Height);

                using (PaintEventArgs parentPaintArgs = new PaintEventArgs(e.Graphics, parentClip))
                {
                    InvokePaintBackground(this.Parent, parentPaintArgs);
                    InvokePaint(this.Parent, parentPaintArgs);
                }
            }
            finally
            {
                e.Graphics.Restore(state);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle bounds = this.ClientRectangle;

            if (bounds.Width < 80 || bounds.Height < 30)
            {
                return;
            }

            RectangleF gaugeRect = new RectangleF(
                12f,
                12f,
                bounds.Width - 24f,
                bounds.Height - 24f
            );

            using (GraphicsPath fullGaugePath = BuildFullGaugePath(gaugeRect))
            {
                DrawGaugeBackground(g, fullGaugePath);
                DrawGaugeFill(g, gaugeRect, fullGaugePath);
                DrawGaugeOutline(g, fullGaugePath);
            }

            DrawThrottleText(g, gaugeRect);
        }

        private void DrawGaugeBackground(Graphics g, GraphicsPath fullGaugePath)
        {
            using (SolidBrush backgroundBrush = new SolidBrush(Color.FromArgb(45, 255, 255, 255)))
            {
                g.FillPath(backgroundBrush, fullGaugePath);
            }
        }

        private void DrawGaugeOutline(Graphics g, GraphicsPath fullGaugePath)
        {
            using (Pen outlinePen = new Pen(Color.FromArgb(175, 255, 255, 255), 1.3f))
            {
                outlinePen.LineJoin = LineJoin.Round;
                g.DrawPath(outlinePen, fullGaugePath);
            }
        }

        private void DrawGaugeFill(Graphics g, RectangleF gaugeRect, GraphicsPath fullGaugePath)
        {
            if (normalizedThrottle <= 0.0)
            {
                return;
            }

            float fillWidth = gaugeRect.Width * (float)normalizedThrottle;

            if (fillWidth <= 0f)
            {
                return;
            }

            RectangleF fillClipRect = new RectangleF(
                gaugeRect.Left,
                gaugeRect.Top - 4f,
                fillWidth,
                gaugeRect.Height + 8f
            );

            GraphicsState state = g.Save();

            try
            {
                g.SetClip(fullGaugePath, CombineMode.Replace);
                g.IntersectClip(fillClipRect);

                using (LinearGradientBrush fillBrush = new LinearGradientBrush(
                    gaugeRect,
                    Color.FromArgb(245, 50, 220, 80),
                    Color.FromArgb(245, 235, 40, 35),
                    LinearGradientMode.Horizontal))
                {
                    ColorBlend blend = new ColorBlend();
                    blend.Positions = new float[] { 0.0f, 0.55f, 1.0f };
                    blend.Colors = new Color[]
                    {
                        Color.FromArgb(245, 50, 220, 80),
                        Color.FromArgb(245, 240, 210, 40),
                        Color.FromArgb(245, 235, 40, 35)
                    };
                    fillBrush.InterpolationColors = blend;
                    g.FillPath(fillBrush, fullGaugePath);
                }
            }
            finally
            {
                g.Restore(state);
            }
        }

        private GraphicsPath BuildFullGaugePath(RectangleF rect)
        {
            int steps = Math.Max(24, (int)(rect.Width / 5f));

            PointF[] topPoints = new PointF[steps + 1];

            float bottomY = rect.Bottom;
            float minHeight = 6f;
            float maxHeight = rect.Height;

            for (int i = 0; i <= steps; i++)
            {
                float t = steps == 0 ? 0f : (float)i / steps;
                float x = rect.Left + rect.Width * t;

                // 밑변은 수평으로 고정하고, 윗변만 지수 함수처럼 위로 올라가게 합니다.
                float exponential = (float)Math.Pow(t, 1.85);
                float height = minHeight + (maxHeight - minHeight) * exponential;
                float topY = bottomY - height;

                topPoints[i] = new PointF(x, topY);
            }

            GraphicsPath path = new GraphicsPath();
            path.StartFigure();

            path.AddLine(rect.Left, bottomY, rect.Left, bottomY - minHeight);
            path.AddLines(topPoints);
            path.AddLine(rect.Right, topPoints[topPoints.Length - 1].Y, rect.Right, bottomY);
            path.AddLine(rect.Right, bottomY, rect.Left, bottomY);

            path.CloseFigure();

            return path;
        }

        private void DrawThrottleText(Graphics g, RectangleF gaugeRect)
        {
            using (Font font = new Font("맑은 고딕", 8.5f, FontStyle.Bold, GraphicsUnit.Point))
            using (Brush brush = new SolidBrush(Color.FromArgb(235, 255, 255, 255)))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
            {
                string text = rawThrottleValue.HasValue
                    ? $"THR {rawThrottleValue.Value:0.00}"
                    : "THR --";

                RectangleF textRect = new RectangleF(
                    gaugeRect.Left,
                    gaugeRect.Top,
                    gaugeRect.Width - 8f,
                    gaugeRect.Height
                );

                g.DrawString(text, font, brush, textRect, sf);
            }
        }
    }
}
