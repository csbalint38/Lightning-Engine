using Editor.Common;

namespace Editor.Content;

public class CodeMaterial : ViewModelBase
{
    private readonly List<MaterialInput> _inputs = [];

    public List<MaterialInput> GetInputs() => _inputs;
}
