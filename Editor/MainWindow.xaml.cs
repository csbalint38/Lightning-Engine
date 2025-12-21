using Editor.Common.Enums;
using Editor.Config;
using Editor.Content;
using Editor.DLLs;
using Editor.GameProject;
using Editor.Utilities;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace Editor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public static string? EnginePath { get; private set; }

    public MainWindow()
    {
        InitializeComponent();

        Loaded += OnMainWindowLoaded;
        Closing += OnMainWindowClosing;
    }

    private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnMainWindowLoaded;

        ConfigManager.TryLoadConfig();
        DefaultAssets.GenerateDefaultAssets();
        GetEnginePath();

        var initResult = EngineAPI.InitializeEngine();

        if (initResult == EngineInitError.SUCCEEDED)
        {
            OpenProjectBrowserDialog();
        }
        else
        {
            MessageBox.Show(
                $"{initResult.GetDescription()}",
                "Engine initialization failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );

            Application.Current.Shutdown();
        }
    }

    private void OnMainWindowClosing(object? sender, CancelEventArgs e)
    {
        if (DataContext is null)
        {
            e.Cancel = true;
            Application.Current.MainWindow.Hide();
            OpenProjectBrowserDialog();

            if (DataContext is not null) Application.Current.MainWindow.Show();
        }
        else Shutdown();
    }

    private void OpenProjectBrowserDialog()
    {
        Project.Current?.Unload();

        Hide();

        var projectBrowser = new ProjectBrowserDialog();
        if (projectBrowser.ShowDialog() == false || projectBrowser.DataContext is null)
        {
            Shutdown();
            Application.Current.Shutdown();
        }
        else
        {
            var project = projectBrowser.DataContext as Project;

            Debug.Assert(project is not null);

            DataContext = project;
        }

        Show();
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

    private void Shutdown()
    {
        Closing -= OnMainWindowClosing;

        Project.Current?.Unload();

        DataContext = null;

        ContentToolsAPI.ShutdownContentTools();
        EngineAPI.ShutdownEngine();
    }
}