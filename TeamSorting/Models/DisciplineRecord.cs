using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using TeamSorting.Enums;

namespace TeamSorting.Models;

public class DisciplineRecord : ObservableObject
{
    public DisciplineRecord(DisciplineInfo disciplineInfo, string value)
    {
        DisciplineInfo = disciplineInfo;
        SetValueFromString(value);
    }

    private static readonly string[] TimeFormats =
    [
        @"hh\:mm\:ss\.f",
        @"h\:mm\:ss\.f",
        @"h\:mm\:ss",
        @"hh\:mm\:ss",
        @"mm\:ss\.f",
        @"mm\:ss",
        @"ss\.f",
        @"ss"
    ];

    public DisciplineInfo DisciplineInfo { get; }

    public void SetValueFromString(string value)
    {
        string normalizedValue = value.Replace(',', '.');
        Value = DisciplineInfo.DataType switch
        {
            DisciplineDataType.Time => string.IsNullOrWhiteSpace(value)
                ? TimeSpan.Zero
                : TimeSpan.ParseExact(normalizedValue, TimeFormats, CultureInfo.InvariantCulture),
            DisciplineDataType.Number => string.IsNullOrWhiteSpace(value)
                ? decimal.Zero
                : decimal.Parse(normalizedValue, NumberStyles.Any, CultureInfo.InvariantCulture),
            _ => throw new FormatException()
        };
    }

    private object _value = null!;

    public object Value
    {
        get => _value;
        set
        {
            if (DisciplineInfo.DataType == DisciplineDataType.Time && value is not TimeSpan
                || DisciplineInfo.DataType == DisciplineDataType.Number && value is not decimal)
            {
                throw new FormatException();
            }

            SetProperty(ref _value, value);
            //updates min/max values for discipline
            //TODO: optimize calculation
            _ = DecimalValue;
        }
    }

    public decimal NormalizedValue
    {
        get
        {
            decimal value = DecimalValue;
            decimal range = DisciplineInfo.MaxValue - DisciplineInfo.MinValue;

            if (range == 0m)
            {
                return 1m;
            }

            return (value - DisciplineInfo.MinValue) / range;
        }
    }

    public decimal DecimalValue
    {
        get
        {
            //TODO: cache value
            decimal decimalValue = DisciplineInfo.DataType switch
            {
                DisciplineDataType.Time => (decimal)((TimeSpan)Value).TotalSeconds,
                DisciplineDataType.Number => (decimal)Value,
                _ => throw new FormatException()
            };

            DisciplineInfo.UpdateMinMax(decimalValue);
            return decimalValue;
        }
    }


    public static string ExampleValue(DisciplineDataType disciplineDataType)
    {
        switch (disciplineDataType)
        {
            case DisciplineDataType.Number:
            {
                const decimal exampleNumber = 0.0m;
                return exampleNumber.ToString("G");
            }
            case DisciplineDataType.Time:
            {
                TimeSpan exampleTime = TimeSpan.Zero;
                return exampleTime.ToString(TimeFormats[0]);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(disciplineDataType), disciplineDataType, null);
        }
    }
}