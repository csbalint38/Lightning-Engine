using Editor.Common;
using Editor.Content;
using System.Diagnostics;

namespace Editor.Editors.GeometryEditor
{
    class GeometryEditor : ViewModelBase, IAssetEditor
    {
        private Geometry _geometry;

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

        public void SetAsset(Asset asset)
        {
            Debug.Assert(asset is Content.Geometry);

            if(asset is Content.Geometry geometry) Geometry = geometry;
        }
    }
}
