using Editor.Content;

namespace Editor.Editors
{
    interface IAssetEditor
    {
        Asset Asset { get; }

        void SetAssetAsync(AssetInfo asset);
    }
}
