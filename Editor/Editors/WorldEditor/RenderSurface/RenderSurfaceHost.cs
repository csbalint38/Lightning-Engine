using Editor.DLLs;
using Editor.Utilities;
using Editor.Utilities.Descriptors;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace Editor.Editors.WorldEditor.RenderSurface
{
    internal class RenderSurfaceHost : HwndHost
    {
        private readonly int _width = 800;
        private readonly int _height = 600;
        private IntPtr _handle = IntPtr.Zero;
        private DelayEventTimer _resizeTimer;

        public int SurfaceId { get; private set; } = Id.InvalidId;

        public RenderSurfaceHost(double width, double height)
        {
            _width = (int)width;
            _height = (int)height;
            _resizeTimer = new DelayEventTimer(TimeSpan.FromMilliseconds(250));

            _resizeTimer.Triggered += Resize;
        }

        public void Resize() => _resizeTimer.Trigger();

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            SurfaceId = EngineAPI.CreateRendererSurface(hwndParent.Handle, _width, _height);

            Debug.Assert(Id.IsValid(SurfaceId));

            _handle = EngineAPI.GetWindowHandle(SurfaceId);

            Debug.Assert(_handle != IntPtr.Zero);

            return new HandleRef(this, _handle);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            EngineAPI.RemoveRendererSurface(SurfaceId);

            SurfaceId = Id.InvalidId;
            _handle = IntPtr.Zero;
        }

        private void Resize(object? sender, DelayEventTimerArgs e)
        {
            e.RepeatEvent = Mouse.LeftButton == MouseButtonState.Pressed;

            if (!e.RepeatEvent) EngineAPI.ResizeRenderSurface(SurfaceId);
        }
    }
}
