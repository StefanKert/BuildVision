using AlekseyNagovitsyn.BuildVision.Helpers;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Sorting
{
    public enum SortOrder
    {
        [DisplayString(ResourceName = "EnumSortOrder_None")]
        None,

        [DisplayString(ResourceName = "EnumSortOrder_Asc")]
        Ascending,

        [DisplayString(ResourceName = "EnumSortOrder_Desc")]
        Descending
    }
}