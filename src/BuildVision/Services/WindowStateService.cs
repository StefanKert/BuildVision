using System;
using System.ComponentModel.Composition;
using BuildVision.Core;
using BuildVision.UI.Settings.Models.ToolWindow;
using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using WindowState = BuildVision.UI.Models.WindowState;

namespace BuildVision.Tool.Building
{
    [Export(typeof(IWindowStateService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class WindowStateService : IWindowStateService
    {
        private DTE _dte;
        private IVsWindowFrame _windowFrame;
        private Window _window;
        private Window _currentActiveWindow;
        private readonly IServiceProvider _serviceProvider;

        [ImportingConstructor]
        public WindowStateService(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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

            var win = _currentActiveWindow;
            if (_dte.ActiveWindow == _window)
            {
                if (win != null && win != _window)
                {
                    win.Activate();
                    return;
                }
            }
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
                    _currentActiveWindow = _dte.ActiveWindow;
                    _windowFrame.Show();
                    break;
                case WindowState.ShowNoActivate:
                    _currentActiveWindow = _dte.ActiveWindow;
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

        public void Initialize(ToolWindowPane toolWindowPane)
        {
            if (toolWindowPane == null)
            {
                return;
            }

            if (_window == null || _windowFrame == null)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _dte = _serviceProvider.GetService(typeof(DTE)) as DTE;
                Assumes.Present(_dte);
                _windowFrame = (IVsWindowFrame)toolWindowPane.Frame;
                _window = GetWindowInstance(_dte, typeof(BuildVisionPane).GUID);
                if (_window == null)
                {
                    throw new InvalidOperationException("Unable to get Window instance.");
                }
            }
        }

        public void ApplyToolWindowStateAction(WindowStateAction windowStateAction)
        {
            var toolWindow = BuildVisionPackage.BuildVisionPackageInstance.FindToolWindow(typeof(BuildVisionPane), 0, false);
            if (toolWindow == null)
            {
                return;
            }

            Initialize(toolWindow);
            ApplyToolWindowStateAction(windowStateAction.State);
        }
    }
}
