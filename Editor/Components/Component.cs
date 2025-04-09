using Editor.Common;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace Editor.Components
{
    [DataContract]
    abstract public class Component : ViewModelBase
    {
        [DataMember]
        public Entity ParentEntity { get; private set; }

        public Component(Entity parent)
        {
            Debug.Assert(parent != null);
            ParentEntity = parent;
        }

        public abstract IMSComponent GetMultiselectComponents(MSEntityBase entity);
        public abstract void WriteToBinary(BinaryWriter bw);
    }
}
