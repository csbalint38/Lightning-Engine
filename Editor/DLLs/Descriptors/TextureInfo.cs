using System.Runtime.InteropServices;

namespace Editor.DLLs.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public class TextureInfo
    {
        public int Width;
        public int Height;
        public int ArraySize;
        public int MipLevels;
        public int Format;
        public int ImportError;
        public int Flags;

        public TextureInfo Clone() => new()
        {
            Width = Width,
            Height = Height,
            ArraySize = ArraySize,
            MipLevels = MipLevels,
            Format = Format,
            ImportError = ImportError,
            Flags = Flags,
        };
    }
}
