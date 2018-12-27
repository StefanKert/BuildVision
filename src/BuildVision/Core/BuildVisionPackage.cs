using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using BuildVision.Common;
using BuildVision.Tool;
using BuildVision.Tool.Building;
using BuildVision.UI;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Settings.Models;
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
    [ProvideToolWindow(typeof(ToolWindow))]
    [Guid(PackageGuids.GuidBuildVisionPackageString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
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
    public sealed partial class BuildVisionPackage : AsyncPackage, IPackageContext
    {
        private const string settingsCategoryName = "BuildVision";
        private const string settingsPropertyName = "Settings";
        private DTE _dte;

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
                var menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandId);
                mcs.AddCommand(menuToolWin);
            }

            ControlSettings = LoadSettings(this);
            var toolWindow = GetWindowPane(typeof(ToolWindow));
            IPackageContext packageContext = this;
            var viewModel = ToolWindow.GetViewModel(toolWindow);
            var buildContext = new BuildContext(packageContext, _dte, _dte.Events.BuildEvents, _dte.Events.WindowEvents, _dte.Events.CommandEvents, viewModel);
            var tool = new Tool.Tool(packageContext, _dte, (IVsWindowFrame) toolWindow.Frame, buildContext, buildContext, viewModel);
        }

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

        public void SaveSettings()
        {
            SaveSettings(ControlSettings, this);
        }

        public void NotifyControlSettingsChanged()
        {
            ControlSettingsChanged(ControlSettings);
        }

        private ToolWindowPane GetWindowPane(Type windowType)
        {
            return FindToolWindow(windowType, 0, false) ?? FindToolWindow(windowType, 0, true);
        }

        private static ControlSettings LoadSettings(IServiceProvider serviceProvider)
        {
            try
            {
                var store = GetWritableSettingsStore(serviceProvider);
                if (store.PropertyExists(settingsCategoryName, settingsPropertyName))
                {
                    var legacySerialized = new LegacyConfigurationSerializer<ControlSettings>();
                    var value = store.GetString(settingsCategoryName, settingsPropertyName);
                    return legacySerialized.Deserialize(value);
                }
            }
            catch (Exception ex)
            {
                ex.Trace("Error when trying to load settings: " + ex.Message, EventLogEntryType.Error);
                MessageBox.Show("An error occurred when trying to load current settings. To make sure everything is still working the settings are set to default.");
            }

            return new ControlSettings();
        }

        private static void SaveSettings(ControlSettings settings, IServiceProvider serviceProvider)
        {
            var store = GetWritableSettingsStore(serviceProvider);
            if (!store.CollectionExists(settingsCategoryName))
                store.CreateCollection(settingsCategoryName);

            var legacySerializer = new LegacyConfigurationSerializer<ControlSettings>();
            var value = legacySerializer.Serialize(settings);
            store.SetString(settingsCategoryName, settingsPropertyName, value);
        }

        public async Task ExecuteCommandAsync(string commandName)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);
            _dte.ExecuteCommand(commandName);
        }

        private static WritableSettingsStore GetWritableSettingsStore(IServiceProvider serviceProvider)
        {
            var shellSettingsManager = new ShellSettingsManager(serviceProvider);
            var writableSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            return writableSettingsStore;
        }
    }
}
