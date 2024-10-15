using System.ComponentModel;
using Serilog;
using TeamSorting.Extensions;
using TeamSorting.Models;
using TeamSorting.Utils;

namespace TeamSorting.Sorting;

[Localizable(false)]
public class EvolutionSorter : ISorter
{
    private const int GenerationSize = 100;
    private const int MaxGenerations = 200;
    private const float CrossSelection = 0.5f;
    private const float ChanceOfMutation = 0.2f;
    private const float PreserveBestResults = 0.1f;
    private const int InvalidSolutionPenalty = 100000;

    public (List<Team> teams, string? seed) Sort(List<Member> members, int numberOfTeams, string? seed = null)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            seed = SeedGenerator.CreateSeed(10);
        }

        var random = new Random(seed.GetHashCode());
        var teamSizes = GetSizeOfTeams(members.Count, numberOfTeams);
        var currentGeneration = new List<List<Member>>(GenerationSize);
        for (var i = 0; i < GenerationSize; i++)
        {
            currentGeneration.Add(CreateRandomCombination(members, random));
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
                numberOfChildren, random);
            MutateSolution(newGeneration, random);

            newGeneration.AddRange(
                sortedGeneration.Take((int)Math.Round(GenerationSize * PreserveBestResults,
                    MidpointRounding.ToPositiveInfinity)).Select(pair => pair.Key));

            currentGeneration = newGeneration;
            generationNumber++;
        } while (generationNumber < MaxGenerations);

        var finalGeneration = currentGeneration.ToDictionary(g => g, g => SolutionScore(g, teamSizes))
            .OrderBy(solution => solution.Value).ToDictionary();

        return (ListToTeams(finalGeneration.First().Key, teamSizes), seed);
    }

    private static void LogGenerationStats(Dictionary<List<Member>, decimal> generationScores, int generationNumber)
    {
        int count = generationScores.Count;
        decimal min = generationScores.Values.Min();
        decimal max = generationScores.Values.Max();
        decimal avg = generationScores.Values.Average();
        decimal mean = generationScores.Values.ElementAt(count / 2);
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

    private static List<Member> CreateRandomCombination(List<Member> members, Random random)
    {
        var newList = new List<Member>(members);
        newList.Shuffle(random);
        return newList;
    }

    private static decimal SolutionScore(List<Member> members, List<int> teamSizes)
    {
        int numberOfTeams = teamSizes.Count;
        decimal score = 0;
        Dictionary<DisciplineInfo, List<decimal>> teamsDisciplineScores = [];
        var startIndex = 0;
        foreach (int teamSize in teamSizes)
        {
            var team = members.Skip(startIndex).Take(teamSize).ToList();
            int invalidMemberCount = Team.InvalidMemberCount(team);
            if (invalidMemberCount > 0)
            {
                score = ((decimal)InvalidSolutionPenalty / numberOfTeams) * invalidMemberCount;
                return score;
            }

            var teamScoreAverages = AverageScoreByDiscipline(team);
            foreach (var discipline in teamScoreAverages)
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
            decimal min = disciplineScore.Value.Min();
            decimal max = disciplineScore.Value.Max();
            score += decimal.Abs(min - max);
        }

        return score;
    }

    private static Dictionary<DisciplineInfo, decimal> AverageScoreByDiscipline(List<Member> members)
    {
        return members.SelectMany(member => member.Records.Values)
            .GroupBy(record => record.DisciplineInfo)
            .ToDictionary(g => g.Key, g => g.Sum(record => record.DecimalValue) / members.Count);
    }

    private List<List<Member>> CrossSolution(List<List<Member>> members, int numberOfChildren, Random random)
    {
        List<List<Member>> children = new(numberOfChildren);
        while (children.Count < numberOfChildren)
        {
            var parent1 = members[random.Next(members.Count)];
            var parent2 = members[random.Next(members.Count)];
            var newChildren = CrossMembers(parent1, parent2);
            children.AddRange(newChildren);
        }

        return children;
    }

    private static List<List<Member>> CrossMembers(List<Member> parent1, List<Member> parent2)
    {
        int startIndex = parent1.Count / 3;
        int endIndex = startIndex * 2 + 1;
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

        return [child1, child2];
    }

    private void MutateSolution(List<List<Member>> members, Random random)
    {
        foreach (var memberList in members)
        {
            MutateMembers(memberList, random);
        }
    }

    private void MutateMembers(List<Member> members, Random random)
    {
        for (var i = 0; i < members.Count; i++)
        {
            if (random.NextDouble() > ChanceOfMutation) continue;
            int index = random.Next(members.Count);
            members.Swap(i, index);
        }
    }

    private static List<Team> ListToTeams(List<Member> members, List<int> teamSizes)
    {
        List<Team> teams = [];
        var startIndex = 0;
        foreach (int teamSize in teamSizes)
        {
            var team = new Team(string.Format(Lang.Resources.Data_TeamName_Template, teams.Count + 1));
            team.AddMembers(members.Skip(startIndex).Take(teamSize));
            teams.Add(team);
            startIndex += teamSize;
        }

        return teams;
    }
}