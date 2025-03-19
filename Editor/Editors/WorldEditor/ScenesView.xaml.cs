using Editor.Components;
using Editor.GameProject;
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
            var entity = (sender as ListBox).SelectedItems[0];
            ComponentsView.Instance.DataContext = entity;
        }
    }
}
