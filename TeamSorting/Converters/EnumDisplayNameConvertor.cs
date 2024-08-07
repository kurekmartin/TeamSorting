using System.Globalization;
using Avalonia.Data.Converters;
using TeamSorting.Extensions;

namespace TeamSorting.Converters;

public class EnumDisplayNameConvertor : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Enum enumValue) return string.Empty;
        return enumValue.GetLocalizedName();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}