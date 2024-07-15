using System.Globalization;
using Avalonia.Data.Converters;
using TeamSorting.Models;

namespace TeamSorting.Converters;

public class DisciplineSortToIconConverter:IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DisciplineSortType dataType)
        {
            return dataType switch
            {
                DisciplineSortType.Asc => "mdi-arrow-up",
                DisciplineSortType.Desc => "mdi-arrow-down",
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