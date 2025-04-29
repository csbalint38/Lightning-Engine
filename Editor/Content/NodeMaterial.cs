using Editor.Common;

namespace Editor.Content
{
    public class NodeMaterial : ViewModelBase
    {
        private readonly List<MaterialInput> _inputs;

        public List<MaterialInput> GetInputs() => _inputs;
    }
}
