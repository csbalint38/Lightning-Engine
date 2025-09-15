using Editor.Common;
using Editor.Content;
using Editor.Utilities;
using System.Diagnostics;

namespace Editor.Components
{
    public class MeshWithMaterial : ViewModelBase
    {
        private AppliedMaterial _material = null!;

        public MeshInfo MeshInfo { get; }

        public AppliedMaterial Material
        {
            get => _material;
            set
            {
                if (_material != value && value is not null)
                {
                    Debug.Assert(Id.IsValid(value.UploadedAsset?.ContentId ?? Id.InvalidId));

                    _material?.UnloadFromEngine();
                    _material = value;
                    OnPropertyChanged(nameof(Material));
                }
            }
        }

        public MeshWithMaterial(MeshInfo mesh, AppliedMaterial material)
        {
            Debug.Assert(mesh is not null && material is not null);

            MeshInfo = mesh;
            Material = material;
        }
    }
}
