namespace Editor.Content;

public class LodInfo
{
    public string? Name { get; init; }
    public float Threshold { get; init; }
    public List<MeshInfo> Meshes { get; init; } = [];
}
