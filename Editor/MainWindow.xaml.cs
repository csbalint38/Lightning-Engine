using Editor.GameProject;
using System.Windows;

namespace Editor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Loaded += OnMainWindowLoaded;
    }

    private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnMainWindowLoaded;
        OpenProjectBrowserDialog();
    }

    private static void OpenProjectBrowserDialog() {
        var projectBrowser = new ProjectBrowserDialog();
        if(projectBrowser.ShowDialog() == false) Application.Current.Shutdown();
    }
}