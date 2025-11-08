using Editor.DLLs;
using System.Diagnostics;

namespace Editor.Editors.WorldEditor.RenderSurface;

internal static class RenderThread
{
    private static readonly Dictionary<IntPtr, RenderFrameCallback> _callbackMap = [];
    private static readonly List<RenderFrameCallback> _callbacks = [];
    private static readonly List<int> _frameCounts = [];
    private static readonly Lock _lock = new();

    private static Thread? _renderThread;
    private static bool _isRunning;

    public static bool IsRunning => _isRunning;

    internal static void RegisterCallback(IntPtr hwnd, RenderFrameCallback callback)
    {
        lock (_lock)
        {
            if (!_callbackMap.ContainsKey(hwnd))
            {
                _callbackMap[hwnd] = callback;

                _callbacks.Add(callback);
                _frameCounts.Add(0);

                if (_callbackMap.Count == 1 && _renderThread == null) Start();
            }
        }
    }

    internal static void UnregisterCallback(IntPtr hwnd)
    {
        lock (_lock)
        {
            if (_callbackMap.TryGetValue(hwnd, out var callback))
            {
                var inndex = _callbacks.IndexOf(callback);

                _frameCounts.RemoveAt(inndex);
                _callbacks.RemoveAt(inndex);
                _callbackMap.Remove(hwnd);

                if (_callbackMap.Count == 0 && _renderThread is null)
                {
                    _ = Task.Run(() => Stop());
                }
            }
        }
    }

    private static void Start()
    {
        Debug.Assert(_renderThread is null);

        _renderThread = new(Render)
        {
            Name = "RenderThread",
            IsBackground = true,
        };

        _isRunning = true;

        _renderThread.Start();
    }

    private static void Stop()
    {
        if (_renderThread is not null)
        {
            _isRunning = false;

            _renderThread.Join();

            _renderThread = null;

            Debug.WriteLine("Render thread stopped");
        }
    }

    private static void Render(object? obj)
    {
        while (_isRunning)
        {
            lock (_lock)
            {
                for (int i = 0; i < _callbacks.Count; i++)
                {
                    var info = _callbacks[i](_frameCounts[i]++);

                    EngineAPI.RenderFrame(info.SurfaceId, info.CameraId, info.LightSetKey);
                }
            }
        }
    }
}
