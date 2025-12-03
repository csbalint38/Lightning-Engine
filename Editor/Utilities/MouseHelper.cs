using System.Drawing;
using System.Runtime.InteropServices;

namespace Editor.Utilities;

static partial class MouseHelper
{
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT(int x, int y)
    {
        public int X = x;
        public int Y = y;

        public POINT(Point pt) : this(pt.X, pt.Y) { }
        public static implicit operator Point(POINT p) => new(p.X, p.Y);
        public static implicit operator POINT(Point p) => new(p.X, p.Y);
    }

    [LibraryImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetCursorPos(int x, int y);

    [LibraryImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(out POINT lpPoint);

    [LibraryImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ClientToScreen(IntPtr hWnd, ref POINT point);

    [LibraryImport("User32.dll")]
    public static partial IntPtr SetCapture(IntPtr hWnd);

    [LibraryImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ReleaseCapture();

    public static void SetCursor(IntPtr hWnd, int x, int y)
    {
        var p = new POINT(x, y);

        ClientToScreen(hWnd, ref p);
        SetCursorPos(p.X, p.Y);
    }

    public static Point GetCursor()
    {
        GetCursorPos(out POINT p);

        return p;
    }
}
