using Editor.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.Config
{
    /// <summary>
    /// Interaction logic for ConfigSettingsDialog.xaml
    /// </summary>
    public partial class ConfigSettingsDialog : Window
    {
        public ICommand SaveCommand { get; set; }

        public ConfigSettingsDialog()
        {
            InitializeComponent();

            Loaded += (_, __) => LBMenu.SelectedIndex = 0;

            SaveCommand = new RelayCommand<object>(x => ConfigManager.SaveConfig(), x => Preferences.IsDirty);
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
