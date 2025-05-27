using Editor.Common;
using Editor.Utilities;
using System.IO;
using System.Windows.Media;

namespace Editor.Content
{
    public class MaterialSurface : ViewModelBase
    {
        private Color _baseColor = Color.FromScRgb(1f, .7f, .7f, .7f);
        private Color _emissiveColor = Color.FromScRgb(1f, 0f, 0f, 0f);
        private float _metallic = 0f;
        private float _roughness = .9f;
        private float _emissiveIntensity = 1f;

        public Color BaseColor
        {
            get => _baseColor;
            set
            {
                if (_baseColor != value)
                {
                    _baseColor = value;
                    OnPropertyChanged(nameof(BaseColor));
                }
            }
        }

        public Color EmissiveColor
        {
            get => _emissiveColor;
            set
            {
                if (_emissiveColor != value)
                {
                    _emissiveColor = value;
                    OnPropertyChanged(nameof(EmissiveColor));
                }
            }
        }

        public float Metallic
        {
            get => _metallic;
            set
            {
                if (_metallic.IsEqual(value))
                {
                    _metallic = value;
                    OnPropertyChanged(nameof(Metallic));
                }
            }
        }

        public float Roughness
        {
            get => _roughness;
            set
            {
                if (_roughness.IsEqual(value))
                {
                    _roughness = value;
                    OnPropertyChanged(nameof(Roughness));
                }
            }
        }

        public float EmissiveIntensity
        {
            get => _emissiveIntensity;
            set
            {
                if (_emissiveIntensity.IsEqual(value))
                {
                    _emissiveIntensity = value;
                    OnPropertyChanged(nameof(EmissiveIntensity));
                }
            }
        }

        public void FromBinary(BinaryReader reader)
        {
            _baseColor.ScR = reader.ReadSingle();
            _baseColor.ScG = reader.ReadSingle();
            _baseColor.ScB = reader.ReadSingle();
            _baseColor.ScA = reader.ReadSingle();
            _emissiveColor.ScR = reader.ReadSingle();
            _emissiveColor.ScG = reader.ReadSingle();
            _emissiveColor.ScB = reader.ReadSingle();
            _emissiveIntensity = reader.ReadSingle();
            _metallic = reader.ReadSingle();
            _roughness = reader.ReadSingle();
        }

        public void ToBinary(BinaryWriter writer)
        {
            writer.Write(_baseColor.ScR);
            writer.Write(_baseColor.ScG);
            writer.Write(_baseColor.ScB);
            writer.Write(_baseColor.ScA);
            writer.Write(_emissiveColor.ScR);
            writer.Write(_emissiveColor.ScG);
            writer.Write(_emissiveColor.ScB);
            writer.Write(_emissiveIntensity);
            writer.Write(_metallic);
            writer.Write(_roughness);
        }

        public MaterialSurface Clone() => new()
        {
            BaseColor = BaseColor,
            EmissiveColor = EmissiveColor,
            EmissiveIntensity = EmissiveIntensity,
            Metallic = Metallic,
            Roughness = Roughness,
        };
    }
}
