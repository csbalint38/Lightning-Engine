using System.Windows;
using System.Windows.Controls;

namespace Editor.GameProject
{
    /// <summary>
    /// Interaction logic for OpenProjectView.xaml
    /// </summary>
    public partial class OpenProjectView : UserControl
    {
        public OpenProjectView()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var item = LbProjects.ItemContainerGenerator.ContainerFromIndex(LbProjects.SelectedIndex) as ListBoxItem;
                item?.Focus();
            };
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedProject();
        }

        private void OpenSelectedProject()
        {
            var project = OpenProject.Open(LbProjects.SelectedItem as ProjectData);
            bool dialogResult = false;
            var win = Window.GetWindow(this);

            if (project is not null)
            {
                dialogResult = true;
                win.DataContext = project;
            }

            win.DialogResult = dialogResult;
            win.Close();
        }

        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenSelectedProject();
        }
    }
}
