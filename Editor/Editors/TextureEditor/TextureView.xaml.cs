using Editor.Utilities;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Editor.Editors
{
    /// <summary>
    /// Interaction logic for TextureView.xaml
    /// </summary>
    public partial class TextureView : UserControl
    {
        private Point _gridClickPosition = new(0, 0);
        private bool _capturedRight;

        public static readonly DependencyProperty PanOffsetProperty = DependencyProperty.Register(
            nameof(PanOffset),
            typeof(Point),
            typeof(TextureView),
            new PropertyMetadata(new Point(0, 0), OnPanOffsetChanged)
        );

        public static readonly DependencyProperty ScaleFactorProperty = DependencyProperty.Register(
            nameof(ScaleFactor),
            typeof(double),
            typeof(TextureView),
            new PropertyMetadata(1.0, OnScaleFactorChanged)
        );

        public Point PanOffset
        {
            get => (Point)GetValue(PanOffsetProperty);
            set => SetValue(PanOffsetProperty, value);
        }

        public double ScaleFactor
        {
            get => (double)GetValue(ScaleFactorProperty);
            set => SetValue(ScaleFactorProperty, value);
        }

        public TextureView()
        {
            InitializeComponent();

            SizeChanged += (_, __) => Center();
            textureImage.SizeChanged += (_, __) => ZoomFit();
        }

        public void Center()
        {
            var offsetX = (RenderSize.Width / ScaleFactor - textureImage.ActualWidth) * 0.5;
            var offsetY = (RenderSize.Height / ScaleFactor - textureImage.ActualHeight) * 0.5;

            PanOffset = new(offsetX, offsetY);
        }

        public void ZoomIn()
        {
            var newScaleFactor = Math.Round(ScaleFactor, 1) + 0.1;

            Zoom(newScaleFactor, new(RenderSize.Width * 0.5, RenderSize.Height * 0.5));
        }

        public void ZoomOut()
        {
            var newScaleFactor = Math.Round(ScaleFactor, 1) - 0.1;

            Zoom(newScaleFactor, new(RenderSize.Width * 0.5, RenderSize.Height * 0.5));
        }

        public void ZoomFit()
        {
            if (textureImage.ActualWidth.IsEqual(0) || textureImage.ActualHeight.IsEqual(0)) return;

            var scaleX = RenderSize.Width / textureImage.ActualWidth;
            var scaleY = RenderSize.Height / textureImage.ActualHeight;
            var ratio = Math.Min(scaleX, scaleY);

            Center();

            Zoom(ratio, new(RenderSize.Width * 0.5, RenderSize.Height * 0.5));
        }

        public void ActualSize()
        {
            Center();
            Zoom(1.0, new(RenderSize.Width * 0.5, RenderSize.Height * 0.5));
        }

        private static void OnPanOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextureView tv)
            {
                var current = (Point)e.NewValue;
                var prev = (Point)e.OldValue;

                if (tv.GrdBackground.Background is TileBrush brush)
                {
                    var offset = current - prev;
                    var viewport = brush.Viewport;

                    viewport.X += offset.X;
                    viewport.Y += offset.Y;
                    brush.Viewport = viewport;
                }

                Canvas.SetLeft(tv.imageBorder, current.X);
                Canvas.SetTop(tv.imageBorder, current.Y);
            }
        }

        private static void OnScaleFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextureView tv && tv.GrdBackground.LayoutTransform is ScaleTransform scale)
            {
                scale.ScaleX = (double)e.NewValue;
                scale.ScaleY = (double)e.NewValue;
            }
        }

        private void GrdBackground_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_capturedRight && sender is Grid)
            {
                var mousePos = e.GetPosition(this);
                var offset = mousePos - _gridClickPosition;

                offset /= ScaleFactor;
                PanOffset = new(PanOffset.X + offset.X, PanOffset.Y + offset.Y);
                _gridClickPosition = mousePos;
            }
        }

        private void GrdBackground_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (zoomLabel.Opacity > 0)
            {
                var newScaleFactor = ScaleFactor * (1 + Math.Sin(e.Delta) * 0.1);

                Zoom(newScaleFactor, e.GetPosition(this));
            }
            else
            {
                SetZoomLabel();
            }
        }

        private void GrdBackground_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _capturedRight = false;
            Mouse.Capture(null);
        }

        private void GrdBackground_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _gridClickPosition = e.GetPosition(this);
            _capturedRight = Mouse.Capture(sender as IInputElement);

            Debug.Assert(_capturedRight);
        }

        private void Zoom(double scale, Point center)
        {
            if (scale < 0.1) scale = 0.1;

            if (MathUtilities.IsEqual(scale, ScaleFactor))
            {
                SetZoomLabel();
                return;
            }

            var oldScaleFactor = ScaleFactor;

            ScaleFactor = scale;

            var newPos = new Point(center.X * scale / oldScaleFactor, center.Y * scale / oldScaleFactor);
            var offset = (center - newPos) / scale;
            var vp = textureBackground.Viewport;
            var rect = new Rect(vp.X, vp.Y, vp.Width * oldScaleFactor / scale, vp.Height * oldScaleFactor / scale);

            textureBackground.Viewport = rect;
            PanOffset = new(PanOffset.X + offset.X, PanOffset.Y + offset.Y);

            SetZoomLabel();
        }

        private void SetZoomLabel()
        {
            var vm = DataContext as TextureEditor;

            DoubleAnimation fadeIn = new(1.0, new(TimeSpan.FromSeconds(2.0)));

            fadeIn.Completed += (_, __) =>
            {
                DoubleAnimation fadeOut = new(0, new(TimeSpan.FromSeconds(2.0)));
                zoomLabel.BeginAnimation(OpacityProperty, fadeOut);
            };

            zoomLabel.BeginAnimation(OpacityProperty, fadeIn);
        }
    }
}
