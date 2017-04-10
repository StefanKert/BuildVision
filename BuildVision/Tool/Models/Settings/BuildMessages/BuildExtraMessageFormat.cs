using AlekseyNagovitsyn.BuildVision.Helpers;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildMessages
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