using Editor.Common;
using Editor.Common.Enums;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.IO;

namespace Editor.Content
{
    public class TextureImportSettings : ViewModelBase, IAssetImportSettings
    {
        private TextureDimension _dimension = TextureDimension.TEXTURE_2D;
        private int _mipLevels;
        private float _alphaThreshold;
        private bool _preferBC7;
        private int _formatIndex;
        private bool _compress;
        private int _cubemapSize;
        private bool _mirrorCubemap;
        private bool _prefilterCubemap;

        public ObservableCollection<string> Sources { get; } = new();

        public TextureDimension Dimension
        {
            get => _dimension;
            set
            {
                if (_dimension != value)
                {
                    _dimension = value;
                    OnPropertyChanged(nameof(Dimension));
                }
            }
        }

        public int MipLevels
        {
            get => _mipLevels;
            set
            {
                value = Math.Clamp(value, 0, Texture.MaxMipLevels);
                if (_mipLevels != value)
                {
                    _mipLevels = value;
                    OnPropertyChanged(nameof(MipLevels));
                }
            }
        }

        public float AlphaThreshold
        {
            get => _alphaThreshold;
            set
            {
                value = Math.Clamp(value, 0f, 1f);
                if (!_alphaThreshold.IsEqual(value))
                {
                    _alphaThreshold = value;
                    OnPropertyChanged(nameof(AlphaThreshold));
                }
            }
        }

        public bool PreferBC7
        {
            get => _preferBC7;
            set
            {
                if (_preferBC7 != value)
                {
                    _preferBC7 = value;
                    OnPropertyChanged(nameof(PreferBC7));
                }
            }
        }

        public int FormatIndex
        {
            get => _formatIndex;
            set
            {
                value = Math.Clamp(value, 0, Enum.GetValues<BCFormat>().Length);
                if (_formatIndex != value)
                {
                    _formatIndex = value;
                    OnPropertyChanged(nameof(FormatIndex));
                    OnPropertyChanged(nameof(OutputFormat));
                }
            }
        }

        public bool Compress
        {
            get => _compress;
            set
            {
                if (_compress != value)
                {
                    _compress = value;
                    OnPropertyChanged(nameof(Compress));
                }
            }
        }

        public int CubemapSize
        {
            get => _cubemapSize;
            set
            {
                if (_cubemapSize != value)
                {
                    _cubemapSize = value;
                    OnPropertyChanged(nameof(CubemapSize));
                }
            }
        }

        public bool MirrorCubemap
        {
            get => _mirrorCubemap;
            set
            {
                if (_mirrorCubemap != value)
                {
                    _mirrorCubemap = value;
                    OnPropertyChanged(nameof(MirrorCubemap));
                }
            }
        }

        public bool PrefilterCubemap
        {
            get => _prefilterCubemap;
            set
            {
                if (_prefilterCubemap != value)
                {
                    _prefilterCubemap = value;
                    OnPropertyChanged(nameof(PrefilterCubemap));
                }
            }
        }

        public DXGIFormat OutputFormat =>
            Compress ? (DXGIFormat)Enum.GetValues<BCFormat>()[FormatIndex] : DXGIFormat.DXGI_FORMAT_UNKNOWN;

        public TextureImportSettings()
        {
            MipLevels = 0;
            AlphaThreshold = 0.5f;
            PreferBC7 = true;
            FormatIndex = 0;
            Compress = true;
            CubemapSize = 256;
            MirrorCubemap = true;
            PrefilterCubemap = true;
        }

        public void FromBinary(BinaryReader reader)
        {
            Sources.Clear();

            reader.ReadString().Split(';').Where(x => !string.IsNullOrEmpty(x)).ToList().ForEach(x => Sources.Add(x));

            Dimension = (TextureDimension)reader.ReadInt32();
            MipLevels = reader.ReadInt32();
            AlphaThreshold = reader.ReadSingle();
            PreferBC7 = reader.ReadBoolean();
            FormatIndex = reader.ReadInt32();
            Compress = reader.ReadBoolean();
            CubemapSize = reader.ReadInt32();
            MirrorCubemap = reader.ReadBoolean();
            PrefilterCubemap = reader.ReadBoolean();
        }

        public void ToBinary(BinaryWriter writer)
        {
            writer.Write(string.Join(';', Sources.ToArray()));
            writer.Write((int)Dimension);
            writer.Write(MipLevels);
            writer.Write(AlphaThreshold);
            writer.Write(PreferBC7);
            writer.Write(FormatIndex);
            writer.Write(Compress);
            writer.Write(CubemapSize);
            writer.Write(MirrorCubemap);
            writer.Write(PrefilterCubemap);
        }
    }
}
