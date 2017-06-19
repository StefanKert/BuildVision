using BuildVision.UI.Models;
using System.Runtime.Serialization;

namespace BuildVision.UI.Settings.Models.BuildProgress
{
  public class BuildProgressSettings
  {
    public bool TaskBarProgressEnabled { get; set; }

    public ResetTaskBarItemInfoCondition ResetTaskBarProgressAfterBuildDone { get; set; } = ResetTaskBarItemInfoCondition.ByMouseClick;

    public int ResetTaskBarProgressDelay { get; set; } = 5000;
  }
}