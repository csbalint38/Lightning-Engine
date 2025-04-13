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
    }
}
