using Editor.Common.Enums;
using Editor.Content;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Editor.Utilities;

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

    public static BitmapSource? ImageFromSlice(
        Slice slice,
        DXGIFormat slice_format,
        bool isNormalMap = false
    )
    {
        var data = slice.RawContent;
        var bytesPerPixel = data.Length / (slice.Width * slice.Height);
        var bytesPerChannel = BytesPerChannel(slice_format);
        var stride = slice.Width * bytesPerPixel;
        var format = PixelFormats.Default;
        byte[] bgrData = null;

        if (bytesPerPixel == 16) format = PixelFormats.Rgba128Float;
        else if (bytesPerPixel == 4) format = PixelFormats.Bgra32;
        else if (bytesPerPixel == 2) format = PixelFormats.Bgr24;
        else if (bytesPerPixel == 1) format = PixelFormats.Gray8;

        if (bytesPerPixel == 16 || bytesPerPixel == 1)
        {
            bgrData = new byte[data.Length];
            Buffer.BlockCopy(data, 0, bgrData, 0, data.Length);
        }
        else if (bytesPerPixel == 4 && bytesPerChannel == 1)
        {
            bgrData = new byte[data.Length];
            Buffer.BlockCopy(data, 0, bgrData, 0, data.Length);

            for (int i = 0; i < data.Length; i += bytesPerPixel)
            {
                var r = bgrData[i + 2];

                bgrData[i + 2] = bgrData[i];
                bgrData[i] = r;
            }
        }
        else if (bytesPerPixel == 4)
        {
            if (bytesPerChannel == 2)
            {
                int offset = 0;
                Half[] dataFloats = data
                    .GroupBy(x => offset++ / bytesPerChannel)
                    .Select(x => BitConverter.ToHalf(x.ToArray(), 0))
                    .ToArray();

                using var writer = new BinaryWriter(new MemoryStream());

                for (int i = 0; i < dataFloats.Length; i += bytesPerChannel)
                {
                    writer.Write((float)dataFloats[i]);
                    writer.Write((float)dataFloats[i + 1]);
                    writer.Write(0f);
                    writer.Write(1f);
                }

                writer.Flush();

                bgrData = (writer.BaseStream as MemoryStream).ToArray();
                format = PixelFormats.Rgba128Float;
                stride = slice.Width * 16;
            }
            else if (bytesPerChannel == 4)
            {
                int offset = 0;
                float[] dataFloats = data
                    .GroupBy(x => offset++ / bytesPerChannel)
                    .Select(x => BitConverter.ToSingle(x.ToArray().Reverse().ToArray(), 0))
                    .ToArray();

                using var writer = new BinaryWriter(new MemoryStream());

                foreach (var f in dataFloats)
                {
                    writer.Write(f);
                    writer.Write(0f);
                    writer.Write(0f);
                    writer.Write(1f);
                }

                writer.Flush();
                bgrData = (writer.BaseStream as MemoryStream).ToArray();
                format = PixelFormats.Rgba128Float;
                stride = slice.Width * 16;
            }
        }
        else if (bytesPerPixel == 2)
        {
            if (bytesPerChannel == 1)
            {
                bgrData = new byte[slice.Width * slice.Height * 3];
                stride = slice.Width * 3;

                int index = 0;

                for (int i = 0; i < data.Length; i += 2)
                {
                    bgrData[index + 2] = data[i];
                    bgrData[index + 1] = data[i + 1];
                    bgrData[index] = 0;

                    index += 3;
                }

                if (isNormalMap)
                {
                    var inv255 = 1.0 / 255.0;

                    index = 0;

                    for (int i = 0; i < data.Length; i += 2)
                    {
                        var r = data[i] * inv255 * 2.0 - 1.0;
                        var g = data[i + 1] * inv255 * 2.0 - 1.0;
                        var b = (Math.Sqrt(Math.Clamp(1.0 - (r * r + g * g), 0.0, 1.0)) + 1.0) * 0.5 * 255.0;

                        bgrData[index] = (byte)b;

                        index += 3;
                    }
                }
            }
            else if (bytesPerChannel == 2)
            {
                int offset = 0;
                Half[] dataFloats = data
                    .GroupBy(x => offset++ / bytesPerChannel)
                    .Select(x => BitConverter.ToHalf(x.ToArray(), 0))
                    .ToArray();

                using var writer = new BinaryWriter(new MemoryStream());

                foreach (var f in dataFloats)
                {
                    writer.Write(f);
                    writer.Write(0f);
                    writer.Write(0f);
                    writer.Write(1f);
                }

                writer.Flush();

                bgrData = (writer.BaseStream as MemoryStream).ToArray();
                format = PixelFormats.Rgba128Float;
                stride = slice.Width * 16;
            }
        }

        BitmapSource image = null;

        if (bgrData is not null) image = BitmapSource.Create(slice.Width, slice.Height, 96.0, 96.0, format, null, bgrData, stride);

        return image;
    }

    public static int BytesPerChannel(DXGIFormat format)
    {
        switch (format)
        {
            case DXGIFormat.DXGI_FORMAT_R32G32B32A32_FLOAT:
            case DXGIFormat.DXGI_FORMAT_R32G32B32A32_UINT:
            case DXGIFormat.DXGI_FORMAT_R32G32B32A32_SINT:
            case DXGIFormat.DXGI_FORMAT_R32G32B32_FLOAT:
            case DXGIFormat.DXGI_FORMAT_R32G32B32_UINT:
            case DXGIFormat.DXGI_FORMAT_R32G32B32_SINT:
            case DXGIFormat.DXGI_FORMAT_R32G32_FLOAT:
            case DXGIFormat.DXGI_FORMAT_R32G32_UINT:
            case DXGIFormat.DXGI_FORMAT_R32G32_SINT:
            case DXGIFormat.DXGI_FORMAT_R32_FLOAT:
            case DXGIFormat.DXGI_FORMAT_R32_UINT:
            case DXGIFormat.DXGI_FORMAT_R32_SINT:
            case DXGIFormat.DXGI_FORMAT_BC6H_SF16:
            case DXGIFormat.DXGI_FORMAT_BC6H_UF16:
                return 4;
            case DXGIFormat.DXGI_FORMAT_R16G16B16A16_FLOAT:
            case DXGIFormat.DXGI_FORMAT_R16G16B16A16_UNORM:
            case DXGIFormat.DXGI_FORMAT_R16G16B16A16_UINT:
            case DXGIFormat.DXGI_FORMAT_R16G16B16A16_SNORM:
            case DXGIFormat.DXGI_FORMAT_R16G16B16A16_SINT:
            case DXGIFormat.DXGI_FORMAT_R16G16_FLOAT:
            case DXGIFormat.DXGI_FORMAT_R16G16_UNORM:
            case DXGIFormat.DXGI_FORMAT_R16G16_UINT:
            case DXGIFormat.DXGI_FORMAT_R16G16_SNORM:
            case DXGIFormat.DXGI_FORMAT_R16G16_SINT:
            case DXGIFormat.DXGI_FORMAT_R16_FLOAT:
            case DXGIFormat.DXGI_FORMAT_R16_UNORM:
            case DXGIFormat.DXGI_FORMAT_R16_UINT:
            case DXGIFormat.DXGI_FORMAT_R16_SNORM:
            case DXGIFormat.DXGI_FORMAT_R16_SINT:
                return 2;
            case DXGIFormat.DXGI_FORMAT_R8G8B8A8_UNORM:
            case DXGIFormat.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:
            case DXGIFormat.DXGI_FORMAT_R8G8B8A8_UINT:
            case DXGIFormat.DXGI_FORMAT_R8G8B8A8_SNORM:
            case DXGIFormat.DXGI_FORMAT_R8G8B8A8_SINT:
            case DXGIFormat.DXGI_FORMAT_R8G8_UNORM:
            case DXGIFormat.DXGI_FORMAT_R8G8_UINT:
            case DXGIFormat.DXGI_FORMAT_R8G8_SNORM:
            case DXGIFormat.DXGI_FORMAT_R8G8_SINT:
            case DXGIFormat.DXGI_FORMAT_R8_UNORM:
            case DXGIFormat.DXGI_FORMAT_R8_UINT:
            case DXGIFormat.DXGI_FORMAT_R8_SNORM:
            case DXGIFormat.DXGI_FORMAT_R8_SINT:
            case DXGIFormat.DXGI_FORMAT_BC1_UNORM:
            case DXGIFormat.DXGI_FORMAT_BC1_UNORM_SRGB:
            case DXGIFormat.DXGI_FORMAT_BC3_UNORM:
            case DXGIFormat.DXGI_FORMAT_BC3_UNORM_SRGB:
            case DXGIFormat.DXGI_FORMAT_BC4_SNORM:
            case DXGIFormat.DXGI_FORMAT_BC4_UNORM:
            case DXGIFormat.DXGI_FORMAT_BC5_SNORM:
            case DXGIFormat.DXGI_FORMAT_BC5_UNORM:
            case DXGIFormat.DXGI_FORMAT_BC7_UNORM:
            case DXGIFormat.DXGI_FORMAT_BC7_UNORM_SRGB:
                return 1;
            default:
                break;
        }

        return -1;
    }
}
