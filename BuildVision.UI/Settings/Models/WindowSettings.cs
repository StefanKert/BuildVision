using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.ToolWindow;
using BuildVision.Common;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings
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