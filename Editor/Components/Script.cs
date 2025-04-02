using System.IO;
using System.Runtime.Serialization;
using System.Text;

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

        public override void WriteToBinaty(BinaryWriter bw)
        {
            var name = Encoding.UTF8.GetBytes(Name);

            bw.Write(name.Length);
            bw.Write(name);
        }
    }
}
