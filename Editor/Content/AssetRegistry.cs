using Editor.Utilities;
using Editor.Utilities.Descriptors;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Editor.Content
{
    static class AssetRegistry
    {
        private static readonly Dictionary<string, AssetInfo> _assetDict = [];
        private static readonly ObservableCollection<AssetInfo> _assets = [];
        private static readonly DelayEventTimer _refreshTimer = new(TimeSpan.FromMilliseconds(250));

        private static readonly FileSystemWatcher _contentWatcher = new FileSystemWatcher()
        {
            IncludeSubdirectories = true,
            Filter = string.Empty,
            NotifyFilter = NotifyFilters.CreationTime |
                NotifyFilters.DirectoryName |
                NotifyFilters.FileName |
                NotifyFilters.LastWrite
        };

        static AssetRegistry()
        {
            _contentWatcher.Changed += OnContentModifiedAsync;
            _contentWatcher.Created += OnContentModifiedAsync;
            _contentWatcher.Deleted += OnContentModifiedAsync;
            _contentWatcher.Renamed += OnContentModifiedAsync;
            _refreshTimer.Triggered += Refresh;
        }

        public static AssetInfo GetAssetInfo(string file) => _assetDict.ContainsKey(file) ? _assetDict[file] : null;
        public static AssetInfo GetAssetInfo(Guid guid) => _assets.FirstOrDefault(x => x.Guid == guid);

        public static ReadOnlyObservableCollection<AssetInfo> Assets { get; } = new(_assets);

        public static void Reset(string contentFolder)
        {
            Clear();

            Debug.Assert(Directory.Exists(contentFolder));

            RegisterAllAssets(contentFolder);

            _contentWatcher.Path = contentFolder;
            _contentWatcher.EnableRaisingEvents = true;
        }

        public static void Clear()
        {
            _contentWatcher.EnableRaisingEvents = false;
            _assetDict.Clear();
            _assets.Clear();
        }

        private static void RegisterAllAssets(string path)
        {
            Debug.Assert(Directory.Exists(path));

            foreach(var entry in Directory.GetFileSystemEntries(path))
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
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static async void OnContentModifiedAsync(object sender, FileSystemEventArgs e)
        {
            if (Path.GetExtension(e.FullPath) != Asset.AssetFileExtension) return;

            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _refreshTimer.Trigger(e);
            }));
        }

        private static void Refresh(object sender, DelayEventTimerArgs e)
        {
            foreach(var item in e.Data)
            {
                if (item is not FileSystemEventArgs eventArgs) continue;

                if (eventArgs.ChangeType == WatcherChangeTypes.Deleted) UnregisterAsset(eventArgs.FullPath);
                else
                {
                    RegisterAsset(eventArgs.FullPath);

                    if(eventArgs.ChangeType == WatcherChangeTypes.Renamed)
                    {
                        _assetDict.Keys.Where(key => !File.Exists(key)).ToList().ForEach(file => UnregisterAsset(file));
                    }
                }
            }
        }

        private static void UnregisterAsset(string file)
        {
            if(_assetDict.ContainsKey(file))
            {
                _assets.Remove(_assetDict[file]);
                _assetDict.Remove(file);
            }
        }
    }
}
