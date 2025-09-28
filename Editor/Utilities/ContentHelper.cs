using Editor.Common.Enums;
using Editor.Content;
using Editor.Content.ContentBrowser;
using Editor.Content.ImportSettingsConfig;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Editor.Utilities;

public static class ContentHelper
{
    public static string[] MeshFileExtensions { get; } = { ".fbx" };

    public static string[] ImageFileExtensions { get; } = {
        ".bmp",
        ".png",
        ".jpg",
        ".jpeg",
        ".tiff",
        ".tif",
        ".tga",
        ".dds",
        ".hdr"
    };

    public static string[] AudioFileExtensions { get; } = { ".ogg", ".waw" };

    internal static IEnumerable<string> SaveAsset(this Asset asset)
    {
        try
        {
            ContentWatcher.EnableFileWatcher(false);

            Debug.Assert(!string.IsNullOrEmpty(asset.FullPath));

            return asset.Save(asset.FullPath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save asset {asset.FullPath}");
            Debug.WriteLine(ex.Message);

            return new List<string>();
        }
        finally
        {
            ContentWatcher.EnableFileWatcher(true);
        }
    }

    public static string SanitizeFileName(string name)
    {
        Debug.Assert(!string.IsNullOrEmpty(name));

        var path = new StringBuilder(name.Substring(0, name.LastIndexOf(Path.DirectorySeparatorChar) + 1));
        var file = new StringBuilder(name[(name.LastIndexOf(Path.DirectorySeparatorChar) + 1)..]);

        foreach (var c in Path.GetInvalidPathChars()) path.Replace(c, '_');
        foreach (var c in Path.GetInvalidFileNameChars()) file.Replace(c, '_');

        return path.Append(file).ToString();
    }

    public static byte[]? ComputeHash(byte[] data, int offset = 0, int count = 0)
    {
        if (data.Length > 0)
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

    public static async Task<List<Asset>> ImportFilesAsync(IEnumerable<AssetProxy> proxies)
    {
        List<Asset> assets = new();

        try
        {
            ImportingItemCollection.Init();
            ContentWatcher.EnableFileWatcher(false);

            var tasks = proxies.Select(async proxy => await Task.Run(() =>
            {
                assets.Add(Import(proxy.FileInfo.FullName, proxy.ImportSettings, proxy.DestinationFolder)!);
            }));

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to import files.");
            Debug.WriteLine(ex.Message);
        }
        finally
        {
            ContentWatcher.EnableFileWatcher(true);

        }

        return assets;
    }

    public static Uri GetPackUri(string relativePath, Type type)
    {
        var assembyShortName = type.Assembly.ToString().Split(',')[0];
        var packUriString = $"pack://application:,,,/{assembyShortName};component/{relativePath}";

        return new Uri(packUriString);
    }

    private static Asset? Import(string file, IAssetImportSettings settings, string destination)
    {
        Debug.Assert(!string.IsNullOrEmpty(file));

        if (IsDirectory(file)) return null;
        if (!destination.EndsWith(Path.DirectorySeparatorChar)) destination += Path.DirectorySeparatorChar;

        var name = Path.GetFileNameWithoutExtension(file).ToLower();
        var ext = Path.GetExtension(file).ToLower();

        Asset? asset = ext switch
        {
            { } when MeshFileExtensions.Contains(ext) => new Geometry(settings),
            { } when ImageFileExtensions.Contains(ext) => new Texture(settings),
            { } when AudioFileExtensions.Contains(ext) => null,
            _ => null
        };

        if (asset is not null)
        {
            Import(asset, name, file, destination);
        }

        return asset;
    }

    private static void Import(Asset asset, string name, string file, string destination)
    {
        destination = destination.Trim();

        Debug.Assert(asset is not null);
        Debug.Assert(!string.IsNullOrEmpty(destination) && Directory.Exists(destination));

        if (!destination.EndsWith(Path.DirectorySeparatorChar)) destination += Path.DirectorySeparatorChar;

        asset.FullPath = destination + name + Asset.AssetFileExtension;

        var importingItem = new ImportingItem(name, asset);
        ImportingItemCollection.Add(importingItem);

        bool importSucceeded = false;

        try
        {
            Debug.Assert(asset.FullPath?.Contains(destination) == true);

            importSucceeded = !string.IsNullOrEmpty(file) && asset.Import(file);

            if (importSucceeded) asset.Save(asset.FullPath);

            return;
        }
        finally
        {
            importingItem.Status = importSucceeded ? ImportStatus.SUCCEEDED : ImportStatus.FAILED;
        }
    }
}
