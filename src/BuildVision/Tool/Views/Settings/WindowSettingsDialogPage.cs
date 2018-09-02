using System.Runtime.InteropServices;

using BuildVision.UI.Settings.Models;
using BuildVision.UI.Settings;

namespace BuildVision.Views.Settings
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("297680A6-99B5-4042-B34A-4101AE105FF1")]
    public class WindowSettingsDialogPage : SettingsDialogPage<WindowSettingsControl, WindowSettings>
    {
        protected override WindowSettings Settings
        {
            get { return ControlSettings.WindowSettings; }
            set { ControlSettings.WindowSettings = value; }
        }
    }
}
