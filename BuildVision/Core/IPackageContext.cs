using System;

using EnvDTE;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE80;
using BuildVision.UI.Settings.Models;

namespace BuildVision.Core
{
    public interface IPackageContext
    {
        ControlSettings ControlSettings { get; }
        void SaveSettings();
        event Action<ControlSettings> ControlSettingsChanged;
        void NotifyControlSettingsChanged();

        DTE GetDTE();
        DTE2 GetDTE2();
        ToolWindowPane GetToolWindow();
        IVsUIShell GetUIShell();
        IVsSolution GetSolution();
        IVsStatusbar GetStatusBar();
        void ShowOptionPage(Type optionsPageType);
    }
}