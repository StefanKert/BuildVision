using BuildVision.UI.Helpers;

namespace BuildVision.UI.Models
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