using Editor.Common.Enums;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace Editor.Content;

[DataContract]
public class AppliedMaterial : Asset
{
    private static readonly Lock _lock = new();
    private static readonly Dictionary<string, UploadedAsset> _packedMaterials = [];
    private static readonly Dictionary<IdType, string> _packedMaterialIds = [];
    private static readonly Dictionary<Guid, RefCountMaterial> _loadedMaterials = [];


    [DataMember(Name = "Inputs")]
    private readonly List<Guid> _inputGuids = [];

    [DataMember(Name = "InputNames")]
    private readonly List<string> _inputNames = [];

    private List<IdType> _shaderIds = [];
    private ObservableCollection<AppliedMaterialInput> _inputs = [];
    private Material _material;
    private byte[]? _packedData = [];
    private byte[] _previousPackedData = [];

    [DataMember(Name = "Material")]
    private Guid _materialGuid;

    private string _name = "Material";

    public ReadOnlyObservableCollection<AppliedMaterialInput> Inputs { get; private set; }

    [DataMember]
    public MaterialSurface MaterialSurface { get; private set; } = new();
    public UploadedAsset? UploadedAsset { get; private set; }

    [DataMember]
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public AppliedMaterial(AssetInfo materialAssetInfo) : base(AssetType.MATERIAL)
    {
        LoadMaterial(materialAssetInfo);

        Debug.Assert(_material is not null);

        _material.MaterialSurface.CopyTo(MaterialSurface);
        _material.GetInput().ForEach(x => _inputs.Add(new(x)));

        Icon = _material.Icon;
        Inputs = new(_inputs);
    }

    public override List<AssetInfo> GetReferencedAssets() =>
        [.. Inputs.Where(x => x.Asset is not null && x.Asset.Guid != Guid.Empty).Select(x => x.Asset)];

    public override MaterialMetadata GetMetadata() => new()
    {
        PackedData = _packedData
    };

    public override IEnumerable<string> Save(string file) => throw new NotImplementedException();
    public override bool Import(string file) => throw new NotImplementedException();
    public override bool Load(string file) => throw new NotImplementedException();

    /// <summary>
    /// Pack the material into a byte array wich can be used by the Engine.
    /// </summary>
    /// <returns>
    /// struct {
    ///     id::id_type* texture_ids;
    ///     MaterialSurface surface;
    ///     MaterialType::Type type;
    ///     u32 texture_count;
    ///     id::id_type shader_ids[ShaderType::count];
    /// }
    /// </returns>
    public override byte[]? PackForEngine()
    {
        using var writer = new BinaryWriter(new MemoryStream());
        var referencedAssets = GetReferencedAssets();

        writer.Write(referencedAssets.Count);

        if (referencedAssets.Count > 0)
        {
            foreach (var input in referencedAssets)
            {
                var contentId = UploadedAsset.GetContentId(input.Guid);

                Debug.Assert(Id.IsValid(contentId));

                if (!Id.IsValid(contentId)) return null;

                writer.Write(contentId);
            }
        }

        writer.Write(IntPtr.Zero);
        MaterialSurface.ToBinary(writer);
        writer.Write((int)_material.MaterialType);
        writer.Write(referencedAssets.Count);
        _shaderIds.ForEach(writer.Write);
        writer.Flush();

        var data = (writer.BaseStream as MemoryStream)?.ToArray();

        Debug.Assert(data?.Length > 0);

        return data;
    }

    public bool UploadToEngine()
    {
        lock (_lock)
        {
            _inputs.ToList().ForEach(x => x.Load());

            UploadShaders();

            _packedData = PackForEngine();

            if (_packedData is null)
            {
                UnloadShaders();

                return false;
            }

            if (_packedData.SequenceEqual(_previousPackedData))
            {
                Debug.Assert(
                    UploadedAsset is not null
                    && UploadedAsset.GetContentId(Guid) == UploadedAsset.ContentId
                );

                UnloadShaders();

                return true;
            }

            var dataString = Convert.ToBase64String(_packedData);

            if (_packedMaterials.TryGetValue(dataString, out var uploadedAsset))
            {
                Debug.Assert(_packedMaterialIds.ContainsKey(uploadedAsset.ContentId));

                Debug.Assert(
                    ((MaterialMetadata)uploadedAsset.Metadata).PackedData!.SequenceEqual(_packedData)
                );

                Guid = uploadedAsset.AssetInfo.Guid;

                var result = UploadedAsset.AddToScene(GetAssetInfo(), this);

                Debug.Assert(result == uploadedAsset);
            }
            else
            {
                if (Id.IsValid(UploadedAsset.GetContentId(Guid)))
                {
                    Debug.Assert(_previousPackedData.Length > 0 && UploadedAsset is not null);

                    Guid = Guid.NewGuid();
                }

                uploadedAsset = UploadedAsset.AddToScene(GetAssetInfo(), this);

                Debug.Assert(!_packedMaterialIds.ContainsKey(uploadedAsset!.ContentId));

                _packedMaterials.Add(dataString, uploadedAsset);
                _packedMaterialIds.Add(uploadedAsset.ContentId, dataString);
            }

            if (_previousPackedData.Length > 0)
            {
                Debug.Assert(UploadedAsset is not null && UploadedAsset.ContentId != uploadedAsset.ContentId);

                UnloadFromEngine();
            }

            _previousPackedData = _packedData;
            UploadedAsset = uploadedAsset;

            return true;
        }
    }

