using Editor.Common;
using Editor.Common.Enums;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace Editor.Content
{
    [DataContract]
    abstract public class Asset : ViewModelBase
    {
        private string _fullPath;

        public const string AssetFileExtension = ".lngasset";

        [DataMember]
        public AssetType Type { get; private set; }

        public byte[] Icon { get; protected set; }

        [DataMember]
        public Guid Guid { get; protected set; } = Guid.NewGuid();

        public DateTime ImportDate { get; protected set; }
        public byte[] Hash { get; protected set; }
        public string FileName => Path.GetFileNameWithoutExtension(FullPath);

        public string FullPath
        {
            get => _fullPath;
            set
            {
                if (_fullPath != value)
                {
                    _fullPath = value;
                    OnPropertyChanged(nameof(FullPath));
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }

        public abstract AssetMetadata GetMetadata();
        public abstract IEnumerable<string> Save(string file);
        public abstract bool Import(string file);
        public abstract bool Load(string file);
        public abstract byte[] PackForEngine();
        public virtual List<AssetInfo> GetReferencedAssets() => [];

        protected Asset(AssetType type)
        {
            Debug.Assert(type != AssetType.UNKNOWN);

            Type = type;
        }

        public static AssetInfo? TryGetAssetInfo(string file) => AssetRegistry.GetAssetInfo(file) ?? GetAssetInfo(file);

        public static AssetInfo GetAssetInfo(string file)
        {
            if (!File.Exists(file) || Path.GetExtension(file) != AssetFileExtension) return null;

            try
            {
                using var reader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read));
                var info = GetAssetInfo(reader);
                info.FullPath = file;

                return info;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        public AssetInfo GetAssetInfo() => new()
        {
            Type = Type,
            Icon = Icon,
            FullPath = FullPath,
            RegisterTime = AssetRegistry.GetAssetInfo(Guid)?.RegisterTime ?? default,
            ImportDate = ImportDate,
            Guid = Guid,
            Hash = Hash,
        };

        protected void WriteAssetFileHeader(BinaryWriter writer)
        {
            var id = Guid.ToByteArray();
            var importDate = DateTime.Now.ToBinary();

            writer.BaseStream.Position = 0;

            writer.Write((int)Type);
            writer.Write(id.Length);
            writer.Write(id);
            writer.Write(importDate);

            if (Hash?.Length > 0)
            {
                writer.Write(Hash.Length);
                writer.Write(Hash);
            }
            else writer.Write(0);

            writer.Write(Icon.Length);
            writer.Write(Icon);
        }

        protected void ReadAssetFileHeader(BinaryReader reader)
        {
            var info = GetAssetInfo(reader);

            Debug.Assert(Type == info.Type);

            Guid = info.Guid;
            ImportDate = info.ImportDate;
            Hash = info.Hash;
            Icon = info.Icon;
        }

        private static AssetInfo GetAssetInfo(BinaryReader reader)
        {
            reader.BaseStream.Position = 0;

            var info = new AssetInfo();
            info.Type = (AssetType)reader.ReadInt32();
            var idSize = reader.ReadInt32();
            info.Guid = new Guid(reader.ReadBytes(idSize));
            info.ImportDate = DateTime.FromBinary(reader.ReadInt64());
            var hashSize = reader.ReadInt32();

            if (hashSize > 0) info.Hash = reader.ReadBytes(hashSize);

            var iconSize = reader.ReadInt32();
            info.Icon = reader.ReadBytes(iconSize);

            return info;
        }
    }
}
