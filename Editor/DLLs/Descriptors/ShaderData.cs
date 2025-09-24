using System.Runtime.InteropServices;

namespace Editor.DLLs.Descriptors;

[StructLayout(LayoutKind.Sequential)]
public class ShaderData : IDisposable
{
    public int Type;
    public int CodeSize;
    public int ByteCodeSize;
    public int ErrorSize;
    public int AssemblySize;
    public int HashSize;
    public IntPtr Code;
    public IntPtr ByteCodeErrorAssemblyHash;
    public string? FunctionName;
    public string? ExtraArgs;

    public void Dispose()
    {
        Marshal.FreeCoTaskMem(ByteCodeErrorAssemblyHash);
        Marshal.FreeCoTaskMem(Code);
        GC.SuppressFinalize(this);
    }

    ~ShaderData()
    {
        Dispose();
    }
}
