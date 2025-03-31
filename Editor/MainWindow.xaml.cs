using Editor.GameProject;
using Editor.Utilities;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Editor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public static string EnginePath { get; private set; }

    public MainWindow()
    {
        InitializeComponent();

        Loaded += OnMainWindowLoaded;
        Closing += OnMainWindowClosing;
    }

    private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnMainWindowLoaded;
        GetEnginePath();
        OpenProjectBrowserDialog();
    }

    private void OnMainWindowClosing(object sender, CancelEventArgs e)
    {
        Closing -= OnMainWindowClosing;
        Project.Current?.Unload();
    }

    private void OpenProjectBrowserDialog()
    {
        var projectBrowser = new ProjectBrowserDialog();
        if (projectBrowser.ShowDialog() == false || projectBrowser.DataContext is null)
        {
            Application.Current.Shutdown();
        }
        else
        {
            Project.Current?.Unload();
            DataContext = projectBrowser.DataContext;
        }
    }

    private void GetEnginePath()
    {
        var enginePath = Environment.GetEnvironmentVariable("LIGHTNING_ENGINE", EnvironmentVariableTarget.User);

        if (enginePath == null || !Directory.Exists(Path.Combine(enginePath, @"Engine\EngineAPI")))
        {
            var dialog = new EnginePathDialog();

            if (dialog.ShowDialog() == true)
            {
                EnginePath = dialog.EnginePath;
                Environment.SetEnvironmentVariable("LIGHTNING_ENGINE", EnginePath, EnvironmentVariableTarget.User);
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
        else EnginePath = enginePath;
    }
}