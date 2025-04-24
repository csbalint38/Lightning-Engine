using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Editor.Content.ImportSettingsConfig
{
    public class TextureProxy : AssetProxy
    {
        private readonly ObservableCollection<TextureProxy> _sources = new();

        public override TextureImportSettings ImportSettings { get; } = new();

        public ReadOnlyObservableCollection<TextureProxy> Sources { get; }

        public TextureProxy(string fileName, string destinationFolder) : base(fileName, destinationFolder)
        {
            _sources.Add(this);
            Sources = new(_sources);
        }

        public override void CopySettings(IAssetImportSettings settings)
        {
            Debug.Assert(settings is TextureImportSettings);

            if (settings is TextureImportSettings textureImportSettings)
            {
                IAssetImportSettings.CopyImportSettings(textureImportSettings, ImportSettings);
            }

            foreach (var source in Sources.Skip(1)) source.CopySettings(settings);
        }

        public bool AddProxy(TextureProxy proxy)
        {
            if (!_sources.Any(x => x.FileInfo.FullName == proxy.FileInfo.FullName) && proxy.Sources.Count == 1)
            {
                _sources.Add(proxy);
                return true;
            }

            return false;
        }

        public void RemoveProxy(TextureProxy proxy)
        {
            if (proxy != this) _sources.Remove(proxy);
        }

        public void MoveUp(List<TextureProxy> proxies)
        {
            proxies.Remove(this);

            if (proxies.Count == 0) return;

            var toIndex = Math.Max(proxies.Select(x => _sources.IndexOf(x)).Min() - 1, 1);

            foreach (var proxy in proxies)
            {
                var index = _sources.IndexOf(proxy);

                if (index != toIndex) _sources.Move(index, toIndex);

                ++toIndex;
            }
        }

        public void MoveDown(List<TextureProxy> proxies)
        {
            proxies.Remove(this);

            if (proxies.Count == 0) return;

            var toIndex = Math.Min(proxies.Select(x => _sources.IndexOf(x)).Max() + 1, _sources.Count - 1);

            foreach (var proxy in proxies)
            {
                var index = _sources.IndexOf(proxy);

                if (index != toIndex) _sources.Move(index, toIndex);
            }
        }
    }
}
