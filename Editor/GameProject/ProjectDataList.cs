using System.Runtime.Serialization;

namespace Editor.GameProject
{
    [DataContract]
    public class ProjectDataList
    {
        [DataMember]
        public List<ProjectData> Projects { get; set; }
    }
}
