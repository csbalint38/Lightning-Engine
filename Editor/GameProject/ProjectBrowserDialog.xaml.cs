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
                BtnToggleOpenNew.Content = "New Project";
                BrdContent.Child = new NewProject();
            }
            else
            {
                _isOpenProject = true;
                BtnToggleOpenNew.Content = "Back";
                BrdContent.Child = new OpenProject();
            }
        }
    }
}
