namespace TeamSorting.Model.New;

public class DisciplineInfo
{
    public string Name { get; set; } = string.Empty;
    public DisciplineDataType DataType { get; set; }
    public DisciplineSortType SortType { get; set; }
}

public enum DisciplineDataType
{
    Time,
    Number
}

public enum DisciplineSortType
{
    Asc,
    Desc
}