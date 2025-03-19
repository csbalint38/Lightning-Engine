using Editor.Common;
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
    public class Entity : ViewModelBase
    {
        private string _name;
        private bool _isEnabled = true;

        [DataMember(Name = nameof(Components))]
        private readonly ObservableCollection<Component> _components = [];

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
        public ICommand RenameCommand { get; private set; }
        public ICommand EnableCommand { get; private set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if(_components is not null)
            {
                Components = new ReadOnlyObservableCollection<Component>(_components);
                OnPropertyChanged(nameof(Components));
            }

            RenameCommand = new RelayCommand<string>(
                x =>
                {
                    var oldName = _name;
                    Name = x;

                    Project.UndoRedo.Add(new UndoRedoAction(nameof(Name), this, oldName, x, $"Rename '{oldName}' to '{x}'"));
                },
                x => x != _name
            );

            EnableCommand = new RelayCommand<bool>(
                x =>
                {
                    var oldValue = _isEnabled;
                    IsEnabled = x;

                    Project.UndoRedo.Add(new UndoRedoAction(nameof(IsEnabled), this, oldValue, x, x ? $"Enable {Name}" : $"Disable {Name}"));
                }
            );
        }

        public Entity(Scene scene)
        {
            Debug.Assert(scene != null);
            ParentScene = scene;

            _components.Add(new Transform(this));

            OnDeserialized(default);
        }
    }
}
