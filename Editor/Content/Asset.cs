using Editor.Common;
using Editor.Common.Enums;
using System.Diagnostics;
using System.IO;

namespace Editor.Content
{
    abstract public class Asset : ViewModelBase
    {
        public const string AssetFileExtension = ".lasset";
        public AssetType Type { get; private set; }
        public byte[] Icon { get; protected set; }
        public string SourcePath { get; protected set; }
        public Guid Guid { get; protected set; } = Guid.NewGuid();
        public DateTime ImportDate { get; protected set; }
        public byte[] Hash { get; protected set; }

        protected void WriteAssetFileHeader(BinaryWriter writer)
        {
            var id = Guid.ToByteArray();
            var importDate = DateTime.Now.ToBinary();

            writer.BaseStream.Position = 0;
            
            writer.Write((int)Type);
            writer.Write(id.Length);
            writer.Write(id);
            writer.Write(importDate);

            if(Hash?.Length > 0)
            {
                writer.Write(Hash.Length);
                writer.Write(Hash);
            }
            else writer.Write(0);

            writer.Write(SourcePath ?? "");
            writer.Write(Icon.Length);
            writer.Write(Icon);
        }

        public Asset(AssetType type)
        {
            Debug.Assert(type != AssetType.UNKNOWN);

            Type = type;
        }

        public abstract IEnumerable<string> Save(string file);
    }
}
