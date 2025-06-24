namespace TeamSorting.Models;

public class SortGeneration(List<Member> members, Dictionary<Team, int>? teamMemberCount = null)
{
    public List<Member> Members { get; set; } = members;
    public Dictionary<Team, int>? TeamMemberCount { get; set; } = teamMemberCount;
}