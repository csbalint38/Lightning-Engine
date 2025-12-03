using Editor.Utilities.Diagnostic;
using System.IO;
using System.Windows;

namespace Editor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        this.UseGlobalExceptionFilter(new()
        {
            AppName = "LightningEngine",
            CrashReporterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CrashReporter.exe"),
            LaunchCrashReporter = false,
#if DEBUG
            FailFast = false,
#else
            FailFast = true,
#endif
            SkipFailFastWhenDebugging = true,
        });

        base.OnStartup(e);
    }
}

