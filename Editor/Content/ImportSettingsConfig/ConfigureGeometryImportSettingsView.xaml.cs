using Editor.Common.Enums;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Content.ImportSettingsConfig;

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
            var item = LBGeometry.ItemContainerGenerator
                .ContainerFromIndex(LBGeometry.SelectedIndex) as ListBoxItem;

            item?.Focus();
        };
    }

    private void BtnImport_Click(object sender, RoutedEventArgs e) =>
        (((FrameworkElement)sender).DataContext as GeometryImportSettingsConfigurator)?.Import();

    private void LBGeometry_Drop(object sender, DragEventArgs e) =>
        ConfigureImportSettingsWindow.AddDroppedFiles(
            (ConfigureImportSettings)DataContext,
            (ListBox)sender,
            e
        );

    private void BtnRemove_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ConfigureImportSettings;
        vm!.GeometryImportSettingsConfigurator
            .RemoveFile((((FrameworkElement)sender).DataContext as GeometryProxy)!);
    }

    private void BtnApplyToSelection_Click(object sender, RoutedEventArgs e)
    {
        var settings = (((FrameworkElement)sender).DataContext as GeometryProxy)!.ImportSettings;
        var selection = LBGeometry.SelectedItems;

        foreach (GeometryProxy proxy in selection) proxy.CopySettings(settings);
    }

    private void BtnApplyToAll_Click(object sender, RoutedEventArgs e)
    {
        var settings = (((FrameworkElement)sender).DataContext as GeometryProxy)!.ImportSettings;
        var vm = DataContext as ConfigureImportSettings;

        foreach (var proxy in vm!.GeometryImportSettingsConfigurator.GeometryProxies)
        {
            proxy.CopySettings(settings);
        }
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e)
    {
        ImportingItemCollection.Clear(AssetType.ANIMATION);
        ImportingItemCollection.Clear(AssetType.MATERIAL);
        ImportingItemCollection.Clear(AssetType.MESH);
        ImportingItemCollection.Clear(AssetType.SKELETON);
    }
}
