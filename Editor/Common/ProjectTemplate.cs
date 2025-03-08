using System.Runtime.Serialization;

namespace Editor.Common
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Lightning.Editor")]
    public class ProjectTemplate
    {
        [DataMember]
        public required string ProjectType { get; set; }

        [DataMember]
        public required string ProjectFile { get; set; }

        [DataMember]
        public required List<string> Folders { get; set; }

        public required byte[] Icon { get; set; }

        public required byte[] Screenshot { get; set; }

        public required string IconFilePath { get; set; }

        public required string ScreenshotFilePath { get; set; }

        public required string ProjectFilePath { get; set; }
    }
}
