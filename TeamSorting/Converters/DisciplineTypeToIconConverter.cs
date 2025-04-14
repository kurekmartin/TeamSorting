using System.ComponentModel;
using System.Globalization;
using Avalonia.Data.Converters;
using TeamSorting.Enums;

namespace TeamSorting.Converters;

public class DisciplineTypeToIconConverter:IValueConverter
{
    //TODO change for style setter
    [Localizable(false)]
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DisciplineDataType dataType)
        {
            return dataType switch
            {
                DisciplineDataType.Time => "mdi-clock-time-three-outline",
                DisciplineDataType.Number => "mdi-numeric",
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