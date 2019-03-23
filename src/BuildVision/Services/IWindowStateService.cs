using BuildVision.UI.Settings.Models.ToolWindow;

namespace BuildVision.Tool.Building
{
    public interface IWindowStateService
    {
        void ApplyToolWindowStateAction(WindowStateAction windowStateAction);
    }
}
