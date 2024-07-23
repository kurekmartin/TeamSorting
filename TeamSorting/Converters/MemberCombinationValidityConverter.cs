using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace TeamSorting.Converters;

public class MemberCombinationValidityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is false ? Brushes.Tomato : Brushes.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}