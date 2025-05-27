using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.Dictionaries
{
    public partial class ControlTemplates : ResourceDictionary
    {
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            var exp = textBox.GetBindingExpression(TextBox.TextProperty);

            if (exp is null) return;

            if (e.Key == Key.Enter)
            {
                if (textBox.Tag is ICommand command && command.CanExecute(textBox.Text)) command.Execute(textBox.Text);
                else exp.UpdateSource();

                Keyboard.ClearFocus();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                exp.UpdateTarget();
                Keyboard.ClearFocus();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) =>
            ((Window)((FrameworkElement)sender).TemplatedParent).Close();

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            window.WindowState = (window.WindowState == WindowState.Normal) ?
                WindowState.Maximized :
                WindowState.Normal;
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) =>
            ((Window)((FrameworkElement)sender).TemplatedParent).WindowState = WindowState.Minimized;
    }
}
