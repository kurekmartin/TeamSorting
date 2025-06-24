using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TeamSorting.Extensions;
using TeamSorting.Models;
using TeamSorting.Utils;

namespace TeamSorting.Sorting;

[Localizable(false)]
public class EvolutionSorter(ILogger<EvolutionSorter> logger) : ISorter
{
    private const int GenerationSize = 100;
    private const int MaxGenerations = 200;
    private const float CrossSelection = 0.5f;
    private const float ChanceOfMutation = 0.2f;
    private const float PreserveBestResults = 0.1f;
    private const int InvalidSolutionPenalty = 100000;
    private const int PriorityMultiplier = 10;

    public (List<Team> teams, string? seed) Sort(List<Member> members, List<Team> teams, ProgressValues progress, string? seed = null)
    {
        var stopwatch = Stopwatch.StartNew();
        logger.LogInformation("Sorting using evolutionary algorithm");
        logger.LogInformation("Sorting settings:" +
                              " GenerationSize - {generationSize}," +
                              " MaxGenerations - {maxGenerations}," +
                              " CrossSelection - {crossSelection}," +
                              " ChanceOfMutation - {chanceOfMutation}," +
                              " PreserveBestResults - {preserveBestResults}," +
                              " InvalidSolutionPenalty - {invalidSolutionPenalty}," +
                              " PriorityMultiplier - {priorityMultiplier}"
            , GenerationSize, MaxGenerations, CrossSelection, ChanceOfMutation, PreserveBestResults, InvalidSolutionPenalty, PriorityMultiplier);

        progress.Minimum = 0;
        progress.Maximum = MaxGenerations;
        progress.Value = 0;
        progress.IsIndeterminate = true;
        progress.Text = Lang.Resources.Sorting_ProgressText;

        int numberOfTeams = teams.Count;
        List<Member> membersToSort = members.Where(member => member.AllowTeamChange).ToList();

        if (string.IsNullOrWhiteSpace(seed))
        {
            logger.LogInformation("Seed not set, creating new seed");
            seed = SeedGenerator.CreateSeed(10);
        }

        logger.LogInformation("Starting sorting with seed {seed}", seed);

        var random = new Random(seed.GetHashCode());
        int minTeamSize = GetMinSizeOfTeams(members.Count, TODO);
        logger.LogInformation("Total members {memberCount}, min team size: {teamSize}", members.Count, string.Join(", ", minTeamSize));
        var currentGeneration = new List<SortGeneration>(GenerationSize);
        for (var i = 0; i < GenerationSize; i++)
        {
            currentGeneration.Add(CreateRandomCombination(membersToSort, random));
        }

        progress.IsIndeterminate = false;
        var generationNumber = 0;
        do
        {
            progress.Value = generationNumber;
            Dictionary<SortGeneration, decimal> sortedGeneration = SortGeneration(currentGeneration, minTeamSize, teams);

            LogGenerationStats(sortedGeneration, generationNumber);

            var crossSelection = (int)Math.Round(GenerationSize * CrossSelection, MidpointRounding.ToPositiveInfinity);
            var numberOfChildren = (int)Math.Round(GenerationSize * (1 - PreserveBestResults), MidpointRounding.ToNegativeInfinity);
            List<SortGeneration> newGeneration = CrossSolution(sortedGeneration.Keys.Take(crossSelection).ToList(), numberOfChildren, random);
            MutateSolution(newGeneration, random);

            newGeneration.AddRange(
                sortedGeneration.Take((int)Math.Round(GenerationSize * PreserveBestResults,
                    MidpointRounding.ToPositiveInfinity)).Select(pair => pair.Key));

            currentGeneration = newGeneration;
            generationNumber++;
        } while (generationNumber < MaxGenerations);

        Dictionary<SortGeneration, decimal> finalGeneration = SortGeneration(currentGeneration, minTeamSize, teams);

        stopwatch.Stop();
        logger.LogInformation("Sorting finished in {time} ms", stopwatch.ElapsedMilliseconds);

        return (ListToTeams(finalGeneration.First().Key, minTeamSize), seed);
    }

    private static Dictionary<SortGeneration, decimal> SortGeneration(List<SortGeneration> currentGeneration, int minTeamSize, List<Team> teams)
    {
        return currentGeneration.ToDictionary(g => g, g => SolutionScore(ref g, minTeamSize, teams))
                                .OrderBy(solution => solution.Value).ToDictionary();
    }

    private void LogGenerationStats(Dictionary<SortGeneration, decimal> generationScores, int generationNumber)
    {
        int count = generationScores.Count;
        decimal min = generationScores.Values.Min();
        decimal max = generationScores.Values.Max();
        decimal avg = generationScores.Values.Average();
        decimal mean = generationScores.Values.ElementAt(count / 2);
        if (generationNumber is 0 or MaxGenerations)
        {
            logger.LogInformation(
                "Generation number {num} - count: {count} - min: {min} - max: {max} - avg: {avg} - mean: {mean}",
                generationNumber, count, min, max, avg, mean);
        }
        else
        {
            logger.LogDebug(
                "Generation number {num} - count: {count} - min: {min} - max: {max} - avg: {avg} - mean: {mean}",
                generationNumber, count, min, max, avg, mean);
        }
    }

