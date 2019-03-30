using System;
using System.ComponentModel.Composition;
using BuildVision.Exports.Services;
using BuildVision.Views.Settings;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using BuildVision.Extensions;

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
            if (!_packageSettingsProvider.Settings.GeneralSettings.EnableStatusBarOutput)
                return;

            var statusBar = _serviceProvider.GetService(typeof(IVsStatusbar)) as IVsStatusbar;
            statusBar.FreezeOutput(0);
            statusBar.SetText(str);
        }

        public void ShowTextWithFreeze(string str)
        {
            if (!_packageSettingsProvider.Settings.GeneralSettings.EnableStatusBarOutput)
                return;

            var statusBar = _serviceProvider.GetService<IVsStatusbar>();
            statusBar.FreezeOutput(0);
            statusBar.SetText(str);
            statusBar.FreezeOutput(1);
        }
    }
}
