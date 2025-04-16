using Editor.Common;
using Editor.Common.Enums;
using Editor.Content;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Media3D;

namespace Editor.Editors
{
    public class GeometryEditor : ViewModelBase, IAssetEditor
    {
        private Geometry _geometry;
        private MeshRenderer _meshRenderer;
        private bool _autoLOD = true;
        private int _lodIndex;
        private AssetEditorState _state;

        public Asset Asset => Geometry;
        public int MaxLODIndex { get; private set; }
        public Guid AssetGuid { get; private set; }

        public Geometry Geometry
        {
            get => _geometry;
            private set
            {
                if (_geometry != value)
                {
                    _geometry = value;
                    OnPropertyChanged(nameof(Geometry));
                }
            }
        }

        public bool AutoLOD
        {
            get => _autoLOD;
            set
            {
                if (_autoLOD != value)
                {
                    _autoLOD = value;
                    OnPropertyChanged(nameof(AutoLOD));
                }
            }
        }

        public int LODIndex
        {
            get => _lodIndex;
            set
            {
                var lods = Geometry.GetLodGroup().LODs;
                value = Math.Clamp(value, 0, lods.Count - 1);
                if (_lodIndex != value)
                {
                    _lodIndex = value;
                    OnPropertyChanged(nameof(LODIndex));
                    MeshRenderer = new MeshRenderer(lods[value], MeshRenderer);
                }
            }
        }

        public MeshRenderer MeshRenderer
        {
            get => _meshRenderer;
            set
            {
                if (_meshRenderer != value)
                {
                    _meshRenderer = value;
                    OnPropertyChanged(nameof(MeshRenderer));

                    var lods = Geometry.GetLodGroup().LODs;
                    MaxLODIndex = (lods.Count > 0) ? lods.Count - 1 : 0;
                    OnPropertyChanged(nameof(MaxLODIndex));

                    if (lods.Count > 1)
                    {
                        MeshRenderer.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == nameof(MeshRenderer.OffsetCameraPosition) && AutoLOD) ComputeLOD(lods);
                        };

                        ComputeLOD(lods);
                    }
                }
            }
        }

        public AssetEditorState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged(nameof(State));
                }
            }
        }

        public void SetAsset(Asset asset)
        {
            Debug.Assert(asset is Content.Geometry);

            if (asset is Content.Geometry geometry)
            {
                AssetGuid = asset.Guid;
                Geometry = geometry;
                var numLods = geometry.GetLodGroup().LODs.Count;

                if (LODIndex >= numLods)
                {
                    LODIndex = numLods - 1;
                }
                else
                {
                    MeshRenderer = new MeshRenderer(Geometry.GetLodGroup().LODs[0], MeshRenderer);
                }
            }
        }

        public async void SetAssetAsync(AssetInfo info)
        {
            try
            {
                AssetGuid = info.Guid;

                Debug.Assert(info is not null && File.Exists(info.FullPath));

                var geometry = new Geometry();

                await Task.Run(() =>
                {
                    geometry.Load(info.FullPath);
                });

                SetAsset(geometry);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void ComputeLOD(IList<MeshLOD> lods)
        {
            if (!AutoLOD) return;

            var p = MeshRenderer.OffsetCameraPosition;
            var distance = new Vector3D(p.X, p.Y, p.Z).Length;

            for (int i = MaxLODIndex; i >= 0; --i)
            {
                if (lods[i].LODThreshold < distance)
                {
                    LODIndex = i;
                    break;
                }
            }
        }
    }
}
