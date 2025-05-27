namespace Editor.Content
{
    public class MaterialInputAsset : MaterialInput
    {
        private AssetInfo _asset;

        public AssetInfo Asset
        {
            get => _asset;
            set
            {
                if (_asset != value)
                {
                    _asset = value;
                    OnPropertyChanged(nameof(Asset));
                }
            }
        }

        public MaterialInputAsset(MaterialInput input) : base(input.Name) { }
    }
}
