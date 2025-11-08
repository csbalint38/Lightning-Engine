using Editor.Content.ContentBrowser.Descriptors;
using Editor.Utilities;
using Editor.Utilities.Descriptors;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Editor.Content.ContentBrowser
{
    static class ContentWatcher
    {
        private static readonly DelayEventTimer _refreshTimer = new(TimeSpan.FromMilliseconds(250));
        private static readonly FileSystemWatcher _contentWatcher = new()
        {
            IncludeSubdirectories = true,
            Filter = string.Empty,
            NotifyFilter = NotifyFilters.CreationTime |
                NotifyFilters.DirectoryName |
                NotifyFilters.FileName |
                NotifyFilters.LastWrite
        };

        private static int _fileWatcherEnableCounter = 0;

        public static event EventHandler<ContentModifiedEventArgs>? ContentModified;

        static ContentWatcher()
        {
            _contentWatcher.Changed += OnContentModifiedAsync;
            _contentWatcher.Created += OnContentModifiedAsync;
            _contentWatcher.Deleted += OnContentModifiedAsync;
            _contentWatcher.Renamed += OnContentModifiedAsync;

            _refreshTimer.Triggered += Refresh;
        }

        public static void Reset(string contentFolder, string projectPath)
        {
            _contentWatcher.EnableRaisingEvents = false;

            ContentInfoCache.Reset(projectPath);

            if (!string.IsNullOrEmpty(contentFolder))
            {
                Debug.Assert(Directory.Exists(contentFolder));

                _contentWatcher.Path = contentFolder;
                _contentWatcher.EnableRaisingEvents = true;

                AssetRegistry.Reset(contentFolder, projectPath);
            }
        }

        public static void EnableFileWatcher(bool isEnabled)
        {
            if (_fileWatcherEnableCounter > 0 && isEnabled)
            {
                --_fileWatcherEnableCounter;
            }
            else if (!isEnabled)
            {
                ++_fileWatcherEnableCounter;
            }
        }

        private static async void OnContentModifiedAsync(object sender, FileSystemEventArgs e) =>
            await Application.Current.Dispatcher.BeginInvoke(new Action(() => _refreshTimer.Trigger(e)));

        private static void Refresh(object? sender, DelayEventTimerArgs e)
        {
            if (_fileWatcherEnableCounter > 0)
            {
                e.RepeatEvent = true;

                return;
            }

            e.Data
                .Cast<FileSystemEventArgs>()
                .GroupBy(x => x.FullPath)
                .Select(x => x.First())
                .ToList()
                .ForEach(x => ContentModified?.Invoke(null, new ContentModifiedEventArgs(x.FullPath)));
        }
    }
}
