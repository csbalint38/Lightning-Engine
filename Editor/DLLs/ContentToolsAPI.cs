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
        private const string _contentToolsDll = "CondentTools.dll";

        [DllImport(_contentToolsDll, EntryPoint = "create_primitive_mesh")]
        private static extern void CreatePrimitiveMesh([In, Out] SceneData data, PrimitiveInitInfo info);

        public static void CreatePrimitiveMesh(Geometry geometry, PrimitiveInitInfo info)
        {
            Debug.Assert(geometry is not null);

            using var sceneData = new SceneData();

            try
            {
                CreatePrimitiveMesh(sceneData, info);

                Debug.Assert(sceneData.Data != IntPtr.Zero && sceneData.DataSize > 0);

                var data = new byte[sceneData.DataSize];
                Marshal.Copy(sceneData.Data, data, 0, sceneData.DataSize);
                geometry.FromRawData(data);
            }
            catch (Exception ex)
            {
                Logger.LogAsync(LogLevel.ERROR, $"Failed to create {info.Type} primitive mesh.");
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
