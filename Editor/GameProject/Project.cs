using Editor.Common;
using Editor.Common.Enums;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace Editor.GameProject
{
    [DataContract]
    internal class Project : ViewModelBase
    {
        [DataMember(Name = "Scenes")]
        private ObservableCollection<Scene> _scenes = [];

        private Scene _activeScene;

        public static readonly string Extension = ".lightning";
        public static Project Current => Application.Current.MainWindow.DataContext as Project; // This should be nullable
        public static UndoRedo UndoRedo { get; } = new UndoRedo();

        [DataMember]
        public string Name { get; private set; } = "New Project";

        [DataMember]
        public string Path { get; private set; }
        public string FullPath => $@"{Path}{Name}\{Name}{Extension}";

        public ReadOnlyObservableCollection<Scene> Scenes { get; private set; }
        public ICommand AddSceneCommand { get; private set; }
        public ICommand RemoveSceneCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public Scene ActiveScene
        {
            get => _activeScene;
            set
            {
                if (_activeScene != value)
                {
                    _activeScene = value;
                    OnPropertyChanged(nameof(ActiveScene));
                }
            }
        }

        public static Project Load(string file)
        {
            Debug.Assert(File.Exists(file));

            return Serializer.FromFile<Project>(file);
        }

        public static void Save(Project project) {
            Serializer.ToFile(project, project.FullPath);
            Logger.Log(LogLevel.INFO, $"Project saved to {project.FullPath}");
        }

        public Project(string name, string path)
        {
            Name = name;
            Path = path;

            OnDeserialized(new StreamingContext());
        }

        public void Unload()
        {
            UndoRedo.Reset();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if(_scenes is not null)
            {
                Scenes = new ReadOnlyObservableCollection<Scene>(_scenes);
                OnPropertyChanged(nameof(Scenes));
            }
            
            ActiveScene = Scenes.FirstOrDefault(x => x.IsActive);

            AddSceneCommand = new RelayCommand<object>(x =>
            {
                AddScene($"New Scene {_scenes.Count}");
                var newScene = _scenes.Last();
                var index = _scenes.Count - 1;
                UndoRedo.Add(new UndoRedoAction(
                    $"Add {newScene.Name}",
                    () => RemoveScene(newScene),
                    () => _scenes.Insert(index, newScene)
                ));
            });

            RemoveSceneCommand = new RelayCommand<Scene>(
                x =>
                {
                    var sceneIndex = _scenes.IndexOf(x);
                    RemoveScene(x);
                    UndoRedo.Add(new UndoRedoAction(
                        $"Remove {x.Name}",
                        () => _scenes.Insert(sceneIndex, x),
                        () => RemoveScene(x)
                    ));
                },
                x => !x.IsActive
            );

            UndoCommand = new RelayCommand<object>(x => UndoRedo.Undo(), x => UndoRedo.UndoList.Any());
            RedoCommand = new RelayCommand<object>(x => UndoRedo.Redo(), x => UndoRedo.RedoList.Any());
            SaveCommand = new RelayCommand<object>(x => Save(this));
        }

        private void AddScene(string sceneName)
        {
            Debug.Assert(!string.IsNullOrEmpty(sceneName.Trim()));

            _scenes.Add(new Scene(this, sceneName));
        }

        private void RemoveScene(Scene scene)
        {
            Debug.Assert(_scenes.Contains(scene));

            _scenes.Remove(scene);
        }
    }
}