    public void UnloadFromEngine()
    {
        lock (_lock)
        {

            Debug.Assert(UploadedAsset is not null && _packedMaterialIds.ContainsKey(UploadedAsset.ContentId));
            Debug.Assert(UploadedAsset.GetContentId(UploadedAsset.AssetInfo.Guid) == UploadedAsset.ContentId);

            if (
                _packedMaterialIds.TryGetValue(UploadedAsset.ContentId, out var dataString) &&
                _packedMaterials.TryGetValue(dataString, out var uploadedAsset)
            )
            {
                var contentId = uploadedAsset.ContentId;

                Debug.Assert(UploadedAsset == uploadedAsset);

                UploadedAsset.RemoveFromScene(uploadedAsset);
                UnloadShaders();

                if (UploadedAsset.ReferenceCount == 0)
                {
                    _packedMaterialIds.Remove(contentId);
                    _packedMaterials.Remove(dataString);

                }

                _inputs.ToList().ForEach(x => x.Unload());

                _previousPackedData = [];
                UploadedAsset = null;
            }
        }
    }

    [OnSerializing]
    private void OnSerializing(StreamingContext context)
    {
        Debug.Assert(_material is not null && _material.Guid != Guid.Empty);

        _materialGuid = _material.Guid;

        _inputGuids.Clear();
        _inputNames.Clear();

        foreach (var input in _inputs)
        {
            Debug.Assert(input.Asset is not null && input.Asset.Guid != Guid.Empty);

            _inputGuids.Add(input.Asset.Guid);
            _inputNames.Add(input.Name);
        }
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        Debug.Assert(Type == AssetType.MATERIAL);
        Debug.Assert(_materialGuid != Guid.Empty);

        var assetInfo = AssetRegistry.GetAssetInfo(_materialGuid) ?? Material.Default;

        Debug.Assert(assetInfo is not null && assetInfo.Type == AssetType.MATERIAL);

        LoadMaterial(assetInfo);

        _inputs = [];
        Inputs = new(_inputs);

        for (int i = 0; i < _inputGuids.Count; ++i)
        {
            var inputAssetInfo = AssetRegistry.GetAssetInfo(_inputGuids[i]) ?? Texture.Default;

            Debug.Assert(inputAssetInfo is not null && inputAssetInfo.Guid == _inputGuids[i]);

            _inputs.Add(new(new(_inputNames[i]), inputAssetInfo));
        }

        Icon = _material.Icon;
        _shaderIds = [];
        _packedData = [];
        _previousPackedData = [];
        _materialGuid = Guid.Empty;

        _inputGuids.Clear();
        _inputNames.Clear();
    }

    private void UploadShaders()
    {
        _shaderIds.Clear();

        foreach (var shaderType in Enum.GetValues<ShaderType>())
        {
            var shaderGroup = _material.GetShaderGroup(shaderType);

            _shaderIds.Add(shaderGroup?.UploadToEngine() ?? Id.InvalidId);
        }

        Debug.Assert(_material is not null);

        if (_loadedMaterials.TryGetValue(_material.Guid, out var loadedMaterial))
        {
            ++loadedMaterial.ReferenceCount;
        }
        else
        {
            _loadedMaterials.Add(_material.Guid, new()
            {
                ReferenceCount = 1,
                Material = _material
            });
        }
    }

    private void UnloadShaders()
    {
        foreach (var shaderType in Enum.GetValues<ShaderType>())
        {
            _material.GetShaderGroup(shaderType)?.UnloadFromEngine();
        }

        _shaderIds.Clear();

        Debug.Assert(_loadedMaterials.ContainsKey(_material.Guid));

        if (_loadedMaterials.TryGetValue(_material.Guid, out var loadedMaterial))
        {
            Debug.Assert(loadedMaterial.ReferenceCount > 0);

            --loadedMaterial.ReferenceCount;

            if (loadedMaterial.ReferenceCount == 0) _loadedMaterials.Remove(_material.Guid);
        }
    }

    private void LoadMaterial(AssetInfo materialAssetInfo)
    {
        Debug.Assert(_material is null);
        Debug.Assert(materialAssetInfo is not null && materialAssetInfo.Guid != Guid.Empty);

        if (_loadedMaterials.TryGetValue(materialAssetInfo.Guid, out var loadedMaterial))
        {
            _material = loadedMaterial.Material;
        }
        else
        {
            _material = new(materialAssetInfo);
            _loadedMaterials.Add(_material.Guid, new()
            {
                ReferenceCount = 0,
                Material = _material
            });
        }
    }
}
