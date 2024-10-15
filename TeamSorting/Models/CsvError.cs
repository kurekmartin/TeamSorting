namespace TeamSorting.Models;

public class CsvError(string message, int? rowNumber = null, int? columnNumber = null)
{
    public int? Row { get; init; } = rowNumber;
    public int? Column { get; init; } = columnNumber;
    public string Message { get; init; } = message;
}