using Editor.Common;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace Editor.GameProject
{
    class NewProject : ViewModelBase
    {
        private readonly string _templatePath = $@"..\..\Editor\ProjectTemplates";
        private string _projectName = "NewProject";
        private string _projectPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\LightningProjects\";
        private readonly ObservableCollection<ProjectTemplate> _templates = [];
        private bool _isValid;
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
            set
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
            set
            {
                if(_errorMessage != value)
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
                var templateFiles = Directory.GetFiles(_templatePath, "template.xml", SearchOption.AllDirectories);
                Debug.Assert(templateFiles.Length != 0);

                foreach (var templateFile in templateFiles) {
                    var template = Serializer.FromFile<ProjectTemplate>(templateFile);

                    template.IconFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(templateFile)!, "icon.png"));
                    template.ScreenshotFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(templateFile)!, "screenshot.png"));
                    template.ProjectFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(templateFile)!, template.ProjectFile));
                    template.Icon = File.ReadAllBytes(template.IconFilePath);
                    template.Screenshot = File.ReadAllBytes(template.ScreenshotFilePath);

                    _templates.Add(template);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Validate();
        }

        public string CreateProject(ProjectTemplate template)
        {
            Validate();

            if(!IsValid) return string.Empty;
            if(!Path.EndsInDirectorySeparator(ProjectPath)) ProjectPath += @"\";

            var path = $@"{ProjectPath}{ProjectName}\";

            try
            {
                if(!Directory.Exists(path)) Directory.CreateDirectory(path);

                foreach (var folder in template.Folders)
                {
                    Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, folder)));
                }

                var dirInfo = new DirectoryInfo(path + @".Lightning\");
                dirInfo.Attributes |= FileAttributes.Hidden;

                File.Copy(template.IconFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "icon.png")));
                File.Copy(template.ScreenshotFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "screenshot.png")));

                var projectFile = File.ReadAllText(template.ProjectFilePath);
                projectFile = string.Format(projectFile, ProjectName, ProjectPath);

                var projectPath = Path.GetFullPath(Path.Combine(path, $"{ProjectName}{Project.Extension}"));
                File.WriteAllText(projectPath, projectFile);

                return path;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        private bool Validate()
        {
            var path = ProjectPath;

            if (!Path.EndsInDirectorySeparator(path)) path += @"\";

            path += $@"{ProjectName}\";
            IsValid = false;

            if (string.IsNullOrWhiteSpace(ProjectName.Trim()))
                ErrorMessage = "Project name cannot be empty.";
            else if (ProjectName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                ErrorMessage = "Project name contains invalid characters.";
            else if (string.IsNullOrWhiteSpace(ProjectPath.Trim()))
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
