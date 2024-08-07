using System.Globalization;
using Avalonia.Data.Converters;
using TeamSorting.Extensions;

namespace TeamSorting.Converters;

public class BoolDisplayNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool @bool) return null;
        return @bool.GetLocalizedName();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}