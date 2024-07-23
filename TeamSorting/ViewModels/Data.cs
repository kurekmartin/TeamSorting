using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Dynamic;
using System.Globalization;
using CsvHelper;
using ReactiveUI;
using TeamSorting.Models;

namespace TeamSorting.ViewModels;

public class Data : ReactiveObject
{
    public ObservableCollection<DisciplineInfo> Disciplines { get; } = [];

    public ObservableCollection<Member> Members { get; } = [];

    private ObservableCollection<Team> _teams = [];

    public ObservableCollection<Team> Teams
    {
        get => _teams;
        set
        {
            _teams = value;
            foreach (var team in _teams)
            {
                team.WhenAnyValue(t => t.TotalScores)
                    .Subscribe(x => this.RaisePropertyChanged(nameof(DisciplineDelta)));
            }
        }
    }

    public Dictionary<DisciplineInfo, double> DisciplineDelta
    {
        get
        {
            var dict = new Dictionary<DisciplineInfo, double>();
            foreach (var discipline in Disciplines)
            {
                var teamScores = Teams.Select(t => t.GetTotalValueByDiscipline(discipline)).ToList();
                double min = teamScores.Min();
                double max = teamScores.Max();
                double diff = double.Abs(min - max);
                dict.Add(discipline, Math.Round(diff, 2));
            }

            return dict;
        }
    }

    #region Discipline

    public bool AddDiscipline(DisciplineInfo discipline)
    {
        if (Disciplines.Any(i => i.Name == discipline.Name))
        {
            return false;
        }

        foreach (var member in Members)
        {
            AddDisciplineRecord(member, discipline, "");
        }

        Disciplines.Add(discipline);
        return true;
    }

    public bool RemoveDiscipline(DisciplineInfo discipline)
    {
        bool result = Disciplines.Remove(discipline);
        if (!result) return result;
        foreach (var member in Members)
        {
            member.RemoveDisciplineRecord(discipline.Id);
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

        foreach (var discipline in Disciplines)
        {
            AddDisciplineRecord(member, discipline, "");
        }

        Members.Add(member);
        return true;
    }

    public bool RemoveMember(Member member)
    {
        bool result = Members.Remove(member);
        if (result)
        {
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

    public double GetMemberDisciplineScore(Member member, DisciplineInfo discipline)
    {
        var range = GetDisciplineRange(discipline);
        int min = discipline.SortType == DisciplineSortType.Asc ? 0 : 100;
        int max = discipline.SortType == DisciplineSortType.Asc ? 100 : 0;
        double value = member.GetRecord(discipline).DoubleValue;
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

    public IEnumerable<Member> GetWithMembers(Member currentMember)
    {
        List<Member> group = [currentMember];
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
                i++;
            }
        } while (newMembersAdded);

        group.Remove(currentMember);
        return group;
    }

    public IEnumerable<Member> GetNotWithMembers(Member currentMember)
    {
        List<Member> group = [];
        var notWithMembers = GetMembersByName(currentMember.NotWith);
        foreach (var notWithMember in notWithMembers)
        {
            var allNotWithMembers = GetWithMembers(notWithMember).ToList();
            allNotWithMembers.Add(notWithMember);

            foreach (var member in allNotWithMembers.Where(member => !group.Contains(member)))
            {
                group.Add(member);
            }
        }

        return group;
    }

    #endregion

    #region DisciplineRecord

    public IEnumerable<DisciplineRecord> GetAllRecords()
    {
        return Members.SelectMany(member => member.Records.Values);
    }

    public bool AddDisciplineRecord(Member member, DisciplineInfo discipline, string value)
    {
        if (!Members.Contains(member) || !Disciplines.Contains(discipline)) return false;
        member.AddDisciplineRecord(discipline, value);
        return true;
    }


    public IEnumerable<DisciplineRecord> GetDisciplineRecordsByDiscipline(DisciplineInfo discipline)
    {
        return GetAllRecords().Where(record => record.DisciplineInfo == discipline);
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

    public Dictionary<Team, double> GetSortedTeamsByValueByDiscipline(DisciplineInfo discipline)
    {
        var dict = new Dictionary<Team, double>();
        foreach (var team in Teams)
        {
            double value = team.GetTotalValueByDiscipline(discipline);
            dict.Add(team, value);
        }

        return dict.OrderBy(pair => pair.Value).ToDictionary();
    }

    public ObservableCollection<Team> CreateTeams(int count)
    {
        Teams.Clear();
        for (var i = 0; i < count; i++)
        {
            AddTeam(new Team($"Team{i + 1}"));
        }

        return Teams;
    }

    #endregion

    #region CSV

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
        ]).Select(d => new DisciplineInfo(d)).ToList();

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
            var withMembers = csv[nameof(Member.With)].Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var notWithMembers = csv[nameof(Member.NotWith)].Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var member = new Member(name: csv[nameof(Member.Name)]);
            member.AddWithMembers(withMembers);
            member.AddNotWithMembers(notWithMembers);
            AddMember(member);

            foreach (var disciplineInfo in Disciplines)
            {
                AddDisciplineRecord(member, disciplineInfo, csv[disciplineInfo.Name]);
            }
        }
    }

    public void WriteTeamsToCsv(string path)
    {
        var records = new List<dynamic>();

        int maxMembers = Teams.MaxBy(team => team.Members.Count)!.Members.Count;
        for (var i = 0; i < maxMembers; i++)
        {
            var record = new ExpandoObject() as IDictionary<string, object>;
            foreach (var team in Teams)
            {
                if (i >= team.Members.Count) continue;
                var member = team.Members[i];
                record.Add(team.Name, member.Name);
            }

            records.Add(record);
        }

        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(records);
    }

    #endregion
}