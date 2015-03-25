using AlekseyNagovitsyn.BuildVision.Helpers;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildMessages
{
    public enum BuildExtraMessageFormat
    {
        [DisplayString(ResourceName = "EnumBuildStateExtraLabelTemplate_Advanced")]
        Custom,

        [DisplayString(ResourceName = "EnumBuildStateExtraLabelTemplate_TotalSeconds")]
        TotalSeconds,

        [DisplayString(ResourceName = "EnumBuildStateExtraLabelTemplate_TotalMinutes")]
        TotalMinutes,

        [DisplayString(ResourceName = "EnumBuildStateExtraLabelTemplate_TotalMinutesWithSeconds")]
        TotalMinutesWithSeconds
    }
}