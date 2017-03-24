using AlekseyNagovitsyn.BuildVision.Helpers;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildProgress
{
    public enum ResetTaskBarItemInfoCondition
    {
        [DisplayString(ResourceName = "ResetTaskBarItemInfoCondition_Never")]
        Never,

        [DisplayString(ResourceName = "ResetTaskBarItemInfoCondition_Immediately")]
        Immediately,

        [DisplayString(ResourceName = "ResetTaskBarItemInfoCondition_AfterDelay")]
        AfterDelay,

        [DisplayString(ResourceName = "ResetTaskBarItemInfoCondition_ByMouseClick")]
        ByMouseClick
    }
}