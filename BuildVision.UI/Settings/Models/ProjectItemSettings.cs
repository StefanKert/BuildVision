using BuildVision.Contracts;
using System.Runtime.Serialization;
using BuildVision.Common;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings
{
    public class ProjectItemSettings : SettingsBase
  {
        public BuildOutputFileTypes CopyBuildOutputFileTypesToClipboard { get; set; }

        public ProjectItemSettings()
        {
            CopyBuildOutputFileTypesToClipboard = new BuildOutputFileTypes();
        }
    }
}