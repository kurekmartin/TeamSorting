using TeamSorting.Models;

namespace TeamSorting.Sorting;

public interface ISorter
{
    public string? Sort(List<Member> members, List<Team> teams, ProgressValues progress, string? seed = null);
}