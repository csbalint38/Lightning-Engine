using Editor.Common;
using Editor.GameProject;
using Editor.Utilities;
using Editor.Utilities.Descriptors;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.Xml;
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
