using DynamicData.Binding;

namespace TeamSorting.Models;

public struct MemberSortCriteria(DisciplineInfo? disciplineInfo, SortOrder sortOrder)
{
    public readonly DisciplineInfo? Discipline = disciplineInfo;
    public readonly SortOrder SortOrder = sortOrder;
}