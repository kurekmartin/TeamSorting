using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using TeamSorting.Enums;

namespace TeamSorting.Models;

public class DisciplineRecord(DisciplineInfo disciplineInfo, string rawValue) : ObservableObject
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

    public DisciplineInfo DisciplineInfo { get; } = disciplineInfo;

    private string _rawValue = rawValue;

    public string RawValue
    {
        get => _rawValue;
        set
        {
            SetProperty(ref _rawValue, value);
            _isValueUpdated = false;
            //updates min/max values for discipline
            //TODO: optimize calculation
            _ = Value;
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
                    ? decimal.Zero
                    : decimal.Parse(RawValue),
                _ => throw new FormatException()
            };

            _isValueUpdated = true;
            //updates min/max values for discipline
            //TODO: optimize calculation
            _ = DecimalValue;
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