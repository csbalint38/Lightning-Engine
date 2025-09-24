using Editor.Common;
using Editor.Utilities;

namespace Editor.Content;

public class MeshLOD : ViewModelBase
{
    private string? _name;
    private float _lodThreshold;

    public string Name
    {
        get => _name!;
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
            if (!_lodThreshold.IsEqual(value))
            {
                _lodThreshold = value;
                OnPropertyChanged(nameof(LODThreshold));
            }
        }
    }

    public List<Mesh> Meshes { get; } = [];
}
