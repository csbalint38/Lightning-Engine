using Editor.Common.Enums;
using Editor.DLLs;
using Editor.GameProject;
using Editor.Utilities;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using static Editor.Utilities.KeyboardHelper;
using static Editor.Utilities.MouseHelper;

namespace Editor.Editors.WorldEditor.RenderSurface;

/// <summary>
/// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
///
/// Step 1a) Using this custom control in a XAML file that exists in the current project.
/// Add this XmlNamespace attribute to the root element of the markup file where it is 
/// to be used:
///
///     xmlns:MyNamespace="clr-namespace:Editor.Editors.WorldEditor.RenderSurface"
///
///
/// Step 1b) Using this custom control in a XAML file that exists in a different project.
/// Add this XmlNamespace attribute to the root element of the markup file where it is 
/// to be used:
///
///     xmlns:MyNamespace="clr-namespace:Editor.Editors.WorldEditor.RenderSurface;assembly=Editor.Editors.WorldEditor.RenderSurface"
///
/// You will also need to add a project reference from the project where the XAML file lives
/// to this project and Rebuild to avoid compilation errors:
///
///     Right click on the target project in the Solution Explorer and
///     "Add Reference"->"Projects"->[Browse to and select this project]
///
///
/// Step 2)
/// Go ahead and use your control in the XAML file.
///
///     <MyNamespace:RenderSurfaceControl/>
///
/// </summary>
public class RenderSurfaceControl : ContentControl, IDisposable
{
    private readonly FrameTimer _frameTimer = new();
    private readonly EditorCamera _camera = new();

    private bool _disposedValue;
    private RenderSurfaceHost? _host = null;
    private Point _clickPosition = new(0, 0);
    private bool _captureLeft;
    private bool _captureRight;
    private bool _isMouseOver;
    private bool _isXYLocked = false;
    private ulong _lightSetKey;

    public static readonly DependencyProperty IsXZLockedProperty =
        DependencyProperty.Register(
            nameof(IsXZLocked),
            typeof(bool),
            typeof(RenderSurfaceControl),
            new PropertyMetadata(false, new PropertyChangedCallback(OnXZLockedChanged))
        );

    public static readonly DependencyProperty CameraSpeedProperty =
        DependencyProperty.Register(
            nameof(CameraSpeed),
            typeof(int),
            typeof(RenderSurfaceControl),
            new PropertyMetadata(5, new PropertyChangedCallback(OnCameraSpeedChanged))
        );

    public static readonly DependencyProperty CameraFoVProperty =
        DependencyProperty.Register(
            nameof(CameraFoV),
            typeof(float),
            typeof(RenderSurfaceControl),
            new PropertyMetadata(45f, new PropertyChangedCallback(OnCameraFoVChanged))
        );

    public static readonly DependencyProperty CameraNearZProperty =
        DependencyProperty.Register(
            nameof(CameraNearZ),
            typeof(float),
            typeof(RenderSurfaceControl),
            new PropertyMetadata(0.1f, new PropertyChangedCallback(OnCameraNearZChanged))
        );

    public static readonly DependencyProperty CameraFarZProperty =
        DependencyProperty.Register(
            nameof(CameraFarZ),
            typeof(float),
            typeof(RenderSurfaceControl),
            new PropertyMetadata(100f, new PropertyChangedCallback(OnCameraFarZChanged))
        );

    public event EventHandler<RenderSurfaceFrameStatsArgs>? FrameStatsUpdated;

    public bool IsXZLocked
    {
        get => (bool)GetValue(IsXZLockedProperty);
        set => SetValue(IsXZLockedProperty, value);
    }

    public int CameraSpeed
    {
        get => (int)GetValue(CameraSpeedProperty);
        set => SetValue(CameraSpeedProperty, value);
    }

    public float CameraFoV
    {
        get => (float)GetValue(CameraFoVProperty);
        set => SetValue(CameraFoVProperty, value);
    }

    public float CameraNearZ
    {
        get => (float)GetValue(CameraNearZProperty);
        set => SetValue(CameraNearZProperty, value);
    }

    public float CameraFarZ
    {
        get => (float)GetValue(CameraFarZProperty);
        set => SetValue(CameraFarZProperty, value);
    }

