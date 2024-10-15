using System.Globalization;
using TeamSorting.Enums;

namespace TeamSorting.Models;

public class DisciplineRecord(Member member, DisciplineInfo disciplineInfo, string rawValue)
{
    public Member Member { get; } = member;
    public DisciplineInfo DisciplineInfo { get; } = disciplineInfo;

    private string _rawValue = rawValue;
    public string RawValue
    {
        get => _rawValue;
        set
        {
            _rawValue = value;
            _isValueUpdated = false;
        }
    }

    private bool _isValueUpdated;
    private object? _value;

    public object Value
    {
        get
        {
            if (_isValueUpdated && _value is not null)
            {
                return _value;
            }

            _value = DisciplineInfo.DataType switch
            {
                DisciplineDataType.Time => string.IsNullOrWhiteSpace(RawValue)
                    ? TimeSpan.Zero
                    : TimeSpan.Parse(RawValue, CultureInfo.InvariantCulture),
                DisciplineDataType.Number => string.IsNullOrWhiteSpace(RawValue)
                    ? 0d
                    : decimal.Parse(RawValue, CultureInfo.InvariantCulture),
                _ => throw new FormatException()
            };

            _isValueUpdated = true;
            return _value;
        }
    }

    public decimal DecimalValue =>
        DisciplineInfo.DataType switch
        {
            DisciplineDataType.Time => (decimal)((TimeSpan)Value).TotalSeconds,
            DisciplineDataType.Number => (decimal)Value,
            _ => throw new FormatException()
        };
}