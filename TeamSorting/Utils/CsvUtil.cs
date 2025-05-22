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
}