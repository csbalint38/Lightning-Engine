using Editor.Common;
using Editor.Common.Enums;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Editor.GameProject
{
    class NewProject : ViewModelBase
    {
        private string _projectName = "NewProject";
        private string _projectPath =
            $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\LightningProjects\";

        private readonly ObservableCollection<ProjectTemplate> _templates = [];
        private bool _isValid = true;
        private string _errorMessage = string.Empty;

        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (_projectName != value)
                {
                    _projectName = value;
                    Validate();
                    OnPropertyChanged(nameof(ProjectName));
                }
            }
        }

        public string ProjectPath
        {
            get => _projectPath;
            set
            {
                if (_projectPath != value)
                {
                    _projectPath = value;
                    Validate();
                    OnPropertyChanged(nameof(ProjectPath));
                }
            }
        }

        public bool IsValid
        {
            get => _isValid;
            private set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }

        public ReadOnlyObservableCollection<ProjectTemplate> Templates { get; }

        public NewProject()
        {
            Templates = new ReadOnlyObservableCollection<ProjectTemplate>(_templates);

            try
            {
                var templatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\ProjectTemplates\");
                var templateFiles = Directory.GetFiles(templatesPath, "template.xml", SearchOption.AllDirectories);
                Debug.Assert(templateFiles.Length != 0);

                foreach (var templateFile in templateFiles)
                {
                    var template = Serializer.FromFile<ProjectTemplate>(templateFile);

                    template.TemplatePath = Path.GetDirectoryName(templateFile);

                    template.IconFilePath = Path.GetFullPath(
                        Path.Combine(template.TemplatePath, "icon.png")
                    );

                    template.ScreenshotFilePath = Path.GetFullPath(
                        Path.Combine(template.TemplatePath, "screenshot.png")
                    );


                    template.ProjectFilePath = Path.GetFullPath(
                        Path.Combine(template.TemplatePath, template.ProjectFile)
                    );

                    template.Icon = File.ReadAllBytes(template.IconFilePath);
                    template.Screenshot = File.ReadAllBytes(template.ScreenshotFilePath);

                    _templates.Add(template);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.LogAsync(LogLevel.ERROR, "Failed to load project templates");

                throw;
            }

            Validate();
        }

        public string CreateProject(ProjectTemplate template)
        {
            if (!Validate()) return string.Empty;

            ProjectName = ProjectName.Trim();
            ProjectPath = ProjectPath.Trim();

            if (!Path.EndsInDirectorySeparator(ProjectPath)) ProjectPath += @"\";

            var path = $@"{ProjectPath}{ProjectName}\";

            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                foreach (var folder in template.Folders)
                {
                    Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, folder)));
                }

                var dirInfo = new DirectoryInfo(path + @".Lightning\");
                dirInfo.Attributes |= FileAttributes.Hidden;

                File.Copy(template.IconFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "icon.png")));
                File.Copy(template.ScreenshotFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "screenshot.png")));

                var projectFile = File.ReadAllText(template.ProjectFilePath);
                projectFile = string.Format(projectFile, ProjectName, path);

                var projectPath = Path.GetFullPath(Path.Combine(path, $"{ProjectName}{Project.Extension}"));
                File.WriteAllText(projectPath, projectFile);

                CreateMSVCSolution(template, path);

                return path;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.LogAsync(LogLevel.ERROR, $"Failed to create project: {path}");

                throw;
            }
        }

        private void CreateMSVCSolution(ProjectTemplate template, string projectPath)
        {
            Debug.Assert(File.Exists(Path.Combine(template.TemplatePath, "SolutionTemplate.xml")));
            Debug.Assert(File.Exists(Path.Combine(template.TemplatePath, "ProjectTemplate.xml")));

            var engineApiPath = @"$(LIGHTNING_ENGINE)Engine\EngineAPI\";

            //Debug.Assert(Directory.Exists(engineApiPath));

            var _0 = ProjectName;
            var _1 = "{" + Guid.NewGuid().ToString().ToUpper() + "}";
            var _2 = "{" + Guid.NewGuid().ToString().ToUpper() + "}";

            var solution = File.ReadAllText(Path.Combine(template.TemplatePath, "SolutionTemplate.xml"));
            solution = string.Format(solution, _0, _1, _2);

            File.WriteAllText(Path.GetFullPath(Path.Combine(projectPath, $"{_0}.sln")), solution);

            _2 = engineApiPath;

            #if DEBUG
            var _3 = "$(LIGHTNING_ENGINE)";
            #else
            var _3 = "$(LIGHTNING_ENGINE)Engine";
            #endif

            var project = File.ReadAllText(Path.Combine(template.TemplatePath, "ProjectTemplate.xml"));
            project = string.Format(project, _0, _1, _2, _3);

            File.WriteAllText(Path.GetFullPath(Path.Combine(projectPath, $@"Code\{_0}.vcxproj")), project);
        }

        private bool Validate()
        {
            var path = ProjectPath;

            if (!Path.EndsInDirectorySeparator(path)) path += @"\";

            path += $@"{ProjectName}\";
            IsValid = false;

            var nameRegex = new Regex(@"[^A-Za-z0-9_]");

            if (string.IsNullOrEmpty(ProjectName.Trim()))
                ErrorMessage = "Project name cannot be empty.";
            else if (nameRegex.IsMatch(ProjectName))
                ErrorMessage = "Project name contains invalid characters.";
            else if (string.IsNullOrEmpty(ProjectPath.Trim()))
                ErrorMessage = "Project path cannot be empty.";
            else if (ProjectPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                ErrorMessage = "Project path contains invalid characters.";
            else if (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any())
                ErrorMessage = "Folder already exists and is not empty.";
            else
            {
                ErrorMessage = string.Empty;
                IsValid = true;
            }

            return IsValid;
        }
    }
}
