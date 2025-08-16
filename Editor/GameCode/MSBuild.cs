using Editor.Common.Enums;
using Editor.GameProject;
using Editor.Utilities;
using Microsoft.Build.Construction;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace Editor.GameCode
{
    public static partial class MSBuild
    {
        private static readonly string[] _buildConfigurationNames = ["Debug", "DebugEditor", "Release", "ReleaseEditor"];

        private static readonly string CachePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LightningEngine",
            "MSBuildCache.txt"
        );

        private static string? MSBuildPath;

        public static bool BuildSucceeded { get; private set; }
        public static bool BuildFinished { get; private set; }

        public static string GetConfigurationName(BuildConfig config) => _buildConfigurationNames[(int)config];

        public static string? FindMSBuild()
        {
            if (TryReadMSBuildCache(out var cached)) return cached;

            var vswhere = GetVswherePath();

            if (vswhere is not null)
            {
                var args =
                    "-latest -products * -requires Microsoft.Component.MSBuild -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64  -find \"MSBuild\\**\\Bin\\MSBuild.exe\"";
                var (exitCode, stdOut, _) = RunProcess(vswhere, args);

                if (exitCode == 0)
                {
                    var msbuild = stdOut.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

                    if (!string.IsNullOrEmpty(msbuild) && File.Exists(msbuild))
                    {
                        var msbuildRoot = GetInstallRootFromMSBuildPath(msbuild);

                        if (msbuildRoot is not null && HasCppToolset(msbuildRoot))
                        {
                            TryWriteMSBuildCache(msbuild);

                            return msbuild;
                        }
                    }
                }
            }

            var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            var candidates = new[]
            {
                Path.Combine(pf, "Microsoft Visual Studio", "2022", "Community", "MSBuild", "Current", "Bin", "MSBuild.exe"),
                Path.Combine(pf, "Microsoft Visual Studio", "2022", "Professional", "MSBuild", "Current", "Bin", "MSBuild.exe"),
                Path.Combine(pf, "Microsoft Visual Studio", "2022", "Enterprise", "MSBuild", "Current", "Bin", "MSBuild.exe"),
                Path.Combine(pf, "Microsoft Visual Studio", "2022", "BuildTools", "MSBuild", "Current", "Bin", "MSBuild.exe"),
                Path.Combine(pf, "LightningEngine", "BuildTools", "MSBuild", "Current", "Bin", "MSBuild.exe")
            };

            foreach (var path in candidates)
            {
                if (!File.Exists(path)) continue;

                var root = GetInstallRootFromMSBuildPath(path);

                if (root is not null && HasCppToolset(root))
                {
                    TryWriteMSBuildCache(path);

                    return path;
                }
            }

            return null;
        }

        public static bool AddFilesToSolution(string solution, string projectName, string[] files)
        {
            Debug.Assert(files?.Length > 0);

            try
            {
                var sln = SolutionFile.Parse(solution);
                var proj = sln.ProjectsInOrder
                    .FirstOrDefault(x => x.ProjectName.Equals(projectName) && x.RelativePath.EndsWith(".vcxproj"));

                if (proj is null) return false;

                var projectDir = Path.GetDirectoryName(proj.AbsolutePath)!;
                var doc = XDocument.Load(proj.AbsolutePath);
                var ns = doc.Root!.Name.Namespace;
                var itemGroup = new XElement(ns + "ItemGroup");

                foreach (var file in files)
                {
                    var path = Path.GetRelativePath(projectDir, file).Replace('/', '\\');

                    if (path.StartsWith(@".\")) path = path.Substring(2);

                    var attr = new XAttribute("Include", path);

                    if (file.EndsWith(".cpp"))
                    {
                        Logger.LogAsync(LogLevel.INFO, $"{file} was added to {solution}");

                        itemGroup.Add(new XElement(ns + "ClCompile", attr));
                    }
                    else if (file.EndsWith(".h"))
                    {
                        Logger.LogAsync(LogLevel.INFO, $"{file} was added to {solution}");

                        itemGroup.Add(new XElement(ns + "ClInclude", attr));
                    }
                }

                if (!itemGroup.HasElements) return false;

                doc.Root?.Add(itemGroup);
                doc.Save(proj.AbsolutePath);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.LogAsync(LogLevel.ERROR, $"Failed to add files to solution.");

                return false;
            }
        }

        public static void BuildSolution(Project project, BuildConfig config)
        {
            if (ICodeEditor.Current.IsDebugging())
            {
                Logger.LogAsync(LogLevel.ERROR, "Game is already running. Stop the running process before rebuild.");
                return;
            }

            if (string.IsNullOrEmpty(MSBuildPath)) MSBuildPath = FindMSBuild();

            if (string.IsNullOrEmpty(MSBuildPath))
            {
                Logger.LogAsync(LogLevel.ERROR, "Failed to locate MSBuild");

                return;
            }

            var configName = GetConfigurationName(config);

            try
            {
                foreach (var pdbFile in Directory.GetFiles(Path.Combine($"{project.Path}", $@"x64\{configName}"), "*.pdb"))
                {
                    File.Delete(pdbFile);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            var args =
                $"\"{project.Solution}\" /t:Build /m /nr:false /nologo /p:Configuration={configName};Platform=x64 /v:m /clp:Summary;ShowTimestamp;ForceNoAlign;DisableConsoleColor";

            OnBuildSolutionBegin(project.Name, configName);

            var exit = RunProcess(MSBuildPath, args, LogMSBuildStdOut, LogMSBuildStdErr);

            OnBuildSolutionDone(configName, exit == 0);
        }

        private static string? GetVswherePath()
        {
            var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var vswherePath = Path.Combine(pf86, "Microsoft Visual Studio", "Installer", "vswhere.exe");

            return File.Exists(vswherePath) ? vswherePath : null;
        }

        private static bool HasCppToolset(string installPath)
        {
            if (string.IsNullOrEmpty(installPath) || !Directory.Exists(installPath)) return false;

            var msvcRoot = Path.Combine(installPath, "VC", "Tools", "MSVC");

            if (!Directory.Exists(msvcRoot)) return false;

            foreach (var dir in Directory.EnumerateDirectories(msvcRoot))
            {
                var cl = Path.Combine(dir, "bin", "Hostx64", "x64", "cl.exe");
                if (File.Exists(cl)) return true;
            }

            return false;
        }

        private static string? GetInstallRootFromMSBuildPath(string msbuildPath)
        {
            try
            {
                var dir = new FileInfo(msbuildPath).Directory;

                if (dir == null) return null;

                return dir.Parent?.Parent?.Parent?.FullName;
            }
            catch
            {
                return null;
            }
        }

        private static void TryWriteMSBuildCache(string msbuildPath)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CachePath)!);
                File.WriteAllText(CachePath, msbuildPath);
            }
            catch
            {
                Logger.LogAsync(LogLevel.WARNING, "Failed to write MSBuild cache.");
            }
        }

        private static bool TryReadMSBuildCache(out string? msbuildPath)
        {
            msbuildPath = null;

            try
            {
                if (!File.Exists(CachePath)) return false;

                var cached = File.ReadAllText(CachePath).Trim();

                if (string.IsNullOrEmpty(cached) || !File.Exists(cached))
                {
                    Logger.LogAsync(LogLevel.WARNING, "Failed to read MSBuild cache.");

                    return false;
                }

                var root = GetInstallRootFromMSBuildPath(cached);

                if (root is null || !HasCppToolset(root))
                {
                    File.Delete(CachePath);
                    Logger.LogAsync(LogLevel.WARNING, "Cached MSBuild path does not have C++ toolset.");

                    return false;
                }

                msbuildPath = cached;

                return true;
            }
            catch
            {
                File.Delete(CachePath);
                Logger.LogAsync(LogLevel.WARNING, "Failed to read MSBuild cache.");

                return false;
            }
        }

        private static (int ExitCode, string StdOut, string StdErr) RunProcess(string fileName, string args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var p = new Process { StartInfo = psi };

            p.Start();

            var stdout = p.StandardOutput.ReadToEnd().Trim();
            var stderr = p.StandardError.ReadToEnd().Trim();

            p.WaitForExit();

            return (p.ExitCode, stdout, stderr);
        }

        private static int RunProcess(string fileName, string args, Action<string>? stdOut, Action<string>? stdErr)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var p = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            if (stdOut is not null)
            {
                p.OutputDataReceived += (_, e) =>
                {
                    if (e.Data is not null) stdOut(e.Data);
                };
            }

            if (stdErr is not null)
            {
                p.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data is not null) stdErr(e.Data);
                };
            }

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();

            return p.ExitCode;
        }

        private static void OnBuildSolutionBegin(string project, string config)
        {
            BuildFinished = BuildSucceeded = false;

            Logger.LogAsync(LogLevel.INFO, $"Building {project}, {config}, x64");
        }

        private static void OnBuildSolutionDone(string config, bool success)
        {
            BuildFinished = true;
            BuildSucceeded = success;

            if (success) Logger.LogAsync(LogLevel.INFO, $"Building {config} configuration succeeded,");
            else Logger.LogAsync(LogLevel.ERROR, $"Building {config} configuration failed");
        }

        private static void LogMSBuildStdOut(string line)
        {
            var isError = line.StartsWith("error ", StringComparison.OrdinalIgnoreCase) ||
                          line.Contains(": error", StringComparison.OrdinalIgnoreCase) ||
                          line.Contains("fatal error", StringComparison.OrdinalIgnoreCase);

            var isWarning = line.StartsWith("warning ", StringComparison.OrdinalIgnoreCase) ||
                           line.Contains(": warning", StringComparison.OrdinalIgnoreCase);

            var level = isError ? LogLevel.ERROR : isWarning ? LogLevel.WARNING : LogLevel.INFO;

            Logger.LogAsync(level, $"[MSBUILD] {line}");
        }

        private static void LogMSBuildStdErr(string line) => Logger.LogAsync(LogLevel.ERROR, $"[MSBUILD] {line}");
    }
}
