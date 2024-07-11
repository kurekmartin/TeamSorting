namespace TeamSorting.Models;

public class Member(string name)
{
    public string Name { get; set; } = name;
    public List<string> With { get; set; } = [];
    public List<string> NotWith { get; set; } = [];
    //TODO try dictionary
    public List<DisciplineRecord> DisciplineRecords = [];
}