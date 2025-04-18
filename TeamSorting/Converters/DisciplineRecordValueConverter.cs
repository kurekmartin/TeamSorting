using System.Globalization;
using Avalonia.Data.Converters;

namespace TeamSorting.Converters;

public class DisciplineRecordValueConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        switch (value)
        {
            case TimeSpan timeSpan:
            {
                TimeSpan roundedTimespan = TimeSpan.FromSeconds(Math.Round(timeSpan.TotalSeconds,1));
                return roundedTimespan.ToString($@"hh\:mm\:ss\{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}f");
            }
            case decimal numberValue:
            {
                decimal number = Math.Round(numberValue, 2);
                return number.ToString("0.##");
            }
            default:
                return value?.ToString();
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}