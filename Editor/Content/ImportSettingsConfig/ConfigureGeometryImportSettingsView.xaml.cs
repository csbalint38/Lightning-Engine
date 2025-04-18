using System.Windows;
using System.Windows.Controls;

namespace Editor.Content.ImportSettingsConfig
{
    /// <summary>
    /// Interaction logic for ConfigureGeometryImportSettingsView.xaml
    /// </summary>
    public partial class ConfigureGeometryImportSettingsView : UserControl
    {
        public ConfigureGeometryImportSettingsView()
        {
            InitializeComponent();

            Loaded += (_, __) =>
            {
                var item = LBGeometry.ItemContainerGenerator.ContainerFromIndex(LBGeometry.SelectedIndex) as ListBoxItem;
                item?.Focus();
            };
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e) =>
            ((sender as FrameworkElement).DataContext as GeometryImportSettingsConfigurator).Import();

        private void LBGeometry_Drop(object sender, DragEventArgs e) =>
            ConfigureImportSettingsWindow.AddDroppedFiles(DataContext as ConfigureImportSettings, sender as ListBox, e);

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ConfigureImportSettings;
            vm.GeometryImportSettingsConfigurator.RemoveFile((sender as FrameworkElement).DataContext as GeometryProxy);
        }

        private void BtnApplyToSelection_Click(object sender, RoutedEventArgs e)
        {
            var settings = ((sender as FrameworkElement).DataContext as GeometryProxy).ImportSettings;
            var selection = LBGeometry.SelectedItems;

            foreach(GeometryProxy proxy in selection) proxy.CopySettings(settings);
        }

        private void BtnApplyToAll_Click(object sender, RoutedEventArgs e)
        {
            var settings = ((sender as FrameworkElement).DataContext as GeometryProxy).ImportSettings;
            var vm = DataContext as ConfigureImportSettings;

            foreach(var proxy in vm.GeometryImportSettingsConfigurator.GeometryProxies) proxy.CopySettings(settings);
        }
    }
}
