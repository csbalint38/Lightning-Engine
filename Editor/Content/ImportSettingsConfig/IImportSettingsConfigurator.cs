namespace Editor.Content.ImportSettingsConfig
{
    internal interface IImportSettingsConfigurator<T> where T : AssetProxy
    {
        void AddFiles(IEnumerable<string> files, string destinationFolder);
        void RemoveFile(T proxy);
        void Import();
    }
}
