namespace Editor.Components;

public class LODWithMaterials(string? name, float threshold, List<MeshWithMaterial> meshes)
{
    public string? Name { get; } = name;
    public float Threshold { get; } = threshold;
    public List<MeshWithMaterial> Meshes { get; } = meshes;
}
