using Editor.Common;
using Editor.Common.Enums;
using Editor.DLLs;
using Editor.GameProject;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Editor.Components;

[DataContract]
[KnownType(typeof(Transform))]
[KnownType(typeof(Script))]
[KnownType(typeof(Geometry))]
public class Entity : ViewModelBase
{
    private IdType _entityId = Id.InvalidId;
    private bool _isActive;
    private string _name = null!;
    private bool _isEnabled = true;

    [DataMember(Name = nameof(Components))]
    private readonly ObservableCollection<Component> _components = [];

    public IdType EntityId
    {
        get => _entityId;
        private set
        {
            if (_entityId != value)
            {
                _entityId = value;
                OnPropertyChanged(nameof(EntityId));
            }
        }
    }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;

                if (_isActive)
                {
                    _components.ToList().ForEach(x => x.Load());

                    EntityId = EngineAPI.CreateGameEntity(this);

                    Debug.Assert(Id.IsValid(EntityId));
                }
                else if (Id.IsValid(EntityId))
                {
                    EngineAPI.RemoveGameEntity(this);

                    _components.ToList().ForEach(x => x.Unload());

                    EntityId = Id.InvalidId;
                }

                OnPropertyChanged(nameof(IsActive));
            }
        }
    }

    [DataMember]
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

    [DataMember]
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
    }

    [DataMember]
    public Scene ParentScene { get; private set; }
    public ReadOnlyObservableCollection<Component> Components { get; private set; } = null!;

    [OnDeserialized]
    void OnDeserialized(StreamingContext context)
    {
        if (_components is not null)
        {
            Components = new ReadOnlyObservableCollection<Component>(_components);
            OnPropertyChanged(nameof(Components));
        }
    }

    public Entity(Scene scene)
    {
        Debug.Assert(scene != null);
        ParentScene = scene;

        _components.Add(new Transform(this));

        OnDeserialized(default);
    }

    public Component? GetComponent(Type type) => Components.FirstOrDefault(c => c.GetType() == type);
    public T? GetComponent<T>() where T : Component => GetComponent(typeof(T)) as T;

    public bool AddComponent(Component component)
    {
        Debug.Assert(component is not null);

        if (!Components.Any(x => x.GetType() == component.GetType()))
        {
            var wasActive = IsActive;

            IsActive = false;
            _components.Add(component);
            IsActive = wasActive;

            return true;
        }
        Logger.LogAsync(LogLevel.WARNING, $"Entity {Name} already has a {component.GetType().Name} component");

        return false;
    }

    public void RemoveComponent(Component component)
    {
        Debug.Assert(component is not null);

        if (component is Transform) return;

        if (_components.Contains(component))
        {
            IsActive = false;
            _components.Remove(component);
            IsActive = true;
        }
    }
}
