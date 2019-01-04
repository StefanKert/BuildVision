using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using BuildVision.Common;
using BuildVision.Tool;
using BuildVision.Tool.Building;
using BuildVision.Tool.Models;
using BuildVision.UI;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Extensions;
using BuildVision.UI.Helpers;
using BuildVision.UI.Models.Indicators.Core;
using BuildVision.UI.Settings.Models;
using BuildVision.UI.ViewModels;
using BuildVision.Views.Settings;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Task = System.Threading.Tasks.Task;

namespace BuildVision.Core
{

    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio. Resources are defined in VSPackage.resx.
    //[InstalledProductRegistration("#110", "#112", BuildVisionVersion.PackageVersion, IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(BuildVisionPane))]
    [Guid(PackageGuids.GuidBuildVisionPackageString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideBindingPath]
    [ProvideBindingPath(SubPath = "Lib")]
    // TODO: Add ProvideProfileAttribute for each DialogPage and implement IVsUserSettings, IVsUserSettingsQuery.
    //// [ProvideProfile(typeof(GeneralSettingsDialogPage), SettingsCategoryName, "General Options", 0, 0, true)]
    // TODO: ProvideOptionPage keywords.
    [ProvideOptionPage(typeof(GeneralSettingsDialogPage), "BuildVision", "General", 0, 0, true)]
    [ProvideOptionPage(typeof(WindowSettingsDialogPage), "BuildVision", "Tool Window", 0, 0, true)]
    [ProvideOptionPage(typeof(GridSettingsDialogPage), "BuildVision", "Projects Grid", 0, 0, true)]
    [ProvideOptionPage(typeof(BuildMessagesSettingsDialogPage), "BuildVision", "Build Messages", 0, 0, true)]
    [ProvideOptionPage(typeof(ProjectItemSettingsDialogPage), "BuildVision", "Project Item", 0, 0, true)]
    public sealed partial class BuildVisionPackage : AsyncPackage, IPackageContext
    {
        private DTE _dte;
        private SolutionEvents _solutionEvents;
        private BuildVisionPaneViewModel _viewModel;

        public ControlSettings ControlSettings { get; set; }

        public BuildVisionPackage()
        {
            string hello = string.Format("{0} {1}", Resources.ProductName, "BuildVisionVersion.PackageVersion");
            TraceManager.Trace(hello, EventLogEntryType.Information);    
        }

        public event Action<ControlSettings> ControlSettingsChanged = delegate { };

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);
            _dte = await GetServiceAsync(typeof(DTE)) as DTE;
            if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                var toolwndCommandId = new CommandID(PackageGuids.GuidBuildVisionCmdSet, (int) PackageIds.CmdIdBuildVisionToolWindow);
                var menuToolWin = new OleMenuCommand(ShowToolWindowAsync, toolwndCommandId);
                mcs.AddCommand(menuToolWin);
            }

            _solutionEvents = _dte.Events.SolutionEvents;
            _solutionEvents.AfterClosing += SolutionEvents_AfterClosing;
            _solutionEvents.Opened += SolutionEvents_Opened;

            var toolWindow = GetWindowPane(typeof(BuildVisionPane));
            IPackageContext packageContext = this;
            _viewModel = BuildVisionPane.GetViewModel(toolWindow);
            ViewModelHelper.UpdateSolution(_dte.Solution, _viewModel.SolutionItem);
            //var buildContext = new BuildContext(packageContext, _dte, _dte.Events.BuildEvents, _dte.Events.WindowEvents, _dte.Events.CommandEvents, viewModel);
        }

        private void SolutionEvents_Opened()
        {
            ViewModelHelper.UpdateSolution(_dte.Solution, _viewModel.SolutionItem);
            _viewModel.ResetIndicators(ResetIndicatorMode.ResetValue);
        }

        private void SolutionEvents_AfterClosing()
        {
            _viewModel.TextCurrentState = Resources.BuildDoneText_BuildNotStarted;
            _viewModel.ImageCurrentState = VectorResources.TryGet(BuildImages.BuildActionResourcesUri, "StandBy");
            _viewModel.ImageCurrentStateResult = null;

            ViewModelHelper.UpdateSolution(_dte.Solution, _viewModel.SolutionItem);
            _viewModel.ProjectsList.Clear();
            _viewModel.ResetIndicators(ResetIndicatorMode.Disable);
            _viewModel.BuildProgressViewModel.ResetTaskBarInfo();
        }

        private async void ShowToolWindowAsync(object sender, EventArgs e)
        {
            try
            {
                // Get the instance number 0 of this tool window. This window is single instance so this instance
                // is actually the only one.
                // The last flag is set to true so that if the tool window does not exists it will be created.
                var window = FindToolWindow(typeof(BuildVisionPane), 0, true);
                if (window == null || window.Frame == null)
                    throw new InvalidOperationException(Resources.CanNotCreateWindow);

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var windowFrame = (IVsWindowFrame)window.Frame;
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        public void NotifyControlSettingsChanged()
        {
            ControlSettingsChanged(ControlSettings);
        }

        private ToolWindowPane GetWindowPane(Type windowType)
        {
            return FindToolWindow(windowType, 0, false) ?? FindToolWindow(windowType, 0, true);
        }

        public async Task ExecuteCommandAsync(string commandName)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);
            _dte.ExecuteCommand(commandName);
        }
    }
}
