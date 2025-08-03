using System.Windows;
using System.Windows.Controls;

namespace Editor.Common.Controls
{
    /// <summary>
    /// Interaction logic for ComponentView.xaml
    /// </summary>

    public class ComponentView : ContentControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(ComponentView));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        static ComponentView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ComponentView),
                new FrameworkPropertyMetadata(typeof(ComponentView))
            );
        }

    }
}
