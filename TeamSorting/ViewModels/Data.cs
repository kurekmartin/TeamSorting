using System.Collections.ObjectModel;
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
    
    
    private readonly ISorter _sorter;
    private readonly ILogger<Data> _logger;
    private readonly CsvUtil _csvUtil;

    public Data(ILogger<Data> logger, ISorter sorter, CsvUtil csvUtil)
    {
        _sorter = sorter;
        _csvUtil = csvUtil;
        _logger = logger;
    }

    private void MemberOnDisciplineRecordChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(DisciplineAverage));
    }



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