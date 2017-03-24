using System;

using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings;

using EnvDTE;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace AlekseyNagovitsyn.BuildVision.Core.Common
{
    public interface IPackageContext
    {
        ControlSettings ControlSettings { get; }
        void SaveSettings();
        event EventHandler ControlSettingsChanged;
        void NotifyControlSettingsChanged();

        DTE GetDTE();
        ToolWindowPane GetToolWindow();
        IVsUIShell GetUIShell();
        IVsSolution GetSolution();
        IVsStatusbar GetStatusBar();
        void ShowOptionPage(Type optionsPageType);
    }
}