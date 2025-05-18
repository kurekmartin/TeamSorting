using System.Data;
using CsvHelper;
using Microsoft.Extensions.Logging;
using TeamSorting.Enums;
using TeamSorting.Lang;
using TeamSorting.Models;

namespace TeamSorting.Utils;

public class CsvUtil(ILogger<CsvUtil> logger)
{
    private static readonly HashSet<string> RequiredColumnNames = [nameof(Member.Name)];

    private static readonly HashSet<string> NonDisciplineColumnNames =
        [nameof(Member.Name), nameof(Member.With), nameof(Member.NotWith)];
    public static bool IsDisciplineColumn(DataColumn column)
    {
        return !NonDisciplineColumnNames.Contains(column.ColumnName);
    }

    public static List<CsvError> CheckHeader(CsvReader csv)
    {
        List<CsvError> errors = [];
        errors.AddRange(CheckRequiredColumns(csv));
        errors.AddRange(CheckDuplicateColumns(csv));
        return errors;
    }

    private static List<CsvError> CheckRequiredColumns(CsvReader csv)
    {
        List<CsvError> errors = [];
        var missingColumns = RequiredColumnNames.Except(csv.HeaderRecord).ToList();

        if (missingColumns.Count > 0)
        {
            errors.Add(new CsvError(
                Resources.Data_ValidateCsvHeader_MissingColumns_Error + string.Join(", ", missingColumns),
                rowNumber: 1)
            );
        }

        return errors;
    }

    private static List<CsvError> CheckDuplicateColumns(CsvReader csv)
    {
        List<CsvError> errors = [];
        var duplicateColumns = csv.HeaderRecord.GroupBy(s => s)
            .Where(g => g.Count() > 1)
            .Select(y => y.Key)
            .ToList();

        if (duplicateColumns.Count > 0)
        {
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

    private static bool CheckDisciplineDataTypesRow(DataTable dataTable)
    {
        foreach (DataColumn column in dataTable.Columns)
        {
            var fieldValue = dataTable.Rows[0][column.ColumnName].ToString();
            if (!IsDisciplineColumn(column) && !string.IsNullOrWhiteSpace(fieldValue))
            {
                return false;
            }

            bool valid = Enum.TryParse(typeof(DisciplineDataType), fieldValue, out _);
            if (!valid)
            {
                return false;
            }
        }

        return true;
    }

    private static bool CheckDuplicateNames(DataTable dataTable)
    {
        var names = dataTable.AsEnumerable()
            .Select(row => row[nameof(Member.Name)].ToString())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Cast<string>()
            .ToList();

        var duplicateNames = names.GroupBy(s => s)
            .Where(g => g.Count() > 1)
            .Select(y => y.Key)
            .ToList();

        return duplicateNames.Count == 0;
    }
}