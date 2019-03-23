using System;
using System.ComponentModel.Composition;
using System.Linq;
using BuildVision.Helpers;
using BuildVision.UI.Settings.Models.ToolWindow;
using BuildVision.Views.Settings;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using WindowState = BuildVision.UI.Models.WindowState;

namespace BuildVision.Tool.Building
{
    public class WindowStateService : IWindowStateService
    {
        private DTE _dte;
        private IVsWindowFrame _windowFrame;
        private Window _window;
        private IServiceProvider _serviceProvider;
        private AsyncPackage _package;

        [ImportingConstructor]
        public WindowStateService(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider, 
            AsyncPackage package)
        {
            _serviceProvider = serviceProvider;
            _package = package;
        }

        private bool IsVisible()
        {
            int result = _windowFrame.IsOnScreen(out var isOnScreen);
            return (result == VSConstants.S_OK && isOnScreen == 1 && _windowFrame.IsVisible() == VSConstants.S_OK);
        }

        private void Hide()
        {
            if (!IsVisible())
            {
                return;
            }

            var window = _window;
            switch (window.WindowState)
            {
                case vsWindowState.vsWindowStateNormal:
                    if (!window.IsFloating)
                    {
                        MinimizeToolWindow();
                    }
                    else
                    {
                        _windowFrame.Hide();
                    }

                    break;

                case vsWindowState.vsWindowStateMaximize:
                    _windowFrame.Hide();
                    break;

                case vsWindowState.vsWindowStateMinimize:
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private void MinimizeToolWindow()
        {
            // Note: throws exception if _window.WindowState = vsWindowState.vsWindowStateMaximize 
            // or _window.IsFloating.
            _window.AutoHides = true;

            var win = _dte.ActiveWindow;
            if (win != _window)
            {
                win.Activate();
                return;
            }

            var doc = _dte.ActiveDocument;
            if (doc != null)
                doc.Activate();
        }

        private static Window GetWindowInstance(DTE dte, Guid windowGuid)
        {
            var windows = dte.Windows;
            for (int i = 1; i <= windows.Count; i++)
            {
                var window = windows.Item(i);
                if (Guid.TryParse(window.ObjectKind, out var guid) && guid == windowGuid)
                {
                    return window;
                }
            }

            return null;
        }

        private void ApplyToolWindowStateAction(WindowState windowState)
        {
            switch (windowState)
            {
                case WindowState.Nothing:
                    break;
                case WindowState.Show:
                    _windowFrame.Show();
                    break;
                case WindowState.ShowNoActivate:
                    _windowFrame.ShowNoActivate();
                    break;
                case WindowState.Hide:
                    Hide();
                    break;
                case WindowState.Close:
                    _windowFrame.Hide();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(windowState));
            }
        }

        public void ApplyToolWindowStateAction(WindowStateAction windowStateAction)
        {
            if (_window == null || _windowFrame == null)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _dte = _serviceProvider.GetService(typeof(DTE)) as DTE;
                if (_dte == null)
                    throw new InvalidOperationException("Unable to get DTE instance.");
                _windowFrame = (IVsWindowFrame)(_package.FindToolWindow(typeof(BuildVisionPane), 0, false) ?? _package.FindToolWindow(typeof(BuildVisionPane), 0, true));
                _window = GetWindowInstance(_dte, typeof(BuildVisionPane).GUID);
                if (_window == null)
                    throw new InvalidOperationException("Unable to get Window instance.");
            }
            ApplyToolWindowStateAction(windowStateAction.State); 
        }
    }
}
