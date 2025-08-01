using Editor.Common;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Editor.Editors
{
    // This is a temporary class.
    public class MeshRendererVertexData : ViewModelBase
    {
        private Brush _specular = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(Constants.DefaultMaterialSpecularColor)
        );

        private Brush _diffuse = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(Constants.DefaultMaterialDiffuseColor)
        );

        private bool _isHighlighted;
        private bool _isIsolated;

        public Point3DCollection Positions { get; } = [];
        public Vector3DCollection Normals { get; } = [];
        public PointCollection UVs { get; } = [];
        public Int32Collection Indices { get; } = [];

        public Brush Specular
        {
            get => _specular;
            set
            {
                if (_specular != value)
                {
                    _specular = value;
                    OnPropertyChanged(nameof(Specular));
                }
            }
        }

        public Brush Diffuse
        {
            get => _isHighlighted ? Brushes.Orange : _diffuse;
            set
            {
                if (_diffuse != value)
                {
                    _diffuse = value;
                    OnPropertyChanged(nameof(Diffuse));
                }
            }
        }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (_isHighlighted != value)
                {
                    _isHighlighted = value;
                    OnPropertyChanged(nameof(IsHighlighted));
                    OnPropertyChanged(nameof(Diffuse));
                }
            }
        }

        public bool IsIsolated
        {
            get => _isIsolated;
            set
            {
                if (_isIsolated != value)
                {
                    _isIsolated = value;
                    OnPropertyChanged(nameof(IsIsolated));
                }
            }
        }

        public string Name { get; set; }
    }
}
