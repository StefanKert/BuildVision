using System.Runtime.Serialization;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings
{
    [DataContract]
    public class ProjectItemSettings
    {
        [DataMember]
        public BuildOutputFileTypes CopyBuildOutputFileTypesToClipboard { get; set; }

        public ProjectItemSettings()
        {
            CopyBuildOutputFileTypesToClipboard = new BuildOutputFileTypes();
        }
    }
}