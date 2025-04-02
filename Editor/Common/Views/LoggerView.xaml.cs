using Editor.Common.Enums;
using Editor.Utilities;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Common
{
    /// <summary>
    /// Interaction logic for LoggerView.xaml
    /// </summary>
    public partial class LoggerView : UserControl
    {
        public LoggerView()
        {
            InitializeComponent();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) => Logger.ClearAsync();

        private void FilterMessage_Click(object sender, RoutedEventArgs e)
        {
            var filter = 0x0;

            if (TbtnInfo.IsChecked == true) filter |= (int)LogLevel.INFO;
            if (TbtnWarning.IsChecked == true) filter |= (int)LogLevel.WARNING;
            if (TbtnError.IsChecked == true) filter |= (int)LogLevel.ERROR;

            Logger.SetMessageFilter(filter);
        }
    }
}
