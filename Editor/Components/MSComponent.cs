using Editor.Common;

namespace Editor.Components
{
    abstract class MSComponent<T> : ViewModelBase, IMSComponent where T : Component
    {

    }
}
