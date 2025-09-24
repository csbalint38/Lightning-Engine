using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Editor.Dictionaries;

public partial class ControlTemplates : ResourceDictionary
{
    private static void MoveUpFocus(UIElement element)
    {
        DependencyObject parent = element;

        while ((parent = VisualTreeHelper.GetParent(parent)) is not null
            && Keyboard.Focus(parent as UIElement) == element) ;
    }

    private static void UpdateTextBoxSource(TextBox textBox, BindingExpression exp)
    {
        if (textBox.Tag is ICommand command && command.CanExecute(textBox.Text))
        {
            command.Execute(textBox.Text);
        }
        else
        {
            exp.UpdateSource();
        }
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        var textBox = (TextBox)sender;
        var exp = textBox.GetBindingExpression(TextBox.TextProperty);

        if (exp is null) return;

        if (e.Key is Key.Enter or Key.Tab)
        {
            UpdateTextBoxSource(textBox, exp);

            if (e.Key is Key.Enter)
            {
                MoveUpFocus(textBox);

                e.Handled = true;
            }

        }
        else if (e.Key == Key.Escape)
        {
            exp.UpdateTarget();
            MoveUpFocus(textBox);
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

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)sender;

        if (!textBox.IsVisible) return;

        var exp = textBox.GetBindingExpression(TextBox.TextProperty);

        if (exp is not null) UpdateTextBoxSource(textBox, exp);
    }

    private void TextBoxWithRename_KeyDown(object sender, KeyEventArgs e)
    {
        var textBox = (TextBox)sender;
        var exp = textBox.GetBindingExpression(TextBox.TextProperty);

        if (exp is null) return;

        if (e.Key == Key.Enter)
        {
            UpdateTextBoxSource(textBox, exp);

            textBox.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }
        else if (e.Key is Key.Tab)
        {
            UpdateTextBoxSource(textBox, exp);
        }
        else if (e.Key == Key.Escape)
        {
            exp.UpdateTarget();
            textBox.Visibility = Visibility.Collapsed;
        }
    }

    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)sender;
        var exp = textBox.GetBindingExpression(TextBox.TextProperty);

        exp?.UpdateSource();

        ((TextBox)sender).SelectAll();
    }

    private void TextBoxRename_LostFocus(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)sender;

        if (!textBox.IsVisible) return;

        var exp = textBox.GetBindingExpression(TextBox.TextProperty);

        if (exp is not null)
        {
            exp.UpdateTarget();
            textBox.Visibility = Visibility.Collapsed;
        }
    }
}
