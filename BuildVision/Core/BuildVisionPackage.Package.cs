using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;

using AlekseyNagovitsyn.BuildVision.Core.Logging;
using AlekseyNagovitsyn.BuildVision.Tool;
using AlekseyNagovitsyn.BuildVision.Tool.Views.Settings;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace AlekseyNagovitsyn.BuildVision.Core
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// <para/>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio. Resources are defined in VSPackage.resx.
    [InstalledProductRegistration("#110", "#112", PackageVersion, IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(ToolWindow))]
    [Guid(GuidList.guidBuildVisionPkgString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideBindingPath]
    [ProvideBindingPath(SubPath = "Lib")]
    // TODO: Add ProvideProfileAttribute for each DialogPage and implement IVsUserSettings, IVsUserSettingsQuery.
    //// [ProvideProfile(typeof(GeneralSettingsDialogPage), SettingsCategoryName, "General Options", 0, 0, true)]
    // TODO: ProvideOptionPage keywords.
    [ProvideOptionPage(typeof(GeneralSettingsDialogPage), SettingsCategoryName, "General", 0, 0, true)]
    [ProvideOptionPage(typeof(WindowSettingsDialogPage), SettingsCategoryName, "Tool Window", 0, 0, true)]
    [ProvideOptionPage(typeof(GridSettingsDialogPage), SettingsCategoryName, "Projects Grid", 0, 0, true)]
    [ProvideOptionPage(typeof(BuildMessagesSettingsDialogPage), SettingsCategoryName, "Build Messages", 0, 0, true)]
    [ProvideOptionPage(typeof(ProjectItemSettingsDialogPage), SettingsCategoryName, "Project Item", 0, 0, true)]
    public sealed partial class BuildVisionPackage : Package
    {
        // Keep the value equal to Version in vsixmanifest.
        internal const string PackageVersion = "1.5.0";

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public BuildVisionPackage()
        {
            string hello = string.Format("{0} {1}", Resources.ProductName, PackageVersion);
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
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (mcs != null)
            {
                // Create the command for the tool window
                var toolwndCommandId = new CommandID(GuidList.guidBuildVisionCmdSet, (int)PkgCmdIDList.cmdidBuildVisionToolWindow);
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
                ToolWindowPane window = FindToolWindow(typeof(ToolWindow), 0, true);
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
