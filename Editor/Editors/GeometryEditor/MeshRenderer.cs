using Editor.Common;
using Editor.Content;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Editor.Editors.GeometryEditor
{
    // This is also temporary.
    class MeshRenderer : ViewModelBase
    {
        private Vector3 _cameraDirection = new(0, 0, -10);
        private Point3D _cameraPosition = new(0, 0, 10);
        private Point3D _cameraTarget = new(0, 0, 0);
        private Color _keyLight = (Color)ColorConverter.ConvertFromString("#FFAEAEAE");
        private Color _skyLight = (Color)ColorConverter.ConvertFromString("#FF111B30");
        private Color _groundLight = (Color)ColorConverter.ConvertFromString("#FF3F2F1E");
        private Color _ambientLight = (Color)ColorConverter.ConvertFromString("#FF3B3B3B");

        public ObservableCollection<MeshRendererVertexData> Meshes { get; } = [];

        public Vector3 CameraDirection
        {
            get => _cameraDirection;
            set
            {
                if (_cameraDirection != value)
                {
                    _cameraDirection = value;
                    OnPropertyChanged(nameof(CameraDirection));
                }
            }
        }

        public Point3D CameraPosition
        {
            get => _cameraPosition;
            set
            {
                if (_cameraPosition != value)
                {
                    _cameraPosition = value;
                    OnPropertyChanged(nameof(OffsetCameraPosition));
                    OnPropertyChanged(nameof(CameraPosition));
                }
            }
        }

        public Point3D CameraTraget
        {
            get => _cameraTarget;
            set
            {
                if (_cameraTarget != value)
                {
                    _cameraTarget = value;
                    OnPropertyChanged(nameof(OffsetCameraPosition));
                    OnPropertyChanged(nameof(CameraTraget));
                }
            }
        }

        public Color KeyLight
        {
            get => _keyLight;
            set
            {
                if (_keyLight != value)
                {
                    _keyLight = value;
                    OnPropertyChanged(nameof(KeyLight));
                }
            }
        }

        public Color SkyLight
        {
            get => _skyLight;
            set
            {
                if (_skyLight != value)
                {
                    _skyLight = value;
                    OnPropertyChanged(nameof(SkyLight));
                }
            }
        }

        public Color GroundLight
        {
            get => _groundLight;
            set
            {
                if (_groundLight != value)
                {
                    _groundLight = value;
                    OnPropertyChanged(nameof(GroundLight));
                }
            }
        }

        public Color AmbientLight
        {
            get => _ambientLight;
            set
            {
                if (_ambientLight != value)
                {
                    _ambientLight = value;
                    OnPropertyChanged(nameof(AmbientLight));
                }
            }
        }

        public MeshRenderer(MeshLOD lod, MeshRenderer old) 
        {
            Debug.Assert(lod?.Meshes.Any() == true);

            var offset = lod.Meshes[0].VertexSize - 3 * sizeof(float) - sizeof(int) - 2 * sizeof(short);
            double minX, minY, minZ;
            double maxX, maxY, maxZ;

            minX = minY = minZ = double.MaxValue;
            maxX = maxY = maxZ = double.MinValue;

            Vector3D avgNormal = new();
        }

        public Point3D OffsetCameraPosition =>
            new(CameraPosition.X + CameraTraget.X, CameraPosition.Y + CameraTraget.Y, CameraPosition.Z + CameraTraget.Z);
    }
}
