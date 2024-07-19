using System.Collections.ObjectModel;
using TeamSorting.Extensions;
using TeamSorting.Models;

namespace TeamSorting.Sorting;

public class EvolutionSorter : ISorter
{
    private readonly Random _random = new Random();
    private const int GenerationSize = 100;
    private const int MaxGenerations = 100;
    private const float ChanceOfMutation = 0.1f;

    public List<Team> Sort(List<Member> members, int numberOfTeams)
    {
        var teamSizes = GetSizeOfTeams(members.Count, numberOfTeams);
        List<List<Member>> currentGeneration = [];
        for (var i = 0; i < GenerationSize; i++)
        {
            currentGeneration.Add(CreateRandomCombination(members));
        }

        var generationNumber = 0;
        do
        {
            currentGeneration = currentGeneration.OrderBy(solution => SolutionScore(solution, teamSizes)).ToList();

            var newGeneration =
                CrossSolution(currentGeneration.Take(GenerationSize / 2).ToList(),
                    (int)Math.Round(GenerationSize * 0.9, MidpointRounding.ToNegativeInfinity));
            MutateSolution(newGeneration);

            newGeneration.AddRange(
                currentGeneration.Take((int)Math.Round(GenerationSize * 0.1, MidpointRounding.ToPositiveInfinity)));

            generationNumber++;
        } while (generationNumber < MaxGenerations);

        return ListToTeams(currentGeneration.First(), teamSizes);
    }

    private static List<int> GetSizeOfTeams(int membersCount, int numberOfTeams)
    {
        int minTeamSize = membersCount / numberOfTeams;
        int teamsWithExtraMembers = membersCount % numberOfTeams;

        var sizeList = Enumerable.Repeat(minTeamSize, numberOfTeams).ToList();
        for (var i = 0; i < teamsWithExtraMembers; i++)
        {
            sizeList[i]++;
        }

        return sizeList;
    }

    private static List<Member> CreateRandomCombination(List<Member> members)
    {
        var newList = new List<Member>(members);
        newList.Shuffle();
        return newList;
    }

    private static double SolutionScore(List<Member> members, List<int> teamSizes)
    {
        double score = 0;
        Dictionary<DisciplineInfo, List<double>> teamsDisciplineScores = [];
        var startIndex = 0;
        foreach (int teamSize in teamSizes)
        {
            var team = members.Skip(startIndex).Take(teamSize).ToList();
            if (!Team.IsValidCheck(team))
            {
                score = double.MaxValue;
                return score;
            }

            var teamScoreSums = SumScoreByDiscipline(team);
            foreach (var discipline in teamScoreSums)
            {
                if (!teamsDisciplineScores.TryGetValue(discipline.Key, out var listOfValues))
                {
                    teamsDisciplineScores.Add(discipline.Key, [discipline.Value]);
                }
                else
                {
                    listOfValues.Add(discipline.Value);
                }
            }

            startIndex += teamSize;
        }

        foreach (var disciplineScore in teamsDisciplineScores)
        {
            double min = disciplineScore.Value.Min();
            double max = disciplineScore.Value.Max();
            score += double.Abs(min - max);
        }

        return score;
    }

    private static Dictionary<DisciplineInfo, double> SumScoreByDiscipline(List<Member> members)
    {
        return members.SelectMany(member => member.Records.Values)
            .GroupBy(record => record.DisciplineInfo)
            .ToDictionary(g => g.Key, g => g.Sum(record => record.DoubleValue));
    }

    private List<List<Member>> CrossSolution(List<List<Member>> members, int numberOfChildren)
    {
        //TODO
        throw new NotImplementedException();
    }

    private Tuple<List<Member>, List<Member>> CrossMembers(List<Member> parent1, List<Member> parent2)
    {
        //TODO
        throw new NotImplementedException();
    }

    private void MutateSolution(List<List<Member>> members)
    {
        foreach (var memberList in members)
        {
            MutateMembers(memberList);
        }
    }

    private void MutateMembers(List<Member> members)
    {
        foreach (var member in members)
        {
            if (_random.NextDouble() > ChanceOfMutation) continue;
            int index = _random.Next(members.Count);
            members.Swap(members.IndexOf(member), index);
        }
    }

    private static List<Team> ListToTeams(List<Member> members, List<int> teamSizes)
    {
        List<Team> teams = [];
        var startIndex = 0;
        foreach (int teamSize in teamSizes)
        {
            teams.Add(
                new Team($"Team{teams.Count + 1}")
                {
                    Members = new ObservableCollection<Member>(members.Skip(startIndex).Take(teamSize))
                });
            startIndex += teamSize;
        }

        return teams;
    }
}