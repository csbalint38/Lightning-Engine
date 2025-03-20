using Editor.Common;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Editor.Components
{
    abstract class MSEntityBase : ViewModelBase
    {
        private bool _enableUpdates = true;
        private bool? _isEnabled;
        private string? _name;
        private readonly ObservableCollection<IMSComponent> _components = new();

        public bool? IsEnabled
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

        public string? Name
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

        public ReadOnlyObservableCollection<IMSComponent> Components { get; }
        public List<Entity> SelectedEntities { get; }

        protected virtual bool UpdateEntities(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(IsEnabled): SelectedEntities.ForEach(x => x.IsEnabled = IsEnabled.Value); return true;
                case nameof(Name): SelectedEntities.ForEach(x => x.Name = Name); return true;
            }

            return false;
        }

        protected virtual bool UpdateMSEntity()
        {
            IsEnabled = GetMixedValue(SelectedEntities, new Func<Entity, bool>(x => x.IsEnabled));
            Name = GetMixedValue(SelectedEntities, new Func<Entity, string>(x => x.Name));

            return true;
        }

        public static float? GetMixedValue(List<Entity> entities, Func<Entity, float> getProperty)
        {
            var value = getProperty(entities.First());

            foreach (var entity in entities.Skip(1))
            {
                if (!value.IsEqual(getProperty(entity))) return null;
            }

            return value;
        }

        public static bool? GetMixedValue(List<Entity> entities, Func<Entity, bool> getProperty)
        {
            var value = getProperty(entities.First());

            foreach (var entity in entities.Skip(1))
            {
                if (value != getProperty(entity)) return null;
            }

            return value;
        }

        public static string? GetMixedValue(List<Entity> entities, Func<Entity, string> getProperty)
        {
            var value = getProperty(entities.First());

            foreach (var entity in entities.Skip(1))
            {
                if (value != getProperty(entity)) return null;
            }

            return value;
        }

        public void Refresh()
        {
            _enableUpdates = false;
            UpdateMSEntity();
            _enableUpdates = true;
        }

        public MSEntityBase(List<Entity> entities)
        {
            Debug.Assert(entities?.Count != 0);
            Components = new ReadOnlyObservableCollection<IMSComponent>(_components);
            SelectedEntities = entities;

            PropertyChanged += (s, e) =>
            {
                if(_enableUpdates) UpdateEntities(e.PropertyName);
            };
        }
    }
}
