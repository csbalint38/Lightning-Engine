using Editor.Common.Enums;
using Editor.Content;
using Editor.DLLs.Descriptors;
using Editor.Utilities;
using System.Diagnostics;
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

        public static void CreatePrimitiveMesh(Geometry geometry, PrimitiveInitInfo info) =>
            GeometryFromSceneData(
                geometry,
                (sceneData) => CreatePrimitiveMesh(sceneData, info),
                $"Failed to create {info.Type} primitive mesh."
            );

        public static void ImportFbx(string file, Geometry geometry) =>
            GeometryFromSceneData(geometry, (sceneData) => ImportFbx(file, sceneData), $"Failed to import from FBX file: {file}");

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
    }
}
