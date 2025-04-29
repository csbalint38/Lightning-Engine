using Editor.Common;

namespace Editor.Content
{
    public class MaterialInput : ViewModelBase
    {
        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                if(_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public MaterialInput(string name)
        {
            Name = name;
        }
    }
}
