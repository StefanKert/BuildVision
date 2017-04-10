using AlekseyNagovitsyn.BuildVision.Helpers;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildProgress
{
    public enum ResetTaskBarItemInfoCondition
    {
        [DisplayString(ResourceName = nameof(Resources.ResetTaskBarItemInfoCondition_Never))]
        Never,

        [DisplayString(ResourceName = nameof(Resources.ResetTaskBarItemInfoCondition_Immediately))]
        Immediately,

        [DisplayString(ResourceName = nameof(Resources.ResetTaskBarItemInfoCondition_AfterDelay))]
        AfterDelay,

        [DisplayString(ResourceName = nameof(Resources.ResetTaskBarItemInfoCondition_ByMouseClick))]
        ByMouseClick
    }
}