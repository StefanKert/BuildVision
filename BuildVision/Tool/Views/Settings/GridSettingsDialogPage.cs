using System.ComponentModel;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Shell;
using BuildVision.UI.ViewModels;
using BuildVision.Tool;
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
            get { return ControlSettings.GridSettings; }
            set { ControlSettings.GridSettings = value; }
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            ToolWindowPane toolWindow = Package.GetToolWindow();
            ControlViewModel viewModel = ToolWindow.GetViewModel(toolWindow);
            viewModel.SyncColumnSettings();

            base.OnActivate(e);
        }
    }
}