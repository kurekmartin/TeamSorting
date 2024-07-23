using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace TeamSorting.Converters;

public class MemberCombinationValidityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var color = value is false ? Brushes.Tomato.Color : Brushes.Transparent.Color;
        return new SolidColorBrush(color);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}