using CommunityToolkit.Mvvm.ComponentModel;
using TeamSorting.Enums;

namespace TeamSorting.Models;

public class DisciplineInfo(string name) : ObservableObject
{
    private int _priority = PriorityMax;

    public void UpdateMinMax(decimal value)
    {
        if (value < MinValue)
        {
            MinValue = value;
        }
        else if (value > MaxValue)
        {
            MaxValue = value;
        }
    }

    public decimal MinValue { get; private set; } = decimal.MaxValue;
    public decimal MaxValue { get; private set; } = decimal.MinValue;

    public const int PriorityMin = 1;
    public const int PriorityMax = 10;

    public int Priority
    {
        get => _priority;
        set => SetProperty(ref _priority, value);
    }

    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; } = name;
    public DisciplineDataType DataType { get; set; }
    public SortOrder SortOrder { get; set; }
}