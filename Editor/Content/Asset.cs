using Editor.Common;
using Editor.Common.Enums;
using System.Diagnostics;

namespace Editor.Content
{
    abstract public class Asset : ViewModelBase
    {
        public AssetType Type { get; private set; }

        public Asset(AssetType type)
        {
            Debug.Assert(type != AssetType.UNKNOWN);

            Type = type;
        }
    }
}
