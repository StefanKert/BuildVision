using BuildVision.UI.Settings;
using BuildVision.UI.Settings.Models;
using System.Runtime.InteropServices;


namespace BuildVision.Views.Settings
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("F7BF1C16-A1D4-4E93-9A10-9AE106D656EE")]
    public class GeneralSettingsDialogPage : SettingsDialogPage<GeneralSettingsControl, GeneralSettings>
    {
        protected override GeneralSettings Settings
        {
            get { return ControlSettings.GeneralSettings; }
            set { ControlSettings.GeneralSettings = value; }
        }
    }
}