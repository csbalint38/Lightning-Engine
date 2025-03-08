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
        private string _projectName = "New Project";
        private string _projectPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\LightningProjects\";
        private ObservableCollection<ProjectTemplate> _templates = [];

        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (_projectName != value)
                {
                    _projectName = value;
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
                    OnPropertyChanged(nameof(ProjectPath));
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
                Debug.Assert(templateFiles.Any());

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
        }
    }
}
