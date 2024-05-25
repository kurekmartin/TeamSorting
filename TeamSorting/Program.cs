using TeamSorting.ViewModel;

namespace TeamSorting;

class Program
{
    private static readonly MembersData MembersData = new();

    static async Task Main()
    {
        await CsvUtil.LoadMembersData(MembersData, @"C:\Users\marku\Programming\Repos\TeamSorting\TeamSorting\Data\input.csv");
        Console.ReadLine();
    }
}