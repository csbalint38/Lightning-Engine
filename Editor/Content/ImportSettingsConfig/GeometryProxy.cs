using System.Diagnostics;

namespace Editor.Content.ImportSettingsConfig
{
    public class GeometryProxy : AssetProxy
    {
        public override GeometryImportSettings ImportSettings { get; } = new();

        public GeometryProxy(string fileName, string destinationFolder) : base(fileName, destinationFolder)
        {
        }

        public override void CopySettings(IAssetImportSettings settings)
        {
            Debug.Assert(settings is GeometryImportSettings);

            if (settings is GeometryImportSettings geometryImportSettings)
            {
                IAssetImportSettings.CopyImportSettings(geometryImportSettings, ImportSettings);
            }
        }
    }
}
