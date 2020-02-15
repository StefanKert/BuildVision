using System;
using System.ComponentModel.Composition;
using BuildVision.Exports.Services;
using BuildVision.Extensions;
using BuildVision.Views.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildVision.Core
{
    [Export(typeof(IStatusBarNotificationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class StatusBarNotificationService : IStatusBarNotificationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPackageSettingsProvider _packageSettingsProvider;

        [ImportingConstructor]
        public StatusBarNotificationService(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            [Import(typeof(IPackageSettingsProvider))] IPackageSettingsProvider packageSettingsProvider)
        {
            _serviceProvider = serviceProvider;
            _packageSettingsProvider = packageSettingsProvider;
        }

        public void ShowText(string str)
        {
            SetText(str);
        }

        public void ShowTextWithFreeze(string str)
        {
            SetText(str, freezeOutput: true);
        }

        private void SetText(string str, bool freezeOutput = false)
        {
            if (!_packageSettingsProvider.Settings.GeneralSettings.EnableStatusBarOutput)
            {
                return;
            }

            var statusBar = _serviceProvider.GetService<IVsStatusbar>();
            statusBar.FreezeOutput(0);
            statusBar.SetText(str);
            if (freezeOutput)
            {
                statusBar.FreezeOutput(1);
            }
        }
    }
}
