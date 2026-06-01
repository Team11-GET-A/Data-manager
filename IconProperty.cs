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

        public static void SetIcon(MaterialButton button, Image icon, int iconWidth, int iconHeight)
        {
            if (button == null || icon == null) return;

            button.Icon = ResizeImage(icon, iconWidth, iconHeight);
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

        public static void SetAutoIconByMargins(MaterialButton button, Image icon, int leftMargin, int topMargin, int rightMargin, int bottomMargin)
        {
            if (button == null || icon == null) return;

            ApplyAutoIconByMargins(button, icon, leftMargin, topMargin, rightMargin, bottomMargin);

            button.Resize += (s, e) =>
            {
                ApplyAutoIconByMargins(button, icon, leftMargin, topMargin, rightMargin, bottomMargin);
            };
        }

        public static void SetAutoIconByHorizontalVerticalMargins(MaterialButton button, Image icon, int horizontalMargin, int verticalMargin)
        {
            if (button == null || icon == null) return;

            SetAutoIconByMargins(button, icon, horizontalMargin, verticalMargin, horizontalMargin, verticalMargin);
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

        public static void SetAutoImageByMargins(Button button, Image image, int leftMargin, int topMargin, int rightMargin, int bottomMargin)
        {
            if (button == null || image == null) return;

            ApplyAutoImageByMargins(button, image, leftMargin, topMargin, rightMargin, bottomMargin);

            button.Resize += (s, e) =>
            {
                ApplyAutoImageByMargins(button, image, leftMargin, topMargin, rightMargin, bottomMargin);
            };
        }

        public static void SetAutoImageByHorizontalVerticalMargins(Button button, Image image, int horizontalMargin, int verticalMargin)
        {
            if (button == null || image == null) return;

            SetAutoImageByMargins(button, image, horizontalMargin, verticalMargin, horizontalMargin, verticalMargin);
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

        public static Image ResizeImageWithMargins(Image image, int canvasWidth, int canvasHeight, int leftMargin, int topMargin, int rightMargin, int bottomMargin)
        {
            if (image == null) return null;

            int safeCanvasWidth = Math.Max(1, canvasWidth);
            int safeCanvasHeight = Math.Max(1, canvasHeight);

            int safeLeft = Math.Max(0, leftMargin);
            int safeTop = Math.Max(0, topMargin);
            int safeRight = Math.Max(0, rightMargin);
            int safeBottom = Math.Max(0, bottomMargin);

            int iconWidth = Math.Max(1, safeCanvasWidth - safeLeft - safeRight);
            int iconHeight = Math.Max(1, safeCanvasHeight - safeTop - safeBottom);

            Bitmap bitmap = new Bitmap(safeCanvasWidth, safeCanvasHeight);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;

                Rectangle destRect = new Rectangle(
                    safeLeft,
                    safeTop,
                    iconWidth,
                    iconHeight
                );

                g.DrawImage(image, destRect);
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

        private static void ApplyAutoIconByMargins(MaterialButton button, Image icon, int leftMargin, int topMargin, int rightMargin, int bottomMargin)
        {
            int canvasWidth = Math.Max(1, button.Width);
            int canvasHeight = Math.Max(1, button.Height);

            button.Icon = ResizeImageWithMargins(
                icon,
                canvasWidth,
                canvasHeight,
                leftMargin,
                topMargin,
                rightMargin,
                bottomMargin
            );
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

        private static void ApplyAutoImageByMargins(Button button, Image image, int leftMargin, int topMargin, int rightMargin, int bottomMargin)
        {
            int canvasWidth = Math.Max(1, button.Width);
            int canvasHeight = Math.Max(1, button.Height);

            button.Image = ResizeImageWithMargins(
                image,
                canvasWidth,
                canvasHeight,
                leftMargin,
                topMargin,
                rightMargin,
                bottomMargin
            );

            button.ImageAlign = ContentAlignment.MiddleCenter;
        }
    }
}
