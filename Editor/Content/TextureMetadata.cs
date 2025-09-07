using Editor.Common.Enums;

namespace Editor.Content
{
    public class TextureMetadata : AssetMetadata
    {
        public int Width { get; init; }
        public int Height { get; init; }
        public int DepthOrArraySize { get; init; }
        public int MipLevels { get; init; }
        public DXGIFormat Format { get; init; }
        public TextureDimension Dimension { get; init; }
    }
}
