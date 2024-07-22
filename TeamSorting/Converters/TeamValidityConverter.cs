using System.Globalization;
using Avalonia.Data.Converters;
using TeamSorting.Models;

namespace TeamSorting.Converters;

public class TeamValidityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool)
        {
            return value switch
            {
                true => "mdi-check-circle",
                false => "mdi-close-circle",
                _ => "mdi-help"
            };
        }

        return "mdi-help";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}