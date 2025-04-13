using Editor.Content.ContentBrowser;
using Editor.Content.ContentBrowser.Descriptors;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace Editor.Content
{
    static class AssetRegistry
    {
        private static readonly Dictionary<string, AssetInfo> _assetDict = [];
        private static readonly ObservableCollection<AssetInfo> _assets = [];
        private static readonly DelayEventTimer _refreshTimer = new(TimeSpan.FromMilliseconds(250));

        public static AssetInfo GetAssetInfo(string file) => _assetDict.ContainsKey(file) ? _assetDict[file] : null;
        public static AssetInfo GetAssetInfo(Guid guid) => _assets.FirstOrDefault(x => x.Guid == guid);

        public static ReadOnlyObservableCollection<AssetInfo> Assets { get; } = new(_assets);

        public static void Reset(string contentFolder)
        {
            ContentWatcher.ContentModified -= OnContentModified;

            _assetDict.Clear();
            _assets.Clear();

            Debug.Assert(Directory.Exists(contentFolder));

            RegisterAllAssets(contentFolder);

            ContentWatcher.ContentModified += OnContentModified;
        }

        private static void RegisterAllAssets(string path)
        {
            Debug.Assert(Directory.Exists(path));

            foreach (var entry in Directory.GetFileSystemEntries(path))
            {
                if (ContentHelper.IsDirectory(entry)) RegisterAllAssets(entry);
                else RegisterAsset(entry);
            }
        }

        private static void RegisterAsset(string file)
        {
            Debug.Assert(File.Exists(file));

            try
            {
                var fileInfo = new FileInfo(file);

                if (!_assetDict.ContainsKey(file) || _assetDict[file].RegisterTime.IsOlder(fileInfo.LastWriteTime))
                {
                    var info = Asset.GetAssetInfo(file);

                    Debug.Assert(info is not null);

                    info.RegisterTime = DateTime.Now;
                    _assetDict[file] = info;

                    Debug.Assert(_assetDict.ContainsKey(file));

                    _assets.Add(_assetDict[file]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void OnContentModified(object sender, ContentModifiedEventArgs e)
        {
            if (ContentHelper.IsDirectory(e.FullPath))
            {
                RegisterAllAssets(e.FullPath);
            }
            else if (File.Exists(e.FullPath))
            {
                RegisterAsset(e.FullPath);
            }

            _assets.Where(x => !File.Exists(x.FullPath)).ToList().ForEach(x => UnregisterAsset(x.FullPath));
        }

        private static void UnregisterAsset(string file)
        {
            if (_assetDict.ContainsKey(file))
            {
                _assets.Remove(_assetDict[file]);
                _assetDict.Remove(file);
            }
        }
    }
}
