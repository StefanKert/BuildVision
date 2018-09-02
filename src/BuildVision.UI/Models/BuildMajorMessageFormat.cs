using BuildVision.UI.Helpers;

namespace BuildVision.UI.Models
{
    public enum BuildMajorMessageFormat
    {
        [DisplayString(ResourceName = nameof(Resources.EnumBuildStateLabelTemplate_Default))]
        Entire,

        [DisplayString(ResourceName = nameof(Resources.EnumBuildStateLabelTemplate_ShortForm))]
        Unnamed
    }
}