using System.ComponentModel.DataAnnotations;
using TeamSorting.Lang;

namespace TeamSorting.Enums;

public enum DisciplineDataType
{
    [Display(Name = nameof(Resources.Enum_DisciplineDataType_Number),ResourceType = typeof(Resources))]
    Number,
    [Display(Name = nameof(Resources.Enum_DisciplineDataType_Time),ResourceType = typeof(Resources))]
    Time
}