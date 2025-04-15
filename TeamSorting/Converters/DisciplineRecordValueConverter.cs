using System.Globalization;
using Avalonia.Data.Converters;

namespace TeamSorting.Converters;

public class DisciplineRecordValueConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            return timeSpan.ToString($@"hh\:mm\:ss\{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}f");
        }
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}