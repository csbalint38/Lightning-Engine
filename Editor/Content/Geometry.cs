using Editor.Common.Enums;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Editor.Content
{
    public class Geometry : Asset
    {
        private readonly List<LODGroup> _lodGroups = [];

        public Geometry() : base(AssetType.MESH) { }

        public void FromRawData(byte[] data)
        {
            Debug.Assert(data?.Length > 0);

            _lodGroups.Clear();

            using var reader = new BinaryReader(new MemoryStream(data));

            var s = reader.ReadInt32();
            reader.BaseStream.Position += s;

            var numLodGroups = reader.ReadInt32();

            Debug.Assert(numLodGroups > 0);

            for (int i = 0; i < numLodGroups; ++i)
            {
                s = reader.ReadInt32();
                string lodGroupName;

                if (s > 0)
                {
                    var nameBytes = reader.ReadBytes(s);
                    lodGroupName = Encoding.UTF8.GetString(nameBytes);
                }
                else lodGroupName = $"lod_{RandomString.GetRandomString()}";

                var numMeshes = reader.ReadInt32();

                Debug.Assert(numMeshes > 0);

                List<MeshLOD> lods = ReadMeshLODs(numMeshes, reader);
                var lodGroup = new LODGroup { Name = lodGroupName };

                lods.ForEach(l => lodGroup.LODs.Add(l));
                _lodGroups.Add(lodGroup);
            }
        }

        public LODGroup GetLodGroup(int lodGroup = 0)
        {
            Debug.Assert(lodGroup >= 0 && lodGroup < _lodGroups.Count);

            return _lodGroups.Any() ? _lodGroups[lodGroup] : null;
        }

        private static List<MeshLOD> ReadMeshLODs(int numMeshes, BinaryReader reader)
        {
            var lodIds = new List<int>();
            var lodList = new List<MeshLOD>();

            for (int i = 0; i < numMeshes; ++i)
            {
                ReadMeshes(reader, lodIds, lodList);
            }

            return lodList;
        }

        private static void ReadMeshes(BinaryReader reader, List<int> lodIds, List<MeshLOD> lodList)
        {
            var s = reader.ReadInt32();
            string meshName;

            if (s > 0)
            {
                var nameBytes = reader.ReadBytes(s);
                meshName = Encoding.UTF8.GetString(nameBytes);
            }
            else meshName = $"mesh_{RandomString.GetRandomString()}";

            var mesh = new Mesh();
            var lodId = reader.ReadInt32();

            mesh.VertexSize = reader.ReadInt32();
            mesh.VertexCount = reader.ReadInt32();
            mesh.IndexSize = reader.ReadInt32();
            mesh.IndexCount = reader.ReadInt32();

            var lodThreshold = reader.ReadSingle();
            var vertexBufferSize = mesh.VertexSize * mesh.VertexCount;
            var indexBufferSize = mesh.IndexSize * mesh.IndexCount;

            mesh.Verticies = reader.ReadBytes(vertexBufferSize);
            mesh.Indicies = reader.ReadBytes(indexBufferSize);

            MeshLOD lod;

            if (Id.IsValid(lodId) && lodIds.Contains(lodId))
            {
                lod = lodList[lodIds.IndexOf(lodId)];

                Debug.Assert(lod is not null);
            }
            else
            {
                lodIds.Add(lodId);
                lod = new MeshLOD() { Name = meshName, LODThreshold = lodThreshold };
                lodList.Add(lod);
            }

            lod.Meshes.Add(mesh);
        }
    }
}
