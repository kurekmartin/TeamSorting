using System.Data;
using System.Globalization;
using CsvHelper;
using TeamSorting.Enums;
using TeamSorting.Models;
using TeamSorting.ViewModels;

namespace TeamSorting.Utils;

public static class CsvCheckerUtil
{
    private static readonly HashSet<string> RequiredColumnNames =
        [nameof(Member.Name), nameof(Member.With), nameof(Member.NotWith)];

    public static bool CheckCsvFile(StreamReader inputFile)
    {
        using var csv = new CsvReader(inputFile, CultureInfo.InvariantCulture);
        csv.Read();
        csv.ReadHeader();

        if (!CheckRequiredColumns(csv) || !CheckDuplicateColumns(csv))
        {
            return false;
        }

        using var dataReader = new CsvDataReader(csv);
        var dataTable = new DataTable();
        dataTable.Load(dataReader);

        if (!CheckDisciplineDataTypesRow(dataTable) || !CheckDuplicateNames(dataTable))
        {
            return false;
        }

        //TODO check empty names
        //TODO check discipline records
        //TODO check duplicate with names
        //TODO check duplicate not with names
        //TODO check unknown with names
        //TODO check unknown not with names
        return true;
    }

    private static bool IsDisciplineColumn(DataColumn column)
    {
        return !RequiredColumnNames.Contains(column.ColumnName);
    }

    private static bool CheckRequiredColumns(CsvReader csv)
    {
        return Equals(csv.HeaderRecord.Intersect(RequiredColumnNames), RequiredColumnNames);
    }

    private static bool CheckDuplicateColumns(CsvReader csv)
    {
        var duplicateColumns = csv.HeaderRecord.GroupBy(s => s)
            .Where(g => g.Count() > 1)
            .Select(y => y.Key)
            .ToList();

        return duplicateColumns.Count == 0;
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