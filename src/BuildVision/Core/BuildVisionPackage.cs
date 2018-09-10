using System;

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using BuildVision.Common;
using BuildVision.Tool.Building;
using BuildVision.UI.Settings.Models;
using BuildVision.UI.ViewModels;
using BuildVision.Tool;
using BuildVision.UI.Common.Logging;
using System.Windows;

namespace BuildVision.Core
{
    public partial class BuildVisionPackage : IPackageContext
    {
        private const string settingsCategoryName = "BuildVision";
        private const string settingsPropertyName = "Settings";

        public ControlSettings ControlSettings { get; set; }

        public DTE2 GetDTE2() => (DTE2)GetService(typeof(DTE));

        public DTE GetDTE() => (DTE)GetService(typeof(DTE));

        public IVsUIShell GetUIShell() => (IVsUIShell)GetService(typeof(IVsUIShell));

        public IVsSolution GetSolution() => (IVsSolution)GetService(typeof(IVsSolution));

        public IVsStatusbar GetStatusBar() => (IVsStatusbar)GetService(typeof(SVsStatusbar));

        public ToolWindowPane GetToolWindow() => GetWindowPane(typeof(ToolWindow));

        private void ToolInitialize()
        {
            try
            {
                ControlSettings = LoadSettings(this);
                var toolWindow = GetToolWindow();
                IPackageContext packageContext = this;
                var viewModel = ToolWindow.GetViewModel(toolWindow);
                var buildContext = new BuildContext(packageContext, viewModel);
                var tool = new Tool.Tool(packageContext, buildContext, buildContext, viewModel);
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
                ex.Trace("Error when trying to load settings: " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
                MessageBox.Show("An error occurred when trying to load current settings. To make sure everything is still working the settings are set to default.");
            }

            return new ControlSettings();
        }

        /// <remarks>
        /// Settings are stored under "HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\[12.0Exp]\BuildVision\".
        /// </remarks>
        private static void SaveSettings(ControlSettings settings, IServiceProvider serviceProvider)
        {
            var store = GetWritableSettingsStore(serviceProvider);
            if (!store.CollectionExists(settingsCategoryName))
                store.CreateCollection(settingsCategoryName);

            var legacySerializer = new LegacyConfigurationSerializer<ControlSettings>();
            var value = legacySerializer.Serialize(settings);
            store.SetString(settingsCategoryName, settingsPropertyName, value);
        }

        private static WritableSettingsStore GetWritableSettingsStore(IServiceProvider serviceProvider)
        {
            var shellSettingsManager = new ShellSettingsManager(serviceProvider);
            var writableSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            return writableSettingsStore;
        }

        public event Action<ControlSettings> ControlSettingsChanged = delegate { };
    }
}
