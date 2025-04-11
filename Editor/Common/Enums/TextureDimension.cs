using System.ComponentModel;

namespace Editor.Common.Enums
{
    public enum TextureDimension : int
    {
        [Description("1D Texture")]
        TEXTURE_1D,

        [Description("2D Texture")]
        TEXTURE_2D,

        [Description("3D Texture")]
        TEXTURE_3D,

        [Description("Texture Cube")]
        TEXTURE_CUBE
    }
}
