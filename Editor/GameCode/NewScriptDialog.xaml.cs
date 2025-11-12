using Editor.Common.Enums;
using Editor.Config;
using Editor.GameProject;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

            TbMessage.Text = $"{name}.h and {name}.cpp will be added to {Project.Current!.Name}";
        }

        private void TbPath_TextChanged(object sender, TextChangedEventArgs e) => Validate();

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (!Validate()) return;

            IsEnabled = false;
            loadingSpinner.Visibility = Visibility.Visible;

            try
            {
                var name = TbScriptName.Text.Trim();
                var path = Path.GetFullPath(Path.Combine(Project.Current!.Path, TbPath.Text.Trim()));
                var solution = Project.Current.Solution;
                var projectName = Project.Current.Name;

                await Task.Run(() => CreateScript(name, path, solution, projectName));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.LogAsync(LogLevel.ERROR, $"Failed to create script {TbScriptName.Text}");
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
            var nameRegex = NameRegex();

            if (string.IsNullOrEmpty(name))
            {
                errorMessage = "Script name cannot be empty.";
            }
            else if (!nameRegex.IsMatch(name))
            {
                errorMessage = "Script name contains invalid characters.";
            }
            else if (name.Length > 64)
            {
                errorMessage = "Script name can't be longer, than 64 characters.";
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
                !Path.GetFullPath(Path.Combine(Project.Current!.Path, path)).Contains(Path.Combine(Project.Current.Path, @"Code\"))
            )
            {
                errorMessage = "Script must be placed in the Code folder or in one of its subfolder.";
            }
            else if (
                File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.cpp")))
                || File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.h")))
                || File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.h")))
            )
            {
                errorMessage = "Script with the same name already exists in the selected folder.";
            }
            else
            {
                isValid = true;
            }

            TbMessage.Foreground = (isValid ? FindResource("Editor.FontBrush") : FindResource("Editor.RedBrush")) as Brush;
            TbMessage.Text = errorMessage;
            BtnOk.IsEnabled = isValid;

            return isValid;
        }

        private static void CreateScript(string name, string path, string solution, string projectName)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var files = ConfigManager.Config!.CodeConfig.CppTemplate == CppTemplate.MODULE
                ? CreateScriptModules(name, path)
                : CreateScriptLegacy(name, path);

            if (ICodeEditor.Current.GetType() == typeof(VisualStudio))
            {
                VisualStudio.AddFilesToSolution(solution, projectName, files);
            }
            else
            {
                MSBuild.AddFilesToSolution(solution, projectName, files);
            }

            ICodeEditor.Current.ShowWindow(solution, files[0]);
        }

        private static string GetNamespaceFromProjectName()
        {
            var projectName = Project.Current!.Name.Trim().ToLower();

            if (string.IsNullOrEmpty(projectName)) return string.Empty;

            return projectName;
        }

        private static string[] CreateScriptLegacy(string name, string path)
        {
            var cpp = Path.GetFullPath(Path.Combine(path, $"{name}.cpp"));
            var h = Path.GetFullPath(Path.Combine(path, $"{name}.h"));
            var cppPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "CodeTemplates",
                "Legacy",
                "cpp.txt"
            );
            var hPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "CodeTemplates",
                "Legacy",
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

            return [cpp, h];
        }

        private static string[] CreateScriptModules(string name, string path)
        {
            var cpp = Path.GetFullPath(Path.Combine(path, $"{name}.cpp"));
            var ixx = Path.GetFullPath(Path.Combine(path, $"{name}.ixx"));
            var cppPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "CodeTemplates",
                "Module",
                "cpp.txt"
            );
            var ixxPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "CodeTemplates",
                "Module",
                "ixx.txt"
            );
            var cppTemplate = File.ReadAllText(cppPath);
            var ixxTemplate = File.ReadAllText(ixxPath);

            using (var sw = File.CreateText(cpp))
            {
                sw.Write(string.Format(cppTemplate, name, _namespace));
            }

            using (var sw = File.CreateText(ixx))
            {
                sw.Write(string.Format(ixxTemplate, name, _namespace));
            }

            return [cpp, ixx];
        }

        [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]*$")]
        private static partial Regex NameRegex();
    }
}