    private static int GetMinSizeOfTeams(int membersCount, List<Team> teams)
    {
        int numberOfTeams = teams.Count;
        int minTeamSize = membersCount / numberOfTeams;
        //TODO check if teams are not already over ideal capacity 
        return minTeamSize;
    }

    private static SortGeneration CreateRandomCombination(List<Member> members, Random random)
    {
        var newList = new List<Member>(members);
        newList.Shuffle(random);
        return new SortGeneration(newList);
    }

    private static decimal SolutionScore(ref SortGeneration generation, int teamSize, List<Team> teams)
    {
        generation.TeamMemberCount ??= new Dictionary<Team, int>();
        int extraMembers = generation.Members.Count % teamSize;
        var startIndex = 0;
        decimal score = 0;
        Dictionary<DisciplineInfo, List<decimal>> teamsDisciplineScores = [];
        foreach (Team team in teams)
        {
            if (generation.TeamMemberCount.TryGetValue(team, out int teamCapacity))
            {
                continue;
            }

            teamCapacity = Math.Max(team.Members.Count(member => !member.AllowTeamChange), teamSize);
            generation.TeamMemberCount[team] = teamCapacity;
        }
        
        //TODO add extra members
        for (var i = 0; i < extraMembers; i++)
        {
        }

        
        foreach (Team team in teams)
        {
            int teamCapacity = generation.TeamMemberCount[team];
            List<Member> teamMembers = generation.Members.Skip(startIndex).Take(teamCapacity).ToList();
            int invalidMemberCount = Team.InvalidMemberCount(teamMembers);
            if (invalidMemberCount > 0)
            {
                score = ((decimal)InvalidSolutionPenalty / teams.Count) * invalidMemberCount;
                return score;
            }

            Dictionary<DisciplineInfo, decimal> teamScoreAverages = AverageScoreByDiscipline(teamMembers);
            foreach (KeyValuePair<DisciplineInfo, decimal> discipline in teamScoreAverages)
            {
                if (!teamsDisciplineScores.TryGetValue(discipline.Key, out List<decimal>? listOfValues))
                {
                    teamsDisciplineScores.Add(discipline.Key, [discipline.Value]);
                }
                else
                {
                    listOfValues.Add(discipline.Value);
                }
            }

            startIndex += teamCapacity;
        }

        foreach (KeyValuePair<DisciplineInfo, List<decimal>> disciplineScore in teamsDisciplineScores)
        {
            decimal min = disciplineScore.Value.Min();
            decimal max = disciplineScore.Value.Max();
            score += decimal.Abs(min - max) * (DisciplineInfo.PriorityMax + 1 - disciplineScore.Key.Priority) * PriorityMultiplier;
        }

        return score;
    }

    private static Dictionary<DisciplineInfo, decimal> AverageScoreByDiscipline(List<Member> members)
    {
        return members.SelectMany(member => member.Records.Values)
                      .GroupBy(record => record.DisciplineInfo)
                      .ToDictionary(g => g.Key, g => g.Sum(record => record.NormalizedValue) / members.Count);
    }

    private static List<SortGeneration> CrossSolution(List<SortGeneration> generations, int numberOfChildren, Random random)
    {
        List<SortGeneration> children = new(numberOfChildren);
        while (children.Count < numberOfChildren)
        {
            SortGeneration parent1 = generations[random.Next(generations.Count)];
            SortGeneration parent2 = generations[random.Next(generations.Count)];
            List<SortGeneration> newChildren = CrossMembers(parent1, parent2);
            children.AddRange(newChildren);
        }

        return children;
    }

    private static List<SortGeneration> CrossMembers(SortGeneration parent1, SortGeneration parent2)
    {
        int startIndex = parent1.Members.Count / 3;
        int endIndex = startIndex * 2 + 1;
        if (parent1.Members.Count % 3 == 0)
        {
            endIndex--;
        }

        List<Member> parent1Section = parent1.Members[startIndex..endIndex];
        List<Member> parent2Section = parent2.Members[startIndex..endIndex];

        List<Member> parent1Rest = [..parent1.Members[endIndex..], ..parent1.Members[..endIndex]];
        parent1Rest = parent1Rest.Except(parent2Section).ToList();

        List<Member> parent2Rest = [..parent2.Members[endIndex..], ..parent2.Members[..endIndex]];
        parent2Rest = parent2Rest.Except(parent1Section).ToList();

        int endSectionSize = parent1.Members.Count - endIndex + 1;
        SortGeneration child1 = new(
        [
            ..parent1Rest[endSectionSize..],
            ..parent2Section,
            ..parent1Rest[..endSectionSize]
        ]);
        SortGeneration child2 = new(
        [
            ..parent2Rest[endSectionSize..],
            ..parent1Section,
            ..parent2Rest[..endSectionSize]
        ]);

        return [child1, child2];
    }

    private void MutateSolution(List<SortGeneration> members, Random random)
    {
        foreach (var memberList in members)
        {
            MutateMembers(memberList, random);
        }
    }

    private static void MutateMembers(SortGeneration generation, Random random)
    {
        for (var i = 0; i < generation.Members.Count; i++)
        {
            if (random.NextDouble() > ChanceOfMutation) continue;
            int index = random.Next(generation.Members.Count);
            generation.Members.Swap(i, index);
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