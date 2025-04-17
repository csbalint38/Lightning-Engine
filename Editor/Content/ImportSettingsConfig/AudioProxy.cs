namespace Editor.Content.ImportSettingsConfig
{
    public class AudioProxy : AssetProxy
    {
        public AudioProxy(string fileName, string destinationFolder) : base(fileName, destinationFolder)
        {
        }

        public override IAssetImportSettings ImportSettings => throw new NotImplementedException();

        public override void CopySettings(IAssetImportSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
