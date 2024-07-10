namespace TeamSorting.Models;

public class Member(string name)
{
    public string Name { get; set; } = name;
    public List<string> With { get; set; } = [];
    public List<string> NotWith { get; set; } = [];
    public List<DisciplineRecord> DisciplineRecords = [];
}