using Editor.Common.Enums;
using Editor.Editors;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Editor.Content
{
    public class Geometry : Asset
    {
        private readonly List<LODGroup> _lodGroups = [];

        public GeometryImportSettings ImportSettings { get; } = new GeometryImportSettings();

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

        public override IEnumerable<string> Save(string file)
        {
            Debug.Assert(_lodGroups.Any());
            
            var savedFiles = new List<string>();

            if(!_lodGroups.Any()) return savedFiles;

            var path = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
            var fileName = Path.GetFileNameWithoutExtension(file);

            try
            {
                foreach(var lodGroup in _lodGroups)
                {
                    Debug.Assert(lodGroup.LODs.Any());

                    var meshFileName = ContentHelper.SanitizeFileName(path + fileName + AssetFileExtension);
                    Guid = Guid.NewGuid();
                    byte[] data = null;

                    using(var writer = new BinaryWriter(new MemoryStream()))
                    {
                        writer.Write(lodGroup.Name);
                        writer.Write(lodGroup.LODs.Count);

                        var hashes = new List<byte>();

                        foreach(var lod in lodGroup.LODs)
                        {
                            LODToBinary(lod, writer, out var hash);
                            hashes.AddRange(hash);
                        }

                        Hash = ContentHelper.ComputeHash([.. hashes]);
                        data = (writer.BaseStream as MemoryStream).ToArray();
                        Icon = GenerateIcon(lodGroup.LODs[0]);
                    }

                    Debug.Assert(data?.Length > 0);

                    using (var writer = new BinaryWriter(File.Open(meshFileName, FileMode.Create, FileAccess.Write)))
                    {
                        WriteAssetFileHeader(writer);
                        ImportSettings.ToBinary(writer);
                        writer.Write(data.Length);
                        writer.Write(data);
                    }

                    savedFiles.Add(meshFileName);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.LogAsync(LogLevel.ERROR, $"Failed to save Geometry to {file}");
            }

            return savedFiles;
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

        private void LODToBinary(MeshLOD lod, BinaryWriter writer, out byte[] hash)
        {
            writer.Write(lod.Name);
            writer.Write(lod.LODThreshold);
            writer.Write(lod.Meshes.Count);

            var meshDataBegin = writer.BaseStream.Position;

            foreach (var mesh in lod.Meshes)
            {
                writer.Write(mesh.VertexSize);
                writer.Write(mesh.VertexCount);
                writer.Write(mesh.IndexSize);
                writer.Write(mesh.IndexCount);
                writer.Write(mesh.Verticies);
                writer.Write(mesh.Indicies);
            }

            var meshDataSize = writer.BaseStream.Position - meshDataBegin;

            Debug.Assert(meshDataSize > 0);

            var buffer = (writer.BaseStream as MemoryStream).ToArray();
            hash = ContentHelper.ComputeHash(buffer, (int)meshDataBegin, (int)meshDataSize);
        }

        private byte[] GenerateIcon(MeshLOD lod)
        {
            var width = 90 * 4; // width * sampling

            BitmapSource bmp = null;

            Application.Current.Dispatcher.Invoke(() =>
            {
                bmp = GeometryView.RenderToBitmap(new MeshRenderer(lod, null), width, width);
                bmp = new TransformedBitmap(bmp, new ScaleTransform(0.25, 0.25, 0.25, 0.25));
            });

            using var memStream = new MemoryStream();
            memStream.SetLength(0);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(memStream);

            return memStream.ToArray();
        }
    }
}
