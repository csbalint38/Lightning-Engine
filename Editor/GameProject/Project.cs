using Editor.Common;
using Editor.Common.Enums;
using Editor.Components;
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
    public class Project : ViewModelBase
    {
        [DataMember(Name = nameof(Scenes))]
        private readonly ObservableCollection<Scene> _scenes = [];

        private Scene _activeScene;
        private int _buildConfig;
        private string[] _availableScripts;

        public static readonly string Extension = ".lng";
        public static Project Current => Application.Current.MainWindow?.DataContext as Project;
        public static UndoRedo UndoRedo { get; } = new UndoRedo();

        [DataMember]
        public string Name { get; private set; } = "New Project";

        [DataMember]
        public string Path { get; private set; }
        public string FullPath => $@"{Path}{Name}{Extension}";
        public string Solution => $@"{Path}{Name}.sln";
        public string ContentPath => $@"{Path}Assets\";
        public string TempFolder => $@"{Path}.Lightning\Temp\";
        public ReadOnlyObservableCollection<Scene> Scenes { get; private set; }
        public BuildConfig StandaloneBuildConfig => BuildConfiguration == 0 ? BuildConfig.DEBUG : BuildConfig.RELEASE;
        public BuildConfig DLLBuildConfig => BuildConfiguration == 0 ? BuildConfig.DEBUG_EDITOR : BuildConfig.RELEASE_EDITOR;
        public ICommand AddSceneCommand { get; private set; }
        public ICommand RemoveSceneCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand BuildCommand { get; private set; }
        public ICommand DebugStartCommand { get; private set; }
        public ICommand DebugStartWithoutDebuggingCommand { get; private set; }
        public ICommand DebugStopCommand { get; private set; }

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
                    OnPropertyChanged(nameof(BuildConfiguration));
                }
            }
        }

        public string[] AvailableScripts
        {
            get => _availableScripts;
            private set
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

        public Project(string name, string path)
        {
            Name = name;
            Path = path;

            Debug.Assert(File.Exists((Path + Name + Extension).ToLower()));

            OnDeserializedAsync(new StreamingContext());
        }

        public void Unload()
        {
            UnloadGameCodeDLL();

            Task.Run(VisualStudio.CloseVisualStudio);

            UndoRedo.Reset();
            DeleteTempFolder();
        }

        [OnDeserialized]
        private async void OnDeserializedAsync(StreamingContext context)
        {
            if (_scenes is not null)
            {
                Scenes = new ReadOnlyObservableCollection<Scene>(_scenes);
                OnPropertyChanged(nameof(Scenes));
            }

            ActiveScene = _scenes.FirstOrDefault(x => x.IsActive);

            Debug.Assert(ActiveScene is not null);

            await BuildGameCodeDLLAsync(false);

            SetCommands();
        }

        private static void Save(Project project)
        {
            Serializer.ToFile(project, project.FullPath);
            Logger.LogAsync(LogLevel.INFO, $"Project saved to {project.FullPath}");
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

        private async Task BuildGameCodeDLLAsync(bool showWindow = true)
        {
            try
            {
                UnloadGameCodeDLL();
                await Task.Run(() => VisualStudio.BuildSolution(this, DLLBuildConfig, showWindow));

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
            var configName = VisualStudio.GetConfigurationName(DLLBuildConfig);
            var dll = $@"{Path}x64\{configName}\{Name}.dll";

            AvailableScripts = [];

            if (File.Exists(dll) && EngineAPI.LoadGameCodeDll(dll) != 0)
            {
                AvailableScripts = EngineAPI.GetScriptNames();
                ActiveScene.Entities.Where(x => x.GetComponent<Script>() is not null).ToList().ForEach(x => x.IsActive = true);
                Logger.LogAsync(LogLevel.INFO, "Game code DLL loaded successfully");
            }
            else
            {
                Logger.LogAsync(LogLevel.WARNING, "Failed to load game code DLL. Try to build the project first.");
            }
        }

        private void UnloadGameCodeDLL()
        {
            ActiveScene.Entities.Where(x => x.GetComponent<Script>() is not null).ToList().ForEach(x => x.IsActive = false);

            if (EngineAPI.UnloadGameCodeDll() != 0) Logger.LogAsync(LogLevel.INFO, "Game code DLL unloaded");
            AvailableScripts = [];
        }

        private async Task RunGameAsync(bool debug)
        {
            var config = StandaloneBuildConfig;

            await Task.Run(() => VisualStudio.BuildSolution(this, config, debug));

            if (VisualStudio.BuildSucceeded)
            {
                SaveToBinary();
                await Task.Run(() => VisualStudio.Run(this, config, debug));
            }
        }

        private async Task StopGameAsync() => await Task.Run(() => VisualStudio.Stop());

        private void SaveToBinary()
        {
            var config = VisualStudio.GetConfigurationName(StandaloneBuildConfig);
            var bin = $@"{Path}x64\{config}\game.bin";

            using (var bw = new BinaryWriter(File.Open(bin, FileMode.Create, FileAccess.Write)))
            {
                bw.Write(ActiveScene.Entities.Count);

                foreach (var entity in ActiveScene.Entities)
                {
                    bw.Write(0);
                    bw.Write(entity.Components.Count);

                    foreach (var component in entity.Components)
                    {
                        bw.Write((int)component.ToEnumType());
                        component.WriteToBinary(bw);
                    }
                }
            }
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
                async x => await BuildGameCodeDLLAsync(x),
                x => !VisualStudio.IsDebugging() && VisualStudio.BuildFinished
            );

            DebugStartCommand = new RelayCommand<object>(
                async x => await RunGameAsync(true),
                x => !VisualStudio.IsDebugging() && VisualStudio.BuildFinished
            );

            DebugStartWithoutDebuggingCommand = new RelayCommand<object>(
                async x => await RunGameAsync(false),
                x => !VisualStudio.IsDebugging() && VisualStudio.BuildFinished
            );

            DebugStopCommand = new RelayCommand<object>(async x => await StopGameAsync(), x => VisualStudio.IsDebugging());

            OnPropertyChanged(nameof(AddSceneCommand));
            OnPropertyChanged(nameof(RemoveSceneCommand));
            OnPropertyChanged(nameof(UndoCommand));
            OnPropertyChanged(nameof(RedoCommand));
            OnPropertyChanged(nameof(SaveCommand));
            OnPropertyChanged(nameof(BuildCommand));
            OnPropertyChanged(nameof(DebugStartCommand));
            OnPropertyChanged(nameof(DebugStartWithoutDebuggingCommand));
            OnPropertyChanged(nameof(DebugStopCommand));
        }

        private void DeleteTempFolder()
        {
            if (Directory.Exists(TempFolder)) Directory.Delete(TempFolder, true);
        }
    }
}
