using Editor.Common.Enums;
using Editor.Editors.WorldEditor.RenderSurface;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Editor.Editors;

/// <summary>
/// Interaction logic for RenderSurfaceView.xaml
/// </summary>
public partial class RenderSurfaceView : UserControl, IDisposable
{
    private RenderSurfaceHost? _host = null;
    private bool _disposedValue;

    public RenderSurfaceView()
    {
        InitializeComponent();

        Loaded += OnRenderSurfaceViewLoaded;
    }

    private void OnRenderSurfaceViewLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnRenderSurfaceViewLoaded;

        /*
        _host = new RenderSurfaceHost(ActualWidth, ActualHeight);
        _host.MessageHook += new HwndSourceHook(HostMsgFilter);
        Content = _host;
        */
    }

    private IntPtr HostMsgFilter(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch ((Win32Msg)msg)
        {
            case Win32Msg.WM_SIZING: throw new Exception();
            case Win32Msg.WM_ENTERSIZEMOVE: throw new Exception();
            case Win32Msg.WM_EXITSIZEMOVE: throw new Exception();
            case Win32Msg.WM_SIZE:
                break;
            default:
                break;
        }

        return IntPtr.Zero;
    }

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _host?.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
