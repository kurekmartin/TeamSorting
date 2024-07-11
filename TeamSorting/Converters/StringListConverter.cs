using System.Globalization;
using Avalonia.Data.Converters;

namespace TeamSorting.Converters;

public class StringListConverter:IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is List<string> array)
        {
            return string.Join(", ", array);
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            return text.Split(',');
        }

        return Array.Empty<string>();
    }
}