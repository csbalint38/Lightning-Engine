using Editor.Components;
using Editor.GameProject;
using Editor.Utilities;
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

        private void BtnAddEntity_Click(object sender, System.Windows.RoutedEventArgs e)
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
            ComponentsView.Instance.DataContext = null;
            var listBox = sender as ListBox;

            if(e.AddedItems.Count > 0)
            {
                ComponentsView.Instance.DataContext = listBox.SelectedItems[0];
            }

            var newSelection = listBox.SelectedItems.Cast<Entity>().ToList();
            var previousSelection = newSelection.Except(e.AddedItems.Cast<Entity>()).Concat(e.RemovedItems.Cast<Entity>()).ToList();

            Project.UndoRedo.Add(new UndoRedoAction(
                "Selection changed",
                () =>
                {
                    listBox.UnselectAll();
                    previousSelection.ForEach(x => (listBox.ItemContainerGenerator.ContainerFromItem(x) as ListBoxItem).IsSelected = true);
                },
                () =>
                {
                    listBox.UnselectAll();  
                    newSelection.ForEach(x => (listBox.ItemContainerGenerator.ContainerFromItem(x) as ListBoxItem).IsSelected = true);
                }
            ));
        }
    }
}
