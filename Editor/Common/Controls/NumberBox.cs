using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.Common.Controls
{
    [TemplatePart(Name = "PART_textBox", Type = typeof(TextBox))]
    internal class NumberBox : Control
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(string),
                typeof(NumberBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        private double _originalValue;
        private double _mouseXStart;
        private double _multiplier;
        private bool _captured = false;
        private bool _valueChanged = false;

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        static NumberBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if(GetTemplateChild("PART_textBox") is TextBox tb)
            {
                tb.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
                tb.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
                tb.PreviewMouseMove += OnMouseMove;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if(_captured)
            {
                var mouseX = e.GetPosition(this).X;
                var d = mouseX - _mouseXStart;

                if(Math.Abs(d) > SystemParameters.MinimumHorizontalDragDistance)
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) _multiplier = 0.01;
                    else if(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) _multiplier = 1;

                    var newValue = _originalValue + (d * _multiplier);

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

            _multiplier = 0.1;
            _mouseXStart = e.GetPosition(this).X;
        }
    }
}
