using System.IO;

namespace Editor.Utilities
{
    static class FileInfoExtension
    {
        public static bool IsDirectory(this FileInfo info) => info.Attributes.HasFlag(FileAttributes.Directory);
    }
}
