using Editor.Common;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Editor.GameProject
{
    [DataContract]
    public class Scene : ViewModelBase
    {
        private string _name;
        private bool _isActive;

        [DataMember]
        public Project Project { get; private set; }

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

        [DataMember]
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        public Scene(Project project, string name)
        {
            Debug.Assert(project != null);

            Project = project;
            Name = name;
        }
    }
}
