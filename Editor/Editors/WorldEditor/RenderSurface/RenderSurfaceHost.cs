using Editor.DLLs;
using Editor.Utilities;
using Editor.Utilities.Descriptors;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Editor.Editors.WorldEditor.RenderSurface
{
    internal class RenderSurfaceHost : HwndHost
    {
        private readonly int VK_LBUTTON = 0x01;
        private readonly int _width = 800;
        private readonly int _height = 600;
        private IntPtr _handle = IntPtr.Zero;
        private DelayEventTimer _resizeTimer;
        private int _times = 0;

        public int SurfaceId { get; private set; } = Id.InvalidId;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public RenderSurfaceHost(double width, double height)
        {
            _width = (int)width;
            _height = (int)height;
            _resizeTimer = new DelayEventTimer(TimeSpan.FromMilliseconds(250));

            _resizeTimer.Triggered += Resize;
            SizeChanged += (s, e) => _resizeTimer.Trigger();
        }

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
            e.RepeatEvent = GetAsyncKeyState(VK_LBUTTON) < 0;

            if (_times > 1)
            {
                if (!e.RepeatEvent) EngineAPI.ResizeRenderSurface(SurfaceId);
            }
            _times++;
        }
    }
}
