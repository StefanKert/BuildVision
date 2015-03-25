using AlekseyNagovitsyn.BuildVision.Helpers;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.ToolWindow
{
    public enum WindowState
    {
        [DisplayString(ResourceName = "EnumWindowState_Nothing")]
        Nothing,

        [DisplayString(ResourceName = "EnumWindowState_Show")]
        Show,

        [DisplayString(ResourceName = "EnumWindowState_ShowNoActivate")]
        ShowNoActivate,

        [DisplayString(ResourceName = "EnumWindowState_Hide")]
        Hide,

        [DisplayString(ResourceName = "EnumWindowState_Close")]
        Close
    }
}