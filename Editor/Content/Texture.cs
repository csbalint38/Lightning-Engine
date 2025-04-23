using Editor.Common.Enums;
using Editor.Content.ContentBrowser;
using Editor.DLLs;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;

namespace Editor.Content
{
    public class Texture : Asset
    {
        private SliceArray3D _slices;
        private int _width;
        private int _height;
        private int _arraySize;
        private TextureFlags _flags;
        private int _mipLevels;
        private DXGIFormat _format;

        public static int MaxMipLevels => 14;
        public static int MaxArraySize => 2048;
        public static int Max3DSize => 2048;

        public TextureImportSettings ImportSettings { get; } = new();

        public SliceArray3D Slices
        {
            get => _slices;
            private set
            {
                if (_slices != value)
                {
                    _slices = value;
                    OnPropertyChanged(nameof(Slices));
                }
            }
        }

        public int Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        public int ArraySize
        {
            get => _arraySize;
            set
            {
                if (_arraySize != value)
                {
                    Debug.Assert(!(IsCubeMap && (value % 6) != 0));
                    _arraySize = value;
                    OnPropertyChanged(nameof(ArraySize));
                }
            }
        }

        public TextureFlags Flags
        {
            get => _flags;
            set
            {
                if (_flags != value)
                {
                    _flags = value;
                    OnPropertyChanged(nameof(IsHDR));
                    OnPropertyChanged(nameof(HasAlpha));
                    OnPropertyChanged(nameof(IsPremultipliedAlpha));
                    OnPropertyChanged(nameof(IsNormalMap));
                    OnPropertyChanged(nameof(IsCubeMap));
                    OnPropertyChanged(nameof(IsVolumeMap));
                    OnPropertyChanged(nameof(IsSRGB));
                }
            }
        }

        public int MipLevels
        {
            get => _mipLevels;
            set
            {
                if (_mipLevels != value)
                {
                    Debug.Assert(value >= 1 && value <= MaxMipLevels);
                    _mipLevels = value;
                    OnPropertyChanged(nameof(MipLevels));
                }
            }
        }

        public DXGIFormat Format
        {
            get => _format;
            set
            {
                if (_format != value)
                {
                    _format = value;
                    OnPropertyChanged(nameof(Format));
                    OnPropertyChanged(nameof(FormatName));
                }
            }
        }

        public bool IsHDR => Flags.HasFlag(TextureFlags.IS_HDR);
        public bool HasAlpha => Flags.HasFlag(TextureFlags.HAS_ALPHA);
        public bool IsPremultipliedAlpha => Flags.HasFlag(TextureFlags.IS_PREMULTIPLIED_ALPHA);
        public bool IsNormalMap => Flags.HasFlag(TextureFlags.IS_IMPORTED_AS_NORMAL_MAP);
        public bool IsCubeMap => Flags.HasFlag(TextureFlags.IS_CUBE_MAP);
        public bool IsVolumeMap => Flags.HasFlag(TextureFlags.IS_VOLUME_MAP);
        public string FormatName => ImportSettings.Compress ? ((BCFormat)Format).GetDescription() : Format.GetDescription();
        public bool IsSRGB => Flags.HasFlag(TextureFlags.IS_SRGB);

        public Texture() : base(AssetType.TEXTURE) { }

        public Texture(IAssetImportSettings importSettings) : this() {
            Debug.Assert(importSettings is TextureImportSettings);

            ImportSettings = (TextureImportSettings)importSettings;
        }

        public override bool Import(string file)
        {
            Debug.Assert(File.Exists(file));

            try
            {
                Logger.LogAsync(LogLevel.INFO, $"Importing image file {file}");

                (var slices, var icon) = ContentToolsAPI.Import(this);

                Debug.Assert(slices.Any() && slices.First().Any() && slices.First().First().Any());

                if (slices.Any() && slices.First().Any() && slices.First().First().Any()) Slices = slices;
                else return false;

                var firstMip = Slices[0][0][0];

                if(!HasValidDimensions(firstMip.Width, firstMip.Height, ArraySize, IsVolumeMap, file)) return false;

                if (icon is null)
                {
                    Debug.Assert(!ImportSettings.Compress);

                    icon = firstMip;
                }

                Icon = BitmapHelper.CreateThumbnail(
                    BitmapHelper.ImageFromSlice(icon, IsNormalMap),
                    ContentInfo.IconWidth,
                    ContentInfo.IconWidth
                );

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                var msg = $"Failed to read {file} for import";
                Debug.WriteLine(msg);
                Logger.LogAsync(LogLevel.ERROR, msg);
            }

            return false;
        }

        public override bool Load(string file)
        {
            Debug.Assert(File.Exists(file));
            Debug.Assert(Path.GetExtension(file).ToLower() == AssetFileExtension);

            try
            {
                using var reader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read));

                ReadAssetFileHeader(reader);
                ImportSettings.FromBinary(reader);

                Width = reader.ReadInt32();
                Height = reader.ReadInt32();
                ArraySize = reader.ReadInt32();
                Flags = (TextureFlags)reader.ReadInt32();
                MipLevels = reader.ReadInt32();
                Format = (DXGIFormat)reader.ReadInt32();

