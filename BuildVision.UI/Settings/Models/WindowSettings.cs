using BuildVision.Common;
using BuildVision.UI.Models;
using BuildVision.UI.Settings.Models.ToolWindow;

namespace BuildVision.UI.Settings.Models
{
  public class WindowSettings : SettingsBase
  {
    public WindowStateAction WindowActionOnBuildBegin { get; set; }

    public WindowStateAction WindowActionOnBuildSuccess { get; set; }

    public WindowStateAction WindowActionOnBuildError { get; set; }

    public WindowSettings()
    {
      WindowActionOnBuildBegin = new WindowStateAction(WindowState.Show);
      WindowActionOnBuildSuccess = new WindowStateAction(WindowState.Nothing);
      WindowActionOnBuildError = new WindowStateAction(WindowState.Show);
    }
  }
}