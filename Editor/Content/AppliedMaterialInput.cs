using Editor.Utilities;
using System.Diagnostics;

namespace Editor.Content
{
    public class AppliedMaterialInput : MaterialInput
    {
        private AssetInfo _asset = Texture.Default;
        private UploadedAsset _uploadedAsset;

        public AssetInfo Asset
        {
            get => _asset;
            private set
            {
                if (_asset != value)
                {
                    _asset = value;
                    OnPropertyChanged(nameof(Asset));
                }
            }
        }

        public AppliedMaterialInput(MaterialInput input, AssetInfo asset = null) : base(input.Name)
        {
            Debug.Assert(!(asset is not null && asset.Guid == Guid.Empty));

            SetInputAsset(asset ?? _asset);
        }

        public void SetInputAsset(AssetInfo asset)
        {
            Debug.Assert(asset is not null && asset.Guid != Guid.Empty);

            if (asset is not null && asset.Guid != Guid.Empty)
            {
                Unload();

                Asset = asset;

                Load();
            }
        }

        public void Load()
        {
            if (_uploadedAsset is null)
            {
                _uploadedAsset = UploadedAsset.AddToScene(Asset);

                Debug.Assert(_uploadedAsset is not null && Id.IsValid(_uploadedAsset.ContentId));
            }
        }

        public void Unload()
        {
            if (_uploadedAsset is not null)
            {
                Debug.Assert(UploadedAsset.GetContentId(Asset.Guid) == _uploadedAsset.ContentId);

                UploadedAsset.RemoveFromScene(_uploadedAsset);

                _uploadedAsset = null;
            }
        }
    }
}
