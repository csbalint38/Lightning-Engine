using Editor.Content;
using Editor.GameCode;
using Editor.GameProject;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.Editors
{
    /// <summary>
    /// Interaction logic for WorldEditorView.xaml
    /// </summary>
    public partial class WorldEditorView : UserControl
    {
        public WorldEditorView()
        {
            InitializeComponent();
            Loaded += WorldEditorView_OnLoaded;
        }

        private void WorldEditorView_OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= WorldEditorView_OnLoaded;
            Focus();
        }

        private void BtnNewScript_Click(object sender, RoutedEventArgs e) => new NewScriptDialog().ShowDialog();

        private void BtnPrimitiveMesh_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PrimitiveMeshDialog();
            dialog.Show();
        }

        private void MINewProject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProjectBrowserDialog.GoToNewProjectTab = true;
            Project.Current?.Unload();
            Application.Current.MainWindow.DataContext = null;
            Application.Current.MainWindow.Close();
        }

        private void MIOpenProject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Project.Current?.Unload();
            Application.Current.MainWindow.DataContext = null;
            Application.Current.MainWindow.Close();
        }

        private void MIExit_Executed(object sender, ExecutedRoutedEventArgs e) => Application.Current.MainWindow.Close();
    }
}
