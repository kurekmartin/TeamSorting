using System.Data;

namespace TeamSorting.Extensions;

public static class DataRowExtensions
{
    public static int GetColumnIndex(this DataRow row, string columnName)
    {
        return row.Table.Columns[columnName]?.Ordinal ?? -1;
    }
}