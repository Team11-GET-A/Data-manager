using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Data_Manager
{
    // Pilot-only throttle gauge. AI and TUB share the same mirrored layout.
    public class pliotThrottleGauge : Control
    {
        private double? throttleValue;

        public pliotThrottleGauge()
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
        public double? ThrottleValue => throttleValue;

        [DefaultValue(true)]
        public bool Mirrored { get; set; } = true;

        [DefaultValue("THR")]
        public string GaugeTitle { get; set; } = "THR";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color FillColor { get; set; } = Color.FromArgb(255, 55, 145, 255);

        public void SetThrottleValue(double? value)
        {
            throttleValue = ClampNullable(value);
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
            if (bounds.Width < 90 || bounds.Height < 48)
            {
                return;
            }

            DrawPanelBackground(g, bounds);

            RectangleF gaugeRect = new RectangleF(
                12f,
                38f,
                bounds.Width - 24f,
                bounds.Height - 54f);

            using GraphicsPath fullGaugePath = BuildFullGaugePath(gaugeRect, Mirrored);
            DrawGaugeBackground(g, fullGaugePath);
            DrawGaugeFill(g, gaugeRect, fullGaugePath);
            DrawGaugeOutline(g, fullGaugePath);
            DrawThrottleText(g, bounds, gaugeRect);
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

        private void DrawGaugeBackground(Graphics g, GraphicsPath fullGaugePath)
        {
            using SolidBrush backgroundBrush = new SolidBrush(Color.FromArgb(
                72,
                FillColor.R,
                FillColor.G,
                FillColor.B));

            g.FillPath(backgroundBrush, fullGaugePath);
        }

        private static void DrawGaugeOutline(Graphics g, GraphicsPath fullGaugePath)
        {
            using Pen outlinePen = new Pen(Color.FromArgb(220, 255, 255, 255), 1.6f);
            outlinePen.LineJoin = LineJoin.Round;
            g.DrawPath(outlinePen, fullGaugePath);
        }

        private void DrawGaugeFill(Graphics g, RectangleF gaugeRect, GraphicsPath fullGaugePath)
        {
            double normalized = Clamp(throttleValue ?? 0.0, 0.0, 1.0);
            if (normalized <= 0.0)
            {
                return;
            }

            float fillWidth = gaugeRect.Width * (float)normalized;
            RectangleF fillClipRect = Mirrored
                ? new RectangleF(gaugeRect.Right - fillWidth, gaugeRect.Top - 4f, fillWidth, gaugeRect.Height + 8f)
                : new RectangleF(gaugeRect.Left, gaugeRect.Top - 4f, fillWidth, gaugeRect.Height + 8f);

            GraphicsState state = g.Save();
            try
            {
                g.SetClip(fullGaugePath, CombineMode.Replace);
                g.IntersectClip(fillClipRect);

                Color dark = Color.FromArgb(
                    255,
                    Math.Max(0, FillColor.R - 18),
                    Math.Max(0, FillColor.G - 18),
                    Math.Max(0, FillColor.B - 18));

                using LinearGradientBrush fillBrush = new LinearGradientBrush(
                    gaugeRect,
                    Mirrored ? FillColor : dark,
                    Mirrored ? dark : FillColor,
                    LinearGradientMode.Horizontal);
                g.FillPath(fillBrush, fullGaugePath);
            }
            finally
            {
                g.Restore(state);
            }
        }

        private static GraphicsPath BuildFullGaugePath(RectangleF rect, bool mirrored)
        {
            int steps = Math.Max(24, (int)(rect.Width / 5f));
            PointF[] topPoints = new PointF[steps + 1];
            float bottomY = rect.Bottom;
            float minHeight = 7f;
            float maxHeight = rect.Height;

            for (int i = 0; i <= steps; i++)
            {
                float t = steps == 0 ? 0f : (float)i / steps;
                float shapeT = mirrored ? 1f - t : t;
                float x = rect.Left + rect.Width * t;
                float exponential = (float)Math.Pow(shapeT, 1.85);
                float height = minHeight + (maxHeight - minHeight) * exponential;
                float topY = bottomY - height;
                topPoints[i] = new PointF(x, topY);
            }

            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddLine(rect.Left, bottomY, rect.Left, topPoints[0].Y);
            path.AddLines(topPoints);
            path.AddLine(rect.Right, topPoints[topPoints.Length - 1].Y, rect.Right, bottomY);
            path.AddLine(rect.Right, bottomY, rect.Left, bottomY);
            path.CloseFigure();
            return path;
        }

        private void DrawThrottleText(Graphics g, Rectangle bounds, RectangleF gaugeRect)
        {
            using Font titleFont = new Font("Segoe UI", 17f, FontStyle.Bold, GraphicsUnit.Point);
            using Font valueFont = new Font("Segoe UI", 19f, FontStyle.Bold, GraphicsUnit.Point);
            using Brush titleBrush = new SolidBrush(FillColor);
            using Brush labelBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
            using StringFormat near = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
            using StringFormat far = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };

            RectangleF titleRect = new RectangleF(12f, 4f, bounds.Width - 24f, 30f);
            RectangleF valueRect = new RectangleF(
                gaugeRect.Left + 10f,
                gaugeRect.Top,
                gaugeRect.Width - 20f,
                gaugeRect.Height);
            StringFormat titleFormat = far;
            StringFormat valueFormat = near;
            string text = throttleValue.HasValue ? $"{throttleValue.Value:0.00}" : "--";

            g.DrawString(GaugeTitle, titleFont, titleBrush, titleRect, titleFormat);
            g.DrawString(text, valueFont, labelBrush, valueRect, valueFormat);
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static double? ClampNullable(double? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return Clamp(value.Value, -1.0, 1.0);
        }
    }
}
