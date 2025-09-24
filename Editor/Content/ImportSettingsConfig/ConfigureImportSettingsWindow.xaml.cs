using Editor.Common.Enums;
using System.ComponentModel;
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
                var vm = (ConfigureImportSettings)DataContext;

                TCAssetTypes.SelectedIndex = vm.GeometryImportSettingsConfigurator.GeometryProxies.Any() ?
                    0 :
                    vm.TextureImportSettingsConfigurator.TextureProxies.Any() ?
                    1 :
                    //vm.AudioImportSettingsConfigurator.AudioProxies.Any() ?
                    //2 :
                    0;
            };

            Closing += ConfigureImportSettingsWindow_Closing;
        }

        private void ConfigureImportSettingsWindow_Closing(object? sender, CancelEventArgs e)
        {
            ImportingItemCollection.Clear(AssetType.ANIMATION);
            ImportingItemCollection.Clear(AssetType.MATERIAL);
            ImportingItemCollection.Clear(AssetType.MESH);
            ImportingItemCollection.Clear(AssetType.SKELETON);
            ImportingItemCollection.Clear(AssetType.TEXTURE);
        }

        internal static void AddDroppedFiles(ConfigureImportSettings dataContext, ListBox listBox, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files?.Length > 0)
            {
                var destFolder = listBox.HasItems ?
                    ((AssetProxy)listBox.Items[^1]).DestinationFolder :
                    dataContext.LastDestinationFolder;

                dataContext.AddFiles(files, destFolder);
            }
        }

        private void TCAssetTypes_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            ImportingItemCollection.SetItemFilter((AssetType)(TCAssetTypes.SelectedItem as TabItem)?.Tag!);
    }
}
