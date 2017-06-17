using BuildVision.UI;
using BuildVision.UI.Helpers;

namespace BuildVision.UI.Models
{
    public enum WindowState
    {
        [DisplayString(ResourceName = nameof(Resources.EnumWindowState_Nothing))]
        Nothing,

        [DisplayString(ResourceName = nameof(Resources.EnumWindowState_Show))]
        Show,

        [DisplayString(ResourceName = nameof(Resources.EnumWindowState_ShowNoActivate))]
        ShowNoActivate,

        [DisplayString(ResourceName = nameof(Resources.EnumWindowState_Hide))]
        Hide,

        [DisplayString(ResourceName = nameof(Resources.EnumWindowState_Close))]
        Close
    }
}