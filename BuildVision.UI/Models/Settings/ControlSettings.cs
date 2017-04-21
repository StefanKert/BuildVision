using System.Runtime.Serialization;

using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildMessages;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings
{
    [DataContract]
    public class ControlSettings
    {
        [DataMember]
        public GeneralSettings GeneralSettings { get; set; }

        [DataMember]
        public WindowSettings WindowSettings { get; set; }

        [DataMember]
        public GridSettings GridSettings { get; set; }

        [DataMember]
        public BuildMessagesSettings BuildMessagesSettings { get; set; }

        [DataMember]
        public ProjectItemSettings ProjectItemSettings { get; set; }

        public ControlSettings()
        {
            Init();
        }

        [OnDeserialized]
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