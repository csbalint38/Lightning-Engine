using Editor.Common;
using Editor.Content;
using System.Diagnostics;

namespace Editor.Editors.GeometryEditor
{
    class GeometryEditor : ViewModelBase, IAssetEditor
    {
        private Geometry _geometry;
        private MeshRenderer _meshRenderer;

        public Asset Asset => throw new NotImplementedException();

        public Geometry Geometry
        {
            get => _geometry;
            set
            {
                if(_geometry != value)
                {
                    _geometry = value;
                    OnPropertyChanged(nameof(Geometry));
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
                }
            }
        }

        public void SetAsset(Asset asset)
        {
            Debug.Assert(asset is Content.Geometry);

            if (asset is Content.Geometry geometry)
            {
                Geometry = geometry;
                MeshRenderer = new MeshRenderer(Geometry.GetLodGroup().LODs[0], MeshRenderer);
            }
        }
    }
}
