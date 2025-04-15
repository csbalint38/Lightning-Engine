using Editor.Utilities;
using System.ComponentModel;
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
        private Point _oldPanOffset = new(0, 0);

        public TextureView()
        {
            InitializeComponent();

            SizeChanged += (_, __) => Center();
            textureImage.SizeChanged += (_, __) => ZoomFit();
            DataContextChanged += OnDataContextChanged;
        }

        public void Center()
        {
            var vm = DataContext as TextureEditor;
            var offsetX = (RenderSize.Width / vm.ScaleFactor - textureImage.ActualWidth) * 0.5;
            var offsetY = (RenderSize.Height / vm.ScaleFactor - textureImage.ActualHeight) * 0.5;

            vm.PanOffset = new(offsetX, offsetY);
        }

        public void ZoomIn()
        {
            var vm = DataContext as TextureEditor;
            var newScaleFactor = Math.Round(vm.ScaleFactor, 1) + 0.1;

            Zoom(newScaleFactor, new(RenderSize.Width * 0.5, RenderSize.Height * 0.5));
        }

        public void ZoomOut()
        {
            var vm = DataContext as TextureEditor;
            var newScaleFactor = Math.Round(vm.ScaleFactor, 1) - 0.1;

            Zoom(newScaleFactor, new(RenderSize.Width * 0.5, RenderSize.Height * 0.5));
        }

        public void ZoomFit()
        {
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

        private void GrdBackground_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var vm = DataContext as TextureEditor;

            if(_capturedRight && sender is Grid)
            {
                var mousePos = e.GetPosition(this);
                var offset = mousePos - _gridClickPosition;

                offset /= vm.ScaleFactor;
                vm.PanOffset = new(vm.PanOffset.X + offset.X, vm.PanOffset.Y + offset.Y);
                _gridClickPosition = mousePos;
            }
        }

        private void GrdBackground_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (zoomLabel.Opacity > 0)
            {
                var vm = DataContext as TextureEditor;
                var newScaleFactor = vm.ScaleFactor * (1 + Math.Sin(e.Delta) * 0.1);

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
            if(scale < 0.1) scale = 0.1;

            var vm = DataContext as TextureEditor;

            if (MathUtilities.IsEqual(scale, vm.ScaleFactor)) return;

            var oldScaleFactor = vm.ScaleFactor;

            vm.ScaleFactor = scale;

            var newPos = new Point(center.X * scale / oldScaleFactor, center.Y * scale / oldScaleFactor);
            var offset = (center - newPos) / scale;
            var vp = textureBackground.Viewport;
            var rect = new Rect(vp.X, vp.Y, vp.Width * oldScaleFactor / scale, vp.Height * oldScaleFactor / scale);

            textureBackground.Viewport = rect;
            vm.PanOffset = new(vm.PanOffset.X + offset.X, vm.PanOffset.Y + offset.Y);

            SetZoomLabel();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is TextureEditor oldVm) oldVm.PropertyChanged -= OnViewModelPropertyChanged;

            if (e.NewValue is TextureEditor newVm)
            {
                _oldPanOffset = newVm.PanOffset;
                newVm.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TextureEditor.PanOffset))
            {
                OnPanOffsetPropertyChanged(sender as TextureEditor);
            }
        }

        private void OnPanOffsetPropertyChanged(TextureEditor editor)
        {
            if(GrdBackground.Background is TileBrush brush)
            {
                var offset = editor.PanOffset - _oldPanOffset;
                var viewport = brush.Viewport;

                viewport.X += offset.X;
                viewport.Y += offset.Y;
                brush.Viewport = viewport;
            }

            _oldPanOffset = editor.PanOffset;
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
