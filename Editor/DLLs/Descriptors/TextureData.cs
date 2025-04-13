using System.Runtime.InteropServices;

namespace Editor.DLLs.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public class TextureData : IDisposable
    {
        public IntPtr SubresourceData;
        public int SubresourceSize;
        public IntPtr Icon;
        public int IconSize;
        public TextureInfo Info = new();
        public TextureImportSettings ImportSettings = new();

        public void Dispose()
        {
            Marshal.FreeCoTaskMem(SubresourceData);
            Marshal.FreeCoTaskMem(Icon);
            GC.SuppressFinalize(this);
        }

        ~TextureData()
        {
            Dispose();
        }
    }
}
