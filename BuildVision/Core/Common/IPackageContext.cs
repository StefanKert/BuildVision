using System;

using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings;

using EnvDTE;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE80;

namespace AlekseyNagovitsyn.BuildVision.Core.Common
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