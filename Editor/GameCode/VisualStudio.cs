using Editor.Common.Enums;
using Editor.GameProject;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Editor.GameCode
{
    static class VisualStudio
    {
        private static readonly string _progId = "VisualStudio.DTE.17.0";
        private static EnvDTE80.DTE2 _vsInstance = null;

        public static bool BuildSucceeded { get; private set; }
        public static bool BuildFinished { get; private set; }

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(uint reserved, out IRunningObjectTable rot);

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        public static void OpenVisualStudio(string solutionPath)
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

                    while(monikerTable.Next(1, currentMoniker, IntPtr.Zero) == 0)
                    {
                        string displayName = string.Empty;

                        currentMoniker[0]?.GetDisplayName(bindCtx, null, out displayName);

                        if(displayName.Contains(_progId))
                        {
                            hResult = rot.GetObject(currentMoniker[0], out object obj);

                            if(hResult < 0 || obj is null)
                            {
                                throw new COMException($"GetObject() returned HRESULT {hResult:X8}");
                            }

                            EnvDTE80.DTE2 dte = obj as EnvDTE80.DTE2;
                            var solutionName = dte.Solution.FullName;

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
                if(monikerTable is not null) Marshal.ReleaseComObject(monikerTable);
                if (rot is not null) Marshal.ReleaseComObject(rot);
                if (bindCtx is not null) Marshal.ReleaseComObject(bindCtx);
            }
        }

        public static void CloseVisualStudio()
        {
            if (_vsInstance?.Solution.IsOpen == true)
            {
                _vsInstance.ExecuteCommand("File.SaveAll");
                _vsInstance.Solution.Close(true);
                _vsInstance.Quit();
            }
        }

        public static bool AddFilesToSolution(string solution, string projectName, string[] files)
        {
            OpenVisualStudio(solution);

            try
            {
                if (_vsInstance is not null)
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

        public static void BuildSolution(Project project, string buildConfig, bool showWindow = true)
        {
            if (IsDebugging())
            {
                Logger.LogAsync(LogLevel.ERROR, "Visual Studio is currently running another process.");
                return;
            }

            OpenVisualStudio(project.Solution);
            BuildFinished = BuildSucceeded = false;

            for (int i = 0; i < 3 && !BuildFinished; ++i)
            {
                try
                {
                    if (!_vsInstance.Solution.IsOpen) _vsInstance.Solution.Open(project.Solution);

                    _vsInstance.MainWindow.Visible = showWindow;
                    _vsInstance.Events.BuildEvents.OnBuildProjConfigBegin += OnBuildSolutionBegin;
                    _vsInstance.Events.BuildEvents.OnBuildProjConfigDone += OnBuildSolutionDone;

                    try
                    {
                        foreach (var pdb in Directory.GetFiles(Path.Combine($"{project.Path}", $@"x64\{buildConfig}"), "*.pdb"))
                        {
                            File.Delete(pdb);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                    _vsInstance.Solution.SolutionBuild.SolutionConfigurations.Item(buildConfig).Activate();
                    _vsInstance.ExecuteCommand("Build.BuildSolution");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine($"Attempt {i}: failed to build {project.Name}");
                    Thread.Sleep(1000);
                }
            }
        }

        public static bool IsDebugging()
        {
            bool result = false;
            bool shouldTryAgain = true;

            for (int i = 0; i < 3 && shouldTryAgain; ++i)
            {
                try
                {
                    result = _vsInstance is not null &&
                             (_vsInstance.Debugger.CurrentProgram is not null ||
                             _vsInstance.Debugger.CurrentMode == EnvDTE.dbgDebugMode.dbgRunMode);
                    shouldTryAgain = false;
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message);
                    Thread.Sleep(1000);
                }
            }

            return result;
        }

        public static void Run(Project project, string configName, bool debug)
        {
            if (_vsInstance is not null && !IsDebugging() && BuildFinished && BuildSucceeded)
            {
                _vsInstance.ExecuteCommand(debug ? "Debug.Start" : "Debug.StartWithoutDebugging");
            }
        }

        public static void Stop()
        {
            if (_vsInstance is not null && IsDebugging()) _vsInstance.ExecuteCommand("Debug.StopDebugging"); 
        }

        private static void OnBuildSolutionBegin(string project, string projectConfig, string platform, string solutionConfig) =>
            Logger.LogAsync(LogLevel.INFO, $"Building {project}, {projectConfig}, {platform}, {solutionConfig}");

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
        }
    }
}
