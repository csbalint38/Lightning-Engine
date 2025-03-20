using Editor.Common;
using Editor.Components;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace Editor.GameProject
{
    [DataContract]
    internal class Scene : ViewModelBase
    {
        private string _name;
        private bool _isActive;

        [DataMember(Name = nameof(Entities))]
        private ObservableCollection<Entity> _entities = [];

        [DataMember]
        public Project Project { get; private set; }

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
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        public ReadOnlyObservableCollection<Entity> Entities { get; private set; }
         
        public ICommand AddEntityCommand { get; private set; }
        public ICommand RemoveEntityCommand { get; private set; }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if(_entities is not null)
            {
                Entities = new ReadOnlyObservableCollection<Entity>(_entities);
                OnPropertyChanged(nameof(Entities));
            }

            AddEntityCommand = new RelayCommand<Entity>(x =>
            {
                AddEntity(x);
                var index = _entities.Count - 1;

                Project.UndoRedo.Add(new UndoRedoAction(
                    $"Added {x.Name} to {Name}",
                    () => RemoveEntity(x),
                    () => _entities.Insert(index, x)
                ));
            });

            RemoveEntityCommand = new RelayCommand<Entity>(x =>
            {
                var index = _entities.IndexOf(x);
                RemoveEntity(x);

                Project.UndoRedo.Add(new UndoRedoAction(
                    $"Removed {x.Name}",
                    () => _entities.Insert(index, x),
                    () => RemoveEntity(x)
                ));
            });
        }

        public Scene(Project project, string name)
        {
            Debug.Assert(project != null);

            Project = project;
            Name = name;

            OnDeserialized(new StreamingContext());
        }

        private void AddEntity(Entity entity)
        {
            Debug.Assert(!_entities.Contains(entity));
            _entities.Add(entity);
        }

        private void RemoveEntity(Entity entity)
        {
            Debug.Assert(_entities.Contains(entity));
            _entities.Remove(entity);
        }
    }
}
