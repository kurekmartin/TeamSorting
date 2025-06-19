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
        $@"hh\:mm\:ss\{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}f",
        $@"h\:mm\:ss\{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}f",
        @"h\:mm\:ss",
        @"hh\:mm\:ss",
        $@"mm\:ss\{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}f",
        @"mm\:ss",
        $@"ss\{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}f",
        @"ss"
    ];

    public DisciplineInfo DisciplineInfo { get; }

    public void SetValueFromString(string value)
    {
        Value = DisciplineInfo.DataType switch
        {
            DisciplineDataType.Time => string.IsNullOrWhiteSpace(value)
                ? TimeSpan.Zero
                : TimeSpan.ParseExact(value, TimeFormats, CultureInfo.CurrentCulture),
            DisciplineDataType.Number => string.IsNullOrWhiteSpace(value)
                ? decimal.Zero
                : decimal.Parse(value),
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
            decimal value;
            try
            {
                value = (DecimalValue - DisciplineInfo.MinValue) / (DisciplineInfo.MaxValue - DisciplineInfo.MinValue);
            }
            catch
            {
                value = 1;
            }

            return value;
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