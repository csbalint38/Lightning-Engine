using Editor.GameProject;
using System.ComponentModel;
using System.Diagnostics;
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
    }
}
