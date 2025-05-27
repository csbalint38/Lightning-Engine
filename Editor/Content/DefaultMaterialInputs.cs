using Editor.Common;
using System.IO;

namespace Editor.Content
{
    public class DefaultMaterialInputs : ViewModelBase
    {
        private readonly List<MaterialInput> _inputs;

        public List<MaterialInput> GetInputs() => _inputs;

        public void AddInput(MaterialInput input)
        {
            if (!_inputs.Any(x => x.Name == input.Name)) _inputs.Add(input);
        }

        public void RemoveInput(string name) => _inputs.Remove(_inputs.Find(x => x.Name == name));

        public void FromBinary(BinaryReader reader)
        {
            foreach (var input in _inputs)
            {
                input.Name = reader.ReadString();
            }
        }

        public void ToBinary(BinaryWriter writer)
        {
            foreach (var input in _inputs)
            {
                writer.Write(input.Name);
            }
        }

        public DefaultMaterialInputs()
        {
            _inputs = [
                new("Base Color"),
                new("Emissive Color"),
                new("Normal Map"),
                new("Metallic and Roughness"),
                new("Ambient Occlusion")
            ];
        }
    }
}
