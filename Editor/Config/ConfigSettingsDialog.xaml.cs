using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Config
{
    /// <summary>
    /// Interaction logic for ConfigSettingsDialog.xaml
    /// </summary>
    public partial class ConfigSettingsDialog : Window
    {
        public ConfigSettingsDialog()
        {
            InitializeComponent();

            Loaded += (_, __) => LBMenu.SelectedIndex = 0;
            Closing += ConfigSettingsDialog_Closing;
        }

        private void ConfigSettingsDialog_Closing(object? sender, CancelEventArgs e)
        {
            if (!ConfigManager.HasValidationErrors()) ConfigManager.SaveConfig();
            else
            {
                var result = MessageBox.Show(
                    "There are validation errors in the configuration. Do you want to discard changes?",
                    "Validation Error",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes) ConfigManager.TryLoadConfig();
                else e.Cancel = true;
            }
        }

        private void LBMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var key = (LBMenu.SelectedItem as ListBoxItem)?.Content as string;

            CCHost.Content = key switch
            {
                "General" => null,
                "Graphics" => null,
                "Code" => new CodeConfigView(),
                _ => null,
            };
        }
    }
}
