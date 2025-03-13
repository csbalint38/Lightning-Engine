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

        private void BtnAddScene_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as Project;

            vm.AddScene("NewScene" + vm.Scenes.Count);  
        }
    }
}
