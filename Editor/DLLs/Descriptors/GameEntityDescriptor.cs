using System.Runtime.InteropServices;

namespace Editor.DLLs.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    internal class GameEntityDescriptor : IDisposable
    {
        public TransformComponentDescriptor Transform = new();
        public ScriptComponentDescriptor Script = new();
        public GeometryComponent Geometry = new();

        public void Dispose()
        {
            Geometry.Dispose();

            GC.SuppressFinalize(this);
        }

        ~GameEntityDescriptor()
        {
            Dispose();
        }
    }
}
