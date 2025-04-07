using System.Windows;

namespace Editor.GameProject
{
    /// <summary>
    /// Interaction logic for ProjectBrowserDialog.xaml
    /// </summary>
    public partial class ProjectBrowserDialog : Window
    {
        public static bool GoToNewProjectTab { get; set; }

        private bool _isOpenProject = true;

        public ProjectBrowserDialog()
        {
            InitializeComponent();
            Loaded += OnProjectBrowserDialogOpened;
        }

        private void OnProjectBrowserDialogOpened(object sender, RoutedEventArgs e)
        {
            Loaded -= OnProjectBrowserDialogOpened;

            if (!OpenProject.Projects.Any() || GoToNewProjectTab)
            {
                if (!GoToNewProjectTab) BtnToggleOpenNew.IsEnabled = false;
                
                BtnToggleOpenNew_Click(BtnToggleOpenNew, new RoutedEventArgs());
            }

            GoToNewProjectTab = false;
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
