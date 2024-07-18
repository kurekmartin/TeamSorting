using TeamSorting.Models;

namespace TeamSorting.Sorting;

public interface ISorter
{
    public List<Team> Sort(List<Member> members, int numberOfTeams);
}