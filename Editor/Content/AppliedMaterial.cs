using Editor.Common.Enums;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace Editor.Content
{
    public class AppliedMaterial : Asset
    {
        private static readonly Dictionary<string, UploadedAsset> _packedMaterials = [];
        private static readonly Dictionary<IdType, string> _packedMaterialIds = [];
        private static readonly Dictionary<Guid, RefCountMaterial> _loadedMaterials = [];

        private readonly Material _material;
        private readonly ObservableCollection<MaterialInputAsset> _inputs = [];
        private readonly List<IdType> _shaderIds = [];

        private byte[] _packedData = [];
        private byte[] _previousPackedData = [];

        public ReadOnlyObservableCollection<MaterialInputAsset> Inputs;
        public MaterialSurface MaterialSurface { get; init; }
        public UploadedAsset UploadedAsset { get; private set; }

        public AppliedMaterial(AssetInfo materialAssetInfo) : base(AssetType.MATERIAL)
        {
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
                    Material = _material,
                });
            }

            Debug.Assert(_material is not null);

            MaterialSurface = _material.MaterialSurface.Clone();

            _material.GetInput().ForEach(x => _inputs.Add(new(x)));

            Icon = _material.Icon;
            Inputs = new(_inputs);
        }

        public override List<AssetInfo> GetReferencedAssets() =>
            [.. Inputs.Where(x => x.Asset is not null && x.Asset.Guid != Guid.Empty).Select(x => x.Asset)];

        public override AssetMetadata GetMetadata() => new MaterialMetadata()
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
        public override byte[] PackForEngine()
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

        public AppliedMaterial Clone()
        {
            var clone = new AppliedMaterial(_material.GetAssetInfo())
            {
                MaterialSurface = MaterialSurface.Clone(),
                Icon = Icon,
            };

            return clone;
        }

        public bool UploadToEngine()
        {
            UploadShaders();

            _packedData = PackForEngine();

            if (_packedData is null)
            {
                UnloadShaders();

                return false;
            }

            if (_packedData.SequenceEqual(_previousPackedData))
            {
                Debug.Assert(UploadedAsset is not null && UploadedAsset.GetContentId(Guid) == UploadedAsset.ContentId);

                UnloadShaders();

                return true;
            }

            var dataString = Convert.ToBase64String(_packedData);

            if (_packedMaterials.TryGetValue(dataString, out var uploadedAsset))
            {
                Debug.Assert(_packedMaterialIds.ContainsKey(uploadedAsset.ContentId));
                Debug.Assert((uploadedAsset.Metadata as MaterialMetadata).PackedData.SequenceEqual(_packedData));

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

                Debug.Assert(!_packedMaterialIds.ContainsKey(uploadedAsset.ContentId));

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

        public void UnloadFromEngine()
        {
            Debug.Assert(UploadedAsset is not null && _packedMaterialIds.ContainsKey(UploadedAsset.ContentId));
            Debug.Assert(UploadedAsset.GetContentId(UploadedAsset.AssetInfo.Guid) == UploadedAsset.ContentId);

            if (
                _packedMaterialIds.TryGetValue(UploadedAsset.ContentId, out var dataString) &&
                _packedMaterials.TryGetValue(dataString, out var uploadedAsset)
            )
            {
                Debug.Assert(UploadedAsset == uploadedAsset);

                UploadedAsset.RemoveFromScene(uploadedAsset);
                UnloadShaders();

                if (UploadedAsset.ReferenceCount == 0)
                {
                    _packedMaterialIds.Remove(UploadedAsset.ContentId);
                    _packedMaterials.Remove(dataString);

                    _previousPackedData = [];
                    UploadedAsset = null;
                }
            }
        }

        private void UploadShaders()
        {
            _shaderIds.Clear();

            foreach (var shaderType in Enum.GetValues<ShaderType>())
            {
                var shaderGroup = _material.GetShaderGroup(shaderType);

                _shaderIds.Add(shaderGroup is not null ? shaderGroup.UploadToEngine() : Id.InvalidId);
            }

            Debug.Assert(_loadedMaterials.ContainsKey(_material.Guid));

            if (_loadedMaterials.TryGetValue(_material.Guid, out var loadedMaterial))
            {
                ++loadedMaterial.ReferenceCount;
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
    }
}
