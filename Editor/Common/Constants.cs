global using SliceArray3D = System.Collections.Generic.List<
    System.Collections.Generic.List<System.Collections.Generic.List<Editor.Content.Slice>>
>;
global using IdType = System.Int32;

namespace Editor.Common
{
    public static class Constants
    {
        public const string EngiveVersion = "v0.3.1";
        public const string EnginePath = "$(LIGHTNING_ENGINE)";
        public const string DefaultMaterialSpecularColor = "#253F4B";
        public const string DefaultMaterialDiffuseColor = "#BFCBD1";
    }
}
