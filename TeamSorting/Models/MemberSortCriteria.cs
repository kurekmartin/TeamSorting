using TeamSorting.Enums;

namespace TeamSorting.Models;

public readonly record struct MemberSortCriteria(
    DisciplineInfo? Discipline,
    SortOrder SortOrder);