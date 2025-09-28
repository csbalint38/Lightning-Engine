using Editor.Common.Enums;
using Editor.Components;
using Editor.Content;
using Editor.GameProject;
using Editor.Utilities;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Editors;

/// <summary>
/// Interaction logic for GeometryComponentView.xaml
/// </summary>
public partial class GeometryComponentView : UserControl
{
    public GeometryComponentView()
    {
        InitializeComponent();
    }

    private static void ResetGeometry(
        List<(Components.Geometry Geometry, Guid Guid, List<AppliedMaterial> Materials)> selection
    )
    {
        var entities = selection.Select(x => x.Geometry.ParentEntity).ToList();

        selection.ForEach(x =>
        {
            x.Geometry.SetGeometry(x.Guid);
        });

        MSEntity.CurrentSelection?.GetMSComponent<MSGeometry>()?.Refresh();
    }

    private async void BrdGeometry_DropAsync(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var file = files
                .Where(
                    x =>
                        Path
                            .GetExtension(x)
                            .Equals(Asset.AssetFileExtension, StringComparison.CurrentCultureIgnoreCase) &&
                    Asset.TryGetAssetInfo(x)?.Type == AssetType.MESH
                )
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(file?.Trim()) && DataContext is MSGeometry vm)
            {
                var assetInfo = Asset.TryGetAssetInfo(file);

                if (assetInfo is not null)
                {
                    var undoSelection = vm.SelectedComponents
                        .Select(x => (x, x.GeometryGuid, x.Materials))
                        .ToList();

                    await Task.Run(() => vm.SetGeometry(assetInfo.Guid));

                    var redoSelection = vm.SelectedComponents
                        .Select(x => (x, assetInfo.Guid, x.Materials))
                        .ToList();

                    Project.UndoRedo.Add(new UndoRedoAction(
                        $"Set Geometry {assetInfo.FileName}",
                        () => ResetGeometry(undoSelection),
                        () => ResetGeometry(redoSelection)
                    ));
                }
            }
        }
    }

    private void BrdGeometry_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount > 1 && DataContext is MSGeometry vm && vm.GeometryGuid != Guid.Empty)
        {
            ContentBrowserView.OpenAssetEditor(AssetRegistry.GetAssetInfo(vm.GeometryGuid));
        }
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is TabControl tabControl && tabControl.SelectedIndex == -1) tabControl.SelectedIndex = 0;
    }
}
