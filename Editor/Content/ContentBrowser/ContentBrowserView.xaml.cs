using Editor.Common.Enums;
using Editor.Content.ContentBrowser;
using Editor.Content.ImportSettingsConfig;
using Editor.Editors;
using Editor.GameProject;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Editor.Content
{
    /// <summary>
    /// Interaction logic for ContentBrowserView.xaml
    /// </summary>
    public partial class ContentBrowserView : UserControl, IDisposable
    {
        public static readonly DependencyProperty FileAccessProperty = DependencyProperty.Register(
            nameof(FileAccess),
            typeof(FileAccess),
            typeof(ContentBrowserView),
            new PropertyMetadata(FileAccess.ReadWrite)
        );

        public static readonly DependencyProperty AllowImportProperty = DependencyProperty.Register(
            nameof(AllowImport),
            typeof(bool),
            typeof(ContentBrowserView),
            new PropertyMetadata(false)
        );

        public FileAccess FileAccess
        {
            get => (FileAccess)GetValue(FileAccessProperty);
            set => SetValue(FileAccessProperty, value);
        }

        public bool AllowImport
        {
            get => (bool)GetValue(AllowImportProperty);
            set => SetValue(AllowImportProperty, value);
        }

        public ContentBrowserView()
        {
            DataContext = null;
            InitializeComponent();

            Loaded += OnContentBrowserLoaded;
        }

        public void OpenImportSettingsConfigurator()
        {
            var vm = DataContext as ContentBrowser.ContentBrowser;

            OpenImportSettingsConfigurator(null, vm.SelectedFolder, true);
        }

        private static IAssetEditor OpenAssetEditor(AssetInfo info)
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
                        editor = OpenEditorPanel<GeometryEditorView>(info, info.Guid, "Geometry Editor");
                        break;
                    case AssetType.SKELETON:
                        break;
                    case AssetType.TEXTURE:
                        editor = OpenEditorPanel<TextureEditorView>(info, info.Guid, "Texture Editor");
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

        private static IAssetEditor OpenEditorPanel<T>(AssetInfo info, Guid guid, string title) where T : FrameworkElement, new()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.Content is FrameworkElement content &&
                    content.DataContext is IAssetEditor editor &&
                    editor.AssetGuid == info.Guid
                )
                {
                    window.Activate();
                    return editor;
                }
            }

            var newEditor = CreateEditorWindow<T>(title);
            (newEditor.DataContext as IAssetEditor).SetAssetAsync(info);

            return newEditor.DataContext as IAssetEditor;
        }

        private static void OpenImportSettingsConfigurator(string[] files, string selectedFolder, bool forceOpen = false)
        {
            ConfigureImportSettings settingsConfigurator = null;

            foreach(Window win in Application.Current.Windows)
            {
                if(win.DataContext is ConfigureImportSettings cfg)
                {
                    if (files?.Length > 0) cfg.AddFiles(files, selectedFolder);

                    settingsConfigurator = cfg;
                    win.Activate();

                    break;
                }
            }

            if(settingsConfigurator is null)
            {
                settingsConfigurator = files?.Length > 0 ? new(files, selectedFolder) : new(selectedFolder);

                if (settingsConfigurator.FileCount > 0 || forceOpen)
                {
                    new ConfigureImportSettingsWindow()
                    {
                        DataContext = settingsConfigurator,
                        Owner = Application.Current.MainWindow,
                    }.Show();
                }
            }
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

        private void BDrop_Drop(object sender, DragEventArgs e)
        {
            var vm = DataContext as ContentBrowser.ContentBrowser;

            if (Directory.Exists(vm.SelectedFolder) && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files?.Length > 0)
                {
                    if (e.OriginalSource == filesDrop)
                    {
                        new ConfigureImportSettings(files, vm.SelectedFolder).Import();

                        e.Handled = true;
                    }
                    else if(e.OriginalSource == cfgDrop)
                    {
                        OpenImportSettingsConfigurator(files, vm.SelectedFolder);
                        e.Handled = true;
                    }
                }
            }

            e.Effects = DragDropEffects.None;
            BDrop_DragLeave(sender, e);
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

        private static FrameworkElement CreateEditorWindow<T>(string title) where T : FrameworkElement, new()
        {
            var newEditor = new T();

            Debug.Assert(newEditor.DataContext is IAssetEditor);

            var win = new Window()
            {
                Content = newEditor,
                Title = title,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                // Set style here
            };

            win.Show();

            return newEditor;
        }

        private void BDrop_DragLeave(object sender, DragEventArgs e)
        {
            if(sender == BDrop && e?.Effects != DragDropEffects.None)
            {
                var point = e.GetPosition(BDrop);
                var result = VisualTreeHelper.HitTest(BDrop, point);

                if (result is not null) return;
            }

            var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(100)));

            fadeOut.Completed += (_, __) => BDrop.Visibility = Visibility.Collapsed;
            BDrop.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void ListView_DragEnter(object sender, DragEventArgs e)
        {
            BDrop.Opacity = 0;
            BDrop.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(100)));

            BDrop.BeginAnimation(OpacityProperty, fadeIn);
        }

        public void Dispose() { }
    }
}
