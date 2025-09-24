using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Editor.Content;

/// <summary>
/// Interaction logic for SaveDialog.xaml
/// </summary>
public partial class SaveDialog : Window
{
    public string? SaveFilePath { get; private set; }

    public SaveDialog()
    {
        InitializeComponent();

        contentBrowserView.Loaded += (_, __) =>
        {
            var contentBrowser = (ContentBrowser.ContentBrowser)contentBrowserView.DataContext;
            contentBrowser.SelectedFolder = contentBrowser.ContentFolder;
        };

        Closing += OnSaveDialogClosing;
    }

    private void OnSaveDialogClosing(object? sender, CancelEventArgs e) => contentBrowserView.Dispose();

    private void Button_Click(object sender, RoutedEventArgs? e)
    {
        if (ValidateFileName(out var saveFilePath))
        {
            SaveFilePath = saveFilePath;
            DialogResult = true;

            Close();
        }
    }

    private bool ValidateFileName(out string saveFilePath)
    {
        var contentBrowser = contentBrowserView.DataContext as ContentBrowser.ContentBrowser;
        var path = contentBrowser?.SelectedFolder;

        if (!Path.EndsInDirectorySeparator(path)) path += @"\";

        var fileName = TBFileName.Text.Trim();

        if (string.IsNullOrEmpty(fileName))
        {
            saveFilePath = string.Empty;

            return false;
        }

        if (!fileName.EndsWith(Asset.AssetFileExtension)) fileName += Asset.AssetFileExtension;

        path += $@"{fileName}";

        var isValid = false;
        string errorMessage = string.Empty;

        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
        {
            errorMessage = "Invalid characters used in asset file name.";
        }
        else if (
            File.Exists(path) &&
            MessageBox.Show(
                "File with the same name already exists. Do you want to replace it?",
                "File name conflict",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            ) == MessageBoxResult.No
        ) { }
        else isValid = true;

        if (!string.IsNullOrEmpty(errorMessage))
        {
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        saveFilePath = path;

        return isValid;
    }

    private void ContentBrowserView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (
            ((FrameworkElement)e.OriginalSource).DataContext == contentBrowserView.SelectedItem &&
            contentBrowserView.SelectedItem.FileName == TBFileName.Text
        )
        {
            Button_Click(sender, null);
        }
    }
}
