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
        var btn = sender as Button;
        var scene = btn.DataContext as Scene;

        scene.AddEntityCommand.Execute(new Entity(scene)
        {
            Name = "Empty Game Entity"
        });
    }

    private void LbEntities_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        EntityView.Instance.DataContext = null;
        var listBox = sender as ListBox;
        var vm = listBox.DataContext as Scene;
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

        MSEntity msEntities = null;

        if (newSelection.Count != 0)
        {
            msEntities = new MSEntity(newSelection);
        }
        EntityView.Instance.DataContext = msEntities;
    }

    private void BtnRenameScene_Click(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)(sender as Button).Tag;

        textBox.Visibility = Visibility.Visible;
        textBox.Focus();
    }

    private void LbEntities_Loaded(object sender, RoutedEventArgs e)
    {
        var gameEntityListBox = sender as ListBox;

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
                EntityView.Instance.DataContext = null;
            }
        }
    }

    private void LbEntities_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Delete)
        {
            // TODO: Implement delete
        }
    }

    private async void LbEntities_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop) && sender is FrameworkElement
            {
                DataContext: Scene scene
            } && scene.IsActive)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            var fileList = files?
                .Where(x =>
                    Path.GetExtension(x).ToLower() == Asset.AssetFileExtension
                    && Asset.TryGetAssetInfo(x)?.Type == AssetType.MESH
                )
                .ToList();

            List<Entity> entities = [];

            await Task.Run(() =>
            {
                foreach (var file in fileList)
                {
                    Debug.Assert(!string.IsNullOrEmpty(file.Trim()));

                    var assetInfo = Asset.TryGetAssetInfo(file);

                    if (assetInfo is not null)
                    {
                        var entity = new Entity(scene)
                        {
                            Name = assetInfo.FileName.Trim()
                        };

                        entity.IsActive = true;
                        entity.AddComponent(new Components.Geometry(entity, assetInfo));
                        entities.Add(entity);
                    }
                }
            });

            entities.ForEach(entity => scene.AddEntityCommand.Execute(entity));
        }
    }
}
