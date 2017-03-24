using System.ComponentModel;
using System.Runtime.InteropServices;

using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings;
using AlekseyNagovitsyn.BuildVision.Tool.ViewModels;

using Microsoft.VisualStudio.Shell;

namespace AlekseyNagovitsyn.BuildVision.Tool.Views.Settings
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("F6A09B98-FBA9-4107-97C9-83ACFF01E4B2")]
    public class GridSettingsDialogPage : SettingsDialogPage<GridSettingsControl, GridSettings>
    {
        protected override GridSettings Settings
        {
            get { return Package.ControlSettings.GridSettings; }
            set { Package.ControlSettings.GridSettings = value; }
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