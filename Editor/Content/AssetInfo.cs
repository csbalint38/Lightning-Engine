using Editor.Common.Enums;
using System.IO;

namespace Editor.Content
{
    public sealed class AssetInfo
    {
        public AssetType Type { get; set; }
        public byte[] Icon { get; set; }
        public string FullPath { get; set; }
        public string FileName => Path.GetFileNameWithoutExtension(FullPath);
        public string? SourcePath { get; set; }
        public DateTime RegisterTime { get; set; }
        public DateTime ImportDate { get; set; }
        public Guid Guid { get; set; }
        public byte[] Hash { get; set; }
    }
}
