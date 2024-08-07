using System.ComponentModel.DataAnnotations;
using TeamSorting.Lang;

namespace TeamSorting.Enums;

public enum SortOrder
{
    [Display(Name = nameof(Resources.Enum_SortOrder_Asc),ResourceType = typeof(Resources))]
    Asc,
    [Display(Name = nameof(Resources.Enum_SortOrder_Desc),ResourceType = typeof(Resources))]
    Desc
}