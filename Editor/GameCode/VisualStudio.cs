using Editor.Common.Enums;
using Editor.GameProject;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Editor.GameCode
{
    static class VisualStudio
    {
        private static readonly string _progId = "VisualStudio.DTE.17.0";
        private static readonly string[] _buildConfigurationNames = ["Debug", "DebugEditor", "Release", "ReleaseEditor"];
        private static readonly ManualResetEventSlim _resetEvent = new(false);
        private static readonly object _lock = new();
        private static EnvDTE80.DTE2 _vsInstance = null;

        public static bool BuildSucceeded { get; private set; }
        public static bool BuildFinished { get; private set; }

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(uint reserved, out IRunningObjectTable rot);

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        public static string GetConfigurationName(BuildConfig config) => _buildConfigurationNames[(int)config];

        public static void OpenVisualStudio(string solutionPath)
        {
            lock (_lock)
            {
                OpenVisualStudioInternal(solutionPath);
            }
        }

        public static void CloseVisualStudio()
        {
            lock (_lock)
            {
                CloseVisualStudioInternal();
            }
        }

        public static bool AddFilesToSolution(string solution, string projectName, string[] files)
        {
            lock (_lock)
            {
                return AddFilesToSolutionInternal(solution, projectName, files);
            }
        }

        public static bool IsDebugging()
        {
            lock (_lock)
            {
                return IsDebuggingInternal();
            }
        }

        public static void BuildSolution(Project project, BuildConfig buildConfig, bool showWindow = true)
        {
            lock (_lock)
            {
                BuildSolutionInternal(project, buildConfig, showWindow);
            }
        }

        public static void Run(Project project, BuildConfig buildConfig, bool debug)
        {
            lock (_lock)
            {
                RunInternal(project, buildConfig, debug);
            }
        }

        public static void Stop()
        {
            lock (_lock)
            {
                StopInternal();
            }
        }

        private static void OpenVisualStudioInternal(string solutionPath)
        {
            IRunningObjectTable rot = null;
            IEnumMoniker monikerTable = null;
            IBindCtx bindCtx = null;

            try
            {
                if (_vsInstance is null)
                {
                    var hResult = GetRunningObjectTable(0, out rot);

                    if (hResult < 0 || rot is null)
                    {
                        throw new COMException($"GetRunningObjecctTable() returned HRESULT {hResult:X8}");
                    }

                    rot.EnumRunning(out monikerTable);
                    monikerTable.Reset();

                    hResult = CreateBindCtx(0, out bindCtx);

                    if (hResult < 0 || bindCtx is null)
                    {
                        throw new COMException($"CreateBindCtx() returned HRESULT {hResult:X8}");
                    }

                    IMoniker[] currentMoniker = new IMoniker[1];

                    while (monikerTable.Next(1, currentMoniker, IntPtr.Zero) == 0)
                    {
                        string displayName = string.Empty;

                        currentMoniker[0]?.GetDisplayName(bindCtx, null, out displayName);

                        if (displayName.Contains(_progId))
                        {
                            hResult = rot.GetObject(currentMoniker[0], out object obj);

                            if (hResult < 0 || obj is null)
                            {
                                throw new COMException($"GetObject() returned HRESULT {hResult:X8}");
                            }

                            EnvDTE80.DTE2 dte = obj as EnvDTE80.DTE2;

                            var solutionName = string.Empty;

                            CallOnSTAThread(() =>
                            {
                                solutionName = dte.Solution.FullName;
                            });

                            if (solutionName == solutionPath)
                            {
                                _vsInstance = dte;
                                break;
                            }
                        }
                    }

                    if (_vsInstance is null)
                    {
                        Type vsType = Type.GetTypeFromProgID(_progId, true);
                        _vsInstance = Activator.CreateInstance(vsType) as EnvDTE80.DTE2;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.LogAsync(LogLevel.ERROR, ex.Message);
            }
            finally
            {
                if (monikerTable is not null) Marshal.ReleaseComObject(monikerTable);
                if (rot is not null) Marshal.ReleaseComObject(rot);
                if (bindCtx is not null) Marshal.ReleaseComObject(bindCtx);
            }
        }

        private static void CloseVisualStudioInternal()
        {
            CallOnSTAThread(() =>
            {
                if (_vsInstance?.Solution.IsOpen == true)
                {
                    _vsInstance.ExecuteCommand("File.SaveAll");
                    _vsInstance.Solution.Close(true);
                }

                _vsInstance?.Quit();
                _vsInstance = null;
            });
        }

        private static bool AddFilesToSolutionInternal(string solution, string projectName, string[] files)
        {
            OpenVisualStudioInternal(solution);

            try
            {
                if (_vsInstance is not null)
                {
                    CallOnSTAThread(() => 
                    {
                        if (!_vsInstance.Solution.IsOpen) _vsInstance.Solution.Open(solution);
                        else _vsInstance.ExecuteCommand("File.SaveAll");

                        foreach (EnvDTE.Project project in _vsInstance.Solution.Projects)
                        {
                            if (project.UniqueName.Contains(projectName))
                            {
                                foreach (var file in files)
                                {
                                    project.ProjectItems.AddFromFile(file);
                                }
                            }
                        }

                        var cpp = files.FirstOrDefault(x => Path.GetExtension(x) == ".cpp");

                        if (!string.IsNullOrEmpty(cpp))
                        {
                            _vsInstance.ItemOperations.OpenFile(cpp, EnvDTE.Constants.vsViewKindTextView).Visible = true;
                        }

                        _vsInstance.MainWindow.Activate();
                        _vsInstance.MainWindow.Visible = true;
                    });
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine("Failed to add files to Visual Studio Solution");

                return false;
            }

            return true;
        }

        private static void BuildSolutionInternal(Project project, BuildConfig buildConfig, bool showWindow = true)
        {
            if (IsDebuggingInternal())
            {
                Logger.LogAsync(LogLevel.ERROR, "Visual Studio is currently running another process.");
                return;
            }

            OpenVisualStudioInternal(project.Solution);
            BuildFinished = BuildSucceeded = false;

            CallOnSTAThread(() => {

                _vsInstance.MainWindow.Visible = showWindow;
                
                if (!_vsInstance.Solution.IsOpen) _vsInstance.Solution.Open(project.Solution);

                _vsInstance.Events.BuildEvents.OnBuildProjConfigBegin += OnBuildSolutionBegin;
                _vsInstance.Events.BuildEvents.OnBuildProjConfigDone += OnBuildSolutionDone;
            });

            var configName = GetConfigurationName(buildConfig);

            try
            {
                foreach (var pdb in Directory.GetFiles(Path.Combine($"{project.Path}", $@"x64\{configName}"), "*.pdb"))
                {
                    File.Delete(pdb);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            CallOnSTAThread(() =>
            {
                _vsInstance.Solution.SolutionBuild.SolutionConfigurations.Item(configName).Activate();
                _vsInstance.ExecuteCommand("Build.BuildSolution");
                _resetEvent.Wait();
                _resetEvent.Reset();
            });
        }

        private static bool IsDebuggingInternal()
        {
            bool result = false;

            CallOnSTAThread(() =>
            {

                result = _vsInstance is not null &&
                    (
                        _vsInstance.Debugger.CurrentProgram is not null ||
                        _vsInstance.Debugger.CurrentMode == EnvDTE.dbgDebugMode.dbgRunMode
                    );
            });

            return result;
        }

        private static void RunInternal(Project project, BuildConfig buildConfig, bool debug)
        {
            CallOnSTAThread(() =>
            {
                if (_vsInstance is not null && !IsDebuggingInternal() && BuildSucceeded)
                {
                    _vsInstance.ExecuteCommand(debug ? "Debug.Start" : "Debug.StartWithoutDebugging");
                }
            });
        }

        private static void StopInternal()
        {
            CallOnSTAThread(() => {
                if (_vsInstance is not null && IsDebuggingInternal()) _vsInstance.ExecuteCommand("Debug.StopDebugging");
            });
        }

        private static void OnBuildSolutionBegin(string project, string projectConfig, string platform, string solutionConfig)
        {
            if (BuildFinished) return;

            Logger.LogAsync(LogLevel.INFO, $"Building {project}, {projectConfig}, {platform}, {solutionConfig}");
        }

        private static void OnBuildSolutionDone(
            string project,
            string projectConfig,
            string platform,
            string solutionConfig,
            bool success
        )
        {
            if (BuildFinished) return;

            if (success) Logger.LogAsync(LogLevel.INFO, $"Building {projectConfig} configuration succeeded");
            else Logger.LogAsync(LogLevel.ERROR, $"Building {projectConfig} configuration failed");

            BuildFinished = true;
            BuildSucceeded = success;

            _resetEvent.Set();
        }

        private static void CallOnSTAThread(Action action)
        {
            Debug.Assert(action is not null);

            var thread = new Thread(() =>
            {
                OleMessageFilter.Register();

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Logger.LogAsync(LogLevel.WARNING, ex.Message);
                }
                finally
                {
                    OleMessageFilter.Revoke();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}
