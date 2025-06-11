using System.Data;
using System.Dynamic;
using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Logging;
using TeamSorting.Enums;
using TeamSorting.Extensions;
using TeamSorting.Lang;
using TeamSorting.Models;

namespace TeamSorting.Utils;

public class CsvUtil(ILogger<CsvUtil> logger, Disciplines disciplines, Members members, Teams teams)
{
    private static readonly HashSet<string> RequiredColumnNames =
    [
        nameof(Member.Name)
    ];

    private static readonly HashSet<string> NonDisciplineColumnNames =
    [
        nameof(Member.Name),
        nameof(Member.With),
        nameof(Member.NotWith)
    ];

    public static bool IsDisciplineColumn(DataColumn column)
    {
        return !NonDisciplineColumnNames.Contains(column.ColumnName);
    }

    public List<CsvError> CheckHeader(CsvReader csv)
    {
        logger.LogInformation("Checking CSV header");
        List<CsvError> errors = [];
        errors.AddRange(CheckRequiredColumns(csv));
        errors.AddRange(CheckDuplicateColumns(csv));
        return errors;
    }

    private List<CsvError> CheckRequiredColumns(CsvReader csv)
    {
        logger.LogInformation("Checking required columns");
        List<CsvError> errors = [];
        if (csv.HeaderRecord is null)
        {
            logger.LogError("CSV header is null");
            errors.Add(new CsvError(Resources.Data_ValidateCsvHeader_MissingHeader, rowNumber: 1));
            return errors;
        }

        List<string> missingColumns = RequiredColumnNames.Except(csv.HeaderRecord).ToList();

        if (missingColumns.Count > 0)
        {
            errors.Add(new CsvError(
                Resources.Data_ValidateCsvHeader_MissingColumns_Error + string.Join(", ", missingColumns),
                rowNumber: 1)
            );
        }

        return errors;
    }

    private List<CsvError> CheckDuplicateColumns(CsvReader csv)
    {
        logger.LogInformation("Checking duplicate columns");
        List<CsvError> errors = [];
        if (csv.HeaderRecord is null)
        {
            logger.LogError("CSV header is null");
            errors.Add(new CsvError(Resources.Data_ValidateCsvHeader_MissingHeader, rowNumber: 1));
            return errors;
        }

        List<string> duplicateColumns = csv.HeaderRecord.GroupBy(s => s)
                                           .Where(g => g.Count() > 1)
                                           .Select(y => y.Key)
                                           .ToList();

        if (duplicateColumns.Count > 0)
        {
            logger.LogError("Duplicate columns count: {count}", duplicateColumns.Count);
            errors.Add(
                new CsvError(
                    Resources.Data_ValidateCsvHeader_DuplicateColumns_Error + string.Join(", ", duplicateColumns),
                    rowNumber: 1)
            );
        }

        return errors;
    }

    public CsvError? ReadDisciplineDataType(DisciplineInfo discipline, DataTable dataTable)
    {
        logger.LogInformation("Reading discipline data type for {disciplineId}", discipline.Id);
        int column = dataTable.Columns.IndexOf(discipline.Name);
        string value = dataTable.Rows[0][column].ToString() ?? string.Empty;
        try
        {
            discipline.DataType = Enum.Parse<DisciplineDataType>(value);
        }
        catch (ArgumentException e)
        {
            logger.LogError("Error parsing discipline data type: {message}", e.Message);
            return new CsvError(string.Format(
                    Resources.Data_ReadDisciplineDataTypes_WrongDisciplineDataTypes_Error,
                    value,
                    discipline.Name,
                    string.Join(", ", Enum.GetValues<DisciplineDataType>())),
                rowNumber: 1,
                columnNumber: column + 1);
        }
        catch (Exception e)
        {
            logger.LogError("Error parsing discipline data type: {message}", e.Message);
            return new CsvError(string.Format(
                    Resources.Data_ReadDisciplineDataTypes_ReadingError, discipline.Name, e.Message),
                rowNumber: 1,
                columnNumber: column + 1);
        }

        return null;
    }

