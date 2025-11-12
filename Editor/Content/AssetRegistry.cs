using Editor.Common.Enums;
using Editor.Content.ContentBrowser;
using Editor.Content.ContentBrowser.Descriptors;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace Editor.Content;
static class AssetRegistry
{
    private static readonly Lock _lock = new();
    private static readonly Dictionary<string, AssetInfo> _assetFileDict = [];
    private static readonly Dictionary<Guid, AssetInfo> _assetGuidDict = [];
    private static readonly ObservableCollection<AssetInfo> _assets = [];

    private static string _cachePath = string.Empty;

    public static ReadOnlyObservableCollection<AssetInfo> Assets { get; } = new(_assets);

    public static AssetInfo? GetAssetInfo(string file)
    {
        lock (_lock)
        {
            return _assetFileDict.TryGetValue(file, out var value) ? value : null;
        }
    }

    public static AssetInfo? GetAssetInfo(Guid guid)
    {
        lock (_lock)
        {
            return _assetGuidDict.TryGetValue(guid, out var value) ? value : null;
        }
    }

    public static void Reset(string contentFolder, string projectPath)
    {
        ContentWatcher.ContentModified -= OnContentModified;

        Debug.Assert(!string.IsNullOrEmpty(projectPath) && Directory.Exists(projectPath));

        _cachePath = $@"{projectPath}.Lightning\AssetInfoCache.lngcache";

        LoadCacheFile();

        Debug.Assert(!string.IsNullOrEmpty(contentFolder) && Directory.Exists(contentFolder));

        lock (_lock)
        {
            RegisterAllAssets(contentFolder);

            DefaultAssets.DefaultAssetsList.ForEach(static x => RegisterAsset(x?.FullPath!, x));
        }

        ContentWatcher.ContentModified += OnContentModified;
    }

    public static void Save() => SaveCacheFile();

    private static void RegisterAllAssets(string path)
    {
        Debug.Assert(Directory.Exists(path));

        foreach (var entry in Directory.GetFileSystemEntries(path))
        {
            if (ContentHelper.IsDirectory(entry)) RegisterAllAssets(entry);
            else RegisterAsset(entry);
        }
    }

    private static void RegisterAsset(string file, AssetInfo? info = null)
    {
        Debug.Assert(File.Exists(file));

        try
        {
            var fileInfo = new FileInfo(file);
            var isNew = !_assetFileDict.ContainsKey(file);

            if (isNew || _assetFileDict[file].RegisterTime.IsOlder(fileInfo.LastWriteTime))
            {
                info ??= Asset.GetAssetInfo(file);

                Debug.Assert(info is not null);

                info.RegisterTime = DateTime.Now;

                if (!isNew && _assetFileDict[file].Guid != info.Guid)
                {
                    _assetGuidDict.Remove(_assetFileDict[file].Guid);
                }

                _assetFileDict[file] = info;
                _assetGuidDict[info.Guid] = info;

                if (isNew)
                {
                    Debug.Assert(!_assets.Contains(info));

                    _assets.Add(info);
                }
                else
                {
                    var oldInfo = _assets.FirstOrDefault(x => x.FullPath == info.FullPath);

                    Debug.Assert(oldInfo is not null);

                    _assets[_assets.IndexOf(oldInfo)] = info;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    private static void OnContentModified(object? sender, ContentModifiedEventArgs e)
    {
        lock (_lock)
        {
            if (ContentHelper.IsDirectory(e.FullPath))
            {
                RegisterAllAssets(e.FullPath);
            }
            else if (File.Exists(e.FullPath))
            {
                RegisterAsset(e.FullPath);
            }

            _assets.Where(x => !File.Exists(x.FullPath)).ToList().ForEach(x => UnregisterAsset(x.FullPath!));
        }
    }

    private static void UnregisterAsset(string file)
    {
        if (_assetFileDict.TryGetValue(file, out var info))
        {
            _assets.Remove(info);
            _assetFileDict.Remove(file);

            if (_assetGuidDict.TryGetValue(info.Guid, out var value) && !File.Exists(value.FullPath))
            {
                _assetGuidDict.Remove(info.Guid);
            }
        }
    }

    private static void SaveCacheFile()
    {
        try
        {
            List<AssetInfo> assets = [];

            lock (_lock)
            {
                assets = [.. _assets];
            }

            using var writer = new BinaryWriter(File.Open(_cachePath, FileMode.Create, FileAccess.Write));

            writer.Write(assets.Count);

            foreach (var info in assets)
            {
                writer.Write((int)info.Type);
                writer.Write(info.Icon.Length);
                writer.Write(info.Icon);
                writer.Write(info.FullPath!);
                writer.Write(info.RegisterTime.ToBinary());
                writer.Write(info.ImportDate.ToBinary());
                writer.Write(info.Guid.ToString());

                var hashSize = info.Hash?.Length ?? 0;

                writer.Write(hashSize);

                if (hashSize > 0) writer.Write(info.Hash!);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Logger.LogAsync(LogLevel.WARNING, "Failed to write Asset Registry cache file.");
            File.Delete(_cachePath);
        }
    }

    private static void LoadCacheFile()
    {
        if (!File.Exists(_cachePath)) return;

        try
        {
            lock (_lock)
            {
                _assetFileDict.Clear();
                _assetGuidDict.Clear();
                _assets.Clear();

                using var reader = new BinaryReader(File.Open(_cachePath, FileMode.Open, FileAccess.Read));
                var numEntities = reader.ReadInt32();

                for (int i = 0; i < numEntities; i++)
                {
                    var info = new AssetInfo
                    {
                        Type = (AssetType)reader.ReadInt32()
                    };

                    var IconSize = reader.ReadInt32();

                    info.Icon = reader.ReadBytes(IconSize);
                    info.FullPath = reader.ReadString();
                    info.RegisterTime = DateTime.FromBinary(reader.ReadInt64());
                    info.ImportDate = DateTime.FromBinary(reader.ReadInt64());
                    info.Guid = new(reader.ReadString());

                    var hashSize = reader.ReadInt32();

                    info.Hash = (hashSize > 0) ? reader.ReadBytes(hashSize) : null;

                    if (File.Exists(info.FullPath))
                    {
                        _assetFileDict[info.FullPath] = info;
                        _assetGuidDict[info.Guid] = info;
                        _assets.Add(info);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Logger.LogAsync(LogLevel.WARNING, "Failed to read Asset Registry cache file.");
        }
    }
}
