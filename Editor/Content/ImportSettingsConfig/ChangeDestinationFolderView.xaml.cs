using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Content.ImportSettingsConfig
{
    /// <summary>
    /// Interaction logic for ChangeDestinationFolderView.xaml
    /// </summary>
    public partial class ChangeDestinationFolderView : UserControl
    {
        public ChangeDestinationFolderView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var proxy = (sender as Button).DataContext as AssetProxy;
            var destinationFolder = proxy.DestinationFolder;

            if (Path.EndsInDirectorySeparator(destinationFolder))
            {
                destinationFolder = Path.GetDirectoryName(destinationFolder);
            }

            var dialog = new SelectFolderDialog(destinationFolder);

            if (dialog.ShowDialog() == true)
            {
                Debug.Assert(!string.IsNullOrEmpty(dialog.SelectedFolder));

                proxy.DestinationFolder = dialog.SelectedFolder;
            }
        }
    }
}
