using Editor.GameProject;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Editor.GameCode
{
    /// <summary>
    /// Interaction logic for NewScriptDialog.xaml
    /// </summary>
    public partial class NewScriptDialog : Window
    {
        public NewScriptDialog()
        {
            InitializeComponent();
        }

        private void TbScriptName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TbPath_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private bool Validate()
        {
            bool isValid = false;

            var name = TbScriptName.Text.Trim();
            var path = TbPath.Text.Trim();
            string errorMessage = string.Empty;

            if (string.IsNullOrEmpty(name))
            {
                errorMessage = "Script name cannot be empty.";
            }
            else if (name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1 || name.Any(x => char.IsWhiteSpace(x)))
            {
                errorMessage = "Script name contains invalid characters.";
            }
            if (string.IsNullOrEmpty(path))
            {
                errorMessage = "Select a valid folder";
            }
            else if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                errorMessage = "Invalid character(s) used in path";
            }
            else if (
                !Path.GetFullPath(Path.Combine(Project.Current.Path, path)).Contains(Path.Combine(Project.Current.Path, @"Code\"))
            )
            {
                errorMessage = "Script must be placed in the Code folder or in one of its subfolder.";
            }
            else if (
                File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.cpp"))) ||
                File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.h")))
            )
            {
                errorMessage = "Script with the same name already exists in the selected folder.";
            }
            else
            {
                isValid = true;
            }

            TbMessage.Text = errorMessage;

            return isValid;
        }
    }
}
