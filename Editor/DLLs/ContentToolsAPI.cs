using Editor.Common.Enums;
using Editor.Content;
using Editor.DLLs.Descriptors;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Editor.DLLs
{
    static class ContentToolsAPI
    {
        private const string _contentToolsDll = "ContentTools.dll";

        [DllImport(_contentToolsDll)] // Modify entry point
        private static extern void CreatePrimitiveMesh([In, Out] SceneData data, PrimitiveInitInfo info);

        [DllImport(_contentToolsDll)] // Modify entry point
        private static extern void ImportFbx(string file, [In, Out] SceneData data);

        [DllImport(_contentToolsDll)] // Modify entry point
        private static extern void Import([In, Out] TextureData data);

        public static void CreatePrimitiveMesh(Geometry geometry, PrimitiveInitInfo info) =>
            GeometryFromSceneData(
                geometry,
                (sceneData) => CreatePrimitiveMesh(sceneData, info),
                $"Failed to create {info.Type} primitive mesh."
            );

        public static void ImportFbx(string file, Geometry geometry) =>
            GeometryFromSceneData(geometry, (sceneData) => ImportFbx(file, sceneData), $"Failed to import from FBX file: {file}");

        public static (List<List<List<Slice>>> slices, Slice icon) Import(Texture texture)
        {
            Debug.Assert(texture.ImportSettings.Sources.Any());

            using var textureData = new TextureData();

            try
            {
                GetTextureDataInfo(texture, textureData);
                textureData.ImportSettings.FromContentSettings(texture);

                Import(textureData);

                if (textureData.Info.ImportError != 0)
                {
                    Logger.LogAsync(
                        LogLevel.ERROR,
                        $"Texture import error: {EnumExtension.GetDescription((TextureImportError)textureData.Info.ImportError)}"
                    );

                    throw new Exception($"Error while trying to import image. Error code: {textureData.Info.ImportError}");
                }

                GetTextureInfo(texture, textureData);

                return (GetSlices(textureData), GetIcon(textureData));
            }
            catch (Exception ex)
            {
                Logger.LogAsync(LogLevel.ERROR, $"Failed to import from {texture.FileName}");
                Debug.WriteLine(ex.Message);

                return new();
            }
        }

        public static List<List<List<Slice>>> SlicesFromBinary(byte[] data, int arraySize, int mipLevels, bool is3D)
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
            var slices = new List<List<List<Slice>>>();

            for (var i = 0; i < arraySize; ++i)
            {
                var arraySlice = new List<List<Slice>>();

                for (var j = 0; j < mipLevels; ++j)
                {
                    var mipSlice = new List<Slice>();

                    for (var k = 0; k < depthPerMipLevel[i]; ++k)
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

        public static byte[] SlicesToBinary(List<List<List<Slice>>> slices)
        {
            Debug.Assert(slices?.Any() == true && slices.First().Any() == true);

            using var writer = new BinaryWriter(new MemoryStream());

            foreach(var arraySlice in slices)
            {
                foreach(var mipLevel in arraySlice)
                {
                    foreach(var slice in mipLevel)
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

        private static void GeometryFromSceneData(Geometry geometry, Action<SceneData> sceneDataGenerator, string failureMessage)
        {
            Debug.Assert(geometry is not null);

            using var sceneData = new SceneData();

            try
            {
                sceneData.ImportSettings.FromContentSettings(geometry);
                sceneDataGenerator(sceneData);

                Debug.Assert(sceneData.Data != IntPtr.Zero && sceneData.DataSize > 0);

                var data = new byte[sceneData.DataSize];
                Marshal.Copy(sceneData.Data, data, 0, sceneData.DataSize);
                geometry.FromRawData(data);
            }
            catch (Exception ex)
            {
                Logger.LogAsync(LogLevel.ERROR, failureMessage);
                Debug.WriteLine(ex.Message);
            }
        }

        private static void GetTextureDataInfo(Texture texture, TextureData data)
        {
            var info = data.Info;

            info.Width = texture.Width;
            info.Height = texture.Height;
            info.ArraySize = texture.ArraySize;
            info.MipLevels = texture.MipLevels;
            info.Format = (int)texture.Format;
            info.Flags = (int)texture.Flags;
        }

        private static void GetTextureInfo(Texture texture, TextureData data)
        {
            var info = data.Info;

            texture.Width = info.Width;
            texture.Height = info.Height;
            texture.ArraySize = info.ArraySize;
            texture.MipLevels = info.MipLevels;
            texture.Format = (DXGIFormat)info.Format;
            texture.Flags = (TextureFlags)info.Flags;
        }

        private static List<List<List<Slice>>> GetSlices(TextureData data)
        {
            Debug.Assert(data.Info.MipLevels > 0);
            Debug.Assert(data.SubresourceData != IntPtr.Zero && data.SubresourceSize > 0);

            var subresourceData = new byte[data.SubresourceSize];

            Marshal.Copy(data.SubresourceData, subresourceData, 0, data.SubresourceSize);

            return SlicesFromBinary(
                subresourceData,
                data.Info.ArraySize,
                data.Info.MipLevels,
                ((TextureFlags)data.Info.Flags).HasFlag(TextureFlags.IS_VOLUME_MAP)
            );
        }

        private static Slice GetIcon(TextureData data)
        {
            if (data.ImportSettings.Compress == 0) return null;

            Debug.Assert(data.Icon != IntPtr.Zero && data.IconSize > 0);

            var icon = new byte[data.IconSize];

            Marshal.Copy(data.Icon, icon, 0, data.IconSize);

            return SlicesFromBinary(icon, 1, 1, false).First()?.First()?.First();
        }
    }
}
