using System.Runtime.Serialization;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.ToolWindow
{
  public class WindowStateAction
  {
    public WindowState State { get; set; }

    public WindowStateAction(WindowState state)
    {
      State = state;
    }

    public WindowStateAction() { }
  }
}