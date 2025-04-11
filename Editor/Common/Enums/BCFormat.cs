using System.ComponentModel;

namespace Editor.Common.Enums
{
    public enum BCFormat : int
    {
        [Description("Pick Best Fit")]
        DXGI_FORMAT_UNKNOWN = 0,

        [Description("BC1 (RGBA) Low Quality Aplpha")]
        DXGI_FORMAT_BC1_UNORM = 71,

        [Description("BC3 (RGBA) Medium Quality")]
        DXGI_FORMAT_BC3_UNORM = 77,

        [Description("BC4 (R8) Single-Channel Gray")]
        DXGI_FORMAT_BC4_UNORM = 80,

        [Description("BC5 (R8G8) Dual-Channel Gray")]
        DXGI_FORMAT_BC5_UNORM = 83,

        [Description("BC6 (UF15) HDR")]
        DXGI_FORMAT_BC6H_UF16 = 95,

        [Description("BC7 (RGBA) High Quality")]
        DXGI_FORMAT_BC7_UNORM = 98,
    }
}
