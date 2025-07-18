using System.Runtime.InteropServices;

namespace Editor.DLLs.Descriptors
{
    [StructLayout(LayoutKind.Sequential)]
    internal class GameEntityDescriptor
    {
        public TransformComponentDescriptor Transform = new();
        public ScriptComponentDescriptor Script = new();
        public GeometryComponent Geometry = new();
    }
}
