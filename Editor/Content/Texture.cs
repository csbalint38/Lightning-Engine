
using Editor.Common.Enums;
using Editor.Content.ContentBrowser;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;

namespace Editor.Content
{
    public class Texture : Asset
    {
        private List<List<List<Slice>>> _slices;
        private int _width;
        private int _height;
        private int _arraySize;
        private TextureFlags _flags;
        private int _mipLevels;
        private DXGIFormat _format;

        public const int MaxMipLevels = 14;

        public TextureImportSettings ImportSettings { get; } = new();

        public Texture() : base(AssetType.TEXTURE) { }

        public List<List<List<Slice>>> Slices
        {
            get => _slices;
            set
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
        public string FormatName => (ImportSettings.Compress) ? ((BCFormat)Format).GetDescription() : Format.GetDescription();

        public override bool Import(string file)
        {
            Debug.Assert(File.Exists(file));

            try
            {
                Logger.LogAsync(LogLevel.INFO, $"Importing image file {file}");

                List<List<List<Slice>>> slices = new();
                Slice icon = new();

                // Debug.Assert(slices.Any() && slices.First().Any() && slices.First().First().Any());

                if (slices.Any() && slices.First().Any() && slices.First().First().Any()) Slices = slices;
                else return false;

                var firstMip = Slices[0][0][0];

                HasValidDimensions(firstMip.Width, firstMip.Height, file);

                if(icon is null)
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
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                var msg = "Failed to read {file} for import";
                Debug.WriteLine(msg);
                Logger.LogAsync(LogLevel.ERROR, msg);
            }
        }

        public override bool Load(string file)
        {
            throw new NotImplementedException();
        }

        public override byte[] PackForEngine()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> Save(string file)
        {
            throw new NotImplementedException();
        }

        private static bool HasValidDimensions(int width, int height, string file)
        {
            bool result = true;

            if(width % 4 != 0 || height % 4 != 0)
            {
                Logger.LogAsync(LogLevel.WARNING, $"Image dimensions not a multiple of 4! (file: {file})");
                result = false;
            }

            if( width != height)
            {
                Logger.LogAsync(LogLevel.WARNING, $"Non-square image (width and height not equal)! (file: {file})");
                result = false;
            }

            if(!MathUtilities.IsPowOf2(width) || !MathUtilities.IsPowOf2(height))
            {
                Logger.LogAsync(LogLevel.WARNING, $"Image dimensions not a power of 2! (file: {file})");
                result = false;
            }

            return result;
        }
    }
}
