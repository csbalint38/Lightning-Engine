using Editor.Common.Enums;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Editor.DLLs.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    internal class PrimitiveInitInfo
    {
        public PrimitiveMeshType Type;
        public int SegmentX = 1;
        public int SegmentY = 1;
        public int SegmentZ = 1;
        public Vector3 Size = new(1f);
        public int LOD = 0;
    }
}
