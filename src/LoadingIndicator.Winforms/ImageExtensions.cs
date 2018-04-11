using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace LoadingIndicator.Winforms
{
    public static class ImageExtensions
    {
        private const float Tolerance = 0.0001f;

        [NotNull]
        public static Image CaptureScreenshot([NotNull] this Control control)
        {
            var clientRectangle = control.ClientRectangle;

            var screenLocation = control.PointToScreen(clientRectangle.Location);
            var screenRectangle = new Rectangle(screenLocation, control.ClientRectangle.Size);

            float scaleX;
            float scaleY;

            using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                var desktop = graphics.GetHdc();

                scaleX = (float)NativeMethods.GetDeviceCaps(desktop, NativeMethods.HorizontalResolution) /
                             NativeMethods.GetDeviceCaps(desktop, NativeMethods.DesktopHorizontalResolution);

                scaleY = (float)NativeMethods.GetDeviceCaps(desktop, NativeMethods.VerticalResolution) /
                           NativeMethods.GetDeviceCaps(desktop, NativeMethods.DesktopVerticalResolution);

                screenRectangle = new Rectangle(
                    (int)Math.Round(screenRectangle.X / scaleX),
                    (int)Math.Round(screenRectangle.Y / scaleY),
                    (int)Math.Round(screenRectangle.Width / scaleX),
                    (int)Math.Round(screenRectangle.Height / scaleY));
            }

            var screenCapture = new Bitmap(screenRectangle.Width, screenRectangle.Height);

            using (var graphics = Graphics.FromImage(screenCapture))
            {
                graphics.CopyFromScreen(
                    screenRectangle.X,
                    screenRectangle.Y,
                    0,
                    0,
                    screenCapture.Size,
                    CopyPixelOperation.SourceCopy);
            }

            if (Math.Abs(scaleX - 1f) < Tolerance && Math.Abs(scaleY - 1f) < Tolerance)
            {
                return screenCapture;
            }

            // Scale out image to allow system to scale it back... :)
            var scaledScreenCapture = new Bitmap(clientRectangle.Width, clientRectangle.Height);
            using (var graphics = Graphics.FromImage(scaledScreenCapture))
            {
                graphics.DrawImage(
                    screenCapture,
                    new Rectangle(Point.Empty, clientRectangle.Size),
                    new Rectangle(Point.Empty, screenCapture.Size),
                    GraphicsUnit.Pixel);
            }

            return scaledScreenCapture;
        }

        [NotNull]
        public static Bitmap ToBitmap([NotNull] this Image sourceImage)
        {
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));

            var bitmap = sourceImage as Bitmap;
            if (bitmap != null)
            {
                return bitmap;
            }

            return new Bitmap(sourceImage);
        }

        [NotNull]
        public static Bitmap ImageBlurFilter([NotNull] this Bitmap sourceBitmap)
        {
            if (sourceBitmap == null) throw new ArgumentNullException(nameof(sourceBitmap));

            var resultBitmap = sourceBitmap.ConvolutionFilter(
                Matrix.GaussianBlur3x3, 1.0 / 16.0);

            return resultBitmap;
        }

        [NotNull]
        public static Bitmap MakeGrayscale([NotNull] this Image original)
        {
            var greyscaleMatrix = new ColorMatrix(
                new[]
                {
                    new[] { .3f, .3f, .3f, 0, 0 },
                    new[] { .59f, .59f, .59f, 0, 0 },
                    new[] { .11f, .11f, .11f, 0, 0 },
                    new[] { 0f, 0, 0, 1, 0 },
                    new[] { 0f, 0, 0, 0, 1 }
                });

            return original.ChangeColors(greyscaleMatrix);
        }

        [NotNull]
        public static Bitmap MakeSepia([NotNull] this Image original)
        {
            var sepiaMatrix = new ColorMatrix(
                new[]
                {
                    new[] { .393f, .349f, .272f, 0, 0 },
                    new[] { .769f, .686f, .534f, 0, 0 },
                    new[] { .189f, .168f, .131f, 0, 0 },
                    new[] { 0f, 0, 0, 1, 0 },
                    new[] { 0f, 0, 0, 0, 1 }
                });

            return original.ChangeColors(sepiaMatrix);
        }

        [NotNull]
        private static Bitmap ChangeColors([NotNull] this Image original, ColorMatrix colorMatrix)
        {
            var newBitmap = new Bitmap(original.Width, original.Height);

            using (var graphics = Graphics.FromImage(newBitmap))
            {
                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                graphics.DrawImage(
                    original,
                    new Rectangle(0, 0, original.Width, original.Height),
                    0,
                    0,
                    original.Width,
                    original.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }

            return newBitmap;
        }

        [NotNull]
        private static Bitmap ConvolutionFilter(
            [NotNull] this Bitmap sourceBitmap,
            [NotNull] double[,] filterMatrix,
            double factor = 1,
            int bias = 0)
        {
            var sourceData = sourceBitmap.LockBits(
                new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            var pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            var resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);

            var filterWidth = filterMatrix.GetLength(1);

            var filterOffset = (filterWidth - 1) / 2;

            for (var offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (var offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++)
                {
                    double blue = 0;
                    double green = 0;
                    double red = 0;

                    var byteOffset = offsetY * sourceData.Stride + offsetX * 4;

                    for (var filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (var filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            var calcOffset = byteOffset +
                                             filterX * 4 +
                                             filterY * sourceData.Stride;

                            blue += pixelBuffer[calcOffset] *
                                    filterMatrix[filterY + filterOffset, filterX + filterOffset];

                            green += pixelBuffer[calcOffset + 1] *
                                     filterMatrix[filterY + filterOffset, filterX + filterOffset];

                            red += pixelBuffer[calcOffset + 2] *
                                   filterMatrix[filterY + filterOffset, filterX + filterOffset];
                        }
                    }

                    blue = factor * blue + bias;
                    green = factor * green + bias;
                    red = factor * red + bias;

                    blue = blue > 255 ? 255 : (blue < 0 ? 0 : blue);

                    green = green > 255 ? 255 : (green < 0 ? 0 : green);

                    red = red > 255 ? 255 : (red < 0 ? 0 : red);

                    resultBuffer[byteOffset] = (byte)blue;
                    resultBuffer[byteOffset + 1] = (byte)green;
                    resultBuffer[byteOffset + 2] = (byte)red;
                    resultBuffer[byteOffset + 3] = 255;
                }
            }

            var resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

            var resultData = resultBitmap.LockBits(
                new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }

        private static class Matrix
        {
            [NotNull]
            public static double[,] GaussianBlur3x3
            {
                get
                {
                    return
                        new double[,]
                        {
                            { 1, 2, 1, },
                            { 2, 4, 2, },
                            { 1, 2, 1, }
                        };
                }
            }
        }
    }
}