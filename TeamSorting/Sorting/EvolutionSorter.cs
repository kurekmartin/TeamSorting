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

    public string Sort(List<Member> members, List<Team> teams, ProgressValues progress, string? seed = null)
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

        List<Member> membersToSort = members.Where(member => member.AllowTeamChange).ToList();
        int membersCount = members.Count;

        if (string.IsNullOrWhiteSpace(seed))
        {
            logger.LogInformation("Seed not set, creating new seed");
            seed = SeedGenerator.CreateSeed(10);
        }

        logger.LogInformation("Starting sorting with seed {seed}", seed);

        var random = new Random(seed.GetHashCode());
        int minTeamSize = GetMinSizeOfTeams(members.Count, teams);
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
            Dictionary<SortGeneration, decimal> sortedGeneration = SortGeneration(currentGeneration, minTeamSize, teams, membersCount, random);

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

        Dictionary<SortGeneration, decimal> finalGeneration = SortGeneration(currentGeneration, minTeamSize, teams, membersCount, random);

        stopwatch.Stop();
        logger.LogInformation("Sorting finished in {time} ms", stopwatch.ElapsedMilliseconds);

        ListToTeams(finalGeneration.First().Key, teams);
        return seed;
    }

    private Dictionary<SortGeneration, decimal> SortGeneration(List<SortGeneration> generation, int minTeamSize, List<Team> teams, int totalMembersCount, Random random)
    {
        foreach (SortGeneration sortGeneration in generation)
        {
            CalculateNewMemberCount(sortGeneration, minTeamSize, teams, totalMembersCount, random);
        }

        return generation.ToDictionary(g => g, g => SolutionScore(g, teams))
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

    private static int GetMinSizeOfTeams(int totalMembersCount, List<Team> teams)
    {
        int numberOfTeams = teams.Count;
        int minTeamSize = totalMembersCount / numberOfTeams;

        //Check if already sorted teams are over capacity
        List<Team> teamsOverCapacity = teams.Where(team => team.Members.Count(member => !member.AllowTeamChange) > minTeamSize).ToList();
        if (teamsOverCapacity.Count == 0)
        {
            return minTeamSize;
        }

        int sortedMembersCount = teamsOverCapacity.Sum(team => team.Members.Count(member => !member.AllowTeamChange));
        int membersToSortCount = totalMembersCount - sortedMembersCount;
        minTeamSize = membersToSortCount / numberOfTeams;
        return minTeamSize;
    }

    private static SortGeneration CreateRandomCombination(List<Member> members, Random random)
    {
        var newList = new List<Member>(members);
        newList.Shuffle(random);
        return new SortGeneration(newList);
    }

    private decimal SolutionScore(SortGeneration generation, List<Team> teams)
    {
        var startIndex = 0;
        decimal score = 0;
        Dictionary<DisciplineInfo, List<decimal>> teamsDisciplineScores = [];

        foreach (Team team in teams)
        {
            List<Member> teamMembers = team.Members.Where(member => !member.AllowTeamChange).ToList();

            if (generation.NewMemberCount == null)
            {
                logger.LogError("NewMemberCount not set for current generation");
                continue;
            }
            int newMemberCount = generation.NewMemberCount[team];
            List<Member> newMembers = generation.Members.Skip(startIndex).Take(newMemberCount).ToList();
            teamMembers.AddRange(newMembers);

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

            startIndex += newMemberCount;
        }

        foreach (KeyValuePair<DisciplineInfo, List<decimal>> disciplineScore in teamsDisciplineScores)
        {
            decimal min = disciplineScore.Value.Min();
            decimal max = disciplineScore.Value.Max();
            score += decimal.Abs(min - max) * (DisciplineInfo.PriorityMax + 1 - disciplineScore.Key.Priority) * PriorityMultiplier;
        }

        return score;
    }

    private static void CalculateNewMemberCount(SortGeneration generation, int minTeamSize, List<Team> teams, int totalMembersCount, Random random)
    {
        generation.NewMemberCount ??= new Dictionary<Team, int>();
        int extraMembers = totalMembersCount;
        foreach (Team team in teams)
        {
            int lockedMembersCount = team.Members.Count(member => !member.AllowTeamChange);
            extraMembers -= lockedMembersCount;
            if (generation.NewMemberCount.TryGetValue(team, out int newMemberCount))
            {
                extraMembers -= newMemberCount;
                continue;
            }

            int memberCountToTeamSize = minTeamSize - team.Members.Count(member => !member.AllowTeamChange);
            newMemberCount = Math.Max(memberCountToTeamSize, 0);
            generation.NewMemberCount[team] = newMemberCount;

            extraMembers -= newMemberCount;
        }

        for (var i = 0; i < extraMembers; i++)
        {
            KeyValuePair<Team, int> teamWithMinCapacity = generation.NewMemberCount.MinBy(pair => pair.Value + pair.Key.Members.Count(member => !member.AllowTeamChange));
            int minCapacity = teamWithMinCapacity.Key.Members.Count(member => !member.AllowTeamChange) + teamWithMinCapacity.Value;
            List<KeyValuePair<Team, int>> teamsWithMinCapacity = generation.NewMemberCount.Where(pair => pair.Value + pair.Key.Members.Count(member => !member.AllowTeamChange) == minCapacity).ToList();
            
            int additionalTeamMemberIndex = random.Next(teamsWithMinCapacity.Count);
            
            KeyValuePair<Team, int> team = generation.NewMemberCount.ElementAt(additionalTeamMemberIndex);
            generation.NewMemberCount[team.Key]++;
        }
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

    private static void MutateSolution(List<SortGeneration> members, Random random)
    {
        foreach (SortGeneration memberList in members)
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

    private static void ListToTeams(SortGeneration generation, List<Team> teams)
    {
        var startIndex = 0;
        foreach (Team team in teams)
        {
            if (generation.NewMemberCount == null)
            {
                continue;
            }

            int newMemberCount = generation.NewMemberCount[team];
            List<Member> newMembers = generation.Members.Skip(startIndex).Take(newMemberCount).ToList();
            team.AddMembers(newMembers);

            startIndex += newMemberCount;
        }
    }
}