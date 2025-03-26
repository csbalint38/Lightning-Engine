using Editor.Common.Enums;
using Editor.GameProject;
using Editor.Utilities;
using System.Diagnostics;
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
        private static readonly string _namespace = GetNamespaceFromProjectName();

        public NewScriptDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            TbPath.Text = @"Code\";
        }

        private void TbScriptName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Validate()) return;

            var name = TbScriptName.Text.Trim();

            TbMessage.Text = $"{name}.h and {name}.cpp will be added to {Project.Current.Name}";
        }

        private void TbPath_TextChanged(object sender, TextChangedEventArgs e) => Validate();

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate()) return;

            IsEnabled = false;

            try
            {
                var name = TbScriptName.Text.Trim();
                var path = Path.GetFullPath(Path.Combine(Project.Current.Path, TbPath.Text.Trim()));
                var solution = Project.Current.Solution;
                var projectName = Project.Current.Name;

                await Task.Run(() => CreateScript(name, path, solution, projectName));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(LogLevel.ERROR, $"Failed to create script {TbScriptName.Text}");
            }
            finally
            {
                Close();
            }
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
            else if (string.IsNullOrEmpty(path))
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

        private static void CreateScript(string name, string path, string solution, string projectName)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var cpp = Path.GetFullPath(Path.Combine(path, $"{name}.cpp"));
            var h = Path.GetFullPath(Path.Combine(path, $"{name}.h"));
            var cppPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "GameCode",
                "Templates",
                "cpp.txt"
            );
            var hPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "GameCode",
                "Templates",
                "h.txt"
            );
            var cppTemplate = File.ReadAllText(cppPath);
            var hTemplate = File.ReadAllText(hPath);

            using (var sw = File.CreateText(cpp))
            {
                sw.Write(string.Format(cppTemplate, name, _namespace));
            }

            using (var sw = File.CreateText(h))
            {
                sw.Write(string.Format(hTemplate, name, _namespace));
            }

            string[] files = [cpp, h];

            for (int i = 0; i < 3; ++i)
            {
                if (!VisualStudio.AddFilesToSolution(solution, projectName, files)) Thread.Sleep(1000);
                else break;
            }
        }

        private static string GetNamespaceFromProjectName()
        {
            var projectName = Project.Current.Name;
            projectName = projectName.ToLower().Replace(" ", "_");

            return projectName;
        }
    }
}
