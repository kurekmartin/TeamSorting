using TeamSorting.Models;

namespace TeamSorting.Sorting;

public class EvolutionSorter : ISorter
{
    public List<Team> Sort(List<Member> members, int numberOfTeams)
    {
        //TODO
        throw new NotImplementedException();
    }

    private List<Team> CreateRandomCombination(List<Member> members)
    {
        //TODO
        throw new NotImplementedException();
    }

    private double TeamScore(List<Member> members)
    {
        //TODO
        throw new NotImplementedException();
    }

    private Tuple<List<Member>, List<Member>> Cross(List<Member> parent1, List<Member> parent2)
    {
        //TODO
        throw new NotImplementedException();
    }

    private List<Member> Mutate(List<Member> members)
    {
        //TODO
        throw new NotImplementedException();
    }
}