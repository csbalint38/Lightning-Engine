using Editor.Common;
using Editor.Utilities;
using System.IO;

namespace Editor.Content
{
    public class GeometryImportSettings : ViewModelBase, IAssetImportSettings
    {
        private bool _calculateNormals;
        private bool _calculateTangents;
        private float _smoothingAngle;
        private bool _reverseHandedness;
        private bool _importEmbeddedTextures;
        private bool _importAnimations;
        private bool _coalesceMeshes;

        public bool CalculateNormals
        {
            get => _calculateNormals;
            set
            {
                if (_calculateNormals != value)
                {
                    _calculateNormals = value;
                    OnPropertyChanged(nameof(CalculateNormals));
                }
            }
        }

        public bool CalculateTangents
        {
            get => _calculateTangents;
            set
            {
                if (_calculateTangents != value)
                {
                    _calculateTangents = value;
                    OnPropertyChanged(nameof(CalculateTangents));
                }
            }
        }

        public float SmoothingAngle
        {
            get => _smoothingAngle;
            set
            {
                if (!_smoothingAngle.IsEqual(value))
                {
                    _smoothingAngle = value;
                    OnPropertyChanged(nameof(SmoothingAngle));
                }
            }
        }

        public bool ReverseHandedness
        {
            get => _reverseHandedness;
            set
            {
                if (_reverseHandedness != value)
                {
                    _reverseHandedness = value;
                    OnPropertyChanged(nameof(ReverseHandedness));
                }
            }
        }

        public bool ImportEmbeddedTextures
        {
            get => _importEmbeddedTextures;
            set
            {
                if (_importEmbeddedTextures != value)
                {
                    _importEmbeddedTextures = value;
                    OnPropertyChanged(nameof(ImportEmbeddedTextures));
                }
            }
        }

        public bool ImportAnimations
        {
            get => _importAnimations;
            set
            {
                if (_importAnimations != value)
                {
                    _importAnimations = value;
                    OnPropertyChanged(nameof(ImportAnimations));
                }
            }
        }

        public bool CoalesceMeshes
        {
            get => _coalesceMeshes;
            set
            {
                if (_coalesceMeshes != value)
                {
                    _coalesceMeshes = value;
                    OnPropertyChanged(nameof(CoalesceMeshes));
                }
            }
        }

        public GeometryImportSettings()
        {
            CalculateNormals = false;
            CalculateTangents = true;
            SmoothingAngle = 178f;
            ReverseHandedness = false;
            ImportEmbeddedTextures = true;
            ImportAnimations = true;
            CoalesceMeshes = false;
        }

        public void ToBinary(BinaryWriter writer)
        {
            writer.Write(CalculateNormals);
            writer.Write(CalculateTangents);
            writer.Write(SmoothingAngle);
            writer.Write(ReverseHandedness);
            writer.Write(ImportEmbeddedTextures);
            writer.Write(ImportAnimations);
            writer.Write(CoalesceMeshes);
        }

        public void FromBinary(BinaryReader reader)
        {
            CalculateNormals = reader.ReadBoolean();
            CalculateTangents = reader.ReadBoolean();
            SmoothingAngle = reader.ReadSingle();
            ReverseHandedness = reader.ReadBoolean();
            ImportEmbeddedTextures = reader.ReadBoolean();
            ImportAnimations = reader.ReadBoolean();
            CoalesceMeshes = reader.ReadBoolean();
        }
    }
}
