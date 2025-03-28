using System.Runtime.Serialization;

namespace Editor.Components
{
    [DataContract]
    class Script : Component
    {
        private string _name;

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

        public Script(Entity owner) : base(owner)
        {
        }

        public override IMSComponent GetMultiselectComponents(MSEntityBase entity) => new MSScript(entity);
    }
}
