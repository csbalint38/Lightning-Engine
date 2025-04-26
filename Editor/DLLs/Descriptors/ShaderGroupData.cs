using System.Runtime.InteropServices;

namespace Editor.DLLs.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    public class ShaderGroupData : IDisposable
    {
        public int Type;
        public int Count;
        public int DataSize;
        public IntPtr Data;

        public void Dispose()
        {
            Marshal.FreeCoTaskMem(Data);
            GC.SuppressFinalize(this);
        }

        ~ShaderGroupData()
        {
            Dispose();
        }
    }
}
