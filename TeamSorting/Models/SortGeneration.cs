namespace TeamSorting.Models;

public class SortGeneration(List<Member> members, Dictionary<Team, int>? newMemberCount = null)
{
    public List<Member> Members { get; set; } = members;
    public Dictionary<Team, int>? NewMemberCount { get; set; } = newMemberCount;
}