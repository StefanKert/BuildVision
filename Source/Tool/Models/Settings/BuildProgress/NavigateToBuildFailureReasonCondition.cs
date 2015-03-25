using AlekseyNagovitsyn.BuildVision.Helpers;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildProgress
{
    public enum NavigateToBuildFailureReasonCondition
    {
        [DisplayString(ResourceName = "NavigateToBuildFailureReasonCondition_Disabled")]
        Disabled,

        [DisplayString(ResourceName = "NavigateToBuildFailureReasonCondition_OnErrorRaised")]
        OnErrorRaised,

        [DisplayString(ResourceName = "NavigateToBuildFailureReasonCondition_OnBuildDone")]
        OnBuildDone
    }
}