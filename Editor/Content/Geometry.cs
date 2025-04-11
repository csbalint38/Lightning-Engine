using Editor.Common.Enums;
using Editor.Content.ContentBrowser;
using Editor.DLLs;
using Editor.Editors;
using Editor.GameProject;
using Editor.Utilities;
using MahApps.Metro.Controls;
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
        private readonly object _lock = new();

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

            return (lodGroup < _lodGroups.Count) ? _lodGroups[lodGroup] : null;
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

                    var meshFileName = ContentHelper.SanitizeFileName(
                        path + fileName + ((_lodGroups.Count > 1) ?
                                '_' + ((lodGroup.LODs.Count > 1) ?
                                lodGroup.Name :
                            lodGroup.LODs[0].Name) :
                            string.Empty))
                        + AssetFileExtension;

                    Guid = TryGetAssetInfo(meshFileName) is AssetInfo info && info.Type == Type ? info.Guid : Guid.NewGuid();
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

                    Logger.LogAsync(LogLevel.INFO, $"Saved geometry to {meshFileName}");
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

        public override void Import(string file)
        {
            Debug.Assert(File.Exists(file));
            Debug.Assert(!string.IsNullOrEmpty(FullPath));

            var ext = Path.GetExtension(file).ToLower();

            SourcePath = file;

            try
            {
                if (ext == ".fbx") ImportFbx(file);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);

                var msg = $"Failed to read {file} for import";

                Debug.WriteLine(msg);
                Logger.LogAsync(LogLevel.ERROR, msg);
            }
        }

        public override void Load(string file)
        {
            Debug.Assert(File.Exists(file));
            Debug.Assert(Path.GetExtension(file).ToLower() == AssetFileExtension);

            try
            {
                byte[] data = null;

                using (var reader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read)))
                {
                    ReadAssetFileHeader(reader);
                    ImportSettings.FromBinary(reader);
                    int dataLength = reader.ReadInt32();

                    Debug.Assert(dataLength > 0);

                    data = reader.ReadBytes(dataLength);
                }

                Debug.Assert(data.Length > 0);

                using (var reader = new BinaryReader(new MemoryStream(data)))
                {
                    LODGroup lodGroup = new();
                    lodGroup.Name = reader.ReadString();

                    var lodCount = reader.ReadInt32();

                    for(int i = 0; i < lodCount; ++i)
                    {
                        lodGroup.LODs.Add(BinaryToLOD(reader));
                    }

                    _lodGroups.Clear();
                    _lodGroups.Add(lodGroup);
                }

                // TEMP
                PackForEngine();
                // ENDTEMP

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.LogAsync(LogLevel.ERROR, $"Failed to load geometry asset from file: {file}");
            }
        }

        public override byte[] PackForEngine()
        {
            byte[] data = null;

            using var writer = new BinaryWriter(new MemoryStream());
            writer.Write(GetLodGroup().LODs.Count);

            foreach(var lod in GetLodGroup().LODs)
            {
                writer.Write(lod.LODThreshold);
                writer.Write(lod.Meshes.Count);

                var sizeOfSubmeshesPosition = writer.BaseStream.Position;

                writer.Write(0);

                foreach(var mesh in lod.Meshes)
                {
                    writer.Write(mesh.ElementSize);
                    writer.Write(mesh.VertexCount);
                    writer.Write(mesh.IndexCount);
                    writer.Write((int)mesh.ElementsType);
                    writer.Write((int)mesh.PrimitiveTopology);

                    var alignedPositionBuffer = new byte[MathUtilities.AlignSizeUp(mesh.Positions.Length, 4)];
                    Array.Copy(mesh.Positions, alignedPositionBuffer, mesh.Positions.Length);
                    var alignedElementBuffer = new byte[MathUtilities.AlignSizeUp(mesh.Elements.Length, 4)];
                    Array.Copy(mesh.Elements, alignedElementBuffer, mesh.Elements.Length);

                    writer.Write(alignedPositionBuffer);
                    writer.Write(alignedElementBuffer);
                    writer.Write(mesh.Indicies);
                }

                var endOfSubmeshes = writer.BaseStream.Position;
                var sizeOfSubmeshes = (int)(endOfSubmeshes - sizeOfSubmeshesPosition - sizeof(int));

                writer.BaseStream.Position = sizeOfSubmeshesPosition;
                writer.Write(sizeOfSubmeshes);
                writer.BaseStream.Position = endOfSubmeshes;
            }

            writer.Flush();

            data = (writer.BaseStream as MemoryStream)?.ToArray();

            Debug.Assert(data?.Length > 0);

            // TEMP
            using (var fs = new FileStream(@"..\..\x64\model.model", FileMode.Create))
            {
                fs.Write(data, 0, data.Length);
            }
            // ENDTEMP

                return data;
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

            var mesh = new Mesh()
            {
                Name = meshName
            };

            var lodId = reader.ReadInt32();

            mesh.ElementSize = reader.ReadInt32();
            mesh.ElementsType = (ElementsType)reader.ReadInt32();
            mesh.PrimitiveTopology = PrimitiveTopology.TRIANGLE_LIST;
            mesh.VertexCount = reader.ReadInt32();
            mesh.IndexSize = reader.ReadInt32();
            mesh.IndexCount = reader.ReadInt32();

            var lodThreshold = reader.ReadSingle();
            var elementBufferSize = mesh.ElementSize * mesh.VertexCount;
            var indexBufferSize = mesh.IndexSize * mesh.IndexCount;

            mesh.Positions = reader.ReadBytes(Mesh.PositionSize * mesh.VertexCount);
            mesh.Elements = reader.ReadBytes(elementBufferSize);
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
                writer.Write(mesh.Name);
                writer.Write(mesh.ElementSize);
                writer.Write((int)mesh.ElementsType);
                writer.Write((int)mesh.PrimitiveTopology);
                writer.Write(mesh.VertexCount);
                writer.Write(mesh.IndexSize);
                writer.Write(mesh.IndexCount);
                writer.Write(mesh.Positions);
                writer.Write(mesh.Elements);
                writer.Write(mesh.Indicies);
            }

            var meshDataSize = writer.BaseStream.Position - meshDataBegin;

            Debug.Assert(meshDataSize > 0);

            var buffer = (writer.BaseStream as MemoryStream).ToArray();
            hash = ContentHelper.ComputeHash(buffer, (int)meshDataBegin, (int)meshDataSize);
        }

        private MeshLOD BinaryToLOD(BinaryReader reader)
        {
            var lod = new MeshLOD();
           
            lod.Name = reader.ReadString();
            lod.LODThreshold = reader.ReadSingle();

            var meshCount = reader.ReadInt32();

            for(int i = 0; i < meshCount; ++i)
            {
                var mesh = new Mesh()
                {
                    Name = reader.ReadString(),
                    ElementSize = reader.ReadInt32(),
                    ElementsType = (ElementsType)reader.ReadInt32(),
                    PrimitiveTopology = (PrimitiveTopology)reader.ReadInt32(),
                    VertexCount = reader.ReadInt32(),
                    IndexSize = reader.ReadInt32(),
                    IndexCount = reader.ReadInt32(),
                };

                mesh.Positions = reader.ReadBytes(Mesh.PositionSize * mesh.VertexCount);
                mesh.Elements = reader.ReadBytes(mesh.ElementSize * mesh.VertexCount);
                mesh.Indicies = reader.ReadBytes(mesh.IndexSize * mesh.IndexCount);

                lod.Meshes.Add(mesh);
            }

            return lod;
        }

        private byte[] GenerateIcon(MeshLOD lod)
        {
            var width = ContentInfo.IconWidth * 4;

            using var memStream = new MemoryStream();
            BitmapSource bmp = null;

            Application.Current.Dispatcher.Invoke(() =>
            {
                bmp = GeometryView.RenderToBitmap(new MeshRenderer(lod, null), width, width);
                bmp = new TransformedBitmap(bmp, new ScaleTransform(0.25, 0.25, 0.25, 0.25));

                memStream.SetLength(0);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(memStream);
            });

            return memStream.ToArray();
        }

        private void ImportFbx(string file)
        {
            Logger.LogAsync(LogLevel.INFO, $"Importing FBX file {file}");

            var tempPath = Application.Current.Dispatcher.Invoke(() => Project.Current.TempFolder);

            if (string.IsNullOrEmpty(tempPath)) return;

            lock(_lock)
            {
                if(!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
            }

            var tempFile = $"{tempPath}{RandomString.GetRandomString()}.fbx";

            File.Copy(file, tempFile, true);
            ContentToolsAPI.ImportFbx(tempFile, this);
        }
    }
}
