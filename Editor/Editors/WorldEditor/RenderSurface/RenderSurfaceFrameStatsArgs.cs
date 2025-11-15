namespace Editor.Editors.WorldEditor.RenderSurface;

public class RenderSurfaceFrameStatsArgs(float averageFrameTime, int fps) : EventArgs
{
    public float AverageFrameTime { get; } = averageFrameTime;
    public int FPS { get; } = fps;
}
