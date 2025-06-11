global using IdType = System.Int32;
global using SliceArray3D = System.Collections.Generic.List<
    System.Collections.Generic.List<System.Collections.Generic.List<Editor.Content.Slice>>
>;
using System.Reflection;

namespace Editor.Common
{
    public static class Constants
    {
        public static readonly string? EngineVersion =
            Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
                .Split('+')[0];
        public const string EnginePath = "$(LIGHTNING_ENGINE)";
        public const string DefaultMaterialSpecularColor = "#253F4B";
        public const string DefaultMaterialDiffuseColor = "#BFCBD1";
    }
}
