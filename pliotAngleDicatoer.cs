using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Data_Manager
{
    // Pilot screen angle gauge that draws both tub driving data and AI judgment data.
    public class pliotAngleDicatoer : Control
    {
        private double? tubAngle;
        private double? aiAngle;

        public pliotAngleDicatoer()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor = Color.Transparent;
            ForeColor = Color.White;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double? TubAngle => tubAngle;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double? AiAngle => aiAngle;

        public void SetAngleValues(double? tubValue, double? aiValue)
        {
            tubAngle = ClampNullable(tubValue);
            aiAngle = ClampNullable(aiValue);
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            PaintTransparentBackground(pevent);
        }

        private void PaintTransparentBackground(PaintEventArgs e)
        {
            if (Parent == null)
            {
                base.OnPaintBackground(e);
                return;
            }

            GraphicsState state = e.Graphics.Save();

            try
            {
                e.Graphics.TranslateTransform(-Left, -Top);
                Rectangle parentClip = new Rectangle(Left, Top, Width, Height);
                using PaintEventArgs parentPaintArgs = new PaintEventArgs(e.Graphics, parentClip);
                InvokePaintBackground(Parent, parentPaintArgs);
                InvokePaint(Parent, parentPaintArgs);
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

            Rectangle bounds = ClientRectangle;
            if (bounds.Width < 180 || bounds.Height < 90)
            {
                return;
            }

            DrawPanelBackground(g, bounds);

            float centerX = bounds.Width / 2f;
            float centerY = bounds.Height - 78f;
            float radius = Math.Min(bounds.Width * 0.40f, bounds.Height * 0.62f);
            float yScale = 0.50f;

            using Pen arcPen = new Pen(Color.FromArgb(230, 255, 255, 255), 5.5f);
            using Pen innerPen = new Pen(Color.FromArgb(105, 255, 255, 255), 1.8f);
            using Pen axisPen = new Pen(Color.FromArgb(90, 255, 255, 255), 1.6f);
            arcPen.StartCap = LineCap.Round;
            arcPen.EndCap = LineCap.Round;
            innerPen.StartCap = LineCap.Round;
            innerPen.EndCap = LineCap.Round;

            PointF[] mainArc = BuildArcPoints(centerX, centerY, radius, yScale, 0, 180);
            PointF[] innerArc = BuildArcPoints(centerX, centerY, radius * 0.82f, yScale, 0, 180);
            if (mainArc.Length > 1) g.DrawLines(arcPen, mainArc);
            if (innerArc.Length > 1) g.DrawLines(innerPen, innerArc);
            g.DrawLine(axisPen, centerX - radius, centerY, centerX + radius, centerY);

            DrawTicks(g, centerX, centerY, radius, yScale);
            DrawLabels(g, bounds);
            DrawArrow(g, centerX, centerY, radius, yScale, tubAngle, Color.FromArgb(250, 235, 40, 40), 0f);
            DrawArrow(g, centerX, centerY, radius, yScale, aiAngle, Color.FromArgb(250, 55, 145, 255), -6f);
        }

        private void DrawPanelBackground(Graphics g, Rectangle bounds)
        {
            Rectangle panelRect = new Rectangle(0, 0, bounds.Width - 1, bounds.Height - 1);
            using GraphicsPath path = RoundedRect(panelRect, 8);
            using SolidBrush brush = new SolidBrush(Color.FromArgb(165, 22, 26, 32));
            using Pen outline = new Pen(Color.FromArgb(135, 255, 255, 255), 1f);
            g.FillPath(brush, path);
            g.DrawPath(outline, path);
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static PointF[] BuildArcPoints(float centerX, float centerY, float radius, float yScale, int startDegree, int endDegree)
        {
            PointF[] points = new PointF[Math.Max(2, endDegree - startDegree + 1)];
            int index = 0;
            for (int degree = startDegree; degree <= endDegree; degree++)
            {
                points[index++] = GetPointOnGauge(centerX, centerY, radius, yScale, degree);
            }

            return points;
        }

        private static PointF GetPointOnGauge(float centerX, float centerY, float radius, float yScale, double degree)
        {
            double theta = Math.PI - (degree * Math.PI / 180.0);
            float x = centerX + (float)(Math.Cos(theta) * radius);
            float y = centerY - (float)(Math.Sin(theta) * radius * yScale);
            return new PointF(x, y);
        }

        private static double ToDegree(double value)
        {
            double clamped = Math.Max(-1.0, Math.Min(1.0, value));
            return 90.0 + (clamped * 90.0);
        }

        private static double? ClampNullable(double? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return Math.Max(-1.0, Math.Min(1.0, value.Value));
        }

        private static void DrawTicks(Graphics g, float centerX, float centerY, float radius, float yScale)
        {
            using Pen majorPen = new Pen(Color.FromArgb(210, 255, 255, 255), 2f);
            using Pen minorPen = new Pen(Color.FromArgb(145, 255, 255, 255), 1f);
            for (int degree = 0; degree <= 180; degree += 10)
            {
                bool major = degree % 30 == 0;
                PointF outer = GetPointOnGauge(centerX, centerY, radius * 0.98f, yScale, degree);
                PointF inner = GetPointOnGauge(centerX, centerY, major ? radius * 0.84f : radius * 0.91f, yScale, degree);
                g.DrawLine(major ? majorPen : minorPen, inner, outer);
            }
        }

        private static void DrawLabels(Graphics g, Rectangle bounds)
        {
            using Font font = new Font("Segoe UI", 36f, FontStyle.Bold, GraphicsUnit.Point);
            using Font valueFont = new Font("Segoe UI", 42f, FontStyle.Bold, GraphicsUnit.Point);
            using Brush tubBrush = new SolidBrush(Color.FromArgb(255, 255, 92, 76));
            using Brush aiBrush = new SolidBrush(Color.FromArgb(255, 55, 145, 255));
            using Brush textBrush = new SolidBrush(Color.FromArgb(245, 255, 255, 255));
            using StringFormat left = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
            using StringFormat right = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            using StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("AI", font, aiBrush, new RectangleF(20f, bounds.Height - 70f, 150f, 60f), left);
            g.DrawString("TUB", font, tubBrush, new RectangleF(bounds.Width - 190f, bounds.Height - 70f, 170f, 60f), right);
            g.DrawString("ANGLE", valueFont, textBrush, new RectangleF(0f, 4f, bounds.Width, 64f), center);
        }

        private static void DrawArrow(Graphics g, float centerX, float centerY, float radius, float yScale, double? angle, Color color, float centerOffset)
        {
            if (!angle.HasValue)
            {
                return;
            }

            double degree = ToDegree(angle.Value);
            PointF start = new PointF(centerX + centerOffset, centerY + 2f);
            PointF end = GetPointOnGauge(centerX + centerOffset, centerY, radius * 0.68f, yScale, degree);

            using Pen shadowPen = new Pen(Color.FromArgb(120, 20, 20, 20), 8f);
            using Pen arrowPen = new Pen(color, 5.5f);
            using SolidBrush centerBrush = new SolidBrush(color);
            using SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(100, 20, 20, 20));
            using AdjustableArrowCap cap = new AdjustableArrowCap(5f, 8f, true);
            shadowPen.CustomEndCap = cap;
            shadowPen.StartCap = LineCap.Round;
            arrowPen.CustomEndCap = cap;
            arrowPen.StartCap = LineCap.Round;
            g.DrawLine(shadowPen, new PointF(start.X + 3f, start.Y + 4f), new PointF(end.X + 3f, end.Y + 4f));
            g.DrawLine(arrowPen, start, end);
            g.FillEllipse(shadowBrush, start.X - 9f + 2f, start.Y - 9f + 3f, 18f, 18f);
            g.FillEllipse(centerBrush, start.X - 8f, start.Y - 8f, 16f, 16f);
        }
    }
}
