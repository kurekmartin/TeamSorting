using TeamSorting.ViewModel;
using Serilog;
using TeamSorting.Model;

namespace TeamSorting;

class Program
{
    private static readonly MembersData MembersData = new();

    static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        using var file = new StreamReader(@"C:\Users\marku\Programming\Repos\TeamSorting\TeamSorting\Data\input.csv");

        await CsvUtil.LoadMembersData(MembersData, file);

        List<Team> teams =
        [
            new Team() { Name = "Team1" },
            new Team() { Name = "Team2" },
            new Team() { Name = "Team3" }
        ];

        var index = 0;
        foreach (var member in MembersData.TeamMembers)
        {
            teams[index].Members.Add(member);
            index++;
            index %= teams.Count;
        }

        Console.ReadLine();
    }
}