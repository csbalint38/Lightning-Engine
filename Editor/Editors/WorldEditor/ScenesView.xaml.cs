using Editor.Common.Enums;
using Editor.Components;
using Editor.Content;
using Editor.GameProject;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.Editors;

/// <summary>
/// Interaction logic for ScenesView.xaml
/// </summary>
public partial class ScenesView : UserControl
{
    private List<int> _previousSelectedIndices = [];

    public ScenesView()
    {
        InitializeComponent();
    }

    private void BtnAddEntity_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        var scene = (Scene)btn.DataContext;

        scene.AddEntities([new(scene) {
            Name = "Empty Game Entity"
        }]);
    }

    private void LbEntities_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        EntityView.Instance!.DataContext = null;
        var listBox = (ListBox)sender;
        var vm = (Scene)listBox.DataContext;
        var newSelection = listBox.SelectedItems.Cast<Entity>().ToList();
        var newSelectedIndices = newSelection.Select(item => vm.Entities.IndexOf(item)).ToList();
        var previousSelectetedIndices = _previousSelectedIndices.ToList();

        _previousSelectedIndices = [.. newSelectedIndices];

        Project.UndoRedo.Add(new UndoRedoAction(
            "Selection changed",
            () =>
            {
                listBox.UnselectAll();

                previousSelectetedIndices.ForEach(x =>
                {
                    if (listBox.ItemContainerGenerator.ContainerFromIndex(x) is ListBoxItem item)
                    {
                        item.IsSelected = true;
                    }
                });
            },
            () =>
            {
                listBox.UnselectAll();

                newSelectedIndices.ForEach(x =>
                {
                    if (listBox.ItemContainerGenerator.ContainerFromIndex(x) is ListBoxItem item)
                    {
                        item.IsSelected = true;
                    }
                });
            }
        ));

        MSEntityBase? msEntities = null;
        MSEntityBase.Reset();

        if (newSelection.Count != 0)
        {
            msEntities = new MSEntity(newSelection);
        }
        EntityView.Instance.DataContext = msEntities;
    }

    private void BtnRenameScene_Click(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)((Button)sender).Tag;

        textBox.Visibility = Visibility.Visible;
        textBox.Focus();
    }

    private void LbEntities_Loaded(object sender, RoutedEventArgs e)
    {
        var gameEntityListBox = (ListBox)sender;

        if (gameEntityListBox.IsEnabled)
        {
            if (gameEntityListBox.Items.Count > 0)
            {
                gameEntityListBox.SelectedIndex = 0;

                var item = gameEntityListBox.ItemContainerGenerator
                    .ContainerFromIndex(gameEntityListBox.SelectedIndex) as ListBoxItem;

                item?.Focus();
            }
            else
            {
                EntityView.Instance!.DataContext = null;
            }
        }
    }

    private void LbEntities_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Delete)
        {
            var listBox = (ListBox)sender;
            var entities = new List<Entity>();

            foreach (Entity entity in listBox.SelectedItems) entities.Add(entity);

            listBox.UnselectAll();

            if (entities.Count > 0) RemoveEntities(entities);
        }
    }

    private async void LbEntities_DropAsync(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop) && sender is FrameworkElement
            {
                DataContext: Scene scene
            } && scene.IsActive)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            List<Entity> entities = [];

            await Task.Run(() =>
            {
                var fileList = files
                    .Where(x =>
                        Path.GetExtension(x).ToLower() == Asset.AssetFileExtension
                        && Asset.TryGetAssetInfo(x)?.Type == AssetType.MESH
                    );

                foreach (var file in fileList)
                {
                    Debug.Assert(!string.IsNullOrEmpty(file?.Trim()));

                    if (Asset.TryGetAssetInfo(file) is AssetInfo assetInfo)
                    {
                        var entity = new Entity(scene)
                        {
                            Name = assetInfo.FileName?.Trim() ?? string.Empty,
                            IsActive = true
                        };

                        entity.AddComponent(new Components.Geometry(entity, assetInfo));
                        entities.Add(entity);
                    }
                }
            });

            if (entities.Count > 0) scene.AddEntities(entities);
        }
    }

    private void RemoveEntities(List<Entity> entities)
    {
        if (DataContext is Project
            {
                ActiveScene: Scene scene
            }) scene.RemoveEntities(entities);
    }

    private void BtnRemoveEntity_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button
            {
                DataContext: Entity entity
            }) RemoveEntities([entity]);
    }
}
