namespace TeamSorting.Model;

public class TeamMember
{
    public string Name { get; set; } = string.Empty;
    public List<string> With { get; set; } = [];
    public List<string> NotWith { get; set; } = [];
    public int Age { get; set; }
    public List<Discipline> Disciplines { get; set; } = [];
}