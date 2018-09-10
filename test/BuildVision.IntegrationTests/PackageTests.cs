using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.VisualStudio;
using BuildVision.Core;

namespace BuildVision.IntegrationTests
{
    [TestClass]
    public class PackageTests
    {
        private static IVsShell ShellService => VsIdeTestHostContext.ServiceProvider.GetService(typeof(SVsShell)) as IVsShell;
        private static IVsUIShell UiShellService => VsIdeTestHostContext.ServiceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;

        [TestMethod]
        [HostType("VS IDE")]
        public void PackageLoad_Should_Succeed()
        {
            UIThreadInvoker.Invoke(new Action(() =>
            {
                var guid = PackageGuids.GuidBuildVisionPackage;
                Assert.IsTrue(0 == ShellService.LoadPackage(ref guid, out var package));
                Assert.IsNotNull(package, "Package failed to load");
            }));
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void ClickOnBuildVisionMenuItem_Should_ShowBuildVision()
        {
            UIThreadInvoker.Invoke(new Action(() =>
            {
                var toolwndCommandId = new CommandID(PackageGuids.GuidBuildVisionCmdSet, (int) PackageIds.CmdIdBuildVisionToolWindow);                
                ExecuteCommand(toolwndCommandId);
                Assert.IsTrue(CanFindToolwindow(PackageGuids.GuidBuildVisionToolWindow));
            }));
        }

        public static void ExecuteCommand(CommandID cmd)
        {
            object customin = null;
            object customout = null;
            VsIdeTestHostContext.Dte.Commands.Raise(cmd.Guid.ToString("B").ToUpper(), cmd.ID, ref customin, ref customout);
        }

        public static bool CanFindToolwindow(Guid persistenceGuid)
        {
            var hr = UiShellService.FindToolWindow((uint) __VSFINDTOOLWIN.FTW_fFindFirst, ref persistenceGuid, out var windowFrame);
            Assert.IsTrue(hr == VSConstants.S_OK);
            return (windowFrame != null);
        }
    }
}
