using System;
using System.Diagnostics;
using System.Windows;
using BuildVision.Common;
using BuildVision.Common.Logging;
using BuildVision.UI.Settings.Models;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

namespace BuildVision.Views.Settings
{
    public class PackageSettingsProvider : BindableBase, IPackageSettingsProvider
    {
        private Serilog.ILogger _logger = LogManager.ForContext<PackageSettingsProvider>();
        public const string settingsCategoryName = "BuildVision";
        public const string settingsPropertyName = "Settings";

        readonly WritableSettingsStore _settingsStore;

        public ControlSettings Settings { get; set; }

        public PackageSettingsProvider(IServiceProvider serviceProvider)
        {
            var shellSettingsManager = new ShellSettingsManager(serviceProvider);
            _settingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            LoadSettings();
        }

        public void Save()
        {
            SaveSettings();
            SettingsChanged?.Invoke();
        }

        private void SaveSettings()
        {
            if (!_settingsStore.CollectionExists(settingsCategoryName))
            {
                _settingsStore.CreateCollection(settingsCategoryName);
            }

            var legacySerializer = new LegacyConfigurationSerializer<ControlSettings>();
            var value = legacySerializer.Serialize(Settings);
            _settingsStore.SetString(settingsCategoryName, settingsPropertyName, value);
        }

        private void LoadSettings()
        {
            try
            {
                if (_settingsStore.PropertyExists(settingsCategoryName, settingsPropertyName))
                {
                    var legacySerialized = new LegacyConfigurationSerializer<ControlSettings>();
                    var value = _settingsStore.GetString(settingsCategoryName, settingsPropertyName);
                    Settings = legacySerialized.Deserialize(value);
                }
                else
                {
                    Settings = new ControlSettings();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error when trying to load settings.");
                MessageBox.Show("An error occurred when trying to load current settings. To make sure everything is still working the settings are set to default.");
            }
        }

        public event Action SettingsChanged;
    }
}
