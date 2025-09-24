using Editor.Common.Enums;
using Editor.Content;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace Editor.Components;

[DataContract]
public class Geometry : Component
{
    private GeometryWithMaterials? _geometryWithMaterials;
    private UploadedAsset? _geometry;

    [DataMember(Name = "Materials")]
    private List<AppliedMaterial> _materials = [];

    [DataMember(Name = "Geometry")]
    public Guid GeometryGuid { get; private set; }

    public GeometryWithMaterials? GeometryWithMaterials
    {
        get => _geometryWithMaterials;
        private set
        {
            if (_geometryWithMaterials != value)
            {
                _geometryWithMaterials = value;
                OnPropertyChanged(nameof(GeometryWithMaterials));
            }
        }
    }

    public List<AppliedMaterial> Materials =>
        GeometryWithMaterials?.LODs?.SelectMany(x => x.Meshes, (x, y) => y.Material)?.ToList() ?? [];

    public IdType ContentId => _geometry?.ContentId ?? Id.InvalidId;

    public Geometry(Entity owner, AssetInfo geometry) : base(owner)
    {
        Debug.Assert(geometry?.Type == AssetType.MESH);

        GeometryGuid = geometry.Guid;
    }

    public override IMSComponent GetMultiselectComponents(MSEntityBase entity) => new MSGeometry(entity);

    public override void WriteToBinary(BinaryWriter bw) => throw new NotImplementedException();

    public override void Load()
    {
        Debug.Assert(_geometry is null && GeometryWithMaterials is null);
        Debug.Assert(GeometryGuid != Guid.Empty);

        var assetInfo = AssetRegistry.GetAssetInfo(GeometryGuid) ?? DefaultAssets.DefaultGeometry;

        Debug.Assert(assetInfo?.Type == AssetType.MESH);
        Debug.Assert(assetInfo?.Guid == GeometryGuid);

        _materials.ForEach(x => x.UploadToEngine());

        Debug.Assert(_materials.All(
            x => x.UploadedAsset is not null
            && Id.IsValid(x.UploadedAsset.ContentId))
        );

        Load(assetInfo);
    }

    public override void Unload()
    {
        Debug.Assert(_geometry is not null && Id.IsValid(_geometry.ContentId));

        if (_geometry is null || !Id.IsValid(_geometry.ContentId)) return;

        _materials = Materials;
        _materials.ForEach(x => x.UnloadFromEngine());

        GeometryWithMaterials = null;
        UploadedAsset.RemoveFromScene(_geometry);
        _geometry = null;
    }

    public void SetGeometry(Guid guid)
    {
        if (_geometry?.AssetInfo.Guid != guid)
        {
            ParentEntity.IsActive = false;
            GeometryGuid = guid;

            _materials.Clear();

            ParentEntity.IsActive = true;
        }
    }

    private static AppliedMaterial CreateAndUploadAppliedMaterial(AssetInfo material)
    {
        var appliedMaterial = new AppliedMaterial(material);

        appliedMaterial.UploadToEngine();

        Debug.Assert(appliedMaterial.UploadedAsset is not null);

        return appliedMaterial;
    }

    [OnSerializing]
    private void OnSerializing(StreamingContext context)
    {
        Debug.Assert(_geometry is not null && _geometry.AssetInfo.Guid != Guid.Empty);

        GeometryGuid = _geometry.AssetInfo.Guid;
        _materials = Materials;
    }

    private void Load(AssetInfo geometry)
    {
        Debug.Assert(_geometry is null && GeometryWithMaterials is null);

        _geometry = UploadedAsset.AddToScene(geometry);

        Debug.Assert(_geometry is not null && Id.IsValid(_geometry.ContentId));

        if (_geometry?.Metadata is GeometryMetadata metadata && Id.IsValid(_geometry.ContentId))
        {
            var index = 0;

            GeometryWithMaterials = new(
                metadata.Name ?? string.Empty,
                _geometry.AssetInfo.Icon,
                [.. metadata.LODs.Select(lod => new LODWithMaterials(
                    lod.Name,
                    lod.Threshold,
                    [.. lod.Meshes.Select(mesh => new MeshWithMaterial(
                        mesh,
                        index < _materials.Count
                            ? _materials[index++]
                            : CreateAndUploadAppliedMaterial(Material.Default!)
                    ))]
                ))]
            );
        }

        Debug.Assert(GeometryWithMaterials is not null && GeometryWithMaterials.LODs.Count > 0);
    }
}
