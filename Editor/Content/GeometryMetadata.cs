namespace Editor.Content
{
    public class GeometryMetadata : AssetMetadata
    {
        public string Name { get; init; }
        public List<LodInfo> LODs { get; init; }
    }
}
