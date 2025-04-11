using System.IO;

namespace Editor.Content
{
    public interface IAssetImportSettings
    {
        void ToBinary(BinaryWriter writer);
        void FromBinary(BinaryReader reader);
    }
}
