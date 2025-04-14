using Editor.Common.Enums;
using Editor.Content;

namespace Editor.Editors
{
    interface IAssetEditor
    {
        AssetEditorState State { get; }
        Guid AssetGuid { get; }
        Asset Asset { get; }

        void SetAssetAsync(AssetInfo info);
    }
}
