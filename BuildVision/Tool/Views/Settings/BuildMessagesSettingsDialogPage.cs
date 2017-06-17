using System.Runtime.InteropServices;

using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildMessages;

namespace AlekseyNagovitsyn.BuildVision.Tool.Views.Settings
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("5F024C61-38E6-4A43-A098-C33489A1A842")]
    public class BuildMessagesSettingsDialogPage : SettingsDialogPage<BuildMessagesSettingsControl, BuildMessagesSettings>
    {
        protected override BuildMessagesSettings Settings
        {
            get { return ControlSettings.BuildMessagesSettings; }
            set { ControlSettings.BuildMessagesSettings = value; }
        }
    }
}