
using Editor.Common.Enums;
using Editor.Content.ContentBrowser;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace Editor.Content
{
    public class Material : Asset
    {
        private readonly Dictionary<ShaderType, ShaderGroup> _shaders = [];
        private MaterialType _materialType;
        private MaterialMode _materialMode;

        public static AssetInfo Default => DefaultAssets.DefaultMaterial;

        public MaterialSurface MaterialSurface { get; } = new();
        public DefaultMaterialInputs DefaultMaterialInputs { get; } = new();
        public NodeMaterial NodeMaterial { get; } = new();
        public CodeMaterial CodeMaterial { get; } = new();

        public MaterialType MaterialType
        {
            get => _materialType;
            set
            {
                if (_materialType != value)
                {
                    _materialType = value;
                    OnPropertyChanged(nameof(MaterialType));
                }
            }
        }

        public MaterialMode MaterialMode
        {
            get => _materialMode;
            set
            {
                if (_materialMode != value)
                {
                    _materialMode = value;
                    OnPropertyChanged(nameof(MaterialMode));
                }
            }
        }

        public Material() : base(AssetType.MATERIAL) { }

        public Material(AssetInfo assetInfo) : this()
        {
            Debug.Assert(assetInfo is not null && assetInfo.Guid != Guid.Empty);
            Debug.Assert(File.Exists(assetInfo.FullPath) && assetInfo.Type == Type);

            Load(assetInfo.FullPath);
        }

        public List<MaterialInput> GetInput() => _materialMode switch
        {
            MaterialMode.NO_INPUT => [],
            MaterialMode.DEFAULT => DefaultMaterialInputs.GetInputs(),
            MaterialMode.NODE => NodeMaterial.GetInputs(),
            MaterialMode.CODE => CodeMaterial.GetInputs(),
            _ => throw new NotImplementedException()
        };

        public override bool Import(string file) => throw new NotImplementedException();

        public override bool Load(string file)
        {
            Debug.Assert(File.Exists(file));
            Debug.Assert(Path.GetExtension(file).ToLower() == AssetFileExtension);

            if (!File.Exists(file)) return false;

            try
            {
                using var reader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read));

                ReadAssetFileHeader(reader);

                var shaderGroupCount = reader.ReadInt32();

                _shaders.Clear();

                for (int i = 0; i < shaderGroupCount; ++i)
                {
                    var shaderGroup = new ShaderGroup();

                    shaderGroup.FromBinary(reader);

                    Debug.Assert(!_shaders.ContainsKey(shaderGroup.Type));

                    _shaders.Add(shaderGroup.Type, shaderGroup);
                }

                MaterialType = (MaterialType)reader.ReadInt32();
                MaterialMode = (MaterialMode)reader.ReadInt32();

                MaterialSurface.FromBinary(reader);
                DefaultMaterialInputs.FromBinary(reader);
                //NodeMaterial.FromBinary(reader);
                //CodeMaterial.FromBinary(reader);

                FullPath = file;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.LogAsync(LogLevel.ERROR, $"Failed to load material asset from file: {file}");
            }

            return false;
        }

        public override byte[] PackForEngine()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> Save(string file)
        {
            try
            {
                if (TryGetAssetInfo(file) is AssetInfo info && info.Type == Type) Guid = info.Guid;

                var bmp = new BitmapImage();

                bmp.BeginInit();
                bmp.UriSource = new Uri("pack://application:,,,/Resources/TextureEditor/Checkerboard.png");
                bmp.DecodePixelWidth = ContentInfo.IconWidth;
                bmp.EndInit();

                Icon = BitmapHelper.CreateThumbnail(bmp, ContentInfo.IconWidth, ContentInfo.IconWidth);

                using var writer = new BinaryWriter(File.Open(file, FileMode.Create, FileAccess.Write));

                WriteAssetFileHeader(writer);

                writer.Write(_shaders.Count);

                foreach (var (_, shaderGroup) in _shaders)
                {
                    shaderGroup.ToBinary(writer);
                }

                writer.Write((int)MaterialType);
                writer.Write((int)MaterialMode);

                MaterialSurface.ToBinary(writer);
                DefaultMaterialInputs.ToBinary(writer);
                // NodeMaterial.ToBinary(writer);
                // CodeMaterial.ToBinary(writer);

                FullPath = file;

                Logger.LogAsync(LogLevel.INFO, $"Saved material to {file}");

                var savedFile = new List<string>()
                {
                    file
                };

                return savedFile;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.LogAsync(LogLevel.ERROR, $"Failed to save material to: {file}");

                return [];
            }
        }

        public override AssetMetadata GetMetadata() => throw new NotImplementedException();

        public bool AddShaderGroup(ShaderGroup shaderGroup)
        {
            Debug.Assert(shaderGroup is not null && !_shaders.ContainsKey(shaderGroup.Type));

            return _shaders.TryAdd(shaderGroup.Type, shaderGroup);
        }

        public ShaderGroup GetShaderGroup(ShaderType type)
        {
            _shaders.TryGetValue(type, out var shaderGroup);

            return shaderGroup;
        }
    }
}
