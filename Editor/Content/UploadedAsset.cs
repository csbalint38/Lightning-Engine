using Editor.Common.Enums;
using Editor.DLLs;
using Editor.Utilities;
using System.Diagnostics;

namespace Editor.Content;

public class UploadedAsset
{
    private static readonly Lock _lock = new();
    private static readonly Dictionary<Guid, UploadedAsset> _uploadedAssets = [];

    private List<UploadedAsset> _referencedAssets = [];

    public IdType ContentId { get; private set; } = Id.InvalidId;
    public int ReferenceCount { get; private set; }
    public AssetInfo? AssetInfo { get; private set; }
    public AssetMetadata? Metadata { get; private set; }

    private UploadedAsset() { }

    public static UploadedAsset? AddToScene(AssetInfo assetInfo, Asset? asset = null)
    {
        Debug.Assert(assetInfo is not null && assetInfo.Guid != Guid.Empty);

        var key = assetInfo.Guid;

        lock (_lock)
        {
            if (_uploadedAssets.TryGetValue(key, out var value))
            {
                ++value.ReferenceCount;
                value._referencedAssets.ForEach(x => ++x.ReferenceCount);
            }
            else
            {
                var uploadedAsset = UploadAssetToEngine(assetInfo, asset) ?? new();

                Debug.Assert(Id.IsValid(uploadedAsset.ContentId));

                if (Id.IsValid(uploadedAsset.ContentId)) _uploadedAssets[key] = uploadedAsset;
                else
                {
                    Logger.LogAsync(LogLevel.ERROR, $"Failed to upload asset {assetInfo.FileName} to engine.");

                    return null;
                }
            }
        }

        Debug.Assert(_uploadedAssets.ContainsKey(key));

        return _uploadedAssets[key];
    }

    public static void RemoveFromScene(UploadedAsset uploadedAsset)
    {
        lock (_lock)
        {

            Debug.Assert(uploadedAsset is not null && _uploadedAssets.ContainsKey(uploadedAsset.AssetInfo!.Guid));

            uploadedAsset._referencedAssets.ForEach(RemoveFromScene);
            --uploadedAsset.ReferenceCount;

            if (uploadedAsset.ReferenceCount == 0)
            {
                UnloadAssetFromEngine(uploadedAsset);
                _uploadedAssets.Remove(uploadedAsset.AssetInfo.Guid);
                uploadedAsset.ContentId = Id.InvalidId;
            }
        }
    }

    public static IdType GetContentId(Guid id)
    {
        Debug.Assert(id != Guid.Empty);

        lock (_lock)
        {
            return _uploadedAssets.TryGetValue(id, out var uploadedAsset) ? uploadedAsset.ContentId : Id.InvalidId;
        }
    }

    private static UploadedAsset? UploadAssetToEngine(AssetInfo assetInfo, Asset? asset = null)
    {
        Debug.Assert(assetInfo is not null);

        asset ??= assetInfo.Type switch
        {
            AssetType.ANIMATION => null,
            AssetType.AUDIO => null,
            AssetType.MATERIAL => null,
            AssetType.MESH => new Geometry(assetInfo),
            AssetType.SKELETON => null,
            AssetType.TEXTURE => new Texture(assetInfo),
            _ => null
        };

        Debug.Assert(asset is not null);

        if (asset is not null)
        {
            Debug.Assert(asset.Guid == assetInfo.Guid);

            var referencedAssets = new List<UploadedAsset>();

            asset.GetReferencedAssets().ForEach(x => referencedAssets.Add(AddToScene(x)!));

            var data = asset.PackForEngine();

            if (data?.Length > 0)
            {
                return new()
                {
                    AssetInfo = assetInfo,
                    Metadata = asset.GetMetadata(),
                    ContentId = EngineAPI.CreateResource(data, assetInfo.Type),
                    ReferenceCount = 1,
                    _referencedAssets = referencedAssets
                };
            }
        }

        return null;
    }

    private static void UnloadAssetFromEngine(UploadedAsset uploadedAsset)
    {
        Debug.Assert(uploadedAsset?.AssetInfo is not null && Id.IsValid(uploadedAsset.ContentId));

        EngineAPI.DestroyResource(uploadedAsset.ContentId, (int)uploadedAsset.AssetInfo.Type);
    }
}
