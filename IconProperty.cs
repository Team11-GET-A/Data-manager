using MaterialSkin.Controls;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AD_AI_LearningData_Editor
{
    public static class IconProperty
    {
        public static void SetIcon(MaterialButton button, Image icon, int iconSize)
        {
            if (button == null || icon == null) return;

            button.Icon = ResizeImage(icon, iconSize, iconSize);
        }

        public static void SetAutoIcon(MaterialButton button, Image icon, int margin = 10)
        {
            if (button == null || icon == null) return;

            ApplyAutoIcon(button, icon, margin);

            button.Resize += (s, e) =>
            {
                ApplyAutoIcon(button, icon, margin);
            };
        }

        public static void SetAutoIconByWidthHeight(MaterialButton button, Image icon, int widthMargin = 10, int heightMargin = 10)
        {
            if (button == null || icon == null) return;

            ApplyAutoIconByWidthHeight(button, icon, widthMargin, heightMargin);

            button.Resize += (s, e) =>
            {
                ApplyAutoIconByWidthHeight(button, icon, widthMargin, heightMargin);
            };
        }

        public static void SetImage(Button button, Image image, int width, int height)
        {
            if (button == null || image == null) return;

            button.Image = ResizeImage(image, width, height);
            button.ImageAlign = ContentAlignment.MiddleCenter;
        }

        public static void SetAutoImage(Button button, Image image, int margin = 10)
        {
            if (button == null || image == null) return;

            ApplyAutoImage(button, image, margin);

            button.Resize += (s, e) =>
            {
                ApplyAutoImage(button, image, margin);
            };
        }

        public static void SetAutoImageByWidthHeight(Button button, Image image, int widthMargin = 10, int heightMargin = 10)
        {
            if (button == null || image == null) return;

            ApplyAutoImageByWidthHeight(button, image, widthMargin, heightMargin);

            button.Resize += (s, e) =>
            {
                ApplyAutoImageByWidthHeight(button, image, widthMargin, heightMargin);
            };
        }

        public static Image ResizeImage(Image image, int width, int height)
        {
            if (image == null) return null;

            int safeWidth = Math.Max(1, width);
            int safeHeight = Math.Max(1, height);

            Bitmap bitmap = new Bitmap(safeWidth, safeHeight);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(image, 0, 0, safeWidth, safeHeight);
            }

            return bitmap;
        }

        private static void ApplyAutoIcon(MaterialButton button, Image icon, int margin)
        {
            int iconSize = Math.Max(1, button.Height - margin);
            button.Icon = ResizeImage(icon, iconSize, iconSize);
        }

        private static void ApplyAutoIconByWidthHeight(MaterialButton button, Image icon, int widthMargin, int heightMargin)
        {
            int iconWidth = Math.Max(1, button.Width - widthMargin);
            int iconHeight = Math.Max(1, button.Height - heightMargin);
            button.Icon = ResizeImage(icon, iconWidth, iconHeight);
        }

        private static void ApplyAutoImage(Button button, Image image, int margin)
        {
            int imageSize = Math.Max(1, button.Height - margin);
            button.Image = ResizeImage(image, imageSize, imageSize);
            button.ImageAlign = ContentAlignment.MiddleCenter;
        }

        private static void ApplyAutoImageByWidthHeight(Button button, Image image, int widthMargin, int heightMargin)
        {
            int imageWidth = Math.Max(1, button.Width - widthMargin);
            int imageHeight = Math.Max(1, button.Height - heightMargin);
            button.Image = ResizeImage(image, imageWidth, imageHeight);
            button.ImageAlign = ContentAlignment.MiddleCenter;
        }
    }
}