                var compressedLength = reader.ReadInt32();

                Debug.Assert(compressedLength > 0);

                var compressed = reader.ReadBytes(compressedLength);

                DecompressContent(compressed);
                HasValidDimensions(Width, Height, ArraySize, IsVolumeMap, file);

                FullPath = file;

                // TEMP
                PackForEngine();
                // TEMP

                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.LogAsync(LogLevel.ERROR, $"Failed to load texture asset from file {file}");
            }

            return false;
        }

        /// <summary>
        /// Pack the texture into a byte array wich can be used by the Engine.
        /// </summary>
        /// <returns>
        /// struct {
        ///     u32 width,
        ///     u32 height,
        ///     u32 array_size_or_depth,
        ///     u32 flags,
        ///     u32 mip_levels,
        ///     u32 format,
        ///     
        ///     struct {
        ///         u32 row_pitch,
        ///         u32 slice_pitch,
        ///         u8 image[mip_level][slice_pitch * depth_per_mip]
        ///     } Images[]
        /// } Texture
        /// </returns>
        public override byte[] PackForEngine()
        {
            using var writer = new BinaryWriter(new MemoryStream());

            writer.Write(Width);
            writer.Write(Height);
            writer.Write(ArraySize);
            writer.Write((int)Flags);
            writer.Write(MipLevels);
            writer.Write((int)Format);

            Debug.Assert(Slices?.Any() == true);

            foreach(var arraySlice in Slices)
            {
                foreach(var mipLevel in arraySlice)
                {
                    writer.Write(mipLevel[0].RowPitch);
                    writer.Write(mipLevel[0].SlicePitch);
                    foreach (var slice in mipLevel)
                    {
                        writer.Write(slice.RawContent);
                    }
                }
            }

            writer.Flush();

            var data = (writer.BaseStream as MemoryStream)?.ToArray();

            Debug.Assert(data?.Length > 0);

            // TEMP
            using (var fs = new FileStream(@"..\..\x64\texture.tex", FileMode.Create))
            {
                fs.Write(data, 0, data.Length);
            }
            // TEMP

            return data;
        }

        public override IEnumerable<string> Save(string file)
        {
            try
            {
                if(TryGetAssetInfo(file) is AssetInfo info && info.Type == Type) Guid = info.Guid;

                var compressed = CompressContent();

                Debug.Assert(compressed?.Length > 0);

                Hash = ContentHelper.ComputeHash(compressed);

                using var writer = new BinaryWriter(File.Open(file, FileMode.Create, FileAccess.Write));

                WriteAssetFileHeader(writer);
                ImportSettings.ToBinary(writer);

                writer.Write(Width);
                writer.Write(Height);
                writer.Write(ArraySize);
                writer.Write((int)Flags);
                writer.Write(MipLevels);
                writer.Write((int)Format);
                writer.Write(compressed.Length);
                writer.Write(compressed);

                FullPath = file;

                Logger.LogAsync(LogLevel.INFO, $"Saved texture to {file}");

                var savedFiles = new List<string>() { file };

                return savedFiles;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.LogAsync(LogLevel.ERROR, $"Failde to save texture to {file}");
            }

            return [];
        }

        private static bool HasValidDimensions(int width, int height, int arrayOrDepth, bool is3D, string file)
        {
            bool result = true;

            if (width > (1 << MaxMipLevels) || height > (1 << MaxMipLevels))
            {
                Logger.LogAsync(LogLevel.ERROR, $"Image dimension greater than {1 << MaxMipLevels}! (file: {file})");
                result = false;
            }

            if (width % 4 != 0 || height % 4 != 0)
            {
                Logger.LogAsync(LogLevel.ERROR, $"Image dimensions not a multiple of 4! (file: {file})");
                result = false;
            }

            if(is3D && (width > Max3DSize || height > Max3DSize || arrayOrDepth > Max3DSize))
            {
                Logger.LogAsync(LogLevel.ERROR, $"3D texture dimension greater than {Max3DSize}! (file: {file})");
                result = false;
            }
            else if(arrayOrDepth > MaxArraySize)
            {
                Logger.LogAsync(LogLevel.ERROR, $"3D texture dimension greater than {MaxArraySize}! (file: {file})");
                result = false;
            }

            if (width != height)
            {
                Logger.LogAsync(LogLevel.WARNING, $"Non-square image (width and height not equal)! (file: {file})");
            }

            if (!MathUtilities.IsPowOf2(width) || !MathUtilities.IsPowOf2(height))
            {
                Logger.LogAsync(LogLevel.WARNING, $"Image dimensions not a power of 2! (file: {file})");
            }

            return result;
        }

        private byte[] CompressContent()
        {
            Debug.Assert(Slices.First().Any() && Slices.First().Count == MipLevels);

            var data = ContentToolsAPI.SlicesToBinary(Slices);

            Debug.Assert(data?.Length > 0);

            return CompressionHelper.Compress(data);
        }

        private void DecompressContent(byte[] compressed)
        {
            var decompressed = CompressionHelper.Decompress(compressed);

            Slices = ContentToolsAPI.SlicesFromBinary(decompressed, ArraySize, MipLevels, IsVolumeMap);
        }
    }
}
