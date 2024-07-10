namespace TeamSorting.Models;

public class Team(string name)
{
    public string Name { get; set; } = name;
    public List<Member> Members { get; init; } = [];
}