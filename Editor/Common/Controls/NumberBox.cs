using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.Common.Controls
{
    [TemplatePart(Name = "PART_textBlock", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_textBox", Type = typeof(TextBox))]
    public class NumberBox : Control
    {
        private double _originalValue;
        private double _mouseXStart;
        private double _multiplier;
        private bool _captured = false;
        private bool _valueChanged = false;

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(string),
                typeof(NumberBox),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnValueChanged)
                )
            );

        public static readonly DependencyProperty MultiplierProperty =
            DependencyProperty.Register(
                nameof(Multiplier),
                typeof(double),
                typeof(NumberBox),
                new PropertyMetadata(1.0)
            );

        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(ValueChanged),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(NumberBox)
            );

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double Multiplier
        {
            get => (double)GetValue(MultiplierProperty);
            set => SetValue(MultiplierProperty, value);
        }

        public event RoutedEventHandler ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }

        static NumberBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if(GetTemplateChild("PART_textBlock") is TextBlock tb)
            {
                tb.MouseLeftButtonDown += OnMouseLeftButtonDown;
                tb.MouseLeftButtonUp += OnMouseLeftButtonUp;
                tb.MouseMove += OnMouseMove;
                tb.LostKeyboardFocus += OnLostKeyboardFocus;
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            (d as NumberBox).RaiseEvent(new RoutedEventArgs(ValueChangedEvent));

        private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var tb = sender as TextBlock;

            tb.Visibility = Visibility.Collapsed;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_captured)
            {
                var mouseX = e.GetPosition(this).X;
                var d = mouseX - _mouseXStart;

                if (Math.Abs(d) > SystemParameters.MinimumHorizontalDragDistance)
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) _multiplier = .01;
                    else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) _multiplier = 1;
                    else _multiplier = .1;

                    var newValue = _originalValue + (d * _multiplier * Multiplier);

                    Value = newValue.ToString("0.#####");

                    _valueChanged = true;
                }
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(_captured)
            {
                Mouse.Capture(null);

                _captured = false;

                e.Handled = true;

                if(!_valueChanged && GetTemplateChild("PART_textBox") is TextBox tb)
                {
                    tb.Visibility = Visibility.Visible;
                    tb.Focus();
                    tb.SelectAll();
                }
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { 
            double.TryParse(Value, out _originalValue);

            Mouse.Capture(sender as UIElement);

            _captured = true;
            _valueChanged = false;

            e.Handled = true;

            _mouseXStart = e.GetPosition(this).X;
        }
    }
}
