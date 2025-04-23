using Editor.Common.Enums;
using Editor.Content;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Editor.DLLs.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public class TextureData : IDisposable
    {
        public IntPtr SubresourceData;
        public int SubresourceSize;
        public IntPtr Icon;
        public int IconSize;
        public TextureInfo Info = new();
        public TextureImportSettings ImportSettings = new();

        public void Dispose()
        {
            Marshal.FreeCoTaskMem(SubresourceData);
            Marshal.FreeCoTaskMem(Icon);
            GC.SuppressFinalize(this);
        }

        public static SliceArray3D SlicesFromBinary(byte[] data, int arraySize, int mipLevels, bool is3D)
        {
            Debug.Assert(data?.Length > 0 && arraySize > 0);
            Debug.Assert(mipLevels > 0 && mipLevels < Texture.MaxMipLevels);

            var depthPerMipLevel = Enumerable.Repeat(1, mipLevels).ToList();

            if (is3D)
            {
                var depth = arraySize;
                arraySize = 1;

                for (var i = 0; i < mipLevels; ++i)
                {
                    depthPerMipLevel[i] = depth;
                    depth = Math.Max(depth >> 1, 1);
                }
            }

            using var reader = new BinaryReader(new MemoryStream(data));
            var slices = new SliceArray3D();

            for (var i = 0; i < arraySize; ++i)
            {
                var arraySlice = new List<List<Slice>>();

                for (var j = 0; j < mipLevels; ++j)
                {
                    var mipSlice = new List<Slice>();

                    for (var k = 0; k < depthPerMipLevel[j]; ++k)
                    {
                        var slice = new Slice();

                        slice.Width = reader.ReadInt32();
                        slice.Height = reader.ReadInt32();
                        slice.RowPitch = reader.ReadInt32();
                        slice.SlicePitch = reader.ReadInt32();
                        slice.RawContent = reader.ReadBytes(slice.SlicePitch);

                        mipSlice.Add(slice);
                    }

                    arraySlice.Add(mipSlice);
                }

                slices.Add(arraySlice);
            }

            return slices;
        }

        public static byte[] SlicesToBinary(SliceArray3D slices)
        {
            Debug.Assert(slices?.Any() == true && slices.First().Any() == true);

            using var writer = new BinaryWriter(new MemoryStream());

            foreach (var arraySlice in slices)
            {
                foreach (var mipLevel in arraySlice)
                {
                    foreach (var slice in mipLevel)
                    {
                        writer.Write(slice.Width);
                        writer.Write(slice.Height);
                        writer.Write(slice.RowPitch);
                        writer.Write(slice.SlicePitch);
                        writer.Write(slice.RawContent);
                    }
                }
            }

            writer.Flush();

            var data = (writer.BaseStream as MemoryStream)?.ToArray();

            Debug.Assert(data?.Length > 0);

            return data;
        }

        public SliceArray3D GetSlices()
        {
            Debug.Assert(Info.MipLevels > 0);
            Debug.Assert(SubresourceData != IntPtr.Zero && SubresourceSize > 0);

            var subresourceData = new byte[SubresourceSize];

            Marshal.Copy(SubresourceData, subresourceData, 0, SubresourceSize);

            return SlicesFromBinary(
                subresourceData,
                Info.ArraySize,
                Info.MipLevels,
                ((TextureFlags)Info.Flags).HasFlag(TextureFlags.IS_VOLUME_MAP)
            );
        }

        public Slice GetIcon()
        {
            if (ImportSettings.Compress == 0) return null;

            Debug.Assert(Icon != IntPtr.Zero && IconSize > 0);

            var icon = new byte[IconSize];

            Marshal.Copy(Icon, icon, 0, IconSize);

            return SlicesFromBinary(icon, 1, 1, false).First()?.First()?.First();
        }

        public void SetSubresourceData(SliceArray3D slices)
        {
            var subresourceData = SlicesToBinary(slices);

            SubresourceData = Marshal.AllocCoTaskMem(subresourceData.Length);
            SubresourceSize = subresourceData.Length;

            Marshal.Copy(subresourceData, 0, SubresourceData, SubresourceSize);
        }

        public void GetTextureDataInfo(Texture texture)
        {
            Info.Width = texture.Width;
            Info.Height = texture.Height;
            Info.ArraySize = texture.ArraySize;
            Info.MipLevels = texture.MipLevels;
            Info.Format = (int)texture.Format;
            Info.Flags = (int)texture.Flags;
        }

        public void GetTextureInfo(Texture texture)
        {
            texture.Flags = (TextureFlags)Info.Flags;
            texture.Width = Info.Width;
            texture.Height = Info.Height;
            texture.ArraySize = Info.ArraySize;
            texture.MipLevels = Info.MipLevels;
            texture.Format = (DXGIFormat)Info.Format;
        }

        public TextureData Clone(Content.TextureImportSettings settings)
        {
            TextureData data = new TextureData();

            if(SubresourceData != IntPtr.Zero && SubresourceSize > 0)
            {
                var bytes = new byte[SubresourceSize];

                data.SubresourceData = Marshal.AllocCoTaskMem(SubresourceSize);
                data.SubresourceSize = SubresourceSize;

                Marshal.Copy(SubresourceData, bytes, 0, SubresourceSize);
                Marshal.Copy(bytes, 0, data.SubresourceData, SubresourceSize);
            }

            if(Icon != IntPtr.Zero && IconSize > 0)
            {
                var bytes = new byte[IconSize];

                data.Icon = Marshal.AllocCoTaskMem(IconSize);
                data.IconSize = IconSize;

                Marshal.Copy(Icon, bytes, 0, IconSize);
                Marshal.Copy(bytes, 0, data.Icon, IconSize);
            }

            data.Info = Info.Clone();
            data.ImportSettings.FromContentSettings(settings);

            return data;
        }

        ~TextureData()
        {
            Dispose();
        }
    }
}
