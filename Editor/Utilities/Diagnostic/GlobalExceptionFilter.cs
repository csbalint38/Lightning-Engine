using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Editor.Utilities.Diagnostic;

public static class GlobalExceptionFilter
{
    private static bool _installed = false;
    private static bool _crashing = false;

    private static ExceptionHandlingOptions? _options;

    public static void UseGlobalExceptionFilter(this Application app, ExceptionHandlingOptions? options)
    {
        _options = options ?? new();

        if (Interlocked.Exchange(ref _installed, true)) return;

        AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;
        app.DispatcherUnhandledException += AppDispatcherUnhandledException;
    }

    private static void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) =>
        Crash("UI", e.Exception);
    private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) =>
        Crash(e.IsTerminating ? "AppDomain-T" : "AppDomain", e.ExceptionObject as Exception);

    private static void TaskSchedulerUnobservedTaskException(object? Sender, UnobservedTaskExceptionEventArgs e)
    {
        try
        {
            e.SetObserved();
        } catch { }

        Crash("Task", e.Exception);
    }

    private static void Crash(string source, Exception? ex)
    {
        if (Interlocked.Exchange(ref _crashing, true)) return;

        try
        {
            WriteCrashLog(source, ex);

            // TODO: Start the reporter

            if (_options!.FailFast && !(_options!.SkipFailFastWhenDebugging && Debugger.IsAttached))
            {
                Environment.FailFast($"Unhandled exception. Source: {source}", ex);
            }
        }
        catch
        {
            if (_options!.FailFast && !(_options!.SkipFailFastWhenDebugging && Debugger.IsAttached))
            {
                Environment.FailFast($"Fatal error - logging failed!", ex);
            }

            throw;
        }
    }

    private static void WriteCrashLog(string source, Exception? ex)
    {
        var now = DateTime.UtcNow;
        var proc = Process.GetCurrentProcess();
        var appName = string.IsNullOrEmpty(_options!.AppName) ? proc.ProcessName : _options.AppName;

        var folder = string.IsNullOrEmpty(_options.LogFolder)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName, "Crashes")
            : _options.LogFolder;

        Directory.CreateDirectory(folder);

        var file = Path.Combine(folder, $"crash_{now:yyyyMMdd_HHmmss}.log");
        var sb = new StringBuilder(8 * 1024);

        sb.AppendLine($"UTC: {now:O}");
        sb.AppendLine($"Source: {source}");
        sb.AppendLine($"Process: {proc.ProcessName}");
        sb.AppendLine($"OS: {Environment.OSVersion}");
        sb.AppendLine($"Bitness: {Environment.Is64BitProcess}");
        sb.AppendLine($"CmdLine: { Environment.CommandLine}");
        sb.AppendLine();

        if (_options.MetadataProvider is not null)
        {
            var extra = _options.MetadataProvider.Invoke();

            if(!string.IsNullOrEmpty(extra))
            {
                sb.AppendLine("Metadata:");
                sb.AppendLine(extra);
                sb.AppendLine();
            }
        }

        if(ex is not null)
        {
            sb.AppendLine("Exception:");
            sb.AppendLine(ex.ToString());
            sb.AppendLine();
        }

        if(_options.IncludeLoadedModules)
        {
            sb.AppendLine("LoadedModules:");

            foreach (ProcessModule m in proc.Modules)
            {
                sb.AppendLine($"{m.ModuleName} @ {m.BaseAddress} v{m.FileVersionInfo.FileVersion}");
            }
        }

        File.WriteAllText(file, sb.ToString());
    }
}
