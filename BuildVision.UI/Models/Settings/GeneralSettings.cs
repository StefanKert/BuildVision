using System.Runtime.Serialization;

using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildProgress;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings
{
    [DataContract]
    public class GeneralSettings
    {
        [DataMember]
        public BuildProgressSettings BuildProgressSettings { get; set; }

        [DataMember]
        public bool EnableStatusBarOutput { get; set; }

        [DataMember]
        public bool IndicatorsPanelVisible { get; set; }

        [DataMember]
        public bool StopBuildAfterFirstError { get; set; }

        [DataMember]
        public NavigateToBuildFailureReasonCondition NavigateToBuildFailureReason { get; set; }

        [DataMember]
        public bool FillProjectListOnBuildBegin { get; set; }

        public GeneralSettings()
        {
            BuildProgressSettings = new BuildProgressSettings();
            EnableStatusBarOutput = true;
            IndicatorsPanelVisible = true;
        }
    }
}