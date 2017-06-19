using System.Runtime.InteropServices;

using BuildVision.UI.Settings.Models;
using BuildVision.UI.Settings;

namespace BuildVision.Views.Settings
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("7734F116-DD1A-4EAF-9FA5-2257A2C2ABA3")]
    public class ProjectItemSettingsDialogPage : SettingsDialogPage<ProjectItemSettingsControl, ProjectItemSettings>
    {
        protected override ProjectItemSettings Settings
        {
            get { return ControlSettings.ProjectItemSettings; }
            set { ControlSettings.ProjectItemSettings = value; }
        }
    }
}