using System.Diagnostics;

namespace Editor.Content.ImportSettingsConfig
{
    public class TextureProxy : AssetProxy
    {
        public override TextureImportSettings ImportSettings { get; } = new();

        public TextureProxy(string fileName, string destinationFolder) : base(fileName, destinationFolder)
        {
        }

        public override void CopySettings(IAssetImportSettings settings)
        {
            Debug.Assert(settings is TextureImportSettings);

            if (settings is TextureImportSettings textureImportSettings)
            {
                IAssetImportSettings.CopyImportSettings(textureImportSettings, ImportSettings);
            }
        }
    }
}
