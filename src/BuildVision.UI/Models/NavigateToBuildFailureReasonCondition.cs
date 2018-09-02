using BuildVision.UI.Helpers;

namespace BuildVision.UI.Models
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