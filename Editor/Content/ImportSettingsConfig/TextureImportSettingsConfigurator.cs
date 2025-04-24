using Editor.Common;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;

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

            foreach (var proxy in _textureProxies)
            {
                proxy.ImportSettings.Sources.Clear();

                foreach (var source in proxy.Sources) proxy.ImportSettings.Sources.Add(source.FileInfo.FullName);
            }

            _ = ContentHelper.ImportFilesAsync(_textureProxies);
            _textureProxies.Clear();
        }

        public void RemoveFile(TextureProxy proxy) => _textureProxies.Remove(proxy);

        public void MoveToTarget(TextureProxy proxy, TextureProxy target)
        {
            if (proxy != target && proxy.Sources.Count == 1 && target.AddProxy(proxy))
            {
                _textureProxies.Remove(proxy);
            }
        }

        public void MoveFromTarget(TextureProxy proxy, TextureProxy target)
        {
            if (proxy != target)
            {
                Debug.Assert(proxy.Sources.Count == 1);

                target.RemoveProxy(proxy);

                if (!_textureProxies.Any(x => x.FileInfo.FullName == proxy.FileInfo.FullName))
                {
                    _textureProxies.Add(proxy);
                }
            }
        }
    }
}
