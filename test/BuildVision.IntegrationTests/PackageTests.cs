using System;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Design;
using BuildVision.Core;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using Microsoft.VisualStudio;
using Xunit;
using System.Linq;
using System.Diagnostics;
using EnvDTE;

[assembly: VsixRunner(TraceLevel = SourceLevels.All)]
namespace BuildVision.IntegrationTests
{
    public class PackageTests
    {
        private static IVsShell ShellService => GlobalServices.GetService<SVsShell>() as IVsShell;
        private static IVsUIShell UiShellService => GlobalServices.GetService<SVsUIShell>() as IVsUIShell;
        private static DTE DTE => GlobalServices.GetService<DTE>() as DTE;

        [VsixFact("15.0")]
        public void PackageLoad_Should_Succeed()
        {
            IVsPackage package;
            var packageGuid = PackageGuids.GuidBuildVisionPackage;
            var packageLoaded = ShellService.LoadPackage(ref packageGuid, out package);

            Assert.Equal(0, packageLoaded);
            Assert.NotNull(package);
        }

        [VsixFact("15.0")]
        public void ClickOnBuildVisionMenuItem_Should_ShowBuildVision()
        {
            var toolwndCommandId = new CommandID(PackageGuids.GuidBuildVisionCmdSet, (int) PackageIds.CmdIdBuildVisionToolWindow);
            ExecuteCommand(toolwndCommandId);
            Assert.True(CanFindToolwindow(PackageGuids.GuidBuildVisionToolWindow));
        }

        private static void ExecuteCommand(CommandID cmd)
        {
            object customin = null;
            object customout = null;
            DTE.Commands.Raise(cmd.Guid.ToString("B").ToUpper(), cmd.ID, ref customin, ref customout);
        }

        private static bool CanFindToolwindow(Guid persistenceGuid)
        {
            var hr = UiShellService.FindToolWindow((uint) __VSFINDTOOLWIN.FTW_fFindFirst, ref persistenceGuid, out var windowFrame);
            Assert.True(hr == VSConstants.S_OK);
            return (windowFrame != null);
        }
    }
}
