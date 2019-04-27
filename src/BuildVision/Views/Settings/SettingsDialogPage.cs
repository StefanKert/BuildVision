using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using BuildVision.Common;
using BuildVision.Core;
using BuildVision.UI.Settings.Models;
using Microsoft;
using BuildVision.Common.Diagnostics;

namespace BuildVision.Views.Settings
{
    public abstract class SettingsDialogPage<TControl, TSettings> : UIElementDialogPage
        where TControl : FrameworkElement, new()
        where TSettings : SettingsBase, new()
    {
        private static bool _notifySettingsChangedOnce;

        private TSettings _editSettings;

        private FrameworkElement _ctrl;
        private IPackageSettingsProvider _packageSettingsProvider;

        protected abstract TSettings Settings { get; set; }

        protected override UIElement Child
        {
            get
            {
                return _ctrl ?? (_ctrl = new TControl());
            }
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            _packageSettingsProvider = Package.GetGlobalService(typeof(IPackageSettingsProvider)) as IPackageSettingsProvider;
            Assumes.Present(_packageSettingsProvider);
            if (_editSettings == null)
            {
                _editSettings = Settings.Clone<TSettings>();
            }

            if (_ctrl.DataContext == null)
            {
                _ctrl.DataContext = _editSettings;
            }

            base.OnActivate(e);
            DiagnosticsClient.TrackPageView(GetType().Name);
        }

        protected override void OnApply(PageApplyEventArgs args)
        {
            if (args.ApplyBehavior == ApplyKind.Apply)
            {
                _notifySettingsChangedOnce = true;
                Settings = _editSettings;
                _packageSettingsProvider.Save();
            }

            base.OnApply(args);
        }

        protected override void OnClosed(EventArgs e)
        {
            _editSettings = null;
            if (_ctrl != null)
            {
                _ctrl.DataContext = null;
            }

            if (_notifySettingsChangedOnce)
            {
                _notifySettingsChangedOnce = false;
                //Package.NotifyControlSettingsChanged();
            }

            base.OnClosed(e);
        }

        public override void ResetSettings()
        {
            Settings = new TSettings();
            _packageSettingsProvider.Save();

            base.ResetSettings();
        }

        protected ControlSettings ControlSettings => _packageSettingsProvider?.Settings;
    }
}
