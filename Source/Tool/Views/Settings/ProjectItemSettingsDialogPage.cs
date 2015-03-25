using System.Runtime.InteropServices;

using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings;

namespace AlekseyNagovitsyn.BuildVision.Tool.Views.Settings
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("7734F116-DD1A-4EAF-9FA5-2257A2C2ABA3")]
    public class ProjectItemSettingsDialogPage : SettingsDialogPage<ProjectItemSettingsControl, ProjectItemSettings>
    {
        protected override ProjectItemSettings Settings
        {
            get { return Package.ControlSettings.ProjectItemSettings; }
            set { Package.ControlSettings.ProjectItemSettings = value; }
        }
    }
}