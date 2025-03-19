using Editor.Common;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Editor.Components
{
    [DataContract]
    public class Component : ViewModelBase
    {
        [DataMember]
        public Entity ParentEntity { get; private set; }
        public Component(Entity parent)
        {
            Debug.Assert(parent != null);
            ParentEntity = parent;
        }
    }
}
