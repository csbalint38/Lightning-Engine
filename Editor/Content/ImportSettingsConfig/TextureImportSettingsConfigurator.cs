using Editor.Common;
using Editor.Utilities;
using System.Collections.ObjectModel;

namespace Editor.Content.ImportSettingsConfig
{
    public class TextureImportSettingsConfigurator : ViewModelBase, IImportSettingsConfigurator<TextureProxy>
    {
        private readonly ObservableCollection<TextureProxy> _textureProxies = new();

        public ReadOnlyObservableCollection<TextureProxy> TextureProxies { get; }

        public TextureImportSettingsConfigurator()
        {
            TextureProxies = new(_textureProxies);
        }

        public void AddFiles(IEnumerable<string> files, string destinationFolder) =>
            files
                .Except(_textureProxies.Select(p => p.FileInfo.FullName))
                .ToList()
                .ForEach(f => _textureProxies.Add(new(f, destinationFolder)));

        public void Import()
        {
            if (!_textureProxies.Any()) return;

            _ = ContentHelper.ImportFilesAsync(_textureProxies);
            _textureProxies.Clear();
        }

        public void RemoveFile(TextureProxy proxy) => _textureProxies.Remove(proxy);
    }
}
