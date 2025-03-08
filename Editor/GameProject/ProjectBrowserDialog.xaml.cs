using System.Windows;

namespace Editor.GameProject
{
    /// <summary>
    /// Interaction logic for ProjectBrowserDialog.xaml
    /// </summary>
    public partial class ProjectBrowserDialog : Window
    {
        private bool _isOpenProject = true;

        public ProjectBrowserDialog()
        {
            InitializeComponent();
        }

        private void BtnToggleOpenNew_Click(object sender, RoutedEventArgs e)
        {
            if (_isOpenProject)
            {
                _isOpenProject = false;
                BtnToggleOpenNew.Content = "Back";
                BrdContent.Child = new NewProjectView();
            }
            else
            {
                _isOpenProject = true;
                BtnToggleOpenNew.Content = "New Project";
                BrdContent.Child = new OpenProjectView();
            }
        }
    }
}
