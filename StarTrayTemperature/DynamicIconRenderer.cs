using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace StarTrayTemperature
{
    public static class DynamicIconRenderer
    {
        public static Icon CreateDynamicIcon(Image baseMask, Color iconColorStart, Color iconColorEnd, Color textColor, string text, int width, int height, Font font, int moveX, int moveY)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Bitmap gradientBmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Bitmap maskBmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            // Pin to 96 DPI so point-sized fonts aren't scaled by the monitor's DPI
            bitmap.SetResolution(96f, 96f);
            gradientBmp.SetResolution(96f, 96f);
            maskBmp.SetResolution(96f, 96f);

            BitmapData gradData = null, maskData = null, outData = null;

            try
            {
                // 1. Draw the gradient background
                using (Graphics g = Graphics.FromImage(gradientBmp))
                using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, width, height), iconColorStart, iconColorEnd, LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, new Rectangle(0, 0, width, height));
                }

                // 2. Draw the mask onto another bitmap to guarantee format
                using (Graphics g = Graphics.FromImage(maskBmp))
                {
                    g.DrawImage(baseMask, 0, 0, width, height);
                }

                // 3. Blend them manually (using safe Marshal.Copy)
                gradData = gradientBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                maskData = maskBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                outData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                int bytes = Math.Abs(gradData.Stride) * height;
                byte[] gradBytes = new byte[bytes];
                byte[] maskBytes = new byte[bytes];
                byte[] outBytes = new byte[bytes];

                Marshal.Copy(gradData.Scan0, gradBytes, 0, bytes);
                Marshal.Copy(maskData.Scan0, maskBytes, 0, bytes);

                for (int i = 0; i < bytes; i += 4)
                {
                    // The base icons are white shapes with anti-aliased transparency.
                    // We use the mask's alpha, and scale it by its brightness (R channel)
                    // to handle any dark/grey pixels gracefully.
                    float brightness = maskBytes[i + 2] / 255f;
                    byte alpha = (byte)(maskBytes[i + 3] * brightness);

                    outBytes[i] = gradBytes[i];         // B
                    outBytes[i + 1] = gradBytes[i + 1]; // G
                    outBytes[i + 2] = gradBytes[i + 2]; // R
                    outBytes[i + 3] = alpha;            // A
                }

                Marshal.Copy(outBytes, 0, outData.Scan0, bytes);

                gradientBmp.UnlockBits(gradData); gradData = null;
                maskBmp.UnlockBits(maskData); maskData = null;
                bitmap.UnlockBits(outData); outData = null;

                // 4. Draw the temperature text on top
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (Brush brush = new SolidBrush(textColor))
                    {
                        if (textColor.R == 0 && textColor.G == 0 && textColor.B == 0) // Is Black
                        {
                            graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                        }
                        else
                        {
                            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                        }

                        SizeF textSize = graphics.MeasureString(text, font);
                        float x = (width - textSize.Width) / 2 + moveX;
                        float y = (height - textSize.Height) / 2 + moveY;

                        graphics.DrawString(text, font, brush, new PointF(x, y));
                    }
                }

                // GetHicon() creates an unmanaged Win32 icon handle.
                // The caller is responsible for calling DisposeIcon() on it.
                IntPtr hIcon = bitmap.GetHicon();
                return Icon.FromHandle(hIcon);
            }
            finally
            {
                if (gradData != null) gradientBmp.UnlockBits(gradData);
                if (maskData != null) maskBmp.UnlockBits(maskData);
                if (outData != null) bitmap.UnlockBits(outData);

                gradientBmp.Dispose();
                maskBmp.Dispose();
                bitmap.Dispose();
            }
        }


        // Used for clearing up GDI's and User's icon handles
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        public static void DisposeIcon(Icon icon)
        {
            if (icon != null)
            {
                DestroyIcon(icon.Handle);
                icon.Dispose();
            }
        }
    }
}
