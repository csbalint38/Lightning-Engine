namespace Editor.Utilities.Diagnostic;

public class ExceptionHandlingOptions()
{
    public string? AppName { get; set; }
    public string? LogFolder { get; set; }
    public string? CrashReporterPath { get; set; }
    public bool LaunchCrashReporter { get; set; } = true;
    public bool FailFast { get; set; } = true;
    public bool IncludeLoadedModules { get; set; } = true;
    public Func<string>? MetadataProvider { get; set; } = null;
    public bool SkipFailFastWhenDebugging { get; set; } = true;
}
