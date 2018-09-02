using System.Runtime.Serialization;

using BuildVision.Common;

namespace BuildVision.UI.Settings.Models
{
  public class ControlSettings : SettingsBase
  {
    public BuildMessagesSettings BuildMessagesSettings { get; set; }

    public GeneralSettings GeneralSettings { get; set; }
    
    public GridSettings GridSettings { get; set; }
    
    public ProjectItemSettings ProjectItemSettings { get; set; }
    
    public WindowSettings WindowSettings { get; set; }

    public ControlSettings()
    {
      Init();
    }
    
    private void OnDeserialized(StreamingContext context)
    {
      Init();
    }

    private void Init()
    {
      if (GeneralSettings == null)
        GeneralSettings = new GeneralSettings();

      if (WindowSettings == null)
        WindowSettings = new WindowSettings();

      if (GridSettings == null)
        GridSettings = new GridSettings();

      if (BuildMessagesSettings == null)
        BuildMessagesSettings = new BuildMessagesSettings();

      if (ProjectItemSettings == null)
        ProjectItemSettings = new ProjectItemSettings();
    }
  }
}