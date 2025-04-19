using Editor.Common.Enums;
using Editor.Content.ImportSettingsConfig;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Content.ImportSettingsConfig
{
    /// <summary>
    /// Interaction logic for ConfigureImportSettingsWindow.xaml
    /// </summary>
    public partial class ConfigureImportSettingsWindow : Window
    {
        public ConfigureImportSettingsWindow()
        {
            InitializeComponent();

            Loaded += (_, __) =>
            {
                var vm = DataContext as ConfigureImportSettings;

                TCAssetTypes.SelectedIndex = vm.GeometryImportSettingsConfigurator.GeometryProxies.Any() ?
                    0 :
                    vm.TextureImportSettingsConfigurator.TextureProxies.Any() ?
                    1 :
                    //vm.AudioImportSettingsConfigurator.AudioProxies.Any() ?
                    //2 :
                    0;
            };
        }

        internal static void AddDroppedFiles(ConfigureImportSettings dataContext, ListBox listBox, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if(files?.Length > 0)
            {
                var destFolder = listBox.HasItems ?
                    (listBox.Items[^1] as AssetProxy).DestinationFolder :
                    dataContext.LastDestinationFolder;

                dataContext.AddFiles(files, destFolder);
            }
        }

        private void TCAssetTypes_SelectionChanged_1(object sender, SelectionChangedEventArgs e) =>
            ImportingItemCollection.SetItemFilter((AssetType)(TCAssetTypes.SelectedItem as TabItem)?.Tag);
    }
}
