using Editor.Common;
using Editor.Common.Enums;
using Editor.Content;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Editor.Editors
{
    // This is also temporary.
    public class MeshRenderer : ViewModelBase
    {
        private Vector3D _cameraDirection = new(0, 0, -10);
        private Point3D _cameraPosition = new(0, 0, 10);
        private Point3D _cameraTarget = new(0, 0, 0);
        private Color _keyLight = (Color)ColorConverter.ConvertFromString("#FFAEAEAE");
        private Color _skyLight = (Color)ColorConverter.ConvertFromString("#FF111B30");
        private Color _groundLight = (Color)ColorConverter.ConvertFromString("#FF3F2F1E");
        private Color _ambientLight = (Color)ColorConverter.ConvertFromString("#FF3B3B3B");

        public ObservableCollection<MeshRendererVertexData> Meshes { get; } = [];

        public Vector3D CameraDirection
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
                    CameraDirection = new Vector3D(-value.X, -value.Y, -value.Z);
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

            double minX, minY, minZ;
            double maxX, maxY, maxZ;

            minX = minY = minZ = double.MaxValue;
            maxX = maxY = maxZ = double.MinValue;

            Vector3D avgNormal = new();
            var intervals = 2.0f / ((1 << 16) - 1);

            foreach (var mesh in lod.Meshes)
            {
                var vertexData = new MeshRendererVertexData()
                {
                    Name = mesh.Name,
                };

                using (var reader = new BinaryReader(new MemoryStream(mesh.Positions)))
                {
                    for (int i = 0; i < mesh.VertexCount; ++i)
                    {
                        var posX = reader.ReadSingle();
                        var posY = reader.ReadSingle();
                        var posZ = reader.ReadSingle();

                        vertexData.Positions.Add(new Point3D(posX, posY, posZ));

                        minX = Math.Min(minX, posX);
                        minY = Math.Min(minY, posY);
                        minZ = Math.Min(minZ, posZ);

                        maxX = Math.Max(maxX, posX);
                        maxY = Math.Max(maxY, posY);
                        maxZ = Math.Max(maxZ, posZ);
                    }
                }

                if (mesh.ElementsType.HasFlag(ElementsType.STATIC_NORMAL))
                {
                    var tSpaceOffset = 0;

                    if (mesh.ElementsType.HasFlag(ElementsType.SKELETAL)) tSpaceOffset = sizeof(short) * 4;

                    using (var reader = new BinaryReader(new MemoryStream(mesh.Elements)))
                    {
                        for (int i = 0; i < mesh.VertexCount; ++i)
                        {
                            var signs = (reader.ReadUInt32() >> 24) & 0x000000ff;

                            reader.BaseStream.Position += tSpaceOffset;

                            var normalX = reader.ReadUInt16() * intervals - 1.0f;
                            var normalY = reader.ReadUInt16() * intervals - 1.0f;
                            var normalZ = Math.Sqrt(
                                Math.Clamp(1f - (normalX * normalX + normalY * normalY), 0f, 1f)
                            ) * (((signs & 0x4) >> 1) - 1f);

                            var normal = new Vector3D(normalX, normalY, normalZ);

                            normal.Normalize();
                            vertexData.Normals.Add(normal);
                            avgNormal += normal;

                            if (mesh.ElementsType.HasFlag(ElementsType.STATIC_NORMAL_TEXTURE))
                            {
                                reader.BaseStream.Position += sizeof(short) * 2;

                                var u = reader.ReadSingle();
                                var v = reader.ReadSingle();

                                vertexData.UVs.Add(new Point(u, v));
                            }

                            if (mesh.ElementsType.HasFlag(ElementsType.SKELETAL_COLOR))
                            {
                                reader.BaseStream.Position += 4;
                            }
                        }
                    }
                }

                using (var reader = new BinaryReader(new MemoryStream(mesh.Indicies)))
                {
                    if (mesh.IndexSize == sizeof(short))
                    {
                        for (int i = 0; i < mesh.IndexCount; ++i) vertexData.Indices.Add(reader.ReadUInt16());
                    }
                    else
                    {
                        for (int i = 0; i < mesh.IndexCount; ++i) vertexData.Indices.Add(reader.ReadInt32());
                    }
                }

                vertexData.Positions.Freeze();
                vertexData.Normals.Freeze();
                vertexData.UVs.Freeze();
                vertexData.Indices.Freeze();
                Meshes.Add(vertexData);
            }

            if (old is not null)
            {
                CameraTraget = old.CameraTraget;
                CameraPosition = old.CameraPosition;

                foreach (var mesh in old.Meshes) mesh.IsHighlighted = false;
                foreach (var mesh in Meshes) mesh.Diffuse = old.Meshes.First().Diffuse;
            }
            else
            {
                var width = maxX - minX;
                var height = maxY - minY;
                var depth = maxZ - minZ;
                var radius = new Vector3D(height, width, depth).Length + 1.2;

                if (avgNormal.Length > 0.8)
                {
                    avgNormal.Normalize();
                    avgNormal *= radius;
                    CameraPosition = new Point3D(avgNormal.X, avgNormal.Y, avgNormal.Z);
                }
                else CameraPosition = new Point3D(width, height * 0.5, radius);

                CameraTraget = new Point3D(minX + width * 0.5, minY + height * 0.5, minZ + depth * 0.5);
            }

        }

        public Point3D OffsetCameraPosition =>
            new(CameraPosition.X + CameraTraget.X, CameraPosition.Y + CameraTraget.Y, CameraPosition.Z + CameraTraget.Z);
    }
}
