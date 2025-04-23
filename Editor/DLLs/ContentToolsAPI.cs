using Editor.Common.Enums;
using Editor.Content;
using Editor.Content.ImportSettingsConfig;
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
        private delegate void ProgressCallback(int value, int maxValue);

        [DllImport(_contentToolsDll)] // Modify entry point
        private static extern void CreatePrimitiveMesh([In, Out] SceneData data, PrimitiveInitInfo info);

        [DllImport(_contentToolsDll)] // Modify entry point
        private static extern void ImportFbx(string file, [In, Out] SceneData data, ProgressCallback callback);

        [DllImport(_contentToolsDll)] // Modify entry point
        private static extern void Import([In, Out] TextureData data);

        [DllImport(_contentToolsDll)] // Modify entry point
        private static extern void Decompress([In, Out] TextureData data);

        [DllImport(_contentToolsDll)] // Modify entry point
        public static extern void ShutdownContentTools();

        [DllImport(_contentToolsDll)] // Modify entry point
        public static extern void PrefilterDiffuseIBL([In, Out] TextureData data);

        [DllImport(_contentToolsDll)] // Modify entry point
        public static extern void PrefilterSpecularIBL([In, Out] TextureData data);

        public static void CreatePrimitiveMesh(Geometry geometry, PrimitiveInitInfo info) =>
            GeometryFromSceneData(
                geometry,
                (sceneData) => CreatePrimitiveMesh(sceneData, info),
                $"Failed to create {info.Type} primitive mesh."
            );

        public static void ImportFbx(string file, Geometry geometry)
        {
            var item = ImportingItemCollection.GetItem(geometry);
            ProgressCallback callback = item is not null ? item.SetProgress : null;

            GeometryFromSceneData(geometry, (sceneData) =>
                ImportFbx(file, sceneData, callback), $"Failed to import from FBX file: {file}");
        }

        public static void Import(Texture texture)
        {
            Debug.Assert(texture.ImportSettings.Sources.Any());

            using var textureData = new TextureData();

            try
            {
                textureData.ImportSettings.FromContentSettings(texture.ImportSettings);

                Import(textureData);

                if (textureData.Info.ImportError != 0)
                {
                    Logger.LogAsync(
                        LogLevel.ERROR,
                        $"Texture import error: {
                            EnumExtension.GetDescription((TextureImportError)textureData.Info.ImportError)
                        }"
                    );

                    throw new Exception($"Error while trying to import image. Error code: {textureData.Info.ImportError}");
                }

                Texture diffuseIBLCubemap = null;

                if(
                    texture.ImportSettings.PrefilterCubemap &&
                    ((TextureFlags)textureData.Info.Flags).HasFlag(TextureFlags.IS_CUBE_MAP)
                )
                {
                    using var diffuseData = textureData.Clone(texture.ImportSettings);

                    var diffuseResult = Task.Run(() => PrefilterDiffuseIBL(diffuseData));
                    var specularResult = Task.Run(() => PrefilterSpecularIBL(diffuseData));

                    diffuseIBLCubemap = texture.IBLPair ?? new();

                    diffuseResult.Wait();
                    IAssetImportSettings.CopyImportSettings(texture.ImportSettings, diffuseIBLCubemap.ImportSettings);
                    diffuseIBLCubemap.ImportSettings.Sources.Clear();
                    diffuseData.GetTextureInfo(diffuseIBLCubemap);
                    diffuseIBLCubemap.SetData(diffuseData.GetSlices(), diffuseData.GetIcon(), texture);
                    specularResult.Wait();
                }

                textureData.GetTextureInfo(texture);
                texture.SetData(textureData.GetSlices(), textureData.GetIcon(), diffuseIBLCubemap);
            }
            catch (Exception ex)
            {
                Logger.LogAsync(LogLevel.ERROR, $"Failed to import from {texture.FileName}");
                Debug.WriteLine(ex.Message);
            }
        }

        public static SliceArray3D Decompress(Texture texture)
        {
            Debug.Assert(texture.ImportSettings.Compress);

            using var textureData = new TextureData();

            try
            {
                textureData.GetTextureDataInfo(texture);
                textureData.ImportSettings.FromContentSettings(texture.ImportSettings);
                textureData.SetSubresourceData(texture.Slices);

                Decompress(textureData);

                if(textureData.Info.ImportError != 0)
                {
                    Logger.LogAsync(
                        LogLevel.ERROR,
                        $"Error: {EnumExtension.GetDescription((TextureImportError)textureData.Info.ImportError)}"
                    );

                    throw new Exception(
                        $"Error while trying to decompress image. Error code: {textureData.Info.ImportError}"
                    );
                }

                return textureData.GetSlices();
            }
            catch (Exception ex)
            {
                Logger.LogAsync(LogLevel.ERROR, $"Failed to decompress {texture.FileName}");
                Debug.WriteLine(ex.Message);

                return new();
            }
        }

        private static void GeometryFromSceneData(
            Geometry geometry,
            Action<SceneData> sceneDataGenerator,
            string failureMessage
        )
        {
            Debug.Assert(geometry is not null);

            using var sceneData = new SceneData();

            try
            {
                sceneData.ImportSettings.FromContentSettings(geometry);
                sceneDataGenerator(sceneData);

                if(sceneData.Data == IntPtr.Zero || sceneData.DataSize == 0) throw new Exception(failureMessage);

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
    }
}
