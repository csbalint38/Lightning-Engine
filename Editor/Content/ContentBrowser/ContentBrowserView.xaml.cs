using Editor.GameProject;
using Editor.Utilities;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Content
{
    /// <summary>
    /// Interaction logic for ContentBrowserView.xaml
    /// </summary>
    public partial class ContentBrowserView : UserControl
    {
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

            if(Application.Current?.MainWindow is not null)
            {
                Application.Current.MainWindow.DataContextChanged += OnProjectChanged;
            }

            OnProjectChanged(null, new DependencyPropertyChangedEventArgs(DataContextProperty, null, Project.Current));
        }

        private void OnProjectChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (DataContext as ContentBrowser.ContentBrowser)?.Dispose();
            DataContext = null;

            if(e.NewValue is Project project)
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

            if(e.PropertyName == nameof(vm.SelectedFolder) && !string.IsNullOrEmpty(vm.SelectedFolder)) { }
        }

        private void LVFolders_Drop(object sender, DragEventArgs e)
        {
            var vm = DataContext as ContentBrowser.ContentBrowser;

            if(vm.SelectedFolder is not null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if(files?.Length > 0 && Directory.Exists(vm.SelectedFolder))
                {
                    _ = ContentHelper.ImportFilesAsync(files, vm.SelectedFolder);
                    e.Handled = true;
                }
            }
        }
    }
}
