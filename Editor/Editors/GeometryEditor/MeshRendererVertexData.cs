using Editor.Common;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Editor.Editors.GeometryEditor
{
    // This is a temporary class.
    class MeshRendererVertexData : ViewModelBase 
    {
        private Brush _specular = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Constants.DefaultMaterialSpecularColor));
        private Brush _diffuse = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Constants.DefaultMaterialDiffuseColor));

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

        private Brush Diffuse
        {
            get => _diffuse;
            set
            {
                if (_diffuse != value)
                {
                    _diffuse = value;
                    OnPropertyChanged(nameof(Diffuse));
                }
            }
        }
    }
}
