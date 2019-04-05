using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using BuildVision.Common.Diagnostics;
using BuildVision.Exports.Providers;
using BuildVision.Exports.Services;
using BuildVision.Helpers;
using BuildVision.Services;
using BuildVision.Tool;
using BuildVision.Tool.Building;
using BuildVision.UI;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Settings.Models;
using BuildVision.Views.Settings;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using Window = EnvDTE.Window;

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
    public sealed partial class BuildVisionPackage : AsyncPackage
    {
        private DTE _dte;
        private DTE2 _dte2;
        private CommandEvents _commandEvents;
        private WindowEvents _windowEvents;
        private SolutionEvents _solutionEvents;
        private IVsSolutionBuildManager2 _solutionBuildManager;
        private IVsSolutionBuildManager5 _solutionBuildManager4;
        private IBuildInformationProvider _buildInformationProvider;
        private uint _updateSolutionEvents4Cookie;
        private SolutionBuildEvents _solutionBuildEvents;
        private ISolutionProvider _solutionProvider;
        private Window _activeProjectContext;

        public ControlSettings ControlSettings { get; set; }

        public BuildVisionPackage()
        {
            string hello = string.Format("{0} {1}", Resources.ProductName, "BuildVisionVersion.PackageVersion");
            TraceManager.Trace(hello, EventLogEntryType.Information);

            if (Application.Current != null)
            {
                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            }
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            DiagnosticsClient.Notify(e.Exception);
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

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
            Assumes.Present(_solutionBuildManager);
            _solutionBuildManager4 = await GetServiceAsync(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager5;
            Assumes.Present(_solutionBuildManager4);
            _buildInformationProvider = await GetServiceAsync(typeof(IBuildInformationProvider)) as IBuildInformationProvider;
            Assumes.Present(_buildInformationProvider);
            _solutionProvider = await GetServiceAsync(typeof(ISolutionProvider)) as ISolutionProvider;
            Assumes.Present(_solutionProvider);

            _commandEvents = _dte.Events.CommandEvents;
            _commandEvents.AfterExecute += CommandEvents_AfterExecute;

            _windowEvents = _dte.Events.WindowEvents;
            _windowEvents.WindowActivated += WindowEvents_WindowActivated;

            _solutionEvents = _dte.Events.SolutionEvents;
            _solutionEvents.AfterClosing += SolutionEvents_AfterClosing;
            _solutionEvents.Opened += SolutionEvents_Opened;

            if (_dte2.Solution?.IsOpen == true)
            {
                SolutionEvents_Opened();
            }
        }

        private void SolutionEvents_Opened()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _solutionProvider.ReloadSolution();
            _buildInformationProvider.ResetCurrentProjects();
            _buildInformationProvider.ResetBuildInformationModel();
            
            _solutionBuildEvents = new SolutionBuildEvents(_solutionProvider, _buildInformationProvider);
            _solutionBuildManager.AdviseUpdateSolutionEvents(_solutionBuildEvents, out _updateSolutionEvents4Cookie);
            _solutionBuildManager4.AdviseUpdateSolutionEvents4(_solutionBuildEvents, out _updateSolutionEvents4Cookie);
        }

        private void SolutionEvents_AfterClosing()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _solutionProvider.ReloadSolution();
            _buildInformationProvider.ResetCurrentProjects();
            _buildInformationProvider.ResetBuildInformationModel();

            _solutionBuildManager.UnadviseUpdateSolutionEvents(_updateSolutionEvents4Cookie);
            _solutionBuildManager4.UnadviseUpdateSolutionEvents4(_updateSolutionEvents4Cookie);
        }

        private void WindowEvents_WindowActivated(Window gotFocus, Window lostFocus)
        {
            if (gotFocus == null)
                return;

            switch (gotFocus.Type)
            {
                case vsWindowType.vsWindowTypeSolutionExplorer:
                    _activeProjectContext = gotFocus;
                    break;

                case vsWindowType.vsWindowTypeDocument:
                case vsWindowType.vsWindowTypeDesigner:
                case vsWindowType.vsWindowTypeCodeWindow:
                    if (gotFocus.Project != null && !gotFocus.Project.IsHidden())
                        _activeProjectContext = gotFocus;
                    break;

                default:
                    return;
            }
        }

        private void CommandEvents_AfterExecute(string guid, int id, object customIn, object customOut)
        {
            if (id == (int)VSConstants.VSStd97CmdID.CancelBuild
                && Guid.Parse(guid) == VSConstants.GUID_VSStandardCommandSet97)
            {
                //_buildCancelled = true;
                //if (!_buildCancelledInternally)
                //    OnBuildCancelled();
            }
        }

        private async void ShowToolWindowAsync(object sender, EventArgs e)
        {
            try
            {
                var window = ShowToolWindow(Guid.Parse(PackageGuids.GuidBuildVisionToolWindowString));

                // Get the instance number 0 of this tool window. This window is single instance so this instance
                // is actually the only one.
                // The last flag is set to true so that if the tool window does not exists it will be created.
               // var window = FindToolWindow(typeof(BuildVisionPane), 0, true);
                if (window == null || window.Frame == null)
                    throw new InvalidOperationException(Resources.CanNotCreateWindow);

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var windowFrame = (IVsWindowFrame)window.Frame;
                ErrorHandler.ThrowOnFailure(windowFrame.Show());

                var windowStateService = await GetServiceAsync(typeof(IWindowStateService)) as IWindowStateService;
                Assumes.Present(windowStateService);
                windowStateService.Initialize(window);
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        ToolWindowPane ShowToolWindow(Guid windowGuid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();


            var shell = GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;
            IVsWindowFrame frame;
            if (ErrorHandler.Failed(shell.FindToolWindow((uint)__VSCREATETOOLWIN.CTW_fForceCreate,
                ref windowGuid, out frame)))
            {
                return null;
            }
            if (ErrorHandler.Failed(frame.Show()))
            {
                return null;
            }

            object docView = null;
            if (ErrorHandler.Failed(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView)))
            {
                return null;
            }
            return docView as ToolWindowPane;
        }
    }
}
