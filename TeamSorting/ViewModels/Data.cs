﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CsvHelper;
using Microsoft.Extensions.Logging;
using TeamSorting.Enums;
using TeamSorting.Extensions;
using TeamSorting.Lang;
using TeamSorting.Models;
using TeamSorting.Sorting;
using TeamSorting.Utils;
using TeamSorting.Views;

namespace TeamSorting.ViewModels;

public class Data : ObservableObject
{
    public ObservableCollection<DisciplineInfo> Disciplines { get; } = [];
    public ObservableCollection<Member> Members { get; } = [];
    public List<Member> SortedMembers => Members.OrderBy(m => m.Name).ToList();

    private int _teamNumber = 1;
    private bool _sortingInProgress;
    private readonly ISorter _sorter;
    private readonly ILogger<Data> _logger;
    private readonly CsvUtil _csvUtil;

    public Data(ILogger<Data> logger, ISorter sorter, CsvUtil csvUtil)
    {
        _sorter = sorter;
        _csvUtil = csvUtil;
        _logger = logger;
        Members.CollectionChanged += MembersOnCollectionChanged;
    }

    private void MembersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (Member member in e.NewItems!)
                {
                    member.DisciplineRecordChanged += MemberOnDisciplineRecordChanged;
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (Member member in e.OldItems!)
                {
                    member.DisciplineRecordChanged -= MemberOnDisciplineRecordChanged;
                }

                break;
            case NotifyCollectionChangedAction.Replace:
                foreach (Member member in e.NewItems!)
                {
                    member.DisciplineRecordChanged += MemberOnDisciplineRecordChanged;
                }

                foreach (Member member in e.OldItems!)
                {
                    member.DisciplineRecordChanged -= MemberOnDisciplineRecordChanged;
                }

                break;
        }
    }

    private void MemberOnDisciplineRecordChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(DisciplineAverage));
    }

    public bool SortingInProgress
    {
        get => _sortingInProgress;
        set => SetProperty(ref _sortingInProgress, value);
    }

    public ProgressValues Progress { get; } = new();

    public ObservableCollection<Team> Teams { get; } = [];

    public Team MembersWithoutTeam { get; } = new(Resources.Data_TeamName_Unsorted) { DisableValidation = true };

    public Dictionary<DisciplineInfo, object> DisciplineDelta
    {
        get
        {
            var dict = new Dictionary<DisciplineInfo, object>();
            foreach (DisciplineInfo discipline in Disciplines)
            {
                if (Teams.All(team => team.Members.Count == 0))
                {
                    dict.Add(discipline, 0);
                    continue;
                }

                List<object> teamScores = Teams.Where(team => team.Members.Count > 0)
                                               .Select(t => t.GetAverageValueByDiscipline(discipline)).ToList();

                switch (discipline.DataType)
                {
                    case DisciplineDataType.Time:
                    {
                        TimeSpan minTimeSpan = teamScores.Select(o => (TimeSpan)o).Min();
                        TimeSpan maxTimeSpan = teamScores.Select(o => (TimeSpan)o).Max();
                        TimeSpan diffTimeSpan = maxTimeSpan - minTimeSpan;
                        dict.Add(discipline, diffTimeSpan);
                        break;
                    }
                    case DisciplineDataType.Number:
                    {
                        decimal min = teamScores.Select(o => (decimal)o).Min();
                        decimal max = teamScores.Select(o => (decimal)o).Max();
                        decimal diff = max - min;
                        dict.Add(discipline, Math.Round(diff, 2));
                        break;
                    }
                    default:
                        dict.Add(discipline, 0);
                        break;
                }
            }

            return dict;
        }
    }

    public Dictionary<DisciplineInfo, object> DisciplineAverage
    {
        get
        {
            var dict = new Dictionary<DisciplineInfo, object>();
            foreach (DisciplineInfo discipline in Disciplines)
            {
                IEnumerable<DisciplineRecord> records = Members.Select(member => member.GetRecord(discipline));
                object average = discipline.DataType switch
                {
                    DisciplineDataType.Time when Members.Count == 0 => TimeSpan.Zero,
                    DisciplineDataType.Time => new TimeSpan(records.Sum(record => ((TimeSpan)record.Value).Ticks) / Members.Count),
                    DisciplineDataType.Number when Members.Count == 0 => 0,
                    DisciplineDataType.Number => records.Sum(record => (decimal)record.Value) / Members.Count,
                    _ => 0
                };
                dict.Add(discipline, average);
            }

            return dict;
        }
    }

    public string InputSeed { get; set; } = string.Empty;
    public string UsedSeed { get; set; } = string.Empty;

    #region Discipline

    public bool AddDiscipline(DisciplineInfo discipline)
    {
        if (Disciplines.Any(i => i.Name == discipline.Name))
        {
            return false;
        }

        _logger.LogInformation("Adding discipline {disciplineId}", discipline.Id);
        Disciplines.Add(discipline);
        foreach (var member in Members)
        {
            AddDisciplineRecord(member, discipline, "");
        }

        return true;
    }

    public bool RemoveDiscipline(DisciplineInfo discipline)
    {
        _logger.LogInformation("Removing discipline {disciplineId}", discipline.Id);
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
        _logger.LogInformation("Adding member {memberId}", member.Id);
        foreach (var discipline in Disciplines)
        {
            AddDisciplineRecord(member, discipline, "");
        }

        member.PropertyChanged += MemberOnPropertyChanged;
        Members.Add(member);
        MembersWithoutTeam.AddMember(member);
        ValidateMemberDuplicates();
        OnPropertyChanged(nameof(SortedMembers));
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
        _logger.LogInformation("Removing member {memberId}", member.Id);
        bool result = Members.Remove(member);
        if (result)
        {
            member.Team?.RemoveMember(member);
            member.ClearWithMembers();
            member.ClearNotWithMembers();
            ValidateMemberDuplicates();
        }

        OnPropertyChanged(nameof(SortedMembers));
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
        if (!Disciplines.Contains(discipline)) return null;
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

        _logger.LogInformation("Adding team {teamId}", team.Id);
        Teams.Add(team);
        _teamNumber++;

        team.PropertyChanged += TeamOnPropertyChanged;

        OnPropertyChanged(nameof(DisciplineDelta));
        return true;
    }

    private void TeamOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender?.GetType() == typeof(Team) &&
            e.PropertyName == nameof(Team.AvgScores))
        {
            OnPropertyChanged(nameof(DisciplineDelta));
        }
    }

    public void RemoveAllTeams()
    {
        _logger.LogInformation("Removing all teams.");
        Teams.Clear();
        _teamNumber = 1;
    }

    public bool RemoveTeam(Team team)
    {
        foreach (Member member in team.Members.ToList())
        {
            member.MoveToTeam(MembersWithoutTeam);
        }

        _logger.LogInformation("Removing team {teamId}", team.Id);
        bool result = Teams.Remove(team);
        OnPropertyChanged(nameof(DisciplineDelta));
        return result;
    }

    public Team CreateAndAddTeam()
    {
        var team = new Team(string.Format(Resources.Data_TeamName_Template, _teamNumber));
        AddTeam(team);
        return team;
    }

    public void SortTeamsByCriteria(MemberSortCriteria sortCriteria)
    {
        foreach (var team in Teams)
        {
            team.SortCriteria = sortCriteria;
        }

        MembersWithoutTeam.SortCriteria = sortCriteria;
    }

    public async Task SortToTeams(UserControl visual, int? numberOfTeams = null)
    {
        var window = TopLevel.GetTopLevel(visual);
        if (window is not MainWindow { DataContext: MainWindowViewModel mainWindowViewModel } mainWindow)
        {
            return;
        }

        //Warn user about deletion of current teams
        if (Teams.Count > 0)
        {
            var dialog = new WarningDialog(
                message: Resources.InputView_Sort_WarningDialog_Message,
                confirmButtonText: Resources.InputView_Sort_WarningDialog_Delete,
                cancelButtonText: Resources.InputView_Sort_WarningDialog_Cancel)
            {
                Position = mainWindow.Position //fix for WindowStartupLocation="CenterOwner" not working
            };
            var result = await dialog.ShowDialog<WarningDialogResult>(mainWindow);
            if (result == WarningDialogResult.Cancel)
            {
                return;
            }
        }

        mainWindow.Cursor = new Cursor(StandardCursorType.Wait);

        int teamsCount = numberOfTeams ?? Teams.Count;
        SortingInProgress = true;
        (List<Team> teams, string? seed) sortResult = await Task.Run(() => _sorter.Sort(Members.ToList(), teamsCount, Progress, InputSeed));
        RemoveAllTeams();
        foreach (Team team in sortResult.teams)
        {
            AddTeam(team);
        }

        UsedSeed = sortResult.seed ?? string.Empty;
        SortingInProgress = false;
        mainWindowViewModel.SwitchToTeamsView();
        mainWindow.Cursor = Cursor.Default;
    }

    #endregion

    #region CSV

    public List<CsvError> LoadFromFile(StreamReader inputFile)
    {
        _logger.LogInformation("Loading data from file started.");
        List<CsvError> csvErrors = [];
        ClearData();
        using var csv = new CsvReader(inputFile, CultureInfo.InvariantCulture);

        using var dataReader = new CsvDataReader(csv);
        var dataTable = new DataTable();
        dataTable.Load(dataReader);

        //TODO check minimum rows (2)

        List<CsvError> headerErrors = _csvUtil.CheckHeader(csv);
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
            _logger.LogError("Error loading members data: {message}", e.Message);
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
        _logger.LogInformation("Clearing all data.");
        Members.Clear();
        Disciplines.Clear();
        RemoveAllTeams();
    }

    /// <summary>
    /// Warning: Disciplines with wrong <see cref="DisciplineDataType"/> or <see cref="SortOrder"/> are not added.
    /// </summary>
    /// <param name="dataTable"></param>
    /// <returns></returns>
    private List<CsvError> LoadDisciplinesInfo(DataTable dataTable)
    {
        _logger.LogInformation("Loading disciplines info");
        List<CsvError> errors = [];
        foreach (DataColumn column in dataTable.Columns)
        {
            if (!CsvUtil.IsDisciplineColumn(column))
            {
                continue;
            }

            var discipline = new DisciplineInfo(column.ColumnName);
            var dataTypeError = _csvUtil.ReadDisciplineDataType(discipline, dataTable);
            if (dataTypeError is not null)
            {
                errors.Add(dataTypeError);
            }

            var sortTypeError = _csvUtil.ReadDisciplineSortType(discipline, dataTable);
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
        const int headerOffset = 3;
        _logger.LogInformation("Loading members data");
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
            int rowNumber = rowIndex + headerOffset;
            if (string.IsNullOrWhiteSpace(memberName))
            {
                int columnNumber = dataRow.GetColumnIndex(nameof(Member.Name)) + 1;
                _logger.LogWarning("Skipping empty member name at row {row}, column {column}", rowNumber, columnNumber);
                errors.Add(new CsvError(
                    Resources.Data_LoadMembersData_EmptyMemberName_Error,
                    rowNumber: rowNumber,
                    columnNumber: columnNumber
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
                int columnNumber = dataRow.GetColumnIndex(nameof(Member.Name)) + 1;
                _logger.LogWarning("Skipping duplicate member name at row {row}, column {column}", rowNumber, columnNumber);
                errors.Add(new CsvError(
                    string.Format(
                        Resources.Data_LoadMembersData_DuplicateMemberNames_Error,
                        memberName),
                    rowNumber: rowNumber,
                    columnNumber: columnNumber
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
                int columnNumber = dataRow.GetColumnIndex(nameof(Member.With)) + 1;
                _logger.LogWarning("Unknown with members at row {rowIndex}, column {columnIndex}", rowNumber, columnNumber);
                errors.Add(new CsvError(
                    string.Format(
                        Resources.Data_LoadMembersData_UnknownMemberInConstrains_Error,
                        string.Join(", ", unknownWithMembers.Select(tuple => tuple.Name))),
                    rowNumber: rowNumber,
                    columnNumber: columnNumber
                ));
            }

            var unknownNotWithMembers = AddNotWithMembers(member, notWithMembers)
                                        .Where(result => !result.Added)
                                        .ToList();
            //TODO add warning if duplicate members are found
            if (unknownNotWithMembers.Count != 0)
            {
                int columnNumber = dataRow.GetColumnIndex(nameof(Member.NotWith)) + 1;
                _logger.LogWarning("Unknown members in constrains at row {rowIndex}, column {column}", rowNumber, columnNumber);
                errors.Add(new CsvError(
                    string.Format(
                        Resources.Data_LoadMembersData_UnknownMemberInConstrains_Error,
                        string.Join(", ", unknownNotWithMembers.Select(tuple => tuple.Name))),
                    rowNumber: rowNumber,
                    columnNumber: columnNumber
                ));
            }

            foreach (var disciplineInfo in Disciplines)
            {
                var record = AddDisciplineRecord(member, disciplineInfo,
                    dataRow[disciplineInfo.Name].ToString() ?? string.Empty);
                if (record is null) continue;

                int columnNumber = dataRow.GetColumnIndex(disciplineInfo.Name) + 1;
                try
                {
                    _ = record.Value;
                }
                catch (FormatException e)
                {
                    _logger.LogError("Format error getting record value - row {row}, column {column}: {message}", e.Message, rowNumber, columnNumber);
                    errors.Add(new CsvError(
                        string.Format(Resources.Data_LoadMembersData_WrongDisciplineRecordFormat_Error,
                            DisciplineRecord.ExampleValue(disciplineInfo.DataType)),
                        rowNumber: rowNumber,
                        columnNumber: columnNumber
                    ));
                }
                catch (Exception e)
                {
                    _logger.LogError("Error getting record value - row {row}, column {column}: {message}", e.Message, rowNumber, columnNumber);
                    errors.Add(new CsvError(
                        string.Format(Resources.Data_LoadMembersData_DisciplineRecord_UnknownError, e.Message),
                        rowNumber: rowNumber,
                        columnNumber: columnNumber)
                    );
                }
            }

            processedMembers.Add(member);
        }

        _logger.LogInformation("Loading members data finished");
        return errors;
    }

    public void WriteTeamsToCsv(string path)
    {
        _logger.LogInformation("Writing teams to CSV");
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
        _logger.LogInformation("Finished writing teams to CSV");
    }

    #endregion
}