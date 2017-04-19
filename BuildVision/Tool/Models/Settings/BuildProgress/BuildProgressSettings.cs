using System.Runtime.Serialization;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildProgress
{
    [DataContract]
    public class BuildProgressSettings
    {
        [DataMember]
        public bool TaskBarProgressEnabled { get; set; }

        [DataMember]
        public ResetTaskBarItemInfoCondition ResetTaskBarProgressAfterBuildDone { get; set; } = ResetTaskBarItemInfoCondition.ByMouseClick;

        [DataMember]
        public int ResetTaskBarProgressDelay { get; set; } = 5000;
    }
}