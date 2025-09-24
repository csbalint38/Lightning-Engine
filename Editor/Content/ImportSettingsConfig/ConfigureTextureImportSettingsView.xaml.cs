using Editor.Common.Enums;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Content.ImportSettingsConfig;

/// <summary>
/// Interaction logic for ConfigureTextureImportSettingsView.xaml
/// </summary>
public partial class ConfigureTextureImportSettingsView : UserControl
{
    public ConfigureTextureImportSettingsView()
    {
        InitializeComponent();

        var item = LBTexture.ItemContainerGenerator
            .ContainerFromIndex(LBTexture.SelectedIndex) as ListBoxItem;

        item?.Focus();
    }

    private void BtnImport_Click(object sender, RoutedEventArgs e) =>
         (((FrameworkElement)sender).DataContext as TextureImportSettingsConfigurator)?.Import();

    private void LBTexture_Drop(object sender, DragEventArgs e) =>
        ConfigureImportSettingsWindow.AddDroppedFiles(
            (ConfigureImportSettings)DataContext,
            (ListBox)sender,
            e
        );

    private void BtnRemove_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ConfigureImportSettings;
        vm?.TextureImportSettingsConfigurator.RemoveFile((((FrameworkElement)sender).DataContext as TextureProxy)!);
    }

    private void BtnApplyToSelection_Click(object sender, RoutedEventArgs e)
    {
        var settings = ((TextureProxy)((FrameworkElement)sender).DataContext).ImportSettings;
        var selection = LBTexture.SelectedItems;

        foreach (TextureProxy proxy in selection) proxy.CopySettings(settings);
    }

    private void BtnApplyToAll_Click(object sender, RoutedEventArgs e)
    {
        var settings = ((TextureProxy)((FrameworkElement)sender).DataContext).ImportSettings;
        var vm = (ConfigureImportSettings)DataContext;

        foreach (var proxy in vm.TextureImportSettingsConfigurator.TextureProxies) proxy.CopySettings(settings);
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e) => ImportingItemCollection.Clear(AssetType.TEXTURE);

    private void BtnAddImageSource_Click(object sender, RoutedEventArgs e)
    {
        var vm = (DataContext as ConfigureImportSettings)?.TextureImportSettingsConfigurator;
        var target = ((FrameworkElement)sender).DataContext as TextureProxy;
        var items = LBTexture.SelectedItems;
        var selection = new TextureProxy[items.Count];

        items.CopyTo(selection, 0);

        foreach (TextureProxy proxy in selection) vm?.MoveToTarget(proxy, target!);
    }

    private void BtnRemoveImageSource_Click(object sender, RoutedEventArgs e)
    {
        var vm = (DataContext as ConfigureImportSettings)?.TextureImportSettingsConfigurator;
        var target = ((FrameworkElement)sender).DataContext as TextureProxy;
        var items = LBImageSources.SelectedItems;
        var selection = new TextureProxy[items.Count];

        items.CopyTo(selection, 0);

        foreach (TextureProxy proxy in selection) vm?.MoveFromTarget(proxy, target!);
    }

    private void BtnMoveUp_Click(object sender, RoutedEventArgs e) =>
        MoveSelection(sender, (target, selection) => target?.MoveUp(selection));

    private void BtnMoveDown_Click(object sender, RoutedEventArgs e) =>
        MoveSelection(sender, (target, selection) => target?.MoveDown(selection));

    private void MoveSelection(object sender, Action<TextureProxy?, List<TextureProxy>> action)
    {
        var target = ((FrameworkElement)sender).DataContext as TextureProxy;
        var items = LBImageSources.SelectedItems;
        var selection = new List<TextureProxy>();

        foreach (TextureProxy proxy in items) selection.Add(proxy);

        action(target, selection);
    }
}
