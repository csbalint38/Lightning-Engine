using Editor.Common;

namespace Editor.Content;

public class LODGroup : ViewModelBase
{
    private string? _name;

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

    public List<MeshLOD> LODs { get; } = [];
}
