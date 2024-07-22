using System.Collections.ObjectModel;
using Serilog;
using TeamSorting.Extensions;
using TeamSorting.Models;

namespace TeamSorting.Sorting;

public class EvolutionSorter : ISorter
{
    private readonly Random _random = new();
    private const int GenerationSize = 100;
    private const int MaxGenerations = 200;
    private const float CrossSelection = 0.5f;
    private const float ChanceOfMutation = 0.2f;
    private const float PreserveBestResults = 0.1f;

    public List<Team> Sort(List<Member> members, int numberOfTeams)
    {
        var teamSizes = GetSizeOfTeams(members.Count, numberOfTeams);
        var currentGeneration = new List<List<Member>>(GenerationSize);
        for (var i = 0; i < GenerationSize; i++)
        {
            currentGeneration.Add(CreateRandomCombination(members));
        }

        var generationNumber = 0;
        do
        {
            var sortedGeneration = currentGeneration.ToDictionary(g => g, g => SolutionScore(g, teamSizes))
                .OrderBy(solution => solution.Value).ToDictionary();


            LogGenerationStats(sortedGeneration, generationNumber);

            var crossSelection = (int)Math.Round(GenerationSize * CrossSelection, MidpointRounding.ToPositiveInfinity);
            var numberOfChildren =
                (int)Math.Round(GenerationSize * (1 - PreserveBestResults), MidpointRounding.ToNegativeInfinity);
            var newGeneration = CrossSolution(sortedGeneration.Keys.Take(crossSelection).ToList(),
                numberOfChildren);
            MutateSolution(newGeneration);

            newGeneration.AddRange(
                sortedGeneration.Take((int)Math.Round(GenerationSize * PreserveBestResults,
                    MidpointRounding.ToPositiveInfinity)).Select(pair => pair.Key));

            currentGeneration = newGeneration;
            generationNumber++;
        } while (generationNumber < MaxGenerations);

        var finalGeneration = currentGeneration.ToDictionary(g => g, g => SolutionScore(g, teamSizes))
            .OrderBy(solution => solution.Value).ToDictionary();

        return ListToTeams(finalGeneration.First().Key, teamSizes);
    }

    private static void LogGenerationStats(Dictionary<List<Member>, double> generationScores, int generationNumber)
    {
        int count = generationScores.Count;
        double min = generationScores.Values.Min();
        double max = generationScores.Values.Max();
        double avg = generationScores.Values.Average();
        double mean = generationScores.Values.ElementAt(count / 2);
        Log.Information(
            "Generation number {num} - count: {count} - min: {min} - max: {max} - avg: {avg} - mean: {mean}",
            generationNumber, count, min, max, avg, mean);
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
        List<List<Member>> children = new(numberOfChildren);
        while (children.Count < numberOfChildren)
        {
            var parent1 = members[_random.Next(members.Count)];
            var parent2 = members[_random.Next(members.Count)];
            var newChildren = CrossMembers(parent1, parent2);
            children.AddRange(newChildren);
        }

        return children;
    }

    private static List<List<Member>> CrossMembers(List<Member> parent1, List<Member> parent2)
    {
        int startIndex = parent1.Count / 3;
        int endIndex = (startIndex * 2) + 1;
        if (parent1.Count % 3 == 0)
        {
            endIndex--;
        }

        var parent1Section = parent1[startIndex..endIndex];
        var parent2Section = parent2[startIndex..endIndex];

        List<Member> parent1Rest = [..parent1[endIndex..], ..parent1[..endIndex]];
        parent1Rest = parent1Rest.Except(parent2Section).ToList();

        List<Member> parent2Rest = [..parent2[endIndex..], ..parent2[..endIndex]];
        parent2Rest = parent2Rest.Except(parent1Section).ToList();

        int endSectionSize = parent1.Count - endIndex + 1;
        List<Member> child1 =
        [
            ..parent1Rest[endSectionSize..],
            ..parent2Section,
            ..parent1Rest[..endSectionSize]
        ];
        List<Member> child2 =
        [
            ..parent2Rest[endSectionSize..],
            ..parent1Section,
            ..parent2Rest[..endSectionSize]
        ];

        // Log.Information("Parent1: {0} - parent2: {1} - child1: {2} - child2: {3}",
        //     parent1.Count, parent2.Count, child1.Count, child2.Count);

        return [child1, child2];
    }

    private static List<Member> CreateChild(List<Member> parent, List<Member> newSection, int startIndex, int endIndex)
    {
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
        for (var i = 0; i < members.Count; i++)
        {
            if (_random.NextDouble() > ChanceOfMutation) continue;
            int index = _random.Next(members.Count);
            members.Swap(i, index);
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