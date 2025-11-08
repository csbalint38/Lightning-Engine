using Editor.Common.Enums;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Editor.GameCode;

public class VisualStudio : ICodeEditor
{
    private static readonly string _progId = "VisualStudio.DTE.17.0";
    private static readonly Lock _lock = new();
    private static EnvDTE80.DTE2? _vsInstance = null;

    public bool CanDebug => true;

    [DllImport("ole32.dll")]
    private static extern int GetRunningObjectTable(uint reserved, out IRunningObjectTable rot);

    [DllImport("ole32.dll")]
    private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

    public void Initialize(string solutionPath)
    {
        lock (_lock)
        {
            OpenVisualStudioInternal(solutionPath);
        }
    }

    public void ShowWindow(string solution, string? file = null)
    {
        lock (_lock)
        {
            ShowWindowInternal(solution, file);
        }
    }

    public void Close()
    {
        lock (_lock)
        {
            CloseVisualStudioInternal();
        }
    }

    public bool IsDebugging()
    {
        lock (_lock)
        {
            return IsDebuggingInternal();
        }
    }

    public void Run(bool debug)
    {
        lock (_lock)
        {
            RunInternal(debug);
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            StopInternal();
        }
    }

    public static bool AddFilesToSolution(string solution, string projectName, string[] files)
    {
        lock (_lock)
        {
            return AddFilesToSolutionInternal(solution, projectName, files);
        }
    }

    private static void ShowWindowInternal(string solution, string? file = null)
    {
        OpenVisualStudioInternal(solution);

        if (_vsInstance is not null)
        {
            CallOnSTAThread(() =>
            {
                if (!_vsInstance.Solution.IsOpen) _vsInstance.Solution.Open(solution);
                else _vsInstance.ExecuteCommand("File.SaveAll");

                if (file is not null)
                {
                    _vsInstance.ItemOperations
                        .OpenFile(file, EnvDTE.Constants.vsViewKindTextView)
                        .Visible = true;
                }

                _vsInstance.MainWindow.Activate();
                _vsInstance.MainWindow.Visible = true;
            });
        }
    }

    private static void OpenVisualStudioInternal(string solutionPath)
    {
        IRunningObjectTable? rot = null;
        IEnumMoniker? monikerTable = null;
        IBindCtx? bindCtx = null;

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

                        EnvDTE80.DTE2 dte = (EnvDTE80.DTE2)obj;

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
                    Type? vsType = Type.GetTypeFromProgID(_progId, true);

                    _vsInstance = Activator.CreateInstance(vsType!) as EnvDTE80.DTE2;

                    _vsInstance?.Solution.Open(solutionPath);
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

    private static void RunInternal(bool debug)
    {
        CallOnSTAThread(() =>
        {
            if (_vsInstance is not null && !_vsInstance.MainWindow.Visible)
            {
                ShowWindowInternal(_vsInstance?.Solution.FullName ?? string.Empty);
            }

            if (_vsInstance is not null && !IsDebuggingInternal() && MSBuild.BuildSucceeded)
            {
                _vsInstance.ExecuteCommand(debug ? "Debug.Start" : "Debug.StartWithoutDebugging");
            }
        });
    }

    private static void StopInternal()
    {
        CallOnSTAThread(() =>
        {
            if (_vsInstance is not null && IsDebuggingInternal())
            {
                _vsInstance.ExecuteCommand("Debug.StopDebugging");
            }
        });
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

    private static bool AddFilesToSolutionInternal(string solution, string projectName, string[] files)
    {
        Debug.Assert(files?.Length > 0);

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
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine("Failed to add files to Visual Studio project");

            return false;
        }

        return true;
    }
}
