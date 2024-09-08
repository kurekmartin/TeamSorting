using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Globalization;
using Avalonia.Controls.Notifications;
using CsvHelper;
using ReactiveUI;
using TeamSorting.Enums;
using TeamSorting.Lang;
using TeamSorting.Models;

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
                team.WhenAnyValue(t => t.TotalScores)
                    .Subscribe(_ => this.RaisePropertyChanged(nameof(DisciplineDelta)));
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
                var teamScores = Teams.Select(t => t.GetAverageValueByDiscipline(discipline)).ToList();
                double min = teamScores.Min();
                double max = teamScores.Max();
                double diff = double.Abs(min - max);
                dict.Add(discipline, Math.Round(diff, 2));
            }

            return dict;
        }
    }

    public string Seed { get; set; } = string.Empty;

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

    public DisciplineInfo? GetDisciplineById(Guid id)
    {
        return Disciplines.FirstOrDefault(discipline => discipline.Id == id);
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
        this.RaisePropertyChanged(nameof(SortedMembers));
        return true;
    }

    public bool RemoveMember(Member member)
    {
        bool result = Members.Remove(member);
        if (result)
        {
            Teams.FirstOrDefault(team => team.Members.Contains(member))?.Members.Remove(member);
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

    public double GetMemberDisciplineScore(Member member, DisciplineInfo discipline)
    {
        var range = GetDisciplineRange(discipline);
        int min = discipline.SortOrder == SortOrder.Asc ? 0 : 100;
        int max = discipline.SortOrder == SortOrder.Asc ? 100 : 0;
        double value = member.GetRecord(discipline).DoubleValue;
        return (((value - range.min) / (range.max - range.min)) * (max - min)) + min;
    }

    public IEnumerable<DisciplineRecord> GetSortedRecordsByDiscipline(DisciplineInfo discipline)
    {
        var records = GetDisciplineRecordsByDiscipline(discipline).ToList();
        if (discipline.SortOrder == SortOrder.Asc)
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
            double value = team.GetAverageValueByDiscipline(discipline);
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

    public void SortTeamsByCriteria(DisciplineInfo? disciplineInfo, SortOrder sortOrder)
    {
        foreach (var team in Teams)
        {
            team.SortCriteria = new MemberSortCriteria(disciplineInfo, sortOrder);
        }
    }

    #endregion

    #region CSV

    public List<ReturnMessage> LoadFromFile(StreamReader inputFile)
    {
        List<ReturnMessage> returnMessages = [];
        ClearData();
        using var csv = new CsvReader(inputFile, CultureInfo.InvariantCulture);

        using var dataReader = new CsvDataReader(csv);
        var dataTable = new DataTable();
        dataTable.Load(dataReader);

        //TODO check required columns
        //TODO check minimum rows (2)

        var csvHeaderResult = ValidateCsvHeader(csv);
        if (csvHeaderResult?.NotificationType == NotificationType.Error)
        {
            return [csvHeaderResult];
        }

        var loadDisciplinesResult = LoadDisciplinesInfo(dataTable);
        if (loadDisciplinesResult?.NotificationType == NotificationType.Error)
        {
            ClearData();
            loadDisciplinesResult.Message = $"{Resources.Data_LoadFromFile_Error}\n{loadDisciplinesResult.Message}";
            return [loadDisciplinesResult];
        }

        var dataRows = dataTable.AsEnumerable();

        List<ReturnMessage> loadMembersResult = [];
        try
        {
            loadMembersResult = LoadMembersData(dataRows.Skip(2).ToList());
        }
        catch (Exception e)
        {
            ClearData();
            return
            [
                new ReturnMessage(NotificationType.Error,
                    $"{Resources.Data_LoadFromFile_Error}\n{e.Message}")
            ];
        }

        if (loadMembersResult.Any(message => message.NotificationType == NotificationType.Error))
        {
            ClearData();
            var message = loadMembersResult.First(msg => msg.NotificationType == NotificationType.Error);
            message.Message = $"{Resources.Data_LoadFromFile_Error}\n{message.Message}";
            return [message];
        }

        if (returnMessages.Count == 0)
        {
            return [new ReturnMessage(NotificationType.Success, Resources.Data_LoadFromFile_Success)];
        }

        return returnMessages;
    }

    private static ReturnMessage? ValidateCsvHeader(CsvReader csv)
    {
        List<string> missingFields = [];
        if (!csv.HeaderRecord.Contains(nameof(Member.Name)))
        {
            missingFields.Add(nameof(Member.Name));
        }

        if (!csv.HeaderRecord.Contains(nameof(Member.With)))
        {
            missingFields.Add(nameof(Member.With));
        }

        if (!csv.HeaderRecord.Contains(nameof(Member.NotWith)))
        {
            missingFields.Add(nameof(Member.NotWith));
        }

        if (missingFields.Count > 0)
        {
            return new ReturnMessage(NotificationType.Error,
                Resources.Data_ValidateCsvHeader_MissingColumns_Error + string.Join('\n', missingFields));
        }

        var duplicateColumns = csv.HeaderRecord.GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateColumns.Count > 0)
        {
            return new ReturnMessage(NotificationType.Error,
                Resources.Data_ValidateCsvHeader_DuplicateColumns_Error + string.Join('\n', duplicateColumns));
        }

        return null;
    }

    private void ClearData()
    {
        Members.Clear();
        Disciplines.Clear();
        Teams.Clear();
    }

    private ReturnMessage? LoadDisciplinesInfo(DataTable dataTable)
    {
        foreach (DataColumn column in dataTable.Columns)
        {
            if (!IsDisciplineColumn(column))
            {
                continue;
            }

            var discipline = new DisciplineInfo(column.ColumnName);
            var dataTypeResult = ReadDisciplineDataType(discipline, dataTable);
            if (dataTypeResult?.NotificationType == NotificationType.Error)
            {
                return dataTypeResult;
            }

            var sortTypeResult = ReadDisciplineSortType(discipline, dataTable);
            if (sortTypeResult?.NotificationType == NotificationType.Error)
            {
                return sortTypeResult;
            }

            AddDiscipline(discipline);
        }

        return null;
    }

    private readonly HashSet<string> _staticColumnNames =
        [nameof(Member.Name), nameof(Member.With), nameof(Member.NotWith)];

    private bool IsDisciplineColumn(DataColumn column)
    {
        return !_staticColumnNames.Contains(column.ColumnName);
    }

    private static ReturnMessage? ReadDisciplineDataType(DisciplineInfo discipline, DataTable dataTable)
    {
        string value = dataTable.Rows[0][discipline.Name].ToString() ?? string.Empty;
        try
        {
            discipline.DataType = Enum.Parse<DisciplineDataType>(value);
        }
        catch (ArgumentException)
        {
            return new ReturnMessage(NotificationType.Error,
                string.Format(Resources.Data_ReadDisciplineDataTypes_WrongDisciplineDataTypes_Error, value,
                    discipline.Name, string.Join(", ", Enum.GetValues<DisciplineDataType>())));
        }
        catch (Exception ex)
        {
            return new ReturnMessage(NotificationType.Error,
                string.Format(Resources.Data_ReadDisciplineDataTypes_ReadingError, discipline.Name, ex.Message));
        }

        return null;
    }

    private static ReturnMessage? ReadDisciplineDataTypes(IEnumerable<DisciplineInfo> disciplines, CsvReader csv)
    {
        foreach (var discipline in disciplines)
        {
            string? value = csv[discipline.Name];
            try
            {
                discipline.DataType = Enum.Parse<DisciplineDataType>(value);
            }
            catch (ArgumentException)
            {
                return new ReturnMessage(NotificationType.Error,
                    string.Format(Resources.Data_ReadDisciplineDataTypes_WrongDisciplineDataTypes_Error, value,
                        discipline.Name, string.Join(", ", Enum.GetValues<DisciplineDataType>())));
            }
            catch (Exception ex)
            {
                return new ReturnMessage(NotificationType.Error,
                    string.Format(Resources.Data_ReadDisciplineDataTypes_ReadingError, discipline.Name, ex.Message));
            }
        }

        return null;
    }

    private static ReturnMessage? ReadDisciplineSortType(DisciplineInfo discipline, DataTable dataTable)
    {
        string value = dataTable.Rows[1][discipline.Name].ToString() ?? string.Empty;
        try
        {
            discipline.SortOrder = Enum.Parse<SortOrder>(value);
        }
        catch (ArgumentException)
        {
            return new ReturnMessage(NotificationType.Error,
                string.Format(Resources.Data_ReadDisciplineDataTypes_WrongDisciplineDataTypes_Error, value,
                    discipline.Name, string.Join(", ", Enum.GetValues<DisciplineDataType>())));
        }
        catch (Exception ex)
        {
            return new ReturnMessage(NotificationType.Error,
                string.Format(Resources.Data_ReadDisciplineDataTypes_ReadingError, discipline.Name, ex.Message));
        }

        return null;
    }

    private static ReturnMessage? ReadDisciplineSortTypes(IEnumerable<DisciplineInfo> disciplines, CsvReader csv)
    {
        foreach (var discipline in disciplines)
        {
            string value = csv[discipline.Name];
            try
            {
                discipline.SortOrder = Enum.Parse<SortOrder>(csv[discipline.Name]);
            }
            catch (ArgumentException)
            {
                return new ReturnMessage(NotificationType.Error,
                    string.Format(Resources.Data_ReadDisciplineSortTypes_WrongDisciplineSortOrder_Error, value,
                        discipline.Name, string.Join(", ", Enum.GetValues<SortOrder>())));
            }
            catch (Exception ex)
            {
                return new ReturnMessage(NotificationType.Error,
                    string.Format(Resources.Data_ReadDisciplineDataTypes_ReadingError, discipline.Name, ex.Message));
            }
        }

        return null;
    }

    private List<ReturnMessage> LoadMembersData(IList<DataRow> dataRows)
    {
        List<ReturnMessage> returnMessages = [];

        var addMemberResult = dataRows
            .Select(row => row[nameof(Member.Name)].ToString())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Cast<string>()
            .Select(s => new { Name = s, Duplicate = !AddMember(new Member(s)) });

        var duplicateNames = addMemberResult
            .Where(m => m.Duplicate)
            .Select(arg => arg.Name)
            .ToHashSet();
        if (duplicateNames.Count > 0)
        {
            return
            [
                new ReturnMessage(NotificationType.Error,
                    string.Format(Resources.Data_LoadMembersData_DuplicateMemberNames_Error,
                        string.Join(", ", duplicateNames)))
            ];
        }

        var unknownMembersFound = false;
        var withDuplicatesFound = false;
        var notWithDuplicatesFound = false;
        foreach (var dataRow in dataRows)
        {
            string memberName = dataRow[nameof(Member.Name)].ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(memberName))
            {
                continue;
            }

            var member = Members.FirstOrDefault(member => member.Name == memberName);
            if (member is null)
            {
                unknownMembersFound = true;
                continue; //TODO add info to list of warnings in Member class
            }

            var withMembers = dataRow[nameof(Member.With)].ToString()?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToList() ?? [];
            var notWithMembers = dataRow[nameof(Member.NotWith)].ToString()
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToList() ?? [];

            var withResult = AddWithMembers(member, withMembers); //TODO add info to list of warnings in Member class
            var notWithResult = AddNotWithMembers(member, notWithMembers); //TODO add info to list of warnings in Member class

            withDuplicatesFound = withResult.Any(tuple => !tuple.Added);
            notWithDuplicatesFound = notWithResult.Any(tuple => !tuple.Added);

            foreach (var disciplineInfo in Disciplines)
            {
                AddDisciplineRecord(member, disciplineInfo,
                    dataRow[disciplineInfo.Name].ToString() ?? string.Empty);
            }
        }

        if (unknownMembersFound)
        {
            returnMessages.Add(new ReturnMessage(NotificationType.Warning,
                Resources.Data_LoadMembersData_UnknownMemberInConstarins_Warning));
        }

        if (withDuplicatesFound || notWithDuplicatesFound)
        {
            returnMessages.Add(new ReturnMessage(NotificationType.Warning,
                Resources.Data_LoadMembersData_DuplicateMembersInConstarins_Warning));
        }

        return returnMessages;
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