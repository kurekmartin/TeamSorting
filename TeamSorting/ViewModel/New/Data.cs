using System.Globalization;
using CsvHelper;
using TeamSorting.Model.New;

namespace TeamSorting.ViewModel.New;

public class Data
{
    public readonly List<DisciplineInfo> Disciplines = [];
    public readonly List<Member> Members = [];
    public readonly List<DisciplineRecord> DisciplineRecords = [];
    public readonly List<Team> Teams = [];

    #region Discipline

    public bool AddDiscipline(DisciplineInfo discipline)
    {
        if (Disciplines.Any(i => i.Name == discipline.Name))
        {
            return false;
        }

        Disciplines.Add(discipline);
        return true;
    }

    public bool RemoveDiscipline(DisciplineInfo discipline)
    {
        bool result = Disciplines.Remove(discipline);
        if (result)
        {
            DisciplineRecords.RemoveAll(record => record.DisciplineInfo == discipline);
        }

        return result;
    }

    public DisciplineInfo? GetDisciplineByName(string name)
    {
        return Disciplines.FirstOrDefault(discipline => discipline.Name == name);
    }

    public (double min, double max) GetDisciplineRange(DisciplineInfo discipline)
    {
        var records = GetDisciplineRecordsByDiscipline(discipline);
        var values = records.Select(record => record.DoubleValue).ToList();

        if (values.Count == 0)
        {
            return (0, 0);
        }

        return (values.Min(), values.Max());
    }

    #endregion

    #region Member

    public bool AddMember(Member member)
    {
        if (Members.Any(m => m.Name == member.Name))
        {
            return false;
        }

        Members.Add(member);
        return true;
    }

    public bool RemoveMember(Member member)
    {
        bool result = Members.Remove(member);
        if (result)
        {
            DisciplineRecords.RemoveAll(record => record.Member == member);
            Teams.FirstOrDefault(team => team.Members.Contains(member))?.Members.Remove(member);
        }

        return result;
    }

    public Member? GetMemberByName(string name)
    {
        return Members.FirstOrDefault(member => member.Name == name);
    }

    public IEnumerable<Member> GetMembersByName(IEnumerable<string> names)
    {
        return Members.Where(member => names.Contains(member.Name));
    }

    public DisciplineRecord? GetMemberDisciplineRecord(Member member, DisciplineInfo discipline)
    {
        return DisciplineRecords.FirstOrDefault(record =>
            record.Member == member
            && record.DisciplineInfo == discipline);
    }

    public IEnumerable<DisciplineRecord> GetMemberRecords(Member member)
    {
        return DisciplineRecords.Where(record => record.Member == member);
    }

    public object? GetMemberDisciplineValue(Member member, DisciplineInfo discipline)
    {
        var record = DisciplineRecords.FirstOrDefault(record =>
            record.Member == member
            && record.DisciplineInfo == discipline);
        return record?.Value;
    }

    private double? GetMemberDisciplineDoubleValue(Member member, DisciplineInfo discipline)
    {
        var record = DisciplineRecords.FirstOrDefault(record =>
            record.Member == member
            && record.DisciplineInfo == discipline);
        return record?.DoubleValue;
    }

    public double GetMemberDisciplineScore(Member member, DisciplineInfo discipline)
    {
        var range = GetDisciplineRange(discipline);
        int min = discipline.SortType == DisciplineSortType.Asc ? 0 : 100;
        int max = discipline.SortType == DisciplineSortType.Asc ? 100 : 0;
        double value = GetMemberDisciplineDoubleValue(member, discipline)
                       ?? (discipline.SortType == DisciplineSortType.Asc ? range.min : range.max);
        return (((value - range.min) / (range.max - range.min)) * (max - min)) + min;
    }

    public IEnumerable<DisciplineRecord> GetSortedRecordsByDiscipline(DisciplineInfo discipline)
    {
        var records = GetDisciplineRecordsByDiscipline(discipline).ToList();
        if (discipline.SortType == DisciplineSortType.Asc)
        {
            return records.OrderBy(record => record.DoubleValue);
        }

        return records.OrderByDescending(record => record.DoubleValue);
    }

    public Dictionary<DisciplineInfo, List<DisciplineRecord>> GetSortedDisciplines()
    {
        var sortedDisciplines = new Dictionary<DisciplineInfo, List<DisciplineRecord>>();
        foreach (var discipline in Disciplines)
        {
            var sortedRecords = GetSortedRecordsByDiscipline(discipline).ToList();
            sortedDisciplines.Add(discipline, sortedRecords);
        }

        return sortedDisciplines;
    }

