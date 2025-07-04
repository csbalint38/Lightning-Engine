using Editor.Common;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Editor.Components
{
    abstract public class MSEntityBase : ViewModelBase
    {
        private bool _enableUpdates = true;
        private bool? _isEnabled;
        private string? _name;
        private readonly ObservableCollection<IMSComponent> _components = [];

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

        public MSEntityBase(List<Entity> entities)
        {
            Debug.Assert(entities?.Count != 0);
            Components = new ReadOnlyObservableCollection<IMSComponent>(_components);
            SelectedEntities = entities;

            PropertyChanged += (s, e) =>
            {
                if (_enableUpdates) UpdateEntities(e.PropertyName);
            };
        }

        public static int? GetMixedValue<T>(List<T> objects, Func<T, int> getProperty)
        {
            var value = getProperty(objects.First());

            return objects.Skip(1).Any(x => value != getProperty(x)) ? null : value;
        }

        public static float? GetMixedValue<T>(List<T> objects, Func<T, float> getProperty)
        {
            var value = getProperty(objects.First());

            return objects.Skip(1).Any(x => !getProperty(x).Equals(value)) ? null : value;
        }

        public static bool? GetMixedValue<T>(List<T> objects, Func<T, bool> getProperty)
        {
            var value = getProperty(objects.First());

            return objects.Skip(1).Any(x => !getProperty(x).Equals(value)) ? null : value;
        }

        public static string? GetMixedValue<T>(List<T> objects, Func<T, string> getProperty)
        {
            var value = getProperty(objects.First());

            return objects.Skip(1).Any(x => !getProperty(x).Equals(value)) ? null : value;
        }

        public void Refresh()
        {
            _enableUpdates = false;
            UpdateMSEntity();
            MakeComponentList();
            _enableUpdates = true;
        }

        public T GetMSComponent<T>() where T : IMSComponent => (T)Components.FirstOrDefault(c => c is T);

        private void MakeComponentList()
        {
            _components.Clear();

            var firstEntity = SelectedEntities.FirstOrDefault();

            if (firstEntity is null) return;

            foreach (var component in firstEntity.Components)
            {
                var type = component.GetType();

                if (!SelectedEntities.Skip(1).Any(entity => entity.GetComponent(type) is null))
                {
                    Debug.Assert(Components.FirstOrDefault(x => x.GetType() == type) is null);

                    _components.Add(component.GetMultiselectComponents(this));
                }
            }
        }
    }
}
