using System.Runtime.InteropServices;

namespace Editor.DLLs.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    class ScriptComponentDescriptor
    {
        public IntPtr ScriptCreator;
    }
}
