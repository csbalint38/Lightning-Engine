using System.IO;

namespace Editor.Content
{
    public interface IAssetImportSettings
    {
        static void CopyImportSettings(IAssetImportSettings from, IAssetImportSettings to)
        {
            if (from is null || to is null)
            {
                throw new ArgumentNullException("Arguments should not be null");
            }
            else if (from.GetType() != to.GetType())
            {
                throw new ArgumentException("Arguments should be of the same type.");
            }

            using BinaryWriter writer = new(new MemoryStream());

            from.ToBinary(writer);
            writer.Flush();

            var bytes = (writer.BaseStream as MemoryStream).ToArray();

            using BinaryReader reader = new(new MemoryStream(bytes));

            to.FromBinary(reader);
        }

        void ToBinary(BinaryWriter writer);
        void FromBinary(BinaryReader reader);
    }
}
