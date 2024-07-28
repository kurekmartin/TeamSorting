namespace TeamSorting.Models;

public class DisciplineInfo(string name)
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; } = name;
    public DisciplineDataType DataType { get; set; }
    public SortOrder SortOrder { get; set; }
}

public enum DisciplineDataType
{
    Number,
    Time
}

public enum SortOrder
{
    Asc,
    Desc
}