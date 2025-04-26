namespace Editor.Common.Enums
{
    [Flags]
    public enum ShaderFlags : int
    {
        NONE = 0x0,
        VERTEX = 0x1,
        HULL = 0x2,
        DOMAIN = 0x4,
        GEOMETRY = 0x8,
        PIXEL = 0x10,
        COMPUTE = 0x20,
        AMPLIFICATION = 0x40,
        MESH = 0x80,
    }
}
