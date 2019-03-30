using System.Runtime.InteropServices;
using BuildVision.UI.Settings.Models;
using BuildVision.UI.Settings;

namespace BuildVision.Views.Settings
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("F6A09B98-FBA9-4107-97C9-83ACFF01E4B2")]
    public class GridSettingsDialogPage : SettingsDialogPage<GridSettingsControl, GridSettings>
    {
        protected override GridSettings Settings
        {
            get => ControlSettings?.GridSettings;
            set => ControlSettings.GridSettings = value;
        }
    }
}
