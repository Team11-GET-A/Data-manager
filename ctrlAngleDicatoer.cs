using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AD_AI_LearningData_Editor
{
    public class ctrlAngleDicatoer : Control
    {
        private double? rawAngleValue;
        private double displayDegree = 90.0;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double? RawAngleValue
        {
            get { return rawAngleValue; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double DisplayDegree
        {
            get { return displayDegree; }
        }

        public ctrlAngleDicatoer()
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

        public void SetAngleValue(double? value)
        {
            rawAngleValue = value;

            if (!value.HasValue)
            {
                displayDegree = 90.0;
                this.Invalidate();
                return;
            }

            displayDegree = ConvertAngleToDegree(value.Value);
            this.Invalidate();
        }

        private double ConvertAngleToDegree(double value)
        {
            // DonkeyCar angle 값은 보통 -1.0 ~ 1.0 범위입니다.
            // -1.0 = 0°, 0.0 = 90°, 1.0 = 180° 로 변환합니다.
            if (value >= -1.0 && value <= 1.0)
            {
                return Clamp(90.0 + (value * 90.0), 0.0, 180.0);
            }

            // 만약 이미 0~180도 값이 들어온 경우도 대응합니다.
            return Clamp(value, 0.0, 180.0);
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

            if (bounds.Width < 80 || bounds.Height < 50)
            {
                return;
            }

            float centerX = bounds.Width / 2f;
            float centerY = bounds.Height - 24f;
            float radius = Math.Min(bounds.Width * 0.43f, bounds.Height * 0.82f);
            float yScale = 0.56f;

            using (Pen shadowPen = new Pen(Color.FromArgb(100, 20, 20, 20), 9f))
            using (Pen arcPen = new Pen(Color.FromArgb(235, 255, 255, 255), 6f))
            using (Pen innerPen = new Pen(Color.FromArgb(110, 255, 255, 255), 2f))
            {
                shadowPen.StartCap = LineCap.Round;
                shadowPen.EndCap = LineCap.Round;
                arcPen.StartCap = LineCap.Round;
                arcPen.EndCap = LineCap.Round;
                innerPen.StartCap = LineCap.Round;
                innerPen.EndCap = LineCap.Round;

                PointF[] shadowArc = BuildArcPoints(centerX + 3f, centerY + 7f, radius, yScale, 0, 180);
                PointF[] mainArc = BuildArcPoints(centerX, centerY, radius, yScale, 0, 180);
                PointF[] innerArc = BuildArcPoints(centerX, centerY, radius * 0.82f, yScale, 0, 180);

                if (shadowArc.Length > 1) g.DrawLines(shadowPen, shadowArc);
                if (mainArc.Length > 1) g.DrawLines(arcPen, mainArc);
                if (innerArc.Length > 1) g.DrawLines(innerPen, innerArc);
            }

            DrawTicks(g, centerX, centerY, radius, yScale);
            DrawLabels(g, centerX, centerY, radius, yScale);
            DrawArrow(g, centerX, centerY, radius, yScale);
        }

        private PointF[] BuildArcPoints(float centerX, float centerY, float radius, float yScale, int startDegree, int endDegree)
        {
            int count = Math.Max(2, endDegree - startDegree + 1);
            PointF[] points = new PointF[count];

            int index = 0;
            for (int degree = startDegree; degree <= endDegree; degree++)
            {
                points[index++] = GetPointOnGauge(centerX, centerY, radius, yScale, degree);
            }

            return points;
        }

        private PointF GetPointOnGauge(float centerX, float centerY, float radius, float yScale, double degree)
        {
            double theta = Math.PI - (degree * Math.PI / 180.0);
            float x = centerX + (float)(Math.Cos(theta) * radius);
            float y = centerY - (float)(Math.Sin(theta) * radius * yScale);
            return new PointF(x, y);
        }

        private void DrawTicks(Graphics g, float centerX, float centerY, float radius, float yScale)
        {
            using (Pen majorPen = new Pen(Color.FromArgb(235, 255, 255, 255), 2.2f))
            using (Pen minorPen = new Pen(Color.FromArgb(170, 255, 255, 255), 1.2f))
            {
                for (int degree = 0; degree <= 180; degree += 10)
                {
                    bool isMajor = degree % 30 == 0;
                    float outerR = radius * 0.98f;
                    float innerR = isMajor ? radius * 0.83f : radius * 0.90f;

                    PointF outer = GetPointOnGauge(centerX, centerY, outerR, yScale, degree);
                    PointF inner = GetPointOnGauge(centerX, centerY, innerR, yScale, degree);

                    g.DrawLine(isMajor ? majorPen : minorPen, inner, outer);
                }
            }
        }

        private void DrawLabels(Graphics g, float centerX, float centerY, float radius, float yScale)
        {
            using (Font font = new Font("맑은 고딕", 9.5f, FontStyle.Bold, GraphicsUnit.Point))
            using (Brush textBrush = new SolidBrush(Color.FromArgb(235, 255, 255, 255)))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                DrawLabelAtDegree(g, "0°", font, textBrush, sf, centerX, centerY, radius * 1.10f, yScale, 0, 0, 2);
                DrawLabelAtDegree(g, "90°", font, textBrush, sf, centerX, centerY, radius * 1.05f, yScale, 90, 0, -4);
                DrawLabelAtDegree(g, "180°", font, textBrush, sf, centerX, centerY, radius * 1.10f, yScale, 180, 3, 2);
            }
        }

        private void DrawLabelAtDegree(Graphics g, string text, Font font, Brush brush, StringFormat sf,
            float centerX, float centerY, float radius, float yScale, int degree, float offsetX, float offsetY)
        {
            PointF p = GetPointOnGauge(centerX, centerY, radius, yScale, degree);
            RectangleF rect = new RectangleF(p.X - 28f + offsetX, p.Y - 12f + offsetY, 56f, 24f);
            g.DrawString(text, font, brush, rect, sf);
        }

        private void DrawArrow(Graphics g, float centerX, float centerY, float radius, float yScale)
        {
            PointF start = new PointF(centerX, centerY + 2f);
            PointF end = GetPointOnGauge(centerX, centerY, radius * 0.68f, yScale, displayDegree);

            using (Pen shadowPen = new Pen(Color.FromArgb(120, 20, 20, 20), 9f))
            using (Pen arrowPen = new Pen(Color.FromArgb(245, 220, 30, 30), 6f))
            using (SolidBrush centerBrush = new SolidBrush(Color.FromArgb(245, 220, 30, 30)))
            using (SolidBrush centerShadowBrush = new SolidBrush(Color.FromArgb(100, 20, 20, 20)))
            {
                using (AdjustableArrowCap arrowCap = new AdjustableArrowCap(5f, 8f, true))
                {
                    shadowPen.CustomEndCap = arrowCap;
                    shadowPen.StartCap = LineCap.Round;

                    arrowPen.CustomEndCap = arrowCap;
                    arrowPen.StartCap = LineCap.Round;

                    g.DrawLine(shadowPen, new PointF(start.X + 3f, start.Y + 4f), new PointF(end.X + 3f, end.Y + 4f));
                    g.DrawLine(arrowPen, start, end);
                }

                g.FillEllipse(centerShadowBrush, centerX - 10f + 2f, centerY - 10f + 3f, 20f, 20f);
                g.FillEllipse(centerBrush, centerX - 9f, centerY - 9f, 18f, 18f);
            }
        }
    }
}
