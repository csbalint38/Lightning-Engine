using Editor.Common;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Editor.GameProject
{
    [DataContract()]
    public class Project : ViewModelBase
    {
        [DataMember(Name = "Scenes")]
        private ObservableCollection<Scene> _scenes = [];

        public static readonly string Extension = ".lightning";

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public string Path { get; private set; }
        public string FullPath => $"{Path}{Name}{Extension}";

        public ReadOnlyObservableCollection<Scene> Scenes { get; }

        public Project(string name, string path)
        {
            Name = name;
            Path = path;

            _scenes.Add(new Scene(this, "Default Scene"));
        }
    }
}
