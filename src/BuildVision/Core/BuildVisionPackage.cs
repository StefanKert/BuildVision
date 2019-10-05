using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using BuildVision.Commands;
using BuildVision.Common;
using BuildVision.Common.Diagnostics;
using BuildVision.Common.Logging;
using BuildVision.Exports.Providers;
using BuildVision.Tool;
using BuildVision.UI;
using BuildVision.UI.Settings.Models;
using BuildVision.Views.Settings;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Serilog;
using Task = System.Threading.Tasks.Task;
using ui = Microsoft.VisualStudio.VSConstants.UICONTEXT;

namespace BuildVision.Core
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(ui.SolutionOpening_string, flags: PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideToolWindow(typeof(BuildVisionPane))]
    [ProvideToolWindow(typeof(BuildVisionPane), Transient = true, MultiInstances = false)]
    [ProvideToolWindowVisibility(typeof(BuildVisionPane), ui.SolutionOpening_string)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.GuidBuildVisionPackageString)]
    [ProvideBindingPath]
    [ProvideBindingPath(SubPath = "Lib")]
    [ProvideProfile(typeof(GeneralSettingsDialogPage), PackageSettingsProvider.settingsCategoryName, "General Options", 0, 0, true)]
    [ProvideOptionPage(typeof(GeneralSettingsDialogPage), "BuildVision", "General", 0, 0, true)]
    [ProvideOptionPage(typeof(WindowSettingsDialogPage), "BuildVision", "Tool Window", 0, 0, true)]
    [ProvideOptionPage(typeof(GridSettingsDialogPage), "BuildVision", "Projects Grid", 0, 0, true)]
    [ProvideOptionPage(typeof(BuildMessagesSettingsDialogPage), "BuildVision", "Build Messages", 0, 0, true)]
    [ProvideOptionPage(typeof(ProjectItemSettingsDialogPage), "BuildVision", "Project Item", 0, 0, true)]
    public sealed class BuildVisionPackage : AsyncPackage, IVsPackageDynamicToolOwnerEx
    {
        private DTE _dte;
        private DTE2 _dte2;
        private CommandEvents _commandEvents;
        private SolutionEvents _solutionEvents;
        private IVsSolutionBuildManager2 _solutionBuildManager;
        private IVsSolutionBuildManager5 _solutionBuildManager4;
        private IBuildInformationProvider _buildInformationProvider;
        private uint _updateSolutionEventsCookie;
        private uint _updateSolutionEvents4Cookie;
        private SolutionBuildEvents _solutionBuildEvents;
        private ISolutionProvider _solutionProvider;
        private ServiceProvider _serviceProvider;
        private ILogger _logger = LogManager.ForContext<BuildVisionPackage>();
        public static ToolWindowPane ToolWindowPane { get; set; }

        public ControlSettings ControlSettings { get; set; }

        public BuildVisionPackage()
        {
            _logger.Information("Starting {ProductName} with Version {PackageVersion}", Resources.ProductName, ApplicationInfo.GetPackageVersion(this));

            if (Application.Current != null)
            {
                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            }
  
            DiagnosticsClient.Initialize(GetEdition(), VisualStudioVersion.ToString(), "c437ad44-0c76-4006-968d-42d4369bc0ed");
        }

        public static Version VisualStudioVersion => GetGlobalService(typeof(DTE)) is DTE dte
                    ? new Version(int.Parse(dte.Version.Split('.')[0], CultureInfo.InvariantCulture), 0)
                    : new Version(0, 0, 0, 0);

        private string GetEdition()
        {
            try
            {
                _dte2 = GetService(typeof(DTE)) as DTE2;
                return _dte2.Edition;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Fatal(e.Exception, "Unhandled Exception");
            DiagnosticsClient.TrackException(e.Exception);
            DiagnosticsClient.Flush();
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);
            await ShowToolWindowCommand.InitializeAsync(this);

            _dte = await GetServiceAsync(typeof(DTE)) as DTE;
            Assumes.Present(_dte);
            _dte2 = await GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(_dte2);
            _solutionBuildManager = await GetServiceAsync(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
            Assumes.Present(_solutionBuildManager);
            _solutionBuildManager4 = await GetServiceAsync(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager5;
            Assumes.Present(_solutionBuildManager4);
            _buildInformationProvider = await GetServiceAsync(typeof(IBuildInformationProvider)) as IBuildInformationProvider;
            Assumes.Present(_buildInformationProvider);
            _solutionProvider = await GetServiceAsync(typeof(ISolutionProvider)) as ISolutionProvider;
            Assumes.Present(_solutionProvider);
            _serviceProvider = new ServiceProvider(Services.Dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
            Assumes.Present(_serviceProvider);

            _commandEvents = _dte.Events.CommandEvents;
            _commandEvents.AfterExecute += CommandEvents_AfterExecute;

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

            _solutionBuildEvents = new SolutionBuildEvents(_solutionProvider, _buildInformationProvider, _serviceProvider, LogManager.ForContext<SolutionBuildEvents>());
            _solutionBuildManager.AdviseUpdateSolutionEvents(_solutionBuildEvents, out _updateSolutionEventsCookie);
            _solutionBuildManager4.AdviseUpdateSolutionEvents4(_solutionBuildEvents, out _updateSolutionEvents4Cookie);
        }

        private void SolutionEvents_AfterClosing()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _solutionProvider.ReloadSolution();
            _buildInformationProvider.ResetCurrentProjects();
            _buildInformationProvider.ResetBuildInformationModel();

            _solutionBuildManager.UnadviseUpdateSolutionEvents(_updateSolutionEventsCookie);
            _solutionBuildManager4.UnadviseUpdateSolutionEvents4(_updateSolutionEvents4Cookie);

            DiagnosticsClient.Flush();
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

        public int QueryShowTool(ref Guid rguidPersistenceSlot, uint dwId, out int pfShowTool)
        {
            ToolWindowPane = FindToolWindow(typeof(BuildVisionPane), 0, false) ?? FindToolWindow(typeof(BuildVisionPane), 0, true);
            pfShowTool = 1;
            return 0;
        }
    }
}
