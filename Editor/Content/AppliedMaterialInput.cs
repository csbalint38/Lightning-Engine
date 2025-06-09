namespace Editor.Content
{
    public class AppliedMaterialInput(MaterialInput input) : MaterialInput(input.Name)
    {
        private AssetInfo? _asset;

        public AssetInfo? Asset
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
    }
}
