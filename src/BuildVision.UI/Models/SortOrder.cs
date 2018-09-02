using BuildVision.UI;
using BuildVision.UI.Helpers;

namespace BuildVision.UI.Models
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