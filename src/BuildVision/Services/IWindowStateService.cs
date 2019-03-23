using BuildVision.UI.Settings.Models.ToolWindow;
using Microsoft.VisualStudio.Shell;

namespace BuildVision.Tool.Building
{
    public interface IWindowStateService
    {
        void ApplyToolWindowStateAction(WindowStateAction windowStateAction);
        void Initialize(ToolWindowPane toolWindowPane);
    }
}
