using AlekseyNagovitsyn.BuildVision.Helpers;
using BuildVision.UI;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Sorting
{
    public enum SortOrder
    {
        [DisplayString(ResourceName = nameof(Resources.EnumSortOrder_None))]
        None,

        [DisplayString(ResourceName = nameof(Resources.EnumSortOrder_Asc))]
        Ascending,

        [DisplayString(ResourceName = nameof(Resources.EnumSortOrder_Desc))]
        Descending
    }
}