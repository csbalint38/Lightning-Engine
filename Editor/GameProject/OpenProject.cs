using Editor.Common;
using Editor.Common.Enums;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;

namespace Editor.GameProject
{
    class OpenProject
    {
        private static readonly string _appdataPath =
            $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\LightningEngine\";

        private static readonly string _projectDataPath;
        private static readonly ObservableCollection<ProjectData> _projects = [];

        public static ReadOnlyObservableCollection<ProjectData> Projects { get; }

        static OpenProject()
        {
            try
            {
                if (!Directory.Exists(_appdataPath)) Directory.CreateDirectory(_appdataPath);
                _projectDataPath = $@"{_appdataPath}ProjectData.xml";
                Projects = new ReadOnlyObservableCollection<ProjectData>(_projects);
                ReadProjectData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.LogAsync(LogLevel.ERROR, "Failed to initialize OpenProject");

                throw;
            }
        }

        public static Project Open(ProjectData data)
        {
            ReadProjectData();

            var project = _projects.FirstOrDefault(x => x.FullPath == data.FullPath);

            if (project is null)
            {
                project = data;

                _projects.Add(project);
            }

            project.LastOpened = DateTime.Now;
            project.EngineVersion = Constants.EngineVersion;

            WriteProjectData();

            return Project.Load(project.FullPath);
        }

        private static void ReadProjectData()
        {
            if (File.Exists(_projectDataPath))
            {
                var projects = Serializer
                    .FromFile<ProjectDataList>(_projectDataPath)
                    .Projects
                    .OrderByDescending(x => x.LastOpened);

                _projects.Clear();

                foreach (var project in projects)
                {
                    if (File.Exists(project.FullPath))
                    {
                        project.Icon = File.ReadAllBytes($@"{project.ProjectPath}\.Lightning\icon.png");
                        project.Screenshot = File.ReadAllBytes($@"{project.ProjectPath}\.Lightning\screenshot.png");
                        _projects.Add(project);
                    }
                }
            }
        }

        private static void WriteProjectData()
        {
            var projects = _projects.OrderBy(x => x.LastOpened).ToList();

            Serializer.ToFile(new ProjectDataList() { Projects = projects }, _projectDataPath);
        }
    }
}
