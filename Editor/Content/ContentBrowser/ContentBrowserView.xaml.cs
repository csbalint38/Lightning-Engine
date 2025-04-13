using Editor.Common.Enums;
using Editor.Content.ContentBrowser;
using Editor.Editors;
using Editor.GameProject;
using Editor.Utilities;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.Content
{
    /// <summary>
    /// Interaction logic for ContentBrowserView.xaml
    /// </summary>
    public partial class ContentBrowserView : UserControl
    {
        public static readonly DependencyProperty FileAccessProperty = DependencyProperty.Register(
            nameof(FileAccess),
            typeof(FileAccess),
            typeof(ContentBrowserView),
            new PropertyMetadata(FileAccess.ReadWrite)
        );

        public FileAccess FileAccess
        {
            get => (FileAccess)GetValue(FileAccessProperty);
            set => SetValue(FileAccessProperty, value);
        }

        public ContentBrowserView()
        {
            DataContext = null;
            InitializeComponent();

            Loaded += OnContentBrowserLoaded;

            AllowDrop = true;
        }

        private void OnContentBrowserLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnContentBrowserLoaded;

            if (Application.Current?.MainWindow is not null)
            {
                Application.Current.MainWindow.DataContextChanged += OnProjectChanged;
            }

            OnProjectChanged(null, new DependencyPropertyChangedEventArgs(DataContextProperty, null, Project.Current));
        }

        private void OnProjectChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (DataContext as ContentBrowser.ContentBrowser)?.Dispose();
            DataContext = null;

            if (e.NewValue is Project project)
            {
                Debug.Assert(e.NewValue == Project.Current);

                var contentBrowser = new ContentBrowser.ContentBrowser(project);
                contentBrowser.PropertyChanged += OnSelectedFolderChanged;
                DataContext = contentBrowser;
            }
        }

        private void OnSelectedFolderChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = sender as ContentBrowser.ContentBrowser;

            if (e.PropertyName == nameof(vm.SelectedFolder) && !string.IsNullOrEmpty(vm.SelectedFolder)) { }
        }

        private void LVFolders_Drop(object sender, DragEventArgs e)
        {
            var vm = DataContext as ContentBrowser.ContentBrowser;

            if (vm.SelectedFolder is not null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files?.Length > 0 && Directory.Exists(vm.SelectedFolder))
                {
                    _ = ContentHelper.ImportFilesAsync(files, vm.SelectedFolder);
                    e.Handled = true;
                }
            }
        }

        private void ExecuteSelection(ContentInfo info)
        {
            if (info is null) return;

            if (info.IsDirectory)
            {
                var vm = DataContext as ContentBrowser.ContentBrowser;
                vm.SelectedFolder = info.FullPath;
            }
            else if (FileAccess.HasFlag(FileAccess.Read))
            {
                var assetInfo = Asset.GetAssetInfo(info.FullPath);

                if (assetInfo is not null)
                {
                    OpenAssetEditor(assetInfo);
                }
            }
        }

        private void LVFolders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var info = (sender as FrameworkElement).DataContext as ContentInfo;
            ExecuteSelection(info);
        }

        private void LVFolders_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var info = (sender as FrameworkElement).DataContext as ContentInfo;
                ExecuteSelection(info);
            }
        }

        private IAssetEditor OpenAssetEditor(AssetInfo info)
        {
            IAssetEditor editor = null;

            try
            {
                switch (info.Type)
                {
                    case AssetType.ANIMATION:
                        break;
                    case AssetType.AUDIO:
                        break;
                    case AssetType.MATERIAL:
                        break;
                    case AssetType.MESH:
                        editor = OpenEditorPanel<GeometryEditorView>(info, info.Guid, "GeometryEditor");
                        break;
                    case AssetType.SKELETON:
                        break;
                    case AssetType.TEXTURE:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return editor;
        }

        private IAssetEditor OpenEditorPanel<T>(AssetInfo info, Guid guid, string title) where T : FrameworkElement, new()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.Content is FrameworkElement content &&
                    content.DataContext is IAssetEditor editor &&
                    editor.Asset.Guid == info.Guid
                )
                {
                    window.Activate();
                    return editor;
                }
            }

            var newEditor = new T();

            Debug.Assert(newEditor.DataContext is IAssetEditor);

            (newEditor.DataContext as IAssetEditor).SetAssetAsync(info);

            var win = new Window()
            {
                Content = newEditor,
                Title = title,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                // Set style here
            };

            win.Show();

            return newEditor.DataContext as IAssetEditor;
        }
    }
}
