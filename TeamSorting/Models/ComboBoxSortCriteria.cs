namespace TeamSorting.Models;

public class ComboBoxSortCriteria(string displayText, object? value)
{
    public string DisplayText { get; set; } = displayText;
    public object? Value { get; set; } = value;
}