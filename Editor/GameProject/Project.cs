using Editor.Common;
using Editor.Common.Enums;
using Editor.DLLs;
using Editor.GameCode;
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
        private static readonly string[] _buildConfigurationNames = ["Debug", "DebugEditor", "Release", "ReleaseEditor"];

        [DataMember(Name = "Scenes")]
        private ObservableCollection<Scene> _scenes = [];

        private Scene _activeScene;
        private int _buildConfig;
        private string[] _availableScripts;

        public static readonly string Extension = ".lightning";
        public static Project Current => Application.Current.MainWindow.DataContext as Project; // This should be nullable
        public static UndoRedo UndoRedo { get; } = new UndoRedo();

        [DataMember]
        public string Name { get; private set; } = "New Project";

        [DataMember]
        public string Path { get; private set; }
        public string FullPath => $@"{Path}{Name}{Extension}";
        public string Solution => $@"{Path}{Name}.sln";
        public ReadOnlyObservableCollection<Scene> Scenes { get; private set; }
        public BuildConfig StandaloneBuildConfig => BuildConfiguration == 0 ? BuildConfig.DEBUG : BuildConfig.RELEASE;
        public BuildConfig DllBuildConfig => BuildConfiguration == 0 ? BuildConfig.DEBUG_EDITOR : BuildConfig.RELEASE_EDITOR;
        public ICommand AddSceneCommand { get; private set; }
        public ICommand RemoveSceneCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand BuildCommand { get; private set; }

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

        [DataMember]
        public int BuildConfiguration
        {
            get => _buildConfig;
            set
            {
                if (_buildConfig != value)
                {
                    _buildConfig = value;
                    OnPropertyChanged(nameof(BuildConfig));
                }
            }
        }

        public string[] AvailableScripts
        {
            get => _availableScripts;
            set
            {
                if (_availableScripts != value)
                {
                    _availableScripts = value;
                    OnPropertyChanged(nameof(AvailableScripts));
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
            Logger.LogAsync(LogLevel.INFO, $"Project saved to {project.FullPath}");
        }

        public Project(string name, string path)
        {
            Name = name;
            Path = path;

            OnDeserializedAsync(new StreamingContext());
        }

        private static string GetConfigurationName(BuildConfig config) => _buildConfigurationNames[(int)config];

        public void Unload()
        {
            VisualStudio.CloseVisualStudio();
            UndoRedo.Reset();
        }

        [OnDeserialized]
        private async  void OnDeserializedAsync(StreamingContext context)
        {
            if(_scenes is not null)
            {
                Scenes = new ReadOnlyObservableCollection<Scene>(_scenes);
                OnPropertyChanged(nameof(Scenes));
            }
            
            ActiveScene = Scenes.FirstOrDefault(x => x.IsActive);

            await BuildGameCodeDllAsync(false);

            SetCommands();
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

        private async Task BuildGameCodeDllAsync(bool showWindow = true)
        {
            try
            {
                UnloadGameCodeDll();
                await Task.Run(() => VisualStudio.BuildSolution(this, GetConfigurationName(DllBuildConfig), showWindow));

                if (VisualStudio.BuildSucceeded) LoadGameCodeDll();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        private void LoadGameCodeDll()
        {
            var configName = GetConfigurationName(DllBuildConfig);
            var dll = $@"{Path}x64\{configName}\{Name}.dll";

            AvailableScripts = [];

            if (File.Exists(dll) && EngineAPI.LoadGameCodeDll(dll) != 0)
            {
                AvailableScripts = EngineAPI.GetScriptNames();
                Logger.LogAsync(LogLevel.INFO, "Game code DLL loaded successfully");
            }
            else
            {
                Logger.LogAsync(LogLevel.WARNING, "Failed to load game code DLL. Try to build the project first.");
            }
        }

        private void UnloadGameCodeDll()
        {
            if (EngineAPI.UnloadGameCodeDll() != 0) Logger.LogAsync(LogLevel.INFO, "Game code DLL unloaded");
            AvailableScripts = [];
        }

        private void SetCommands()
        {
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
            BuildCommand = new RelayCommand<bool>(
                async x => await BuildGameCodeDllAsync(x),
                x => !VisualStudio.IsDebugging() && VisualStudio.BuildFinished
            );

            OnPropertyChanged(nameof(AddSceneCommand));
            OnPropertyChanged(nameof(RemoveSceneCommand));
            OnPropertyChanged(nameof(UndoCommand));
            OnPropertyChanged(nameof(RedoCommand));
            OnPropertyChanged(nameof(SaveCommand));
            OnPropertyChanged(nameof(BuildCommand));
        }
    }
}
