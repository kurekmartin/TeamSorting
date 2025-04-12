using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Globalization;
using CsvHelper;
using ReactiveUI;
using TeamSorting.Enums;
using TeamSorting.Extensions;
using TeamSorting.Lang;
using TeamSorting.Models;
using TeamSorting.Utils;

namespace TeamSorting.ViewModels;

public class Data : ReactiveObject
{
    public ObservableCollection<DisciplineInfo> Disciplines { get; } = [];
    public ObservableCollection<Member> Members { get; } = [];
    public List<Member> SortedMembers => Members.OrderBy(m => m.Name).ToList();

    private ObservableCollection<Team> _teams = [];

    public ObservableCollection<Team> Teams
    {
        get => _teams;
        set
        {
            _teams = value;
            foreach (var team in _teams)
            {
                team.WhenAnyValue(t => t.AvgScores)
                    .Subscribe(_ => this.RaisePropertyChanged(nameof(DisciplineDelta)));
            }
        }
    }

    public Team MembersWithoutTeam { get; } = new(Resources.Data_TeamName_Unsorted) { DisableValidation = true };

    public Dictionary<DisciplineInfo, decimal> DisciplineDelta
    {
        get
        {
            var dict = new Dictionary<DisciplineInfo, decimal>();
            foreach (var discipline in Disciplines)
            {
                var teamScores = Teams.Select(t => t.GetAverageValueByDiscipline(discipline)).ToList();
                decimal min = teamScores.Min();
                decimal max = teamScores.Max();
                decimal diff = decimal.Abs(min - max);
                dict.Add(discipline, Math.Round(diff, 2));
            }

            return dict;
        }
    }

    public string InputSeed  { get; set; } = string.Empty;
    public string UsedSeed { get; set; } = string.Empty;

    #region Discipline

    public bool AddDiscipline(DisciplineInfo discipline)
    {
        if (Disciplines.Any(i => i.Name == discipline.Name))
        {
            return false;
        }

        Disciplines.Add(discipline);
        foreach (var member in Members)
        {
            AddDisciplineRecord(member, discipline, "");
        }

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

    public DisciplineInfo? GetDisciplineById(Guid id)
    {
        return Disciplines.FirstOrDefault(discipline => discipline.Id == id);
    }

    public (decimal min, decimal max) GetDisciplineRange(DisciplineInfo discipline)
    {
        var records = GetDisciplineRecordsByDiscipline(discipline);
        var values = records.Select(record => record.DecimalValue).ToList();

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
        foreach (var discipline in Disciplines)
        {
            AddDisciplineRecord(member, discipline, "");
        }

        member.PropertyChanged += MemberOnPropertyChanged;
        Members.Add(member);
        MembersWithoutTeam.AddMember(member);
        ValidateMemberDuplicates();
        this.RaisePropertyChanged(nameof(SortedMembers));
        return true;
    }

    private void MemberOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Member)
        {
            return;
        }

