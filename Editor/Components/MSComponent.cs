using Editor.Common;
using System.Diagnostics;

namespace Editor.Components
{
    abstract class MSComponent<T> : ViewModelBase, IMSComponent where T : Component
    {
        private bool _enableUpdates = true;

        public List<T> SelectedComponents { get; }

        protected abstract bool UpdateComponents(string propertyName);
        protected abstract bool UpdateMSComponent();

        public MSComponent(MSEntityBase msEntity)
        {
            Debug.Assert(msEntity?.SelectedEntities?.Count > 0);

            SelectedComponents = [.. msEntity.SelectedEntities.Select(static entity => entity.GetComponent<T>()).OfType<T>()];

            PropertyChanged += (s, e) =>
            {
                if (_enableUpdates && e.PropertyName is not null) UpdateComponents(e.PropertyName);
            };
        }

        public void Refresh()
        {
            _enableUpdates = false;
            UpdateMSComponent();
            _enableUpdates = true;
        }
    }
}
