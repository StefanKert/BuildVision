using System;

using AlekseyNagovitsyn.BuildVision.Core.Common;
using AlekseyNagovitsyn.BuildVision.Core.Logging;
using AlekseyNagovitsyn.BuildVision.Helpers;
using AlekseyNagovitsyn.BuildVision.Tool;
using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings;
using AlekseyNagovitsyn.BuildVision.Tool.ViewModels;

using EnvDTE;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;

namespace AlekseyNagovitsyn.BuildVision.Core
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    public partial class BuildVisionPackage : IPackageContext
    {
        private const string SettingsCategoryName = "BuildVision";

        private const string SettingsPropertyName = "Settings";

        private ControlSettings _controlSettings;

        private void ToolInitialize()
        {
            try
            {
                _controlSettings = LoadSettings(this);
                ToolWindowPane toolWindow = GetToolWindow();
                IPackageContext packageContext = this;
                ControlViewModel viewModel = ToolWindow.GetViewModel(toolWindow);
                var buildContext = new BuildContext(packageContext, viewModel);
                var tool = new Tool.Tool(packageContext, buildContext, buildContext, viewModel);
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        public event Action<ControlSettings> ControlSettingsChanged = delegate { };

        public void NotifyControlSettingsChanged()
        {
            ControlSettingsChanged(ControlSettings);
        }

        public ControlSettings ControlSettings
        {
            get { return _controlSettings; }
        }

        public void SaveSettings()
        {
            SaveSettings(_controlSettings, this);
        }

        public DTE GetDTE()
        {
            return (DTE)GetService(typeof(DTE));
        }

        public IVsUIShell GetUIShell()
        {
            return (IVsUIShell)GetService(typeof(IVsUIShell));
        }

        public IVsSolution GetSolution()
        {
            return (IVsSolution)GetService(typeof(IVsSolution));
        }

        public IVsStatusbar GetStatusBar()
        {
            return (IVsStatusbar)GetService(typeof(SVsStatusbar));
        }

        public ToolWindowPane GetToolWindow()
        {
            return GetWindowPane(typeof(ToolWindow));
        }

        private ToolWindowPane GetWindowPane(Type windowType)
        {
            return FindToolWindow(windowType, 0, false) ?? FindToolWindow(windowType, 0, true);
        }

        private static ControlSettings LoadSettings(IServiceProvider serviceProvider)
        {
            ControlSettings settings = null;

            WritableSettingsStore store = GetWritableSettingsStore(serviceProvider);
            if (store.PropertyExists(SettingsCategoryName, SettingsPropertyName))
            {
                string value = store.GetString(SettingsCategoryName, SettingsPropertyName);
                settings = value.Deserialize<ControlSettings>();
            }

            return (settings ?? new ControlSettings());
        }

        /// <remarks>
        /// Settings are stored under "HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\[12.0Exp]\BuildVision\".
        /// </remarks>
        private static void SaveSettings(ControlSettings settings, IServiceProvider serviceProvider)
        {
            WritableSettingsStore store = GetWritableSettingsStore(serviceProvider);
            if (!store.CollectionExists(SettingsCategoryName))
                store.CreateCollection(SettingsCategoryName);

            string value = settings.Serialize();
            store.SetString(SettingsCategoryName, SettingsPropertyName, value);
        }

        private static WritableSettingsStore GetWritableSettingsStore(IServiceProvider serviceProvider)
        {
            var shellSettingsManager = new ShellSettingsManager(serviceProvider);
            WritableSettingsStore writableSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            return writableSettingsStore;
        }
    }
}