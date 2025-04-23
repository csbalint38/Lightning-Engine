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
        private static readonly Dictionary<string, AssetInfo> _assetFileDict = [];
        private static readonly Dictionary<Guid, AssetInfo> _assetGuidDict = [];
        private static readonly ObservableCollection<AssetInfo> _assets = [];

        public static AssetInfo GetAssetInfo(string file) => _assetFileDict.ContainsKey(file) ? _assetFileDict[file] : null;
        public static AssetInfo GetAssetInfo(Guid guid) => _assetGuidDict.ContainsKey(guid) ? _assetGuidDict[guid] : null;

        public static ReadOnlyObservableCollection<AssetInfo> Assets { get; } = new(_assets);

        public static void Reset(string contentFolder)
        {
            ContentWatcher.ContentModified -= OnContentModified;

            _assetFileDict.Clear();
            _assetGuidDict.Clear();
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
                var isNew = !_assetFileDict.ContainsKey(file);

                if (isNew || _assetFileDict[file].RegisterTime.IsOlder(fileInfo.LastWriteTime))
                {
                    var info = Asset.GetAssetInfo(file);

                    Debug.Assert(info is not null);

                    info.RegisterTime = DateTime.Now;
                    
                    if(!isNew && _assetFileDict[file].Guid != info.Guid)
                    {
                        _assetGuidDict.Remove(_assetFileDict[file].Guid);
                    }

                    _assetFileDict[file] = info;
                    _assetGuidDict[info.Guid] = info;

                    if(isNew)
                    {
                        Debug.Assert(!_assets.Contains(info));

                        _assets.Add(info);
                    }
                    else
                    {
                        var oldInfo = _assets.FirstOrDefault(x => x.FullPath == info.FullPath);

                        Debug.Assert(oldInfo is not null);

                        _assets[_assets.IndexOf(oldInfo)] = info;
                    }
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
            if (_assetFileDict.ContainsKey(file))
            {
                var info = _assetFileDict[file];
                
                _assets.Remove(info);
                _assetFileDict.Remove(file);

                if(_assetGuidDict.ContainsKey(info.Guid) && !File.Exists(_assetGuidDict[info.Guid].FullPath))
                {
                    _assetGuidDict.Remove(info.Guid);
                }
            }
        }
    }
}
