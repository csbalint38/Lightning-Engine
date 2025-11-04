using Editor.Utilities;

namespace Editor.Editors.WorldEditor.RenderSurface
{
    public class FrameInfo
    {
        public int SurfaceId { get; init; }
        public ulong LightSetKey { get; init; }
        public IdType CameraId { get; init; } = Id.InvalidId;
    }
}
