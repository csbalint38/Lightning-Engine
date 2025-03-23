using Editor.Components;
using Editor.GameProject;
using Editor.Utilities;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Editors
{
    /// <summary>
    /// Interaction logic for ScenesView.xaml
    /// </summary>
    public partial class ScenesView : UserControl
    {
        public ScenesView()
        {
            InitializeComponent();
        }

        private void BtnAddEntity_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var vm = btn.DataContext as Scene;

            vm.AddEntityCommand.Execute(new Entity(vm)
            {
                Name = "Empty Game Entity"
            });
        }

        private void LbEntities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EntityView.Instance.DataContext = null;
            var listBox = sender as ListBox;
            var newSelection = listBox.SelectedItems.Cast<Entity>().ToList();
            var previousSelection = newSelection
                .Except(e.AddedItems.Cast<Entity>())
                .Concat(e.RemovedItems.Cast<Entity>())
                .ToList();

            Project.UndoRedo.Add(new UndoRedoAction(
                "Selection changed",
                () =>
                {
                    listBox.UnselectAll();
                    previousSelection.ForEach(x =>
                        (listBox.ItemContainerGenerator.ContainerFromItem(x) as ListBoxItem).IsSelected = true
                    );
                },
                () =>
                {
                    listBox.UnselectAll();  
                    newSelection.ForEach(x =>
                        (listBox.ItemContainerGenerator.ContainerFromItem(x) as ListBoxItem).IsSelected = true
                    );
                }
            ));

            MSEntity msEntity = null;

            if(newSelection.Any())
            {
                msEntity = new MSEntity(newSelection);
            }
            EntityView.Instance.DataContext = msEntity;
        }
    }
}
