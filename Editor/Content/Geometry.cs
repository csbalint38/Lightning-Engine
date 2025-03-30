using Editor.Common.Enums;
using System.Diagnostics;

namespace Editor.Content
{
    internal class Geometry : Asset
    {
        public Geometry() : base(AssetType.MESH) { }

        internal void FromRawData(byte[] data)
        {
            Debug.Assert(data?.Length > 0);
        }
    }
}
