using Editor.Common.Enums;
using Editor.DLLs;
using Editor.Utilities;
using Editor.Utilities.Descriptors;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Editor.Editors.WorldEditor.RenderSurface;

delegate FrameInfo RenderFrameCallback(int frame);

internal partial class RenderSurfaceHost : HwndHost
{
    private readonly int _width = 800;
    private readonly int _height = 600;
    private readonly DelayEventTimer _resizeTimer;
    private readonly RenderFrameCallback _renderFrameCallback;
    private readonly AutoResetEvent _resetEvent = new(false);

    private IntPtr _handle = IntPtr.Zero;
    private bool _disposed;

    public int SurfaceId { get; private set; } = Id.InvalidId;
    public bool HostReady { get; private set; }

    public RenderSurfaceHost(double width, double height, RenderFrameCallback renderFrameCallback)
    {
        _width = (int)width;
        _height = (int)height;
        _resizeTimer = new DelayEventTimer(TimeSpan.FromMilliseconds(50));

        _resizeTimer.Triggered += Resize;

        _renderFrameCallback = renderFrameCallback;

        SizeChanged += (s, e) => _resizeTimer.Trigger();
    }

    public async Task WaitReadyAsync() => await Task.Run(_resetEvent.WaitOne);

    [LibraryImport("user32.dll", EntryPoint = "DestroyWindow")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyWindow(IntPtr hwnd);

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        SurfaceId = EngineAPI.CreateRendererSurface(hwndParent.Handle, _width, _height);

        Debug.Assert(Id.IsValid(SurfaceId));

        _handle = EngineAPI.GetWindowHandle(SurfaceId);

        Debug.Assert(_handle != IntPtr.Zero);

        RenderThread.RegisterCallback(_handle, _renderFrameCallback);

        HostReady = RenderThread.IsRunning;

        _resetEvent.Set();

        return new HandleRef(this, _handle);
    }

    protected override void DestroyWindowCore(HandleRef hwnd) => DestroyWindow(hwnd.Handle);

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                RenderThread.UnregisterCallback(_handle);
                EngineAPI.RemoveRendererSurface(SurfaceId);

                _handle = IntPtr.Zero;

                Debug.WriteLine($"Render host for surface {SurfaceId} disposed.");

                SurfaceId = Id.InvalidId;
            }

            _disposed = true;
        }

        base.Dispose(disposing);
    }

    protected override nint WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = false;

        return IntPtr.Zero;
    }

    private void Resize(object? sender, DelayEventTimerArgs e)
    {
        e.RepeatEvent = KeyboardHelper.GetAsyncKeyState(VKey.MouseLeft) < 0;

        if (!e.RepeatEvent) EngineAPI.ResizeRenderSurface(SurfaceId);
    }
}
