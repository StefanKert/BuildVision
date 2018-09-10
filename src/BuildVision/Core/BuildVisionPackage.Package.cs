using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using BuildVision.UI;
using BuildVision.Views.Settings;
using BuildVision.Tool;
using BuildVision.UI.Common.Logging;

namespace BuildVision.Core
{
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio. Resources are defined in VSPackage.resx.
    [InstalledProductRegistration("#110", "#112", BuildVisionVersion.PackageVersion, IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(ToolWindow))]
    [Guid(PackageGuids.GuidBuildVisionPackageString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideBindingPath]
    [ProvideBindingPath(SubPath = "Lib")]
    // TODO: Add ProvideProfileAttribute for each DialogPage and implement IVsUserSettings, IVsUserSettingsQuery.
    //// [ProvideProfile(typeof(GeneralSettingsDialogPage), SettingsCategoryName, "General Options", 0, 0, true)]
    // TODO: ProvideOptionPage keywords.
    [ProvideOptionPage(typeof(GeneralSettingsDialogPage), settingsCategoryName, "General", 0, 0, true)]
    [ProvideOptionPage(typeof(WindowSettingsDialogPage), settingsCategoryName, "Tool Window", 0, 0, true)]
    [ProvideOptionPage(typeof(GridSettingsDialogPage), settingsCategoryName, "Projects Grid", 0, 0, true)]
    [ProvideOptionPage(typeof(BuildMessagesSettingsDialogPage), settingsCategoryName, "Build Messages", 0, 0, true)]
    [ProvideOptionPage(typeof(ProjectItemSettingsDialogPage), settingsCategoryName, "Project Item", 0, 0, true)]
    public sealed partial class BuildVisionPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public BuildVisionPackage()
        {
            string hello = string.Format("{0} {1}", Resources.ProductName, BuildVisionVersion.PackageVersion);
            TraceManager.Trace(hello, EventLogEntryType.Information);    
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            if (GetService(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                // Create the command for the tool window
                var toolwndCommandId = new CommandID(PackageGuids.GuidBuildVisionCmdSet, (int) PackageIds.CmdIdBuildVisionToolWindow);
                var menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandId);
                mcs.AddCommand(menuToolWin);
            }

            ToolInitialize();
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            try
            {
                // Get the instance number 0 of this tool window. This window is single instance so this instance
                // is actually the only one.
                // The last flag is set to true so that if the tool window does not exists it will be created.
                var window = FindToolWindow(typeof(ToolWindow), 0, true);
                if (window == null || window.Frame == null)
                    throw new InvalidOperationException(Resources.CanNotCreateWindow);

                var windowFrame = (IVsWindowFrame)window.Frame;
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }
    }
}
