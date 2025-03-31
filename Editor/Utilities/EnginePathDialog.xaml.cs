using System.IO;
using System.Windows;

namespace Editor.Utilities
{
    /// <summary>
    /// Interaction logic for EnginePathDialog.xaml
    /// </summary>
    public partial class EnginePathDialog : Window
    {
        public string EnginePath { get; private set; }

        public EnginePathDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var path = TbPath.Text.Trim();

            TbMessage.Text = string.Empty;

            if (string.IsNullOrEmpty(path))
            {
                TbMessage.Text = "Path cannot be empty.";
            }
            else if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                TbMessage.Text = "Invalid character(s) used in path";
            }
            else if (!Directory.Exists(Path.Combine(path, @"Engine\EngineAPI\")))
            {
                TbMessage.Text = "Unable to fing the engine at the specific location.";
            }
            else
            {
                if (!Path.EndsInDirectorySeparator(path)) path += @"\";

                EnginePath = path;
                DialogResult = true;
                Close();
            }
        }
    }
}
