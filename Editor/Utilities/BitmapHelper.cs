using Editor.Content;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Editor.Utilities
{
    static class BitmapHelper
    {
        public static byte[] CreateThumbnail(BitmapSource image, int maxWidth, int maxHeight)
        {
            var scaleX = maxWidth / (double)image.PixelWidth;
            var scaleY = maxHeight / (double)image.PixelHeight;
            var ratio = Math.Min(scaleX, scaleY);
            var thumbnail = new TransformedBitmap(image, new ScaleTransform(ratio, ratio, 0.5, 0.5));

            using var memoryStream = new MemoryStream();
            memoryStream.SetLength(0);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(thumbnail));
            encoder.Save(memoryStream);

            return memoryStream.ToArray();
        }

        public static BitmapSource ImageFromSlice(Slice slice, bool isNormalMap = false)
        {
            var data = slice.RawContent;
            var bytesPerPixel = data.Length / (slice.Width * slice.Height);
            var stride = slice.Width * bytesPerPixel;
            var format = PixelFormats.Default;
            byte[] bgrData = null;

            if (bytesPerPixel == 16) format = PixelFormats.Rgba128Float;
            else if (bytesPerPixel == 4) format = PixelFormats.Bgra32;
            else if (bytesPerPixel == 3 || bytesPerPixel == 2) format = PixelFormats.Bgr24;
            else if (bytesPerPixel == 1) format = PixelFormats.Gray8;

            if(bytesPerPixel == 16)
            {
                bgrData = new byte[data.Length];
                Buffer.BlockCopy(data, 0, bgrData, 0, data.Length);
            }
            else if (bytesPerPixel == 4 || bytesPerPixel == 3)
            {
                bgrData = new byte[data.Length];
                Buffer.BlockCopy(data, 0, bgrData, 0, data.Length);

                for(int i = 0; i< data.Length; i += bytesPerPixel)
                {
                    var r = bgrData[i + 2];

                    bgrData[i + 2] = bgrData[i];
                    bgrData[i] = r;
                }
            }
            else if(bytesPerPixel == 2)
            {
                bgrData = new byte[slice.Width * slice.Height * 3];
                stride = slice.Width * 3;

                var inv255 = 1.0 / 255.0;
                var isNormal = isNormalMap ? 1 : 0;
                int index = 0;

                for(int i = 0; i <data.Length; i+=2)
                {
                    var r = data[i] * inv255 * 2.0 - 1.0;
                    var g = data[i+1] * inv255 * 2.0 - 1.0;
                    var b = (Math.Sqrt(Math.Clamp(1.0 - (r * r + g * g), 0, 1.0)) + 1.0) * .5 * 255.0;

                    bgrData[index + 2] = data[i];
                    bgrData[index + 1] = data[i + 1];
                    bgrData[index] = (byte)(b * isNormal);

                    index += 3;
                }
            }
            else if(bytesPerPixel == 1)
            {
                bgrData = new byte[data.Length];
                Buffer.BlockCopy(data, 0, bgrData, 0, data.Length);
            }

            BitmapSource image = null;

            if (bgrData is not null) image = BitmapSource.Create(slice.Width, slice.Height, 96.0, 96.0, format, null, bgrData, stride);

            return image;
        }
    }
}
