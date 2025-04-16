namespace Editor.Common.Enums
{
    public enum TextureFlags : int
    {
        IS_HDR = 0x01,
        HAS_ALPHA = 0x02,
        IS_PREMULTIPLIED_ALPHA = 0x04,
        IS_IMPORTED_AS_NORMAL_MAP = 0x08,
        IS_CUBE_MAP = 0x10,
        IS_VOLUME_MAP = 0x20,
        IS_SRGB = 0x40,
    }
}
