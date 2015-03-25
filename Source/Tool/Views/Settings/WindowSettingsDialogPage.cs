using System.Runtime.InteropServices;

using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings;

namespace AlekseyNagovitsyn.BuildVision.Tool.Views.Settings
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("297680A6-99B5-4042-B34A-4101AE105FF1")]
    public class WindowSettingsDialogPage : SettingsDialogPage<WindowSettingsControl, WindowSettings>
    {
        protected override WindowSettings Settings
        {
            get { return Package.ControlSettings.WindowSettings; }
            set { Package.ControlSettings.WindowSettings = value; }
        }
    }
}
