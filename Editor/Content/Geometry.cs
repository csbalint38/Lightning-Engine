using Editor.Common.Enums;
using Editor.Content.ContentBrowser;
using Editor.Content.ImportSettingsConfig;
using Editor.DLLs;
using Editor.Editors;
using Editor.GameProject;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Editor.Content;

public class Geometry : Asset
{
    private static readonly Lock _lock = new();

    private readonly List<LODGroup> _lodGroups = [];

    public static AssetInfo? Default = DefaultAssets.DefaultGeometry;

    public GeometryImportSettings ImportSettings { get; } = new GeometryImportSettings();

    public Geometry() : base(AssetType.MESH) { }

    public Geometry(IAssetImportSettings importSettings) : this()
    {
        Debug.Assert(importSettings is GeometryImportSettings);

        ImportSettings = (GeometryImportSettings)importSettings;
    }

    public Geometry(AssetInfo assetInfo) : this()
    {
        Debug.Assert(assetInfo is not null && assetInfo.Guid != Guid.Empty);
        Debug.Assert(File.Exists(assetInfo.FullPath) && assetInfo.Type == Type);

        Load(assetInfo.FullPath);
    }

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

    public LODGroup? GetLodGroup(int lodGroup = 0)
    {
        Debug.Assert(lodGroup >= 0 && lodGroup < _lodGroups.Count);

        return (lodGroup < _lodGroups.Count) ? _lodGroups[lodGroup] : null;
    }

    public override IEnumerable<string> Save(string file)
    {
        Debug.Assert(_lodGroups.Count != 0);

        var savedFiles = new List<string>();

        if (_lodGroups.Count == 0) return savedFiles;

        var path = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
        var fileName = Path.GetFileNameWithoutExtension(file);

        try
        {
            foreach (var lodGroup in _lodGroups)
            {
                Debug.Assert(lodGroup.LODs.Count > 0);

                var meshFileName = ContentHelper.SanitizeFileName(
                    path + fileName + ((_lodGroups.Count > 1) ?
                            '_' + ((lodGroup.LODs.Count > 1) ?
                            lodGroup.Name :
                        lodGroup.LODs[0].Name) :
                        string.Empty))
                    + AssetFileExtension;

                Guid = TryGetAssetInfo(meshFileName) is AssetInfo info && info.Type == Type ? info.Guid : Guid.NewGuid();
                byte[]? data = null;

                using (var writer = new BinaryWriter(new MemoryStream()))
                {
                    writer.Write(lodGroup.Name);
                    writer.Write(lodGroup.LODs.Count);

                    var hashes = new List<byte>();

                    foreach (var lod in lodGroup.LODs)
                    {
                        LODToBinary(lod, writer, out var hash);
                        hashes.AddRange(hash);
                    }

                    Hash = ContentHelper.ComputeHash([.. hashes]);
                    data = (writer.BaseStream as MemoryStream)?.ToArray();
                    Icon = GenerateIcons(lodGroup.LODs[0])[0];
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

            FullPath = file;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Logger.LogAsync(LogLevel.ERROR, $"Failed to save Geometry to {file}");
        }

        return savedFiles;
    }

    public override bool Import(string file)
    {
        Debug.Assert(File.Exists(file));
        Debug.Assert(!string.IsNullOrEmpty(FullPath));

        var ext = Path.GetExtension(file).ToLower();

        if (ext == ".fbx") return ImportFbx(file);

        return false;

    }

    public override bool Load(string file)
    {
        Debug.Assert(File.Exists(file));
        Debug.Assert(Path.GetExtension(file).ToLower() == AssetFileExtension);

        try
        {
            byte[]? data = null;

            using (var reader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read)))
            {
                ReadAssetFileHeader(reader);
                ImportSettings.FromBinary(reader);
                int dataLength = reader.ReadInt32();

                Debug.Assert(dataLength > 0);

                data = reader.ReadBytes(dataLength);
            }

            Debug.Assert(data?.Length > 0);

            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                LODGroup lodGroup = new();
                lodGroup.Name = reader.ReadString();

                var lodCount = reader.ReadInt32();

                for (int i = 0; i < lodCount; ++i)
                {
                    lodGroup.LODs.Add(BinaryToLOD(reader));
                }

                _lodGroups.Clear();
                _lodGroups.Add(lodGroup);
            }

            // TEMP
            // PackForEngine();
            // ENDTEMP

            return true;

        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Logger.LogAsync(LogLevel.ERROR, $"Failed to load geometry asset from file: {file}");
        }

        return false;
    }

    /// <summary>
    /// Packs the geometry into a byte array wich can be used by the Engine.
    /// </summary>
    /// <returns>
    /// struct {
    ///     u32 lod_count,
    ///     
    ///     struct {
    ///         f32 lod_threshold,
    ///         u32 submesh_count,
    ///         u32 size_of_submeshes,
    ///         
    ///         struct {
    ///             u32 element_size,
    ///             u32 vertex_count,
    ///             u32 index_count,
    ///             u32 elements_type,
    ///             u32 primitive_topology,
    ///             u8 positions[sizeof(f32) * 3 * vertex_count],
    ///             u8 elements[sizeof(element_size) * vertex_count],
    ///             u8 indices[index_size * index_count]
    ///         } Submeshes[submesh_vount]
    ///     } MeshLods[lod_count]
    /// } Geometry
    /// </returns>
    public override byte[] PackForEngine()
    {
        byte[]? data = null;

        using var writer = new BinaryWriter(new MemoryStream());
        writer.Write(GetLodGroup()!.LODs.Count);

        foreach (var lod in GetLodGroup()!.LODs)
        {
            writer.Write(lod.LODThreshold);
            writer.Write(lod.Meshes.Count);

            var sizeOfSubmeshesPosition = writer.BaseStream.Position;

            writer.Write(0);

            foreach (var mesh in lod.Meshes)
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
        // using (var fs = new FileStream(@"..\..\x64\model.model", FileMode.Create))
        // {
        //     fs.Write(data, 0, data.Length);
        // }
        // ENDTEMP

        return data;
    }

    public override GeometryMetadata GetMetadata()
    {
        var lodGroup = GetLodGroup();

        if (lodGroup is null) return new()
        {
            LODs = []
        };

        var lods = new List<LodInfo>();

        foreach (var lod in lodGroup.LODs)
        {
            LodInfo lodInfo = new LodInfo()
            {
                Name = lod.Name,
                Threshold = lod.LODThreshold,
                Meshes = []
            };

            lods.Add(lodInfo);

            foreach (var mesh in lod.Meshes)
            {
                MeshInfo meshInfo = new()
                {
                    Name = mesh.Name,
                    Icon = Icon,
                    IndexCount = mesh.IndexCount,
                    TriangleCount = mesh.IndexCount / 3,
                    VertexCount = mesh.VertexCount,
                };

                lodInfo.Meshes.Add(meshInfo);
            }

            _ = Task.Run(() => GenerateAndSetIcons(lodInfo.Meshes, lod));
        }

        return new()
        {
            Name = lodGroup.Name,
            LODs = lods
        };
    }

    internal static List<byte[]> GenerateIcons(MeshLOD lod, bool createIconPerSubmesh = false)
    {
        var ready = new AutoResetEvent(false);
        var width = ContentInfo.IconWidth * 4;
        var iconList = new List<byte[]>();
        var color = (Color)Application.Current.FindResource("Editor.Background4");

        Thread thread = new(() =>
        {
            Dispatcher.CurrentDispatcher.UnhandledException += (s, e) =>
                Debug.WriteLine($"Dispatcher Exception: {e.Exception.Message}");

            SynchronizationContext.SetSynchronizationContext(
                new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher)
            );

            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                try
                {
                    GeometryView view = new()
                    {
                        Background = new SolidColorBrush(color),
                        DataContext = new MeshRenderer(lod, null),
                        Width = width,
                        Height = width,
                    };

                    for (int i = createIconPerSubmesh ? 0 : -1; i < lod.Meshes.Count; ++i)
                    {
                        view.SetGeometry(i);
                        view.Measure(new Size(width, width));
                        view.Arrange(new Rect(0, 0, width, width));
                        view.UpdateLayout();

                        var bmp = new RenderTargetBitmap(width, width, 96, 96, PixelFormats.Default);

                        bmp.Render(view);
                        iconList.Add(BitmapHelper.CreateThumbnail(bmp, ContentInfo.IconWidth, ContentInfo.IconWidth));

                        if (i == -1) break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Renderer Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    throw;
                }
                finally
                {
                    ready.Set();

                    Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                }
            });

            Dispatcher.Run();
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        ready.WaitOne();
        thread.Join();

        return iconList;
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
        var elementsBufferSize = mesh.ElementSize * mesh.VertexCount;
        var indexBufferSize = mesh.IndexSize * mesh.IndexCount;

        mesh.Positions = reader.ReadBytes(Mesh.PositionSize * mesh.VertexCount);
        mesh.Elements = reader.ReadBytes(elementsBufferSize);
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

    private static void GenerateAndSetIcons(List<MeshInfo> meshes, MeshLOD lod)
    {
        var iconList = GenerateIcons(lod, true);
        var index = 0;

        meshes.ForEach(mesh => mesh.Icon = iconList[index++]);
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

        var buffer = (writer.BaseStream as MemoryStream)?.ToArray() ?? [];
        hash = ContentHelper.ComputeHash(buffer, (int)meshDataBegin, (int)meshDataSize)!;
    }

    private MeshLOD BinaryToLOD(BinaryReader reader)
    {
        var lod = new MeshLOD();

        lod.Name = reader.ReadString();
        lod.LODThreshold = reader.ReadSingle();

        var meshCount = reader.ReadInt32();

        for (int i = 0; i < meshCount; ++i)
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

    private bool ImportFbx(string file)
    {
        Logger.LogAsync(LogLevel.INFO, $"Importing FBX file {file}");

        var tempPath = Application.Current.Dispatcher.Invoke(() => Project.Current?.TempFolder);

        if (string.IsNullOrEmpty(tempPath)) return false;

        lock (_lock)
        {
            if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
        }

        var tempFile = $"{tempPath}{RandomString.GetRandomString()}.fbx";

        File.Copy(file, tempFile, true);

        bool result = false;

        try
        {
            ContentToolsAPI.ImportFbx(tempFile, this);
            result = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            var msg = $"Failed to read {file} for import";
            Debug.WriteLine(msg);
            Logger.LogAsync(LogLevel.ERROR, msg);
        }

        if (ImportSettings.ImportEmbeddedTextures)
        {
            var embeddedMediaDir = $@"{tempPath}{Path.GetFileNameWithoutExtension(tempFile)}.fbm{Path.DirectorySeparatorChar}";

            if (Directory.Exists(embeddedMediaDir))
            {
                Debug.Assert(!string.IsNullOrEmpty(FullPath));

                var files = Directory.GetFiles(embeddedMediaDir);

                new ConfigureImportSettings(files, FullPath).Import();
            }
        }

        return result;
    }
}
