using System;
using System.ComponentModel.Design;
using BuildVision.Core;
using BuildVision.Tool;
using BuildVision.UI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace BuildVision.Commands
{
    internal sealed class ShowToolWindowCommand
    {
        public static async Task InitializeAsync(AsyncPackage package)
        {
            var commandService = (IMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService));
            var toolwndCommandId = new CommandID(PackageGuids.GuidBuildVisionCmdSet, (int)PackageIds.CmdIdBuildVisionToolWindow);
            var cmd = new MenuCommand((s, e) => Execute(package), toolwndCommandId);
            commandService.AddCommand(cmd);
        }

        private static void Execute(AsyncPackage package)
        {
            package.JoinableTaskFactory.RunAsync(async () =>
            {
                var window = ShowToolWindow(package);
            });
        }

        private static ToolWindowPane ShowToolWindow(AsyncPackage package)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            var window = package.FindToolWindow(typeof(BuildVisionPane), 0, true);
            if (window == null || window.Frame == null)
                throw new InvalidOperationException(Resources.CanNotCreateWindow);

            var windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
            return window;
        }
    }
}
