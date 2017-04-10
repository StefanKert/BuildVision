using AlekseyNagovitsyn.BuildVision.Helpers;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildProgress
{
    public enum NavigateToBuildFailureReasonCondition
    {
        [DisplayString(ResourceName = nameof(Resources.NavigateToBuildFailureReasonCondition_Disabled))]
        Disabled,

        [DisplayString(ResourceName = nameof(Resources.NavigateToBuildFailureReasonCondition_OnErrorRaised))]
        OnErrorRaised,

        [DisplayString(ResourceName = nameof(Resources.NavigateToBuildFailureReasonCondition_OnBuildDone))]
        OnBuildDone
    }
}