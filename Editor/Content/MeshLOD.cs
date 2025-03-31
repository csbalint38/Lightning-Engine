using Editor.Common;
using System.Collections.ObjectModel;

namespace Editor.Content
{
    internal class MeshLOD : ViewModelBase
    {
        private string _name;
        private float _lodThreshold;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public float LODThreshold
        {
            get => _lodThreshold;
            set
            {
                if (_lodThreshold != value)
                {
                    _lodThreshold = value;
                    OnPropertyChanged(nameof(LODThreshold));
                }
            }
        }

        public ObservableCollection<Mesh> Meshes { get; } = [];
    }
}
