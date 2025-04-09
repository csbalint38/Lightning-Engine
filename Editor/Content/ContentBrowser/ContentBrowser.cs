using Editor.Common;
using Editor.Common.Enums;
using Editor.GameProject;
using Editor.Utilities;
using Editor.Utilities.Descriptors;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Editor.Content.ContentBrowser
{
    public class ContentBrowser : ViewModelBase, IDisposable
    {
        private static readonly object _lock = new();
        private static readonly DelayEventTimer _refreshTimer = new DelayEventTimer(TimeSpan.FromMilliseconds(250));
        private static readonly FileSystemWatcher _contentWatcher = new FileSystemWatcher()
        {
            IncludeSubdirectories = true,
            Filter = string.Empty,
            NotifyFilter = NotifyFilters.CreationTime |
                NotifyFilters.DirectoryName |
                NotifyFilters.FileName |
                NotifyFilters.LastWrite
        };

        private static readonly Dictionary<string, ContentInfo> _contentInfoCache = [];

        private static string _cacheFilePath = string.Empty;

        private readonly ObservableCollection<ContentInfo> _folderContent = [];

        private string _selectedFolder;

        public string ContentFolder { get; }
        public ReadOnlyObservableCollection<ContentInfo> FolderContent { get; }

        public string SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (_selectedFolder != value)
                {
                    _selectedFolder = value;

                    if (!string.IsNullOrEmpty(_selectedFolder)) GetFolderContentAsync();

                    OnPropertyChanged(nameof(SelectedFolder));
                }
            }
        }

        public ContentBrowser(Project project)
        {
            Debug.Assert(project is not null);

            var contentFolder = project.ContentPath;

            Debug.Assert(!string.IsNullOrEmpty(contentFolder.Trim()));

            contentFolder = Path.TrimEndingDirectorySeparator(contentFolder);
            ContentFolder = contentFolder;
            SelectedFolder = contentFolder;
            FolderContent = new ReadOnlyObservableCollection<ContentInfo>(_folderContent);
            
            if(string.IsNullOrEmpty(_cacheFilePath))
            {
                _cacheFilePath = $@"{project.Path}.Lightning\ContentInfoCache.bin";
                LoadInfoCache(_cacheFilePath);
            }

            _contentWatcher.Path = contentFolder;
            _contentWatcher.Changed += OnContentModified;
            _contentWatcher.Created += OnContentModified;
            _contentWatcher.Deleted += OnContentModified;
            _contentWatcher.Renamed += OnContentModified;
            _contentWatcher.EnableRaisingEvents = true;
            _refreshTimer.Triggered += Refresh;
        }

        private static List<ContentInfo> GetFolderContent(string path)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));

            var folderContent = new List<ContentInfo>();

            try
            {
                foreach(var dir in Directory.GetDirectories(path))
                {
                    folderContent.Add(new ContentInfo(dir));
                }

                lock (_lock)
                {
                    foreach (var file in Directory.GetFiles(path, $"*{Asset.AssetFileExtension}"))
                    {
                        var fileInfo = new FileInfo(file);

                        if (!_contentInfoCache.ContainsKey(file) || _contentInfoCache[file].DateModified.IsOlder(fileInfo.LastWriteTime))
                        {
                            var info = AssetRegistry.GetAssetInfo(file) ?? Asset.GetAssetInfo(file);

                            Debug.Assert(info is not null);

                            _contentInfoCache[file] = new ContentInfo(file, info.Icon);
                        }

                        Debug.Assert(_contentInfoCache.ContainsKey(file));

                        folderContent.Add(_contentInfoCache[file]);
                    }
                }
            }
            catch(IOException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return folderContent;
        }

        private static void SaveInfoCache(string file)
        {
            lock(_lock)
            {
                using var writer = new BinaryWriter(File.Open(file, FileMode.Create, FileAccess.Write));

                writer.Write(_contentInfoCache.Keys.Count);

                foreach (var key in _contentInfoCache.Keys) 
                {
                    var info = _contentInfoCache[key];

                    writer.Write(key);
                    writer.Write(info.DateModified.ToBinary());
                    writer.Write(info.Icon.Length);
                    writer.Write(info.Icon);
                }
            }
        }

        private static void LoadInfoCache(string file)
        {
            if (!File.Exists(file)) return;

            try
            {
                lock(_lock)
                {
                    using var reader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read));
                    var numEntries = reader.ReadInt32();

                    _contentInfoCache.Clear();

                    for(int i = 0; i < numEntries; ++i)
                    {
                        var assetFile = reader.ReadString();
                        var date = DateTime.FromBinary(reader.ReadInt64());
                        var iconSize = reader.ReadInt32();
                        var icon = reader.ReadBytes(iconSize);

                        if(File.Exists(assetFile))
                        {
                            _contentInfoCache[assetFile] = new ContentInfo(assetFile, icon, null, date);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.LogAsync(LogLevel.WARNING, "Failed to read Content Browser cache file.");
                _contentInfoCache.Clear();
            }
        } 

        private async void OnContentModified(object sender, FileSystemEventArgs e)
        {
            if (Path.GetDirectoryName(e.FullPath) != SelectedFolder) return;

            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _refreshTimer.Trigger();
            }));
        }

        private void Refresh(object sender, DelayEventTimerArgs e) => GetFolderContentAsync();

        private async void GetFolderContentAsync()
        {
             var folderContent = new List<ContentInfo>();

            await Task.Run(() =>
            {
                folderContent = GetFolderContent(SelectedFolder);
            });

            _folderContent.Clear();
            folderContent.ForEach(x => _folderContent.Add(x));
        }

        public void Dispose()
        {
            ((IDisposable)_contentWatcher).Dispose();

            if(!string.IsNullOrEmpty(_cacheFilePath))
            {
                SaveInfoCache(_cacheFilePath);
                _cacheFilePath = string.Empty;
            }
        }
    }
}
