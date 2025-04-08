using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Editor.Utilities
{
    public static class ContentHelper
    {
        public static string SanitizeFileName(string name)
        {
            var path = new StringBuilder(name.Substring(0, name.LastIndexOf(Path.DirectorySeparatorChar) + 1));
            var file = new StringBuilder(name[(name.LastIndexOf(Path.DirectorySeparatorChar) + 1)..]);

            foreach (var c in Path.GetInvalidPathChars()) path.Replace(c, '_');
            foreach (var c in Path.GetInvalidFileNameChars()) file.Replace(c, '_');

            return path.Append(file).ToString();
        }

        public static byte[] ComputeHash(byte[] data, int offset = 0, int count = 0)
        {
            if(data.Length > 0)
            {

                return SHA256.HashData(data.AsSpan(offset, count > 0 ? count : data.Length));
            }

            return null;
        }

        public static bool IsDirectory(string path)
        {
            try
            {
                return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return false;
        }
    }
}
