using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
using Window = EnvDTE.Window;

namespace BuildVision.Core
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(ui.SolutionOpening_string, flags: PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideToolWindow(typeof(BuildVisionPane), MultiInstances = false)]
    [ProvideToolWindowVisibility(typeof(BuildVisionPane), ui.SolutionOpening_string)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.GuidBuildVisionPackageString)]
    [ProvideBindingPath(SubPath = "Lib")]
    [ProvideProfile(typeof(GeneralSettingsDialogPage), PackageSettingsProvider.settingsCategoryName, "General Options", 0, 0, true)]
    [ProvideOptionPage(typeof(GeneralSettingsDialogPage), "BuildVision", "General", 0, 0, true)]
    [ProvideOptionPage(typeof(WindowSettingsDialogPage), "BuildVision", "Tool Window", 0, 0, true)]
    [ProvideOptionPage(typeof(GridSettingsDialogPage), "BuildVision", "Projects Grid", 0, 0, true)]
    [ProvideOptionPage(typeof(BuildMessagesSettingsDialogPage), "BuildVision", "Build Messages", 0, 0, true)]
    [ProvideOptionPage(typeof(ProjectItemSettingsDialogPage), "BuildVision", "Project Item", 0, 0, true)]
    public sealed class BuildVisionPackage : AsyncPackage
    {
        private DTE2 _dte;
        private IVsSolutionBuildManager2 _solutionBuildManager;
        private IVsSolutionBuildManager5 _solutionBuildManager4;
        private IBuildInformationProvider _buildInformationProvider;
        private uint _updateSolutionEventsCookie;
        private uint _updateSolutionEvents4Cookie;
        private SolutionBuildEvents _solutionBuildEvents;
        private ISolutionProvider _solutionProvider;
        private ILogger _logger = LogManager.ForContext<BuildVisionPackage>();
        private object _buildManager;
        private MethodInfo _adviseMethod;
        private MethodInfo _unadviseMethod;

        public static BuildVisionPackage BuildVisionPackageInstance { get; set; }

        public ControlSettings ControlSettings { get; set; }

        public BuildVisionPackage()
        {
            _logger.Information("Starting {ProductName} with Version {PackageVersion}", Resources.ProductName, ApplicationInfo.GetPackageVersion(this));

            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(new SerilogTraceListener.SerilogTraceListener(_logger));
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error | SourceLevels.Critical | SourceLevels.Warning;

            if (Application.Current != null)
            {
                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            }
  
            DiagnosticsClient.Initialize(GetEdition(), VisualStudioVersion.ToString(), "c437ad44-0c76-4006-968d-42d4369bc0ed");
            BuildVisionPackageInstance = this;
        }

        public static Version VisualStudioVersion => GetGlobalService(typeof(DTE2)) is DTE2 dte
                    ? new Version(int.Parse(dte.Version.Split('.')[0], CultureInfo.InvariantCulture), 0)
                    : new Version(0, 0, 0, 0);

        private string GetEdition()
        {
            try
            {
                _dte = GetService(typeof(DTE)) as DTE2;
                return _dte.Edition;
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

            _dte = await GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(_dte);
            _solutionBuildManager = await GetServiceAsync(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
            Assumes.Present(_solutionBuildManager);
            _buildManager = await GetServiceAsync(typeof(SVsSolutionBuildManager));
            var paramTypes = new Type[] { typeof(IVsUpdateSolutionEvents4), typeof(uint).MakeByRefType() };
            _adviseMethod = _buildManager.GetType().GetInterfaces().FirstOrDefault(x => x.Name == nameof(IVsSolutionBuildManager5)).GetMethod(nameof(IVsSolutionBuildManager5.AdviseUpdateSolutionEvents4), BindingFlags.Public | BindingFlags.Static, null, paramTypes, null);
            _unadviseMethod = _buildManager.GetType().GetInterfaces().FirstOrDefault(x => x.Name == nameof(IVsSolutionBuildManager5)).GetMethod(nameof(IVsSolutionBuildManager5.UnadviseUpdateSolutionEvents4));
            //Assumes.Present(_solutionBuildManager4);
            _buildInformationProvider = await GetServiceAsync(typeof(IBuildInformationProvider)) as IBuildInformationProvider;
            Assumes.Present(_buildInformationProvider);
            _solutionProvider = await GetServiceAsync(typeof(ISolutionProvider)) as ISolutionProvider;
            Assumes.Present(_solutionProvider);

            Community.VisualStudio.Toolkit.VS.Events.SolutionEvents.OnBeforeOpenSolution += SolutionEvents_Opened;
            Community.VisualStudio.Toolkit.VS.Events.SolutionEvents.OnBeforeCloseSolution += SolutionEvents_AfterClosing;

            if (_dte.Solution?.IsOpen == true)
            {
                SolutionEvents_Opened();
            }
        }
        private void SolutionEvents_Opened(string solutionFileName = "")
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _solutionProvider.ReloadSolution();
            _buildInformationProvider.ResetCurrentProjects();
            _buildInformationProvider.ResetBuildInformationModel();

            _solutionBuildEvents = new SolutionBuildEvents(_solutionProvider, _buildInformationProvider, LogManager.ForContext<SolutionBuildEvents>());
            _solutionBuildManager.AdviseUpdateSolutionEvents(_solutionBuildEvents, out _updateSolutionEventsCookie);
            object[] parameters = new object[] { _solutionBuildEvents, null };
            var result = (int) _adviseMethod.Invoke(null, parameters);
            if(result == VSConstants.S_OK)
            {
                _updateSolutionEvents4Cookie = (uint) parameters[1];
            }
            //_solutionBuildManager5.AdviseUpdateSolutionEvents4(_solutionBuildEvents, out _updateSolutionEvents4Cookie);
        }

        private void SolutionEvents_AfterClosing()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _solutionProvider.ReloadSolution();
            _buildInformationProvider.ResetCurrentProjects();
            _buildInformationProvider.ResetBuildInformationModel();

            _solutionBuildManager.UnadviseUpdateSolutionEvents(_updateSolutionEventsCookie);
            object[] parameters = new object[] { _solutionBuildEvents, null };
            var result = (int)_unadviseMethod.Invoke(null, parameters);
            if (result == VSConstants.S_OK)
            {
                _updateSolutionEvents4Cookie = (uint)parameters[1];
            }
            //_solutionBuildManager4.UnadviseUpdateSolutionEvents4(_updateSolutionEvents4Cookie);

            DiagnosticsClient.Flush();
        }
    }
}