    public CsvError? ReadDisciplineSortType(DisciplineInfo discipline, DataTable dataTable)
    {
        logger.LogInformation("Reading discipline sort type for {disciplineId}", discipline.Id);
        int column = dataTable.Columns.IndexOf(discipline.Name);
        string value = dataTable.Rows[1][column].ToString() ?? string.Empty;
        try
        {
            discipline.SortOrder = Enum.Parse<SortOrder>(value);
        }
        catch (ArgumentException e)
        {
            logger.LogError("Error parsing discipline sort order: {message}", e.Message);
            return new CsvError(string.Format(
                    Resources.Data_ReadDisciplineSortTypes_WrongDisciplineSortOrder_Error,
                    value,
                    discipline.Name,
                    string.Join(", ", Enum.GetValues<SortOrder>())),
                rowNumber: 2,
                columnNumber: column + 1);
        }
        catch (Exception e)
        {
            logger.LogError("Error parsing discipline sort order: {message}", e.Message);
            return new CsvError(string.Format(
                    Resources.Data_ReadDisciplineDataTypes_ReadingError, discipline.Name, e.Message),
                rowNumber: 2,
                columnNumber: column + 1);
        }

        return null;
    }

    public List<CsvError> LoadFromFile(StreamReader inputFile)
    {
        logger.LogInformation("Loading data from file started.");
        List<CsvError> csvErrors = [];
        //TODO: delete original data only after successful load
        ClearData();
        using var csv = new CsvReader(inputFile, CultureInfo.InvariantCulture);

        using var dataReader = new CsvDataReader(csv);
        var dataTable = new DataTable();
        dataTable.Load(dataReader);

        //TODO check minimum rows (2)

        List<CsvError> headerErrors = CheckHeader(csv);
        if (headerErrors.Count != 0)
        {
            csvErrors.AddRange(headerErrors);
            return csvErrors;
        }

        List<CsvError> loadDisciplinesErrors = LoadDisciplinesInfo(dataTable);
        //disciplines with wrong data types or sort types are not added
        csvErrors.AddRange(loadDisciplinesErrors);

        EnumerableRowCollection<DataRow> dataRows = dataTable.AsEnumerable();

        try
        {
            csvErrors.AddRange(LoadMembersData(dataRows.Skip(2).ToList()));
        }
        catch (Exception e)
        {
            logger.LogError("Error loading members data: {message}", e.Message);
            csvErrors.Add(new CsvError(Resources.Data_LoadFromFile_Error + ": " + e.Message));
        }

        if (csvErrors.Count != 0)
        {
            ClearData();
        }

        return csvErrors;
    }

    private void ClearData()
    {
        members.RemoveAllMembers();
        disciplines.RemoveAllDisciplines();
        teams.RemoveAllTeams();
    }

    /// <summary>
    /// Warning: Disciplines with wrong <see cref="DisciplineDataType"/> or <see cref="SortOrder"/> are not added.
    /// </summary>
    /// <param name="dataTable"></param>
    /// <returns></returns>
    private List<CsvError> LoadDisciplinesInfo(DataTable dataTable)
    {
        logger.LogInformation("Loading disciplines info");
        List<CsvError> errors = [];
        foreach (DataColumn column in dataTable.Columns)
        {
            if (!IsDisciplineColumn(column))
            {
                continue;
            }

            var discipline = new DisciplineInfo(column.ColumnName);
            CsvError? dataTypeError = ReadDisciplineDataType(discipline, dataTable);
            if (dataTypeError is not null)
            {
                errors.Add(dataTypeError);
            }

            CsvError? sortTypeError = ReadDisciplineSortType(discipline, dataTable);
            if (sortTypeError is not null)
            {
                errors.Add(sortTypeError);
            }

            if (dataTypeError is null && sortTypeError is null)
            {
                disciplines.AddDiscipline(discipline);
            }
        }

        return errors;
    }

