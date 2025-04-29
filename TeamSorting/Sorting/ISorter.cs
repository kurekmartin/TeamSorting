using TeamSorting.Models;

namespace TeamSorting.Sorting;

public interface ISorter
{
    public (List<Team> teams, string? seed) Sort(List<Member> members, int numberOfTeams, ProgressValues progress, string? seed = null);
}