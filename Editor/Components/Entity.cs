using Editor.Common;
using Editor.DLLs;
using Editor.GameProject;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace Editor.Components
{
    [DataContract]
    [KnownType(typeof(Transform))]
    class Entity : ViewModelBase
    {
        private int _entityId = Id.InvalidId;
        private bool _isActive;
        private string _name;
        private bool _isEnabled = true;

        [DataMember(Name = nameof(Components))]
        private readonly ObservableCollection<Component> _components = [];

        public int EntityId
        {
            get => _entityId;
            set
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
                        EntityId = EngineAPI.CreateGameEntity(this);

                        Debug.Assert(Id.IsValid(EntityId));
                    }
                    else if (Id.IsValid(EntityId))
                    {
                        EngineAPI.RemoveGameEntity(this);
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

        public Scene ParentScene { get; private set; }
        public ReadOnlyObservableCollection<Component> Components { get; private set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if(_components is not null)
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

        public Component GetComponent(Type type) => Components.FirstOrDefault(c => c.GetType() == type);
        public T GetComponent<T>() where T : Component => (T)GetComponent(typeof(T));
    }
}
