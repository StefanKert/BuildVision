using BuildVision.UI.Helpers;

namespace BuildVision.UI.Models
{
    public enum BuildExtraMessageFormat
    {
        [DisplayString(ResourceName = nameof(Resources.EnumBuildStateExtraLabelTemplate_Advanced))]
        Custom,

        [DisplayString(ResourceName = nameof(Resources.EnumBuildStateExtraLabelTemplate_TotalSeconds))]
        TotalSeconds,

        [DisplayString(ResourceName = nameof(Resources.EnumBuildStateExtraLabelTemplate_TotalMinutes))]
        TotalMinutes,

        [DisplayString(ResourceName = nameof(Resources.EnumBuildStateExtraLabelTemplate_TotalMinutesWithSeconds))]
        TotalMinutesWithSeconds
    }
}