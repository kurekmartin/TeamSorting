namespace TeamSorting.Models;

public class CsvError(string message, int rowNumber = -1, int columnNumber = -1)
{
    public int Row { get; init; } = rowNumber;
    public int Column { get; init; } = columnNumber;
    public string Message { get; init; } = message;
}