using System.ComponentModel;

namespace Editor.Common.Enums
{
    public enum EngineInitError : int
    {
        [Description("Engine initialization succeeded")]
        SUCCEEDED = 0,

        [Description("Unknown error occured during engine initialization")]
        UNKNOWN,

        [Description("Built-in shader compilation failed")]
        SHADER_COMPILATION,

        [Description("Graphics module initialization failed")]
        GRAPHICS,
    }
}
