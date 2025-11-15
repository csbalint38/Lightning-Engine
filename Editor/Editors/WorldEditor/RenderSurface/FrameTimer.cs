using System.Diagnostics;

namespace Editor.Editors.WorldEditor.RenderSurface;

internal class FrameTimer
{
    private long _timerStart = Stopwatch.GetTimestamp();
    private int _timerCount = 1;
    private float _avgFrameTimeMicroseconds = 0f;

    public float AverageFrameTime { get; private set; } = 0.016667f;
    public int FPS { get; private set; }
    public float LastFrameTime { get; private set; } = 0.016667f;

    internal bool MeasureFrameTime()
    {
        var timerEnd = Stopwatch.GetTimestamp();
        var elapsed = Stopwatch.GetElapsedTime(_timerStart, timerEnd);

        LastFrameTime = (float)(elapsed.TotalMicroseconds * 1e-6f);
        _timerStart = timerEnd;
        _avgFrameTimeMicroseconds += (float)(elapsed.TotalMicroseconds - _avgFrameTimeMicroseconds) / _timerCount;

        if(AverageFrameTime * _timerCount > 1)
        {
            AverageFrameTime = _avgFrameTimeMicroseconds * 1e-6f;
            FPS = _timerCount;
            _avgFrameTimeMicroseconds = 0f;

            return true;
        }

        return false;
    }
}
