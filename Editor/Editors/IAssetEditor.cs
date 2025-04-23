using Editor.Common.Enums;
using Editor.Content;

namespace Editor.Editors
{
    interface IAssetEditor
    {
        AssetEditorState State { get; }
        Guid AssetGuid { get; }
        Asset Asset { get; }

        Task SetAssetAsync(AssetInfo info);
        bool CheckAssetGuid(Guid guid);
    }
}
