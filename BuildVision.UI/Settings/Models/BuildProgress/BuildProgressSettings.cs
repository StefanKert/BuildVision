using System.Runtime.Serialization;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildProgress
{
  public class BuildProgressSettings
  {
    public bool TaskBarProgressEnabled { get; set; }

    public ResetTaskBarItemInfoCondition ResetTaskBarProgressAfterBuildDone { get; set; } = ResetTaskBarItemInfoCondition.ByMouseClick;

    public int ResetTaskBarProgressDelay { get; set; } = 5000;
  }
}