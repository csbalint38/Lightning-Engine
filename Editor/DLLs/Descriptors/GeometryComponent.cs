using Editor.Components;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Editor.DLLs.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public class GeometryComponent : IDisposable
    {
        public IdType GeometryContentId = Id.InvalidId;
        public int MaterialCount;
        public IntPtr MaterialIds;

        public GeometryComponent() { }

        public GeometryComponent(Geometry geometry)
        {
            GeometryContentId = geometry.ContentId;
            MaterialCount = geometry.GeometryWithMaterials.LODs.Sum(x => x.Meshes.Count);

            Debug.Assert(MaterialCount == geometry.Materials.Count);

            byte[] data = null;

            using (var writer = new BinaryWriter(new MemoryStream()))
            {
                geometry.Materials.ForEach(material => writer.Write(material.UploadedAsset.ContentId));
                writer.Flush();

                data = (writer.BaseStream as MemoryStream).ToArray();
            }

            Debug.Assert(data?.Length == geometry.Materials.Count * sizeof(IdType));

            MaterialIds = Marshal.AllocCoTaskMem(data.Length);

            Marshal.Copy(data, 0, MaterialIds, data.Length);
        }

        public void Dispose()
        {
            Marshal.FreeCoTaskMem(MaterialIds);

            MaterialIds = IntPtr.Zero;

            GC.SuppressFinalize(this);
        }

        ~GeometryComponent()
        {
            Dispose();
        }
    }
}
