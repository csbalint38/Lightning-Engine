using Editor.Common;
using Editor.Content.ContentBrowser.Descriptors;
using Editor.GameProject;
using Editor.Utilities;
using Editor.Utilities.Descriptors;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace Editor.Content.ContentBrowser
{
    public class ContentBrowser : ViewModelBase, IDisposable
    {
        private readonly DelayEventTimer _refreshTimer = new DelayEventTimer(TimeSpan.FromMilliseconds(250));

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

                    if (!string.IsNullOrEmpty(_selectedFolder)) _ = GetFolderContentAsync();

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

            ContentWatcher.ContentModified += OnContentModified;
            _refreshTimer.Triggered += Refresh;
        }

        private static List<ContentInfo> GetFolderContent(string path)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));

            var folderContent = new List<ContentInfo>();

            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    folderContent.Add(new ContentInfo(dir));
                }

                foreach (var file in Directory.GetFiles(path, $"*{Asset.AssetFileExtension}"))
                {
                    var fileInfo = new FileInfo(file);
                    folderContent.Add(ContentInfoCache.Add(file));
                }
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return folderContent;
        }

        private void OnContentModified(object sender, ContentModifiedEventArgs e)
        {
            if (Path.GetDirectoryName(e.FullPath) != SelectedFolder) return;

            _refreshTimer.Trigger();
        }

        private void Refresh(object sender, DelayEventTimerArgs e) => _ = GetFolderContentAsync();

        private async Task GetFolderContentAsync()
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
            ContentWatcher.ContentModified -= OnContentModified;
            ContentInfoCache.Save();
        }
    }
}