    private List<CsvError> LoadMembersData(IList<DataRow> dataRows)
    {
        const int headerOffset = 3;
        logger.LogInformation("Loading members data");
        List<CsvError> errors = [];

        //add all members for later constrains (with/not with) check 
        _ = dataRows
            .Select(row => row[nameof(Member.Name)].ToString())
            .Cast<string>()
            .Select(s => members.AddMember(new Member(s)))
            .ToList();

        List<Member> processedMembers = [];

        for (var rowIndex = 0; rowIndex < dataRows.Count; rowIndex++)
        {
            DataRow dataRow = dataRows[rowIndex];
            string memberName = dataRow[nameof(Member.Name)].ToString() ?? string.Empty;
            int rowNumber = rowIndex + headerOffset;
            if (string.IsNullOrWhiteSpace(memberName))
            {
                int columnNumber = dataRow.GetColumnIndex(nameof(Member.Name)) + 1;
                logger.LogWarning("Skipping empty member name at row {row}, column {column}", rowNumber, columnNumber);
                errors.Add(new CsvError(
                    Resources.Data_LoadMembersData_EmptyMemberName_Error,
                    rowNumber: rowNumber,
                    columnNumber: columnNumber
                ));
                continue;
            }

            Member? member = members.GetMemberByName(memberName);
            if (member is null)
            {
                member = new Member(memberName);
                members.AddMember(member);
            }

            //check duplicate names
            if (processedMembers.Contains(member))
            {
                int columnNumber = dataRow.GetColumnIndex(nameof(Member.Name)) + 1;
                logger.LogWarning("Skipping duplicate member name at row {row}, column {column}", rowNumber, columnNumber);
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

            List<(string Name, bool Added)> unknownWithMembers = members.AddWithMembers(member, withMembers)
                                                                         .Where(result => !result.Added)
                                                                         .ToList();
            //TODO add warning if duplicate members are found
            if (unknownWithMembers.Count != 0)
            {
                int columnNumber = dataRow.GetColumnIndex(nameof(Member.With)) + 1;
                logger.LogWarning("Unknown with members at row {rowIndex}, column {columnIndex}", rowNumber, columnNumber);
                errors.Add(new CsvError(
                    string.Format(
                        Resources.Data_LoadMembersData_UnknownMemberInConstrains_Error,
                        string.Join(", ", unknownWithMembers.Select(tuple => tuple.Name))),
                    rowNumber: rowNumber,
                    columnNumber: columnNumber
                ));
            }

            List<(string Name, bool Added)> unknownNotWithMembers = members.AddNotWithMembers(member, notWithMembers)
                                                                            .Where(result => !result.Added)
                                                                            .ToList();
            //TODO add warning if duplicate members are found
            if (unknownNotWithMembers.Count != 0)
            {
                int columnNumber = dataRow.GetColumnIndex(nameof(Member.NotWith)) + 1;
                logger.LogWarning("Unknown members in constrains at row {rowIndex}, column {column}", rowNumber, columnNumber);
                errors.Add(new CsvError(
                    string.Format(
                        Resources.Data_LoadMembersData_UnknownMemberInConstrains_Error,
                        string.Join(", ", unknownNotWithMembers.Select(tuple => tuple.Name))),
                    rowNumber: rowNumber,
                    columnNumber: columnNumber
                ));
            }

            foreach (DisciplineInfo disciplineInfo in disciplines.DisciplineList)
            {
                DisciplineRecord? record = disciplines.AddDisciplineRecord(member, disciplineInfo,
                    dataRow[disciplineInfo.Name].ToString() ?? string.Empty);
                if (record is null) continue;

                int columnNumber = dataRow.GetColumnIndex(disciplineInfo.Name) + 1;
                try
                {
                    _ = record.Value;
                }
                catch (FormatException e)
                {
                    logger.LogError("Format error getting record value - row {row}, column {column}: {message}", e.Message, rowNumber, columnNumber);
                    errors.Add(new CsvError(
                        string.Format(Resources.Data_LoadMembersData_WrongDisciplineRecordFormat_Error,
                            DisciplineRecord.ExampleValue(disciplineInfo.DataType)),
                        rowNumber: rowNumber,
                        columnNumber: columnNumber
                    ));
                }
                catch (Exception e)
                {
                    logger.LogError("Error getting record value - row {row}, column {column}: {message}", e.Message, rowNumber, columnNumber);
                    errors.Add(new CsvError(
                        string.Format(Resources.Data_LoadMembersData_DisciplineRecord_UnknownError, e.Message),
                        rowNumber: rowNumber,
                        columnNumber: columnNumber)
                    );
                }
            }

            processedMembers.Add(member);
        }

        logger.LogInformation("Loading members data finished");
        return errors;
    }

    public void WriteTeamsToCsv(string path)
    {
        logger.LogInformation("Writing teams to CSV");
        var records = new List<dynamic>();

        int maxMembers = teams.TeamList.MaxBy(team => team.Members.Count)!.Members.Count;
        for (var i = 0; i < maxMembers; i++)
        {
            IDictionary<string, object> record = new ExpandoObject()!;
            foreach (Team team in teams.TeamList)
            {
                if (i >= team.Members.Count)
                {
                    record.Add(team.Name, string.Empty);
                    continue;
                }

                Member member = team.Members[i];
                record.Add(team.Name, member.Name);
            }

            records.Add(record);
        }

        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(records);
        logger.LogInformation("Finished writing teams to CSV");
    }
}