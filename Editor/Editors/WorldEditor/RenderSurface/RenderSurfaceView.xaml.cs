using Editor.Editors.WorldEditor.RenderSurface;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Editors;

/// <summary>
/// Interaction logic for RenderSurfaceView.xaml
/// </summary>
public partial class RenderSurfaceView : UserControl
{
    internal RenderSurfaceControl RenderSurfaceControl => renderSurfaceControl;

    public RenderSurfaceView()
    {
        InitializeComponent();

        Loaded += OnRenderSurfaceViewLoaded;
    }

    private void OnRenderSurfaceViewLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnRenderSurfaceViewLoaded;

        renderSurfaceControl.FrameStatsUpdated += (_, e) =>
        {
            _ = Application.Current.Dispatcher.BeginInvoke(() =>
            {
                TbFrameTime.Text = $"{e.AverageFrameTime * 1000F:F1} ms";
                TbFrameRate.Text = $"{e.FPS} Hz";
            });
        };
    }

    private void TgBtnCameraSettings_Click(object sender, RoutedEventArgs e) => PpCameraSettings.IsOpen = true;
}
