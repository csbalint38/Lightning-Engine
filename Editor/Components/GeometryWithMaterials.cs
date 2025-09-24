namespace Editor.Components;

public class GeometryWithMaterials(string? name, byte[] icon, List<LODWithMaterials> LODs)
{
    public string? Name { get; } = name;
    public byte[] Icon { get; } = icon;
    public List<LODWithMaterials> LODs { get; } = LODs;
}