    static RenderSurfaceControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(RenderSurfaceControl),
            new FrameworkPropertyMetadata(typeof(RenderSurfaceControl))
        );
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        DataContextChanged += OnDataContextChangedAsync;
        KeyDown += OnRenderSurfaceControl_KeyDown;

        OnDataContextChangedAsync(this, new(DataContextProperty, null, DataContext));
    }

    public void SetComponentIds(List<IdType> componentIds)
    {
        Debug.Assert(componentIds is not null);

        if (_host?.HostReady == true)
        {
            EngineAPI.SetGeometryIds(_host.SurfaceId, [.. componentIds], componentIds.Count);
        }
    }

    public void UseLightSet(ulong lightSetKey) => _lightSetKey = lightSetKey;

    public void FocusPosition(Vector3 position)
    {
        if (_isMouseOver || IsFocused) _camera.GoTo(position);
    }

    private static void OnXZLockedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        (d as RenderSurfaceControl)?._isXYLocked = (bool)e.NewValue;

    private static void OnCameraSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        (d as RenderSurfaceControl)?._camera.Speed = (int)e.NewValue;

    private static void OnCameraFoVChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        (d as RenderSurfaceControl)?._camera.FoV = (float)e.NewValue;

    private static void OnCameraNearZChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        (d as RenderSurfaceControl)?._camera.NearZ = (float)e.NewValue;

    private static void OnCameraFarZChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        (d as RenderSurfaceControl)?._camera.FarZ = (float)e.NewValue;

    private static Vector3 GetMoveDirection()
    {
        static float GetKey(VKey key) => GetAsyncKeyState(key) < 0 ? 1f : 0f;
        var moveDir = Vector3.Zero;

        moveDir.X += GetKey(VKey.A);
        moveDir.X -= GetKey(VKey.D);
        moveDir.Z += GetKey(VKey.W);
        moveDir.Z -= GetKey(VKey.S);
        moveDir.Y += GetKey(VKey.E);
        moveDir.Y -= GetKey(VKey.Q);

        return moveDir;
    }

    private static (short, short) SplitParam(IntPtr param)
    {
        var p = param.ToInt64();

        return (unchecked((short)(p & 0xffff)), unchecked((short)((p >> 16) & 0xffff)));
    }

    private async void OnDataContextChangedAsync(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not null && _host is null)
        {
            _host = new RenderSurfaceHost(ActualWidth, ActualHeight, new RenderFrameCallback(OnRenderFrame));
            _host.MessageHook += new HwndSourceHook(HostMsgFilter);
            Content = _host;

            await _host.WaitReadyAsync();

            Project.Current?.UpdateScene();

            _camera.SetSurfaceId(_host.SurfaceId);

            _disposedValue = false;
        }
        else if (e.NewValue is null && _host is not null)
        {
            SetComponentIds([]);

            Content = null;

            Dispose();
        }
    }

    private FrameInfo OnRenderFrame(int frame)
    {
        if (_frameTimer.MeasureFrameTime())
        {
            FrameStatsUpdated?.Invoke(this, new(_frameTimer.AverageFrameTime, _frameTimer.FPS));
        }

        if (_captureLeft && !_isXYLocked && GetAsyncKeyState(VKey.Alt) == 0)
        {
            var moveDir = GetMoveDirection();

            if (moveDir.LengthSquared() >= MathUtilities.Epsilon) _camera.ChangePosition(moveDir, _frameTimer.AverageFrameTime);
        }

        _camera.Update(_frameTimer.AverageFrameTime);

        return new()
        {
            SurfaceId = _host!.SurfaceId,
            LightSetKey = _lightSetKey,
        };
    }

    private IntPtr HostMsgFilter(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch ((Win32Msg)msg)
        {
            case Win32Msg.WM_SIZING:
                break;
            case Win32Msg.WM_ENTERSIZEMOVE:
                break;
            case Win32Msg.WM_EXITSIZEMOVE:
                break;
            case Win32Msg.WM_SIZE:
                break;

            case Win32Msg.WM_LBUTTONDOWN:
                {
                    var (x, y) = SplitParam(lParam);

                    OnRenderHostMouseLBD(x, y);
                }
                break;

            case Win32Msg.WM_LBUTTONUP:
                OnRenderHostMouseLBU();
                break;

            case Win32Msg.WM_MBUTTONDOWN:
                break;
            case Win32Msg.WM_MBUTTONUP:
                break;

            case Win32Msg.WM_RBUTTONDOWN:
                if (_isXYLocked || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                {
                    var (x, y) = SplitParam(lParam);

                    OnRenderHostMouseRBD(x, y);
                }
                break;

            case Win32Msg.WM_RBUTTONUP:
                if (_isXYLocked || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) OnRenderHostMouseRBU();
                break;

            case Win32Msg.WM_MOUSEHOVER:
                _isMouseOver = true;
                break;

            case Win32Msg.WM_MOUSELEAVE:
                _isMouseOver = false;
                break;

            case Win32Msg.WM_MOUSEMOVE:
                {
                    var (x, y) = SplitParam(lParam);
                    OnRenderHostMouseMove(x, y);
                }
                break;

            case Win32Msg.WM_MOUSEWHEEL:
                {
                    var (_, high) = SplitParam(wParam);

                    OnRenderHostMouseWheel(Math.Sign(high));
                }
                break;
            case Win32Msg.WM_KEYDOWN:
                break;
            case Win32Msg.WM_KEYUP:
                break;
            default:
                break;
        }

        return IntPtr.Zero;
    }

    private void OnRenderSurfaceControl_KeyDown(object sender, KeyEventArgs e) =>
        e.Handled = e.Key == Key.System && e.OriginalSource is RenderSurfaceControl;

    private void OnRenderHostMouseLBD(int x, int y)
    {
        Focus();

        _clickPosition = new(x, y);
        _captureLeft = true;

        SetCapture(_host!.Handle);
    }

    private void OnRenderHostMouseLBU()
    {
        _captureLeft = false;

        ReleaseCapture();
    }

    private void OnRenderHostMouseRBD(int x, int y)
    {
        Focus();

        _clickPosition = new(x, y);
        _captureRight = true;

        SetCapture(_host!.Handle);
    }

    private void OnRenderHostMouseRBU()
    {
        _captureRight = false;

        ReleaseCapture();
    }

    private void OnRenderHostMouseMove(int x, int y)
    {
        var currentPosition = new Point(x, y);
        var d = currentPosition - _clickPosition;

        _clickPosition = currentPosition;

        if (_captureLeft)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) || _isXYLocked) _camera.Orbit(d.X, d.Y, 0);
            else _camera.ChangeDirection(d.X, d.Y);
        }
        else if (_captureRight && (_isXYLocked || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)))
        {
            var target = _camera.Target;

            target.Y += (float)d.Y * .0005f * _camera.OrbitRadius;

            _camera.GoTo(target);
        }
    }

    private void OnRenderHostMouseWheel(int sign)
    {
        if (_isXYLocked || (IsFocused && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))) _camera.Orbit(0, 0, sign);
    }

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Content = null;

                _host?.Dispose();

                _host = null;
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
