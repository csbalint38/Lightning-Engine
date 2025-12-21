using Editor.Common;
using Editor.Components;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace Editor.GameProject;

[DataContract]
public class Scene : ViewModelBase
{
    private string? _name;
    private bool _isActive;

    public ICommand? RenameCommand { get; private set; }

    [DataMember(Name = nameof(Entities))]
    private ObservableCollection<Entity> _entities = [];

    [DataMember]
    public Project Project { get; private set; }

    public ReadOnlyObservableCollection<Entity> Entities { get; private set; }

    [DataMember]
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

    [DataMember]
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;

                SetActiveGameEntities(_isActive);
                OnPropertyChanged(nameof(IsActive));
            }
        }
    }

    public Scene(Project project, string name)
    {
        Debug.Assert(project != null);

        Project = project;
        Name = name;
        Entities = new ReadOnlyObservableCollection<Entity>(_entities);

        OnDeserialized(new StreamingContext());
    }

    public void AddEntities(List<Entity> entities, int index = -1)
    {
        if (entities.Count == 0) return;

        var enableList = entities.Select(entity => (entity, entity.IsEnabled, index)).ToList();

        AddEntitiesInternal(enableList);

        index = _entities.Count - 1;

        Project.UndoRedo.Add(
            new UndoRedoAction(
                entities.Count == 1 ? $"Add {entities[0].Name} to {Name}" : $"Add {entities.Count} entities to {Name}",
                () => RemoveEntitiesInternal(entities),
                () => AddEntitiesInternal(enableList)
            )
        );
    }
    public void RemoveEntities(List<Entity> entities)
    {
        if (entities.Count == 0) return;

        var enableList = RemoveEntitiesInternal(entities);

        Project.UndoRedo.Add(
            new UndoRedoAction(
                entities.Count == 1 ? $"Remove {entities[0].Name} from {Name}" : $"Remove {entities.Count} entities from {Name}",
                () => AddEntitiesInternal(enableList),
                () => RemoveEntitiesInternal(entities)
            )
        );
    }

    public List<IdType> GetGeometryComponentIds()
    {
        var ids = Entities
            .Where(entity => entity.IsEnabled && entity.IsActive)
            .Select(entity => entity.GetComponent<Geometry>())
            .Where(c => c is not null)
            .Select(c => c!.GetComponentId())
            .ToList();

        Debug.Assert(ids.All(id => Id.IsValid(id)));

        return ids;
    }

    public List<(Entity Entity, bool IsEnabled)> DisableAndUpdate(List<Entity> entities, bool update = true)
    {
        var enableList = entities.Select(x => (x, x.IsEnabled)).ToList();

        entities.ForEach(x => x.IsEnabled = false);

        if (update) Project.UpdateScene();

        return enableList;
    }

    public void EnableAndUpdate(List<(Entity Entity, bool IsEnabled)> enableList, bool update = true)
    {
        enableList.ForEach(x => x.Entity.IsEnabled = x.IsEnabled);

        if (update) Project.UpdateScene();
    }


    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (_entities is not null)
        {
            Entities = new ReadOnlyObservableCollection<Entity>(_entities);
            OnPropertyChanged(nameof(Entities));
        }

        RenameCommand = new RelayCommand<string>(x =>
        {
            var oldName = Name!;
            Name = x;

            Project.UndoRedo.Add(
                new UndoRedoAction(nameof(Name), this, oldName, x, $"Rename secene '{oldName}' to '{x}'.")
            );
        }, x => x != Name);
    }

    private void SetActiveGameEntities(bool isActive)
    {
        foreach (var entity in _entities) entity.IsActive = isActive;
    }

    private void AddEntitiesInternal(List<(Entity, bool, int)> entities)
    {
        if (entities.Count == 0) return;

        foreach (var (entity, isEnabled, index) in entities)
        {
            Debug.Assert(!_entities.Contains(entity));

            entity.IsEnabled = isEnabled;
            entity.IsActive = IsActive;

            if (index == -1 || index >= _entities.Count) _entities.Add(entity);
            else _entities.Insert(index, entity);
        }

        Project.UpdateScene();
    }

    private List<(Entity, bool, int)> RemoveEntitiesInternal(List<Entity> entities)
    {
        if (entities.Count == 0) return [];

        var enableList = DisableAndUpdate(entities);
        var indices = enableList.Select(x => (x.Entity, x.IsEnabled, _entities.IndexOf(x.Entity))).ToList();

        foreach (var entity in entities)
        {
            Debug.Assert(_entities.Contains(entity));

            entity.IsActive = false;

            _entities.Remove(entity);
        }

        return indices;
    }
}
