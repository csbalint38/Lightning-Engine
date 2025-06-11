using Editor.Common.Enums;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Common.Controls
{
    public class VectorBox : Control
    {
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register(
                nameof(X),
                typeof(string),
                typeof(VectorBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );

        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register(
                nameof(Y),
                typeof(string),
                typeof(VectorBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );

        public static readonly DependencyProperty ZProperty =
            DependencyProperty.Register(
                nameof(Z),
                typeof(string),
                typeof(VectorBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );

        public static readonly DependencyProperty AlphaProperty =
            DependencyProperty.Register(
                nameof(Alpha),
                typeof(string),
                typeof(VectorBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );

        public static readonly DependencyProperty MultiplierProperty =
            DependencyProperty.Register(
                nameof(Multiplier),
                typeof(double),
                typeof(VectorBox),
                new PropertyMetadata(1.0)
            );

        public static readonly DependencyProperty VectorTypeProperty =
            DependencyProperty.Register(
                nameof(VectorType),
                typeof(VectorType),
                typeof(VectorBox),
                new PropertyMetadata(VectorType.Vector3)
            );

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(VectorBox),
                new PropertyMetadata(Orientation.Horizontal)
            );

        public string X
        {
            get => (string)GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        public string Y
        {
            get => (string)GetValue(YProperty);
            set => SetValue(YProperty, value);
        }

        public string Z
        {
            get => (string)GetValue(ZProperty);
            set => SetValue(ZProperty, value);
        }

        public string Alpha
        {
            get => (string)GetValue(AlphaProperty);
            set => SetValue(AlphaProperty, value);
        }

        public double Multiplier
        {
            get => (double)GetValue(MultiplierProperty);
            set => SetValue(MultiplierProperty, value);
        }

        public VectorType VectorType
        {
            get => (VectorType)GetValue(VectorTypeProperty);
            set => SetValue(VectorTypeProperty, value);
        }

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        static VectorBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VectorBox), new FrameworkPropertyMetadata(typeof(VectorBox)));
        }
    }
}
