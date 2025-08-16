using Editor.Common.Controls;
using Editor.Common.Enums;
using Editor.Content.ContentBrowser;
using Editor.Content.ImportSettingsConfig;
using Editor.Editors;
using Editor.GameProject;
using Editor.Utilities;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Editor.Content;

/// <summary>
/// Interaction logic for ContentBrowserView.xaml
/// </summary>
public partial class ContentBrowserView : UserControl, IDisposable
{
    private string _sortedProperty = nameof(ContentInfo.FileName);
    private ListSortDirection _sortDirection;
    private Point _clickPosition;
    private bool _startDrag;

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

    public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(
        nameof(SelectionMode),
        typeof(SelectionMode),
        typeof(ContentBrowserView),
        new PropertyMetadata(SelectionMode.Extended)
    );

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem),
        typeof(ContentInfo),
        typeof(ContentBrowserView),
        new PropertyMetadata(null)
    );

    internal ContentInfo SelectedItem
    {
        get => (ContentInfo)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

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

    public SelectionMode SelectionMode
    {
        get => (SelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
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

    public void Dispose()
    {
        if (Application.Current?.MainWindow is not null)
        {
            Application.Current.MainWindow.DataContextChanged -= OnProjectChanged;
        }

        (DataContext as ContentBrowser.ContentBrowser)?.Dispose();
        DataContext = null;
    }

    internal static IAssetEditor OpenAssetEditor(AssetInfo info)
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

    private static IAssetEditor OpenEditorPanel<T>(AssetInfo info, Guid guid, string title)
        where T : FrameworkElement, new()
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

        foreach (Window win in Application.Current.Windows)
        {
            if (win.DataContext is ConfigureImportSettings cfg)
            {
                if (files?.Length > 0) cfg.AddFiles(files, selectedFolder);

                settingsConfigurator = cfg;
                win.Activate();

                break;
            }
        }

        if (settingsConfigurator is null)
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
            Style = Application.Current.FindResource("LightningWindowStyle") as Style
        };

        win.Show();

        return newEditor;
    }

    private void OnContentBrowserLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnContentBrowserLoaded;

        if (Application.Current?.MainWindow is not null)
        {
            Application.Current.MainWindow.DataContextChanged += OnProjectChanged;
        }

        OnProjectChanged(null, new DependencyPropertyChangedEventArgs(DataContextProperty, null, Project.Current));

        LVFolders.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(Thumb_DragDelta), true);
        LVFolders.Items.SortDescriptions.Add(new SortDescription(_sortedProperty, _sortDirection));
    }

    private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (e.OriginalSource is Thumb thumb && thumb.TemplatedParent is GridViewColumnHeader header)
        {
            if (header.Column.ActualWidth < 50) header.Column.Width = 50;
            else if (header.Column.ActualWidth > 300) header.Column.Width = 300;
        }
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

        if (e.PropertyName == nameof(vm.SelectedFolder) && !string.IsNullOrEmpty(vm.SelectedFolder))
        {
            GeneratePathStackButtons();
        }
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
                else if (e.OriginalSource == cfgDrop)
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
            var assetInfo = Asset.TryGetAssetInfo(info.FullPath);

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

    private IAssetEditor OpenEditorPanel<T>(AssetInfo info, string title) where T : FrameworkElement, new()
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window.Content is FrameworkElement content &&
                content.DataContext is IAssetEditor editor &&
                editor.CheckAssetGuid(info.Guid)
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

    private bool TryEdit(ListView list, string path)
    {
        foreach (ContentInfo item in list.Items)
        {
            if (item.FullPath == path)
            {
                var listBoxItem = list.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;

                listBoxItem.IsSelected = true;
                list.SelectedItem = item;
                list.SelectedIndex = list.Items.IndexOf(item);

                TryEdit(listBoxItem, path);

                return true;
            }
        }

        return false;
    }

    private void TryEdit(ListBoxItem item, string path)
    {
        var textBox = item.FindVisualChild<TextBox>();

        if (textBox is not null)
        {
            textBox.Visibility = Visibility.Visible;
            textBox.Focus();
        }
    }

    private async void MINewFolder_ClickAsync(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ContentBrowser.ContentBrowser;
        var path = vm.SelectedFolder;

        if (!Path.EndsInDirectorySeparator(path)) path += Path.DirectorySeparatorChar;

        var folder = "NewFolder";
        var index = 1;

        while (Directory.Exists(path + folder))
        {
            folder = $"NewFolder{index++:0#}";
        }

        folder = path + folder;

        try
        {
            Directory.CreateDirectory(folder);

            var waitCounter = 0;

            while (waitCounter < 30 && !TryEdit(LVFolders, folder))
            {
                await Task.Run(() => Thread.Sleep(100));
                ++waitCounter;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine($"Error: failed to create new folder: {folder}");
        }
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = LVFolders.SelectedItem as ContentInfo;
        SelectedItem = item?.IsDirectory == true ? null : item;
    }

    private void BDrop_DragLeave(object sender, DragEventArgs e)
    {
        if (sender == BDrop && e?.Effects != DragDropEffects.None)
        {
            var point = e.GetPosition(BDrop);
            var result = VisualTreeHelper.HitTest(BDrop, point);

            if (result is not null) return;
        }

        var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(100)));

        fadeOut.Completed += (_, _) => BDrop.Visibility = Visibility.Collapsed;
        BDrop.BeginAnimation(OpacityProperty, fadeOut);
    }

    private void ListView_DragEnter(object sender, DragEventArgs e)
    {
        if (!_startDrag)
        {
            BDrop.Opacity = 0;
            BDrop.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(100)));

            BDrop.BeginAnimation(OpacityProperty, fadeIn);
        }
    }

    private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
    {
        var column = sender as GridViewColumnHeader;
        var sortBy = column.Tag.ToString();

        LVFolders.Items.SortDescriptions.Clear();

        var newDir = ListSortDirection.Ascending;

        if (_sortedProperty == sortBy && _sortDirection == newDir)
        {
            newDir = ListSortDirection.Descending;
        }

        _sortDirection = newDir;
        _sortedProperty = sortBy;

        LVFolders.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
    }

    private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var info = (sender as FrameworkElement).DataContext as ContentInfo;

        ExecuteSelection(info);
    }

    private void ListViewItem_KeyDown(object sender, KeyEventArgs e)
    {
        var info = (sender as FrameworkElement).DataContext as ContentInfo;

        if (e.Key == Key.Enter) ExecuteSelection(info);
        else if (e.Key == Key.F2) TryEdit(LVFolders, info.FullPath);
    }

    private void GeneratePathStackButtons()
    {
        var vm = DataContext as ContentBrowser.ContentBrowser;
        var path = Directory.GetParent(Path.TrimEndingDirectorySeparator(vm.SelectedFolder)).FullName;
        var contentPath = Path.TrimEndingDirectorySeparator(vm.ContentFolder);

        SPPath.Children.RemoveRange(1, SPPath.Children.Count - 1);

        if (vm.SelectedFolder == vm.ContentFolder) goto _addCurrentDirectory;

        string[] paths = new string[3];
        string[] labels = new string[3];

        int i;

        for (i = 0; i < 3; ++i)
        {
            paths[i] = path;
            labels[i] = path[(path.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];

            if (path == contentPath) break;

            path = path[..path.LastIndexOf(Path.DirectorySeparatorChar)];
        }

        if (i == 3) i = 2;

        for (; i >= 0; --i)
        {
            var btn = new Button()
            {
                DataContext = paths[i],
                Content = new TextBlock()
                {
                    Text = labels[i],
                    TextTrimming = TextTrimming.CharacterEllipsis,
                }
            };

            SPPath.Children.Add(btn);

            if (i >= 0) SPPath.Children.Add(new System.Windows.Shapes.Path());
        }

        _addCurrentDirectory:
            SPPath.Children.Add(new TextBlock()
            {
                Text = $"{Path.GetFileName(Path.TrimEndingDirectorySeparator(vm.SelectedFolder))}",
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.White
            });
    }

    private void OnPathStack_Button_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ContentBrowser.ContentBrowser;
        vm.SelectedFolder = (sender as Button).DataContext as string;
    }

    private void ListViewEx_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _clickPosition = e.GetPosition(null);
        
        var item = (e.OriginalSource as DependencyObject)?.FindVisualParent<ListViewItemEx>();

        _startDrag = item is not null;
    }

    private void ListViewEx_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => _startDrag = false;

    private void ListViewEx_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            var mousePosition = e.GetPosition(this);
            var diff = mousePosition - _clickPosition;

            if (_startDrag && diff.LengthSquared > 100)
            {
                var files = new List<string>();

                foreach (ContentInfo item in LVFolders.SelectedItems) files.Add(item.FullPath);

                if (files.Count > 0)
                {
                    var fileArray = files.ToArray();
                    var dataObj = new DataObject(DataFormats.FileDrop, fileArray);

                    DragDrop.DoDragDrop(LVFolders, dataObj, DragDropEffects.Copy);

                    _startDrag = false;
                }
            }
        }
    }
}
