using Editor.Common.Enums;
using Editor.GameProject;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;

namespace Editor.GameCode
{
    public class Notepad : ICodeEditor
    {
        private static readonly Lock _lock = new();

        private string? _solutionDir;
        private Process? _spawnedProcess;
        private bool _ownsProcess;
        private Process? _gameProcess;

        public bool CanDebug => false;

        public void Close()
        {
            if (_ownsProcess && _spawnedProcess is not null && !_spawnedProcess.HasExited)
            {
                try
                {
                    if (!_spawnedProcess.CloseMainWindow())
                    {
                        _spawnedProcess.Kill(entireProcessTree: true);
                    }
                    else
                    {
                        _spawnedProcess.WaitForExit(2000);

                        if (!_spawnedProcess.HasExited)
                        {
                            _spawnedProcess.Kill(entireProcessTree: true);
                        }
                    }
                } catch { }
                finally
                {
                    _spawnedProcess.Dispose();
                    _spawnedProcess = null;
                    _ownsProcess = false;
                }
            }
        }

        public void Initialize(string solution)
        {
            _solutionDir = Path.GetDirectoryName(Path.GetFullPath(solution));
        }

        public bool IsDebugging() => false;

        public void Run(bool debug)
        {
            if (!MSBuild.BuildSucceeded) return;

            lock (_lock)
            {

                var exePath = Path.Combine(_solutionDir!, "x64", "Debug");
                var exe = Path.Combine(exePath, $"{Project.Current.Name}.exe");

                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = exe,
                        WorkingDirectory = _solutionDir,
                        UseShellExecute = true
                    };

                    var proc = Process.Start(psi);

                    if (proc is null) return;

                    Interlocked.Exchange(ref _gameProcess, proc);

                    proc.EnableRaisingEvents = true;

                    proc.Exited += (_, __) =>
                    {
                        var p = Interlocked.Exchange(ref _gameProcess, null);
                        p?.Dispose();
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Logger.LogAsync(LogLevel.ERROR, "Failed to launch the game process");
                }
            }
        }

        public void ShowWindow(string solution, string? file = null)
        {
            var fullPath = file is not null ? Path.GetFullPath(file) : "";

            var psi = new ProcessStartInfo
            {
                FileName = "notepad.exe",
                Arguments = $"\"{fullPath}\"",
                WorkingDirectory = _solutionDir,
                UseShellExecute = true
            };

            var proc = Process.Start(psi);

            _spawnedProcess = null;
            _ownsProcess = false;

            if (proc is not null)
            {
                try
                {
                    var quickExit = proc.WaitForExit(500);

                    _ownsProcess = !quickExit && !proc.HasExited;
                    _spawnedProcess = _ownsProcess ? proc : null;

                    if (!_ownsProcess) proc.Dispose();
                }
                catch
                {
                    proc.Dispose();

                    _spawnedProcess = null;
                    _ownsProcess = false;
                }
            }
        }

        public void Stop()
        {
            var proc = Interlocked.Exchange(ref _gameProcess, null);

            if (proc is null) return;

            try
            {
                if(!proc.HasExited)
                {
                    if(!proc.CloseMainWindow()) proc.Kill(entireProcessTree: true);
                    else
                    {
                        proc.WaitForExit(2000);
                        if(!proc.HasExited) proc.Kill(entireProcessTree: true);
                    }
                }
            }
            catch { }
            finally
            {
                proc.Dispose();
            }
        }
    }
}