        if (e.PropertyName == nameof(Member.Name))
        {
            ValidateMemberDuplicates();
        }
    }

    private void ValidateMemberDuplicates()
    {
        List<IGrouping<string, Member>> memberGroups = Members
            .GroupBy(member => member.Name).ToList();
        foreach (IGrouping<string, Member> members in memberGroups)
        {
            if (members.Count() > 1)
            {
                foreach (Member member in members)
                {
                    member.AddError(nameof(Member.Name), Resources.InputView_Member_DuplicateName_Error);
                }
            }
            else
            {
                foreach (Member member in members)
                {
                    member.RemoveError(nameof(Member.Name), Resources.InputView_Member_DuplicateName_Error);
                }
            }
        }
    }

    public bool RemoveMember(Member member)
    {
        bool result = Members.Remove(member);
        if (result)
        {
            member.Team?.RemoveMember(member);
            member.ClearWithMembers();
            member.ClearNotWithMembers();
            ValidateMemberDuplicates();
        }

        this.RaisePropertyChanged(nameof(SortedMembers));
        return result;
    }

    public bool AddWithMember(Member member, string withMemberName)
    {
        var withMember = Members.FirstOrDefault(m => m.Name == withMemberName);
        if (withMember is null)
        {
            return false;
        }

        member.AddWithMember(withMember);
        return true;
    }

    public List<(string Name, bool Added)> AddWithMembers(Member member, List<string> withMemberNames)
    {
        List<(string Name, bool Added)> results = [];
        foreach (string withMemberName in withMemberNames)
        {
            bool result = AddWithMember(member, withMemberName);
            results.Add((withMemberName, result));
        }

        return results;
    }

    public bool AddNotWithMember(Member member, string notWithMemberName)
    {
        var notWithMember = Members.FirstOrDefault(m => m.Name == notWithMemberName);
        if (notWithMember is null)
        {
            return false;
        }

        member.AddNotWithMember(notWithMember);
        return true;
    }

    public List<(string Name, bool Added)> AddNotWithMembers(Member member, List<string> notWithMemberNames)
    {
        List<(string Name, bool Added)> results = [];
        foreach (string notWithMemberName in notWithMemberNames)
        {
            bool result = AddNotWithMember(member, notWithMemberName);
            results.Add((notWithMemberName, result));
        }

        return results;
    }

    public Member? GetMemberByName(string name)
    {
        return Members.FirstOrDefault(member => member.Name == name);
    }

    public IEnumerable<Member> GetMembersByName(IEnumerable<string> names)
    {
        return Members.Where(member => names.Contains(member.Name));
    }

    public decimal GetMemberDisciplineScore(Member member, DisciplineInfo discipline)
    {
        var range = GetDisciplineRange(discipline);
        int min = discipline.SortOrder == SortOrder.Asc ? 0 : 100;
        int max = discipline.SortOrder == SortOrder.Asc ? 100 : 0;
        decimal value = member.GetRecord(discipline).DecimalValue;
        return (value - range.min) / (range.max - range.min) * (max - min) + min;
    }

    public IEnumerable<DisciplineRecord> GetSortedRecordsByDiscipline(DisciplineInfo discipline)
    {
        var records = GetDisciplineRecordsByDiscipline(discipline).ToList();
        if (discipline.SortOrder == SortOrder.Asc)
        {
            return records.OrderBy(record => record.DecimalValue);
        }

        return records.OrderByDescending(record => record.DecimalValue);
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
                var withMembers = newMembers.SelectMany(member => GetMembersByName(member.With.Select(m => m.Name)))
                    .Distinct();
                foreach (var withMember in withMembers)
                {
                    if (group.Contains(withMember)) continue;
                    newMembersAdded = true;
                    group.Add(withMember);
                    membersToCheck.Remove(withMember);
                    i++;
                }
            } while (newMembersAdded);

            var notWithMembers = group.SelectMany(member => GetMembersByName(member.NotWith.Select(m => m.Name)))
                .Distinct().ToList();
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
            var withMembers = newMembers.SelectMany(member => GetMembersByName(member.With.Select(m => m.Name)))
                .Distinct();
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
        var notWithMembers = GetMembersByName(currentMember.NotWith.Select(m => m.Name));
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

    public DisciplineRecord? AddDisciplineRecord(Member member, DisciplineInfo discipline, string value)
    {
        if (!Members.Contains(member) || !Disciplines.Contains(discipline)) return null;
        var record = member.AddDisciplineRecord(discipline, value);
        return record;
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

    public Dictionary<Team, decimal> GetSortedTeamsByValueByDiscipline(DisciplineInfo discipline)
    {
        var dict = new Dictionary<Team, decimal>();
        foreach (var team in Teams)
        {
            decimal value = team.GetAverageValueByDiscipline(discipline);
            dict.Add(team, value);
        }

        return dict.OrderBy(pair => pair.Value).ToDictionary();
    }

    public ObservableCollection<Team> CreateTeams(int count)
    {
        Teams.Clear();
        for (var i = 0; i < count; i++)
        {
            AddTeam(new Team(string.Format(Resources.Data_TeamName_Template, i + 1)));
        }

        return Teams;
    }

    public void SortTeamsByCriteria(MemberSortCriteria sortCriteria)
    {
        foreach (var team in Teams)
        {
            team.SortCriteria = sortCriteria;
        }
        MembersWithoutTeam.SortCriteria = sortCriteria;
    }

    #endregion

    #region CSV

    public List<CsvError> LoadFromFile(StreamReader inputFile)
    {
        List<CsvError> csvErrors = [];
        ClearData();
        using var csv = new CsvReader(inputFile, CultureInfo.InvariantCulture);

        using var dataReader = new CsvDataReader(csv);
        var dataTable = new DataTable();
        dataTable.Load(dataReader);

        //TODO check minimum rows (2)

        var headerErrors = CsvUtil.CheckHeader(csv);
        if (headerErrors.Count != 0)
        {
            csvErrors.AddRange(headerErrors);
            return csvErrors;
        }

        var loadDisciplinesErrors = LoadDisciplinesInfo(dataTable);
        //disciplines with wrong data types or sort types are not added
        csvErrors.AddRange(loadDisciplinesErrors);

        var dataRows = dataTable.AsEnumerable();

        try
        {
            csvErrors.AddRange(LoadMembersData(dataRows.Skip(2).ToList()));
        }
        catch (Exception e)
        {
            csvErrors.Add(new CsvError(Resources.Data_LoadFromFile_Error + ": " + e.Message));
        }

        if (csvErrors.Count != 0)
        {
            ClearData();
        }

        return csvErrors;
    }

    public void ClearData()
    {
        Members.Clear();
        Disciplines.Clear();
        Teams.Clear();
    }

    /// <summary>
    /// Warning: Disciplines with wrong <see cref="DisciplineDataType"/> or <see cref="SortOrder"/> are not added.
    /// </summary>
    /// <param name="dataTable"></param>
    /// <returns></returns>
    private List<CsvError> LoadDisciplinesInfo(DataTable dataTable)
    {
        List<CsvError> errors = [];
        foreach (DataColumn column in dataTable.Columns)
        {
            if (!CsvUtil.IsDisciplineColumn(column))
            {
                continue;
            }

            var discipline = new DisciplineInfo(column.ColumnName);
            var dataTypeError = CsvUtil.ReadDisciplineDataType(discipline, dataTable);
            if (dataTypeError is not null)
            {
                errors.Add(dataTypeError);
            }

            var sortTypeError = CsvUtil.ReadDisciplineSortType(discipline, dataTable);
            if (sortTypeError is not null)
            {
                errors.Add(sortTypeError);
            }

            if (dataTypeError is null && sortTypeError is null)
            {
                AddDiscipline(discipline);
            }
        }

        return errors;
    }

    private List<CsvError> LoadMembersData(IList<DataRow> dataRows)
    {
        List<CsvError> errors = [];

        //add all members for later constrains (with/not with) check 
        _ = dataRows
            .Select(row => row[nameof(Member.Name)].ToString())
            .Cast<string>()
            .Select(s => AddMember(new Member(s)))
            .ToList();

        List<Member> processedMembers = [];

        for (var rowIndex = 0; rowIndex < dataRows.Count; rowIndex++)
        {
            var dataRow = dataRows[rowIndex];
            string memberName = dataRow[nameof(Member.Name)].ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(memberName))
            {
                errors.Add(new CsvError(
                    Resources.Data_LoadMembersData_EmptyMemberName_Error,
                    rowNumber: rowIndex + 3,
                    columnNumber: dataRow.GetColumnIndex(nameof(Member.Name)) + 1
                ));
                continue;
            }

            var member = GetMemberByName(memberName);
            if (member is null)
            {
                member = new Member(memberName);
                AddMember(member);
            }

            //check duplicate names
            if (processedMembers.Contains(member))
            {
                errors.Add(new CsvError(
                    string.Format(
                        Resources.Data_LoadMembersData_DuplicateMemberNames_Error,
                        memberName),
                    rowNumber: rowIndex + 3,
                    columnNumber: dataRow.GetColumnIndex(nameof(Member.Name)) + 1
                ));
                continue;
            }


            List<string> withMembers = [];
            int withMembersIndex = dataRow.GetColumnIndex(nameof(Member.With));
            if (withMembersIndex != -1)
            {
                withMembers = dataRow[withMembersIndex].ToString()?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList() ?? [];
            }

            List<string> notWithMembers = [];
            int notWithMembersIndex = dataRow.GetColumnIndex(nameof(Member.NotWith));
            if (notWithMembersIndex != -1)
            {
                notWithMembers = dataRow[nameof(Member.NotWith)].ToString()
                    ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList() ?? [];
            }

            var unknownWithMembers = AddWithMembers(member, withMembers)
                .Where(result => !result.Added)
                .ToList();
            //TODO add warning if duplicate members are found
            if (unknownWithMembers.Count != 0)
            {
                errors.Add(new CsvError(
                    string.Format(
                        Resources.Data_LoadMembersData_UnknownMemberInConstarins_Error,
                        string.Join(", ", unknownWithMembers.Select(tuple => tuple.Name))),
                    rowNumber: rowIndex + 3,
                    columnNumber: dataRow.GetColumnIndex(nameof(Member.With)) + 1
                ));
            }

            var unknownNotWithMembers = AddNotWithMembers(member, notWithMembers)
                .Where(result => !result.Added)
                .ToList();
            //TODO add warning if duplicate members are found
            if (unknownNotWithMembers.Count != 0)
            {
                errors.Add(new CsvError(
                    string.Format(
                        Resources.Data_LoadMembersData_UnknownMemberInConstarins_Error,
                        string.Join(", ", unknownNotWithMembers.Select(tuple => tuple.Name))),
                    rowNumber: rowIndex + 3,
                    columnNumber: dataRow.GetColumnIndex(nameof(Member.NotWith)) + 1
                ));
            }

            foreach (var disciplineInfo in Disciplines)
            {
                var record = AddDisciplineRecord(member, disciplineInfo,
                    dataRow[disciplineInfo.Name].ToString() ?? string.Empty);
                if (record is null) continue;

                try
                {
                    _ = record.Value;
                }
                catch (FormatException)
                {
                    errors.Add(new CsvError(
                        string.Format(Resources.Data_LoadMembersData_WrongDisciplineRecordFormat_Error,
                            DisciplineRecord.ExampleValue(disciplineInfo.DataType)),
                        rowNumber: rowIndex + 3,
                        columnNumber: dataRow.GetColumnIndex(disciplineInfo.Name) + 1
                    ));
                }
                catch (Exception e)
                {
                    errors.Add(new CsvError(
                        string.Format(Resources.Data_LoadMembersData_DisciplineRecord_UnknownError, e.Message),
                        rowNumber: rowIndex + 3,
                        columnNumber: dataRow.GetColumnIndex(disciplineInfo.Name) + 1)
                    );
                }
            }

            processedMembers.Add(member);
        }

        return errors;
    }

    public void WriteTeamsToCsv(string path)
    {
        var records = new List<dynamic>();

        int maxMembers = Teams.MaxBy(team => team.Members.Count)!.Members.Count;
        for (var i = 0; i < maxMembers; i++)
        {
            IDictionary<string, object> record = new ExpandoObject()!;
            foreach (var team in Teams)
            {
                if (i >= team.Members.Count)
                {
                    record.Add(team.Name, string.Empty);
                    continue;
                }

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