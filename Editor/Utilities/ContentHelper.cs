using Editor.Content;
using Editor.Content.ContentBrowser;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Editor.Utilities
{
    public static class ContentHelper
    {
        public static string[] MeshFileExtension { get; } = { ".fbx" };
        public static string[] ImageFileExtension { get; } = { ".bmp", ".png", ".jpg", ".jpeg", ".tiff", ".tif", ".tga", ".dds", ".hdr" };
        public static string[] AudioFileExtensions { get; } = { ".ogg", ".waw" };

        public static string SanitizeFileName(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

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

        public static async Task ImportFilesAsync(string[] files, string destination)
        {
            try
            {
                Debug.Assert(!string.IsNullOrEmpty(destination));

                ContentWatcher.EnableFileWatcher(false);

                var tasks = files.Select(async file => await Task.Run(() =>
                {
                    Import(file, destination);
                }));

                await Task.WhenAll(tasks);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Failed to import files to {destination}");
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                ContentWatcher.EnableFileWatcher(true);
            }
        }

        private static Asset Import(string file, string destination)
        {
            Debug.Assert(!string.IsNullOrEmpty(file));

            if (IsDirectory(file)) return null;
            if(!destination.EndsWith(Path.DirectorySeparatorChar)) destination += Path.DirectorySeparatorChar;

            var name = Path.GetFileNameWithoutExtension(file).ToLower();
            var ext = Path.GetExtension(file).ToLower();

            Asset asset = ext switch
            {
                { } when MeshFileExtension.Contains(ext) => new Geometry(),
                { } when ImageFileExtension.Contains(ext) => new Texture(),
                { } when AudioFileExtensions.Contains(ext) => null,
                _ => null
            };

            if(asset is not null)
            {
                Import(asset, name, file, destination);
            }

            return asset;
        }

        private static void Import(Asset asset, string name, string file, string destination)
        {
            destination = destination?.Trim();

            Debug.Assert(asset is not null);
            Debug.Assert(!string.IsNullOrEmpty(destination) && Directory.Exists(destination));

            if(!destination.EndsWith(Path.DirectorySeparatorChar)) destination += Path.DirectorySeparatorChar;

            asset.FullPath = destination + name + Asset.AssetFileExtension;

            bool importSucceeded = false;

            try
            {
                importSucceeded = !string.IsNullOrEmpty(file) && asset.Import(file);

                if (importSucceeded) asset.Save(asset.FullPath);

                return;
            }
            finally
            {

            }
        }
    }
}
