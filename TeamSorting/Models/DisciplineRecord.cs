using System.Globalization;
using TeamSorting.Enums;

namespace TeamSorting.Models;

public class DisciplineRecord(Member member, DisciplineInfo disciplineInfo, string rawValue)
{
    private static readonly string[] TimeFormats =
    [
        $@"hh\:mm\:ss\{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}f",
        $@"h\:mm\:ss\{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}f",
        @"h\:mm\:ss",
        @"hh\:mm\:ss",
        $@"mm\:ss\{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}f",
        @"mm\:ss",
        $@"ss\{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}f",
        @"ss"
    ];

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
                    : TimeSpan.ParseExact(RawValue, TimeFormats, CultureInfo.CurrentCulture),
                DisciplineDataType.Number => string.IsNullOrWhiteSpace(RawValue)
                    ? 0d
                    : decimal.Parse(RawValue),
                _ => throw new FormatException()
            };

            _isValueUpdated = true;
            return _value;
        }
        set
        {
            if (value is TimeSpan timeSpan)
            {
                RawValue = timeSpan.ToString("g", CultureInfo.CurrentCulture);
            }
            else
            {
                RawValue = value.ToString() ?? string.Empty;
            }

            _value = value;
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