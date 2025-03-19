using Editor.Common;
using System.Diagnostics;

namespace Editor.Components
{
    public class Component : ViewModelBase
    {
        public Entity ParentEntity { get; private set; }
        public Component(Entity parent)
        {
            Debug.Assert(parent != null);
            ParentEntity = parent;
        }
    }
}
