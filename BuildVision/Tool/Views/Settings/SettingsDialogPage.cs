using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using BuildVision.Common;
using BuildVision.Core;
using BuildVision.UI.Settings.Models;

namespace BuildVision.Views.Settings
{
    public abstract class SettingsDialogPage<TControl, TSettings> : UIElementDialogPage
        where TControl : FrameworkElement, new() 
        where TSettings : SettingsBase, new()
  {
        private static bool _notifySettingsChangedOnce;

        private TSettings _editSettings;

        private FrameworkElement _ctrl;

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
            if (_editSettings == null)
                _editSettings = Settings.Clone<TSettings>();

            if (_ctrl.DataContext == null)
                _ctrl.DataContext = _editSettings;

            base.OnActivate(e);
        }

        protected override void OnApply(PageApplyEventArgs args)
        {
            if (args.ApplyBehavior == ApplyKind.Apply)
            {
                _notifySettingsChangedOnce = true;
                Settings = _editSettings;
                Package.SaveSettings();
            }

            base.OnApply(args);
        }

        protected override void OnClosed(EventArgs e)
        {
            _editSettings = null;
            if(_ctrl != null)
                _ctrl.DataContext = null;

            if (_notifySettingsChangedOnce)
            {
                _notifySettingsChangedOnce = false;
                Package.NotifyControlSettingsChanged();
            }

            base.OnClosed(e);
        }

        public override void ResetSettings()
        {
            Settings = new TSettings();
            Package.SaveSettings();
            Package.NotifyControlSettingsChanged();

            base.ResetSettings();
        }

        protected ControlSettings ControlSettings => Package.ControlSettings;

        protected IPackageContext Package
        {
            get
            {
                var package = (IPackageContext)GetService(typeof(BuildVisionPackage));
                Debug.Assert(package != null);
                return package;
            }
        }
    }
}
