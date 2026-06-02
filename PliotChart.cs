using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Data_Manager
{
    public partial class PliotChart : Form
    {
        private readonly List<DonkeyAsyncWorker.PilotFrameData> _frames;

        public PliotChart(
            string modelName,
            IReadOnlyList<DonkeyAsyncWorker.PilotFrameData> frames)
        {
            InitializeComponent();

            _frames = frames
                .OrderBy(frame => frame.Index)
                .ToList();

            lblTitle.Text = string.IsNullOrWhiteSpace(modelName)
                ? "Pilot Chart"
                : $"{modelName} Chart";
            lblSummary.Text = BuildSummaryText();

            // A single paint surface draws both value groups, so the chart is created once per form.
            pnlChart.Paint += PnlChart_Paint;
            pnlChart.Resize += (sender, e) => pnlChart.Invalidate();
        }

        private string BuildSummaryText()
        {
            int total = _frames.Count;
            int angleAi = _frames.Count(frame => frame.PilotAngle.HasValue);
            int throttleAi = _frames.Count(frame => frame.PilotThrottle.HasValue);
            return $"Frames {total:N0} | AI Angle {angleAi:N0} | AI Throttle {throttleAi:N0}";
        }

        private void PnlChart_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Color.FromArgb(25, 29, 34));

            Rectangle bounds = pnlChart.ClientRectangle;
            if (bounds.Width < 160 || bounds.Height < 180)
            {
                return;
            }

            int gap = 18;
            int chartHeight = (bounds.Height - (gap * 3)) / 2;
            Rectangle angleBounds = Rectangle.FromLTRB(
                bounds.Left + 14,
                bounds.Top + gap,
                bounds.Right - 14,
                bounds.Top + gap + chartHeight);
            Rectangle throttleBounds = Rectangle.FromLTRB(
                bounds.Left + 14,
                angleBounds.Bottom + gap,
                bounds.Right - 14,
                angleBounds.Bottom + gap + chartHeight);

            DrawSeriesChart(e.Graphics, angleBounds, "Angle", frame => frame.UserAngle, frame => frame.PilotAngle);
            DrawSeriesChart(e.Graphics, throttleBounds, "Throttle", frame => frame.UserThrottle, frame => frame.PilotThrottle);
        }

        private void DrawSeriesChart(
            Graphics graphics,
            Rectangle bounds,
            string title,
            Func<DonkeyAsyncWorker.PilotFrameData, double?> userSelector,
            Func<DonkeyAsyncWorker.PilotFrameData, double?> pilotSelector)
        {
            Rectangle plot = Rectangle.FromLTRB(
                bounds.Left + 58,
                bounds.Top + 38,
                bounds.Right - 24,
                bounds.Bottom - 30);

            using Font titleFont = new Font(Font, FontStyle.Bold);
            using Font smallFont = new Font(Font.FontFamily, 9F, FontStyle.Regular);
            using Brush textBrush = new SolidBrush(Color.Gainsboro);
            using Brush mutedBrush = new SolidBrush(Color.FromArgb(160, Color.Gainsboro));
            using Pen borderPen = new Pen(Color.FromArgb(95, Color.White), 1);
            using Pen gridPen = new Pen(Color.FromArgb(38, Color.White), 1);
            using Pen zeroPen = new Pen(Color.FromArgb(110, Color.White), 1);
            using Pen userPen = new Pen(Color.Lime, 2.4F);
            using Pen pilotPen = new Pen(Color.DeepSkyBlue, 2.4F);

            graphics.DrawString(title, titleFont, textBrush, bounds.Left + 14, bounds.Top + 11);
            DrawLegend(graphics, bounds, userPen, pilotPen, smallFont, textBrush);

            for (int i = 0; i <= 4; i++)
            {
                double value = 1.0 - (i * 0.5);
                int y = MapY(value, plot);
                graphics.DrawLine(Math.Abs(value) < double.Epsilon ? zeroPen : gridPen, plot.Left, y, plot.Right, y);
                graphics.DrawString(value.ToString("0.0"), smallFont, mutedBrush, bounds.Left + 14, y - 8);
            }

            graphics.DrawRectangle(borderPen, plot);

            if (_frames.Count == 0)
            {
                DrawEmptyMessage(graphics, plot, smallFont, textBrush);
                return;
            }

            DrawSeries(graphics, plot, userSelector, userPen);
            DrawSeries(graphics, plot, pilotSelector, pilotPen);
        }

        private static void DrawLegend(
            Graphics graphics,
            Rectangle bounds,
            Pen userPen,
            Pen pilotPen,
            Font font,
            Brush textBrush)
        {
            int x = bounds.Right - 210;
            int y = bounds.Top + 18;

            graphics.DrawLine(userPen, x, y + 6, x + 28, y + 6);
            graphics.DrawString("User", font, textBrush, x + 36, y - 3);

            graphics.DrawLine(pilotPen, x + 92, y + 6, x + 120, y + 6);
            graphics.DrawString("AI", font, textBrush, x + 128, y - 3);
        }

        private void DrawSeries(
            Graphics graphics,
            Rectangle plot,
            Func<DonkeyAsyncWorker.PilotFrameData, double?> selector,
            Pen pen)
        {
            PointF? previous = null;

            for (int i = 0; i < _frames.Count; i++)
            {
                double? value = selector(_frames[i]);
                if (!value.HasValue)
                {
                    previous = null;
                    continue;
                }

                PointF current = new PointF(MapX(i, plot), MapY(value.Value, plot));
                if (previous.HasValue)
                {
                    graphics.DrawLine(pen, previous.Value, current);
                }

                previous = current;
            }
        }

        private float MapX(int framePosition, Rectangle plot)
        {
            if (_frames.Count <= 1)
            {
                return plot.Left;
            }

            return plot.Left + (float)(plot.Width * (framePosition / (double)(_frames.Count - 1)));
        }

        private static int MapY(double value, Rectangle plot)
        {
            double clamped = Math.Max(-1.0, Math.Min(1.0, value));
            double ratio = (1.0 - clamped) / 2.0;
            return plot.Top + (int)Math.Round(plot.Height * ratio);
        }

        private static void DrawEmptyMessage(Graphics graphics, Rectangle plot, Font font, Brush brush)
        {
            const string message = "No chart data";
            SizeF size = graphics.MeasureString(message, font);
            graphics.DrawString(
                message,
                font,
                brush,
                plot.Left + (plot.Width - size.Width) / 2,
                plot.Top + (plot.Height - size.Height) / 2);
        }
    }
}
