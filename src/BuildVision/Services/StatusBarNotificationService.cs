using System;
using System.ComponentModel.Composition;
using BuildVision.Exports.Services;
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

        [ImportingConstructor]
        public StatusBarNotificationService([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowText(string str)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var statusBar = _serviceProvider.GetService(typeof(IVsStatusbar)) as IVsStatusbar;
            var settings = _serviceProvider.GetService(typeof(IPackageSettingsProvider)) as IPackageSettingsProvider;
            if (settings == null || statusBar == null)
                return;

            if (!settings.Settings.GeneralSettings.EnableStatusBarOutput)
                return;
            statusBar.FreezeOutput(0);
            statusBar.SetText(str);
        }

        public void ShowTextWithFreeze(string str)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var statusBar = _serviceProvider.GetService(typeof(IVsStatusbar)) as IVsStatusbar;
            var settings = _serviceProvider.GetService(typeof(IPackageSettingsProvider)) as IPackageSettingsProvider;
            if (settings == null || statusBar == null)
                return;

            if (!settings.Settings.GeneralSettings.EnableStatusBarOutput)
                return;
            statusBar.FreezeOutput(0);
            statusBar.SetText(str);
            statusBar.FreezeOutput(1);
        }
    }
}