    public List<Member> InvalidMembersCombination()
    {
        List<Member> invalidMembers = [];
        List<Member> membersToCheck = [..Members];
        while (membersToCheck.Count > 0)
        {
            List<Member> group = [membersToCheck.First()];
            membersToCheck.Remove(group.First());
            var i = 0;
            bool newMembersAdded;
            do
            {
                newMembersAdded = false;
                var newMembers = group[i..];
                var withMembers = newMembers.SelectMany(member => GetMembersByName(member.With)).Distinct();
                foreach (var withMember in withMembers)
                {
                    if (group.Contains(withMember)) continue;
                    newMembersAdded = true;
                    group.Add(withMember);
                    membersToCheck.Remove(withMember);
                    i++;
                }
            } while (newMembersAdded);

            var notWithMembers = group.SelectMany(member => GetMembersByName(member.NotWith)).Distinct().ToList();
            bool intersectExists = group.Intersect(notWithMembers).Any();
            if (intersectExists)
            {
                invalidMembers.AddRange(group);
            }
        }

        return invalidMembers;
    }

    #endregion

    #region DisciplineRecord

    public bool AddDisciplineRecord(DisciplineRecord record)
    {
        if (!Members.Contains(record.Member) ||
            !Disciplines.Contains(record.DisciplineInfo))
        {
            return false;
        }

        DisciplineRecords.Add(record);
        return true;
    }

    public bool AddDisciplineRecord(Member member, DisciplineInfo discipline, string value)
    {
        return AddDisciplineRecord(new DisciplineRecord(member, discipline, value));
    }

    public bool RemoveDisciplineRecord(DisciplineRecord record)
    {
        return DisciplineRecords.Remove(record);
    }

    public IEnumerable<DisciplineRecord> GetDisciplineRecordsByDiscipline(DisciplineInfo discipline)
    {
        return DisciplineRecords.Where(record => record.DisciplineInfo == discipline);
    }

    #endregion

    #region Team

    public bool AddTeam(Team team)
    {
        if (Teams.Any(t => t.Name == team.Name))
        {
            return false;
        }

        Teams.Add(team);
        return true;
    }

    public bool RemoveTeam(Team team)
    {
        return Teams.Remove(team);
    }

    public bool AddMemberToTeam(Member member, Team team)
    {
        if (!Members.Contains(member))
        {
            return false;
        }

        team.Members.Add(member);
        return true;
    }

    public bool RemoveMemberFromTeam(Member member, Team team)
    {
        return team.Members.Remove(member);
    }

    public double GetTeamTotalValueByDiscipline(Team team, DisciplineInfo discipline)
    {
        var records = DisciplineRecords.Where(record =>
            record.DisciplineInfo == discipline
            && team.Members.Contains(record.Member));
        return records.Sum(record => record.DoubleValue);
    }

    public Dictionary<Team, double> GetSortedTeamsByValueByDiscipline(DisciplineInfo discipline)
    {
        var dict = new Dictionary<Team, double>();
        foreach (var team in Teams)
        {
            double value = GetTeamTotalValueByDiscipline(team, discipline);
            dict.Add(team,value);
        }

        return dict.OrderBy(pair => pair.Value).ToDictionary();
    }

    public List<Team> CreateTeams(int count)
    {
        Teams.Clear();
        for (var i = 0; i < count; i++)
        {
            AddTeam(new Team($"Team{i + 1}"));
        }

        return Teams;
    }

    #endregion

    #region Loading

    public async Task LoadFromFile(StreamReader inputFile)
    {
        var csv = new CsvReader(inputFile, CultureInfo.InvariantCulture);

        await csv.ReadAsync();
        csv.ReadHeader();
        await csv.ReadAsync();

        LoadDisciplinesInfo(csv);
        LoadMembersData(csv);
    }

    private void LoadDisciplinesInfo(CsvReader csv)
    {
        var disciplines = csv.HeaderRecord.Except([
            nameof(Member.Name),
            nameof(Member.With),
            nameof(Member.NotWith)
        ]).Select(d => new DisciplineInfo() { Name = d }).ToList();

        ReadDisciplineDataTypes(disciplines, csv);
        csv.Read();
        ReadDisciplineSortTypes(disciplines, csv);

        foreach (var discipline in disciplines)
        {
            AddDiscipline(discipline);
        }
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

    private void LoadMembersData(CsvReader csv)
    {
        while (csv.Read())
        {
            var member = new Member(name: csv[nameof(Member.Name)])
            {
                With = csv[nameof(Member.With)].Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                NotWith = csv[nameof(Member.NotWith)].Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            };
            AddMember(member);

            foreach (var disciplineInfo in Disciplines)
            {
                var record = new DisciplineRecord(member, disciplineInfo, csv[disciplineInfo.Name]);
                AddDisciplineRecord(record);
            }
        }
    }

    #endregion
}