using System.Globalization;
using Avalonia.Data.Converters;
using TeamSorting.Utils;

namespace TeamSorting.Converters;

public class FormattedTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return TextParser.Parse(value as string ?? string.Empty);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
