using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using BuildVision.Common;
using BuildVision.Contracts;
using BuildVision.Exports.Providers;
using BuildVision.Helpers;
using BuildVision.Tool;
using BuildVision.Tool.Building;
using BuildVision.Tool.Models;
using BuildVision.UI;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Extensions;
using BuildVision.UI.Helpers;
using BuildVision.UI.Settings.Models;
using BuildVision.UI.ViewModels;
using BuildVision.Views.Settings;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using VSLangProj;
using Task = System.Threading.Tasks.Task;

namespace BuildVision.Core
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(BuildVisionPane))]
    [Guid(PackageGuids.GuidBuildVisionPackageString)]
    [ProvideBindingPath]
    [ProvideBindingPath(SubPath = "Lib")]
    [ProvideProfile(typeof(GeneralSettingsDialogPage), PackageSettingsProvider.settingsCategoryName, "General Options", 0, 0, true)]
    [ProvideOptionPage(typeof(GeneralSettingsDialogPage), "BuildVision", "General", 0, 0, true)]
    [ProvideOptionPage(typeof(WindowSettingsDialogPage), "BuildVision", "Tool Window", 0, 0, true)]
    [ProvideOptionPage(typeof(GridSettingsDialogPage), "BuildVision", "Projects Grid", 0, 0, true)]
    [ProvideOptionPage(typeof(BuildMessagesSettingsDialogPage), "BuildVision", "Build Messages", 0, 0, true)]
    [ProvideOptionPage(typeof(ProjectItemSettingsDialogPage), "BuildVision", "Project Item", 0, 0, true)]
    public sealed partial class BuildVisionPackage : AsyncPackage, IPackageContext
    {
        private DTE _dte;
        private DTE2 _dte2;
        private SolutionEvents _solutionEvents;
        private BuildVisionPaneViewModel _viewModel;
        private IVsSolutionBuildManager2 _solutionBuildManager;
        private IVsSolutionBuildManager5 _solutionBuildManager4;
        private IBuildInformationProvider _buildInformationProvider;
        private uint _updateSolutionEvents4Cookie;
        private readonly Guid _parsingErrorsLoggerId = new Guid("{64822131-DC4D-4087-B292-61F7E06A7B39}");
        private BuildOutputLogger _buildLogger;
        private SolutionBuildEvents _solutionBuildEvents;
        private ISolutionProvider _solutionProvider;
        private IBuildingProjectsProvider _buildingProjectsProvider;

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
            _dte2 = await GetServiceAsync(typeof(DTE)) as DTE2;
            if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                var toolwndCommandId = new CommandID(PackageGuids.GuidBuildVisionCmdSet, (int) PackageIds.CmdIdBuildVisionToolWindow);
                var menuToolWin = new OleMenuCommand(ShowToolWindowAsync, toolwndCommandId);
                mcs.AddCommand(menuToolWin);
            }
            _solutionBuildManager = await GetServiceAsync(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
            _solutionBuildManager4 = await GetServiceAsync(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager5;
            _buildInformationProvider = await GetServiceAsync(typeof(IBuildInformationProvider)) as IBuildInformationProvider;
            _solutionProvider = await GetServiceAsync(typeof(ISolutionProvider)) as ISolutionProvider;
            _buildingProjectsProvider = await GetServiceAsync(typeof(IBuildingProjectsProvider)) as IBuildingProjectsProvider;

            IPackageContext packageContext = this;
            _solutionEvents = _dte.Events.SolutionEvents;
            _solutionEvents.BeforeClosing += SolutionEvents_BeforeClosing;
            _solutionEvents.AfterClosing += SolutionEvents_AfterClosing;
            _solutionEvents.Opened += SolutionEvents_Opened;

            if (_dte2.Solution?.IsOpen == true)
            {
                SolutionEvents_Opened();
            }
        }

        private void SolutionEvents_BeforeClosing()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _solutionBuildManager.UnadviseUpdateSolutionEvents(_updateSolutionEvents4Cookie);
            _solutionBuildManager4.UnadviseUpdateSolutionEvents4(_updateSolutionEvents4Cookie);
        }

        private void SolutionEvents_Opened()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _solutionBuildEvents = new SolutionBuildEvents(_solutionProvider, _buildInformationProvider, _buildingProjectsProvider);
            _solutionBuildManager.AdviseUpdateSolutionEvents(_solutionBuildEvents, out _updateSolutionEvents4Cookie);
            _solutionBuildManager4.AdviseUpdateSolutionEvents4(_solutionBuildEvents, out _updateSolutionEvents4Cookie);
        }

        private void SolutionEvents_AfterClosing()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
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
