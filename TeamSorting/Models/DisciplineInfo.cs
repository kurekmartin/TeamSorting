using TeamSorting.Enums;

namespace TeamSorting.Models;

public class DisciplineInfo(string name)
{
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
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; } = name;
    public DisciplineDataType DataType { get; set; }
    public SortOrder SortOrder { get; set; }
}



