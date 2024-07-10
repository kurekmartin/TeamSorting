namespace TeamSorting.Model.New;

public class DisciplineInfo(string name)
{
    public string Name { get; set; } = name;
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