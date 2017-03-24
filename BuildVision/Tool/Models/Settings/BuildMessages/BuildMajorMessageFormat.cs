using AlekseyNagovitsyn.BuildVision.Helpers;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildMessages
{
    public enum BuildMajorMessageFormat
    {
        [DisplayString(ResourceName = "EnumBuildStateLabelTemplate_Default")]
        Entire,

        [DisplayString(ResourceName = "EnumBuildStateLabelTemplate_ShortForm")]
        Unnamed
    }
}