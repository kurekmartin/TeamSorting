using System.Globalization;
using CsvHelper;
using TeamSorting.Model;
using TeamSorting.ViewModel;

namespace TeamSorting;

public static class CsvUtil
{
    public static async Task LoadMembersData(MembersData membersData, string inputFile)
    {
        using var reader = new StreamReader(inputFile);
        var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        await csv.ReadAsync();
        csv.ReadHeader();
        await csv.ReadAsync();
        var disciplines = ReadDisciplinesInfo(csv);
        var members = ReadTeamMembers(csv, disciplines);

        membersData.DisciplinesInfo = disciplines;
        membersData.TeamMembers = members;
    }

    private static List<DisciplineInfo> ReadDisciplinesInfo(CsvReader csv)
    {
        var disciplines = csv.HeaderRecord.Except([
            nameof(TeamMember.Name),
            nameof(TeamMember.With),
            nameof(TeamMember.NotWith),
            nameof(TeamMember.Age)
        ]).Select(d => new DisciplineInfo() { Name = d }).ToList();

        ReadDisciplineDataTypes(disciplines, csv);
        csv.Read();
        ReadDisciplineSortTypes(disciplines, csv);

        return disciplines;
    }

    private static void ReadDisciplineDataTypes(IEnumerable<DisciplineInfo> disciplines, CsvReader csv)
    {
        foreach (var discipline in disciplines)
        {
            discipline.DataType = Enum.Parse<DisciplineDataType>(csv[discipline.Name]);
        }
    }

    private static void ReadDisciplineSortTypes(IEnumerable<DisciplineInfo> disciplines, CsvReader csv)
    {
        foreach (var discipline in disciplines)
        {
            discipline.SortType = Enum.Parse<DisciplineSortType>(csv[discipline.Name]);
        }
    }

    private static List<TeamMember> ReadTeamMembers(CsvReader csv, List<DisciplineInfo> disciplines)
    {
        List<TeamMember> teamMembers = [];

        while (csv.Read())
        {
            var teamMember = new TeamMember()
            {
                Name = csv[nameof(TeamMember.Name)],
                With = csv[nameof(TeamMember.With)].Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                NotWith = csv[nameof(TeamMember.NotWith)].Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Age = int.Parse(csv[nameof(TeamMember.Age)])
            };

            foreach (var disciplineInfo in disciplines)
            {
                teamMember.Disciplines.Add(new Discipline(disciplineInfo, csv[disciplineInfo.Name]));
            }

            teamMembers.Add(teamMember);
        }

        return teamMembers;
    }
}