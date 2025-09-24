using System.ComponentModel;
using System.Windows;

namespace Editor.Content;

/// <summary>
/// Interaction logic for SelectFolderDialog.xaml
/// </summary>
public partial class SelectFolderDialog : Window
{
    public string? SelectedFolder { get; private set; }

    public SelectFolderDialog(string startFolder)
    {
        InitializeComponent();

        Closing += OnDialogClosing;
    }

    private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
    {
        var contentBrowser = (ContentBrowser.ContentBrowser)contentBrowserView.DataContext;

        SelectedFolder = contentBrowser.SelectedFolder;
        DialogResult = true;

        Close();
    }

    private void OnDialogClosing(object? sender, CancelEventArgs e) => contentBrowserView.Dispose();
}
