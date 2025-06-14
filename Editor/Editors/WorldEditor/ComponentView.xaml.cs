using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Editor.Editors
{
    /// <summary>
    /// Interaction logic for ComponentView.xaml
    /// </summary>

    [ContentProperty("ComponentContent")]
    public partial class ComponentView : UserControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(ComponentView));

        public static readonly DependencyProperty ComponentContentProperty =
            DependencyProperty.Register(nameof(ComponentContent), typeof(FrameworkElement), typeof(ComponentView));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public FrameworkElement ComponentContent
        {
            get => (FrameworkElement)GetValue(ComponentContentProperty);
            set => SetValue(ComponentContentProperty, value);
        }

        public ComponentView()
        {
            InitializeComponent();
        }

    }
}
