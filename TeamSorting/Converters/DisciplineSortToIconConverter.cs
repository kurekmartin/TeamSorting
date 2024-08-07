using System.ComponentModel;
using System.Globalization;
using Avalonia.Data.Converters;
using TeamSorting.Models;

namespace TeamSorting.Converters;

public class DisciplineSortToIconConverter:IValueConverter
{
    [Localizable(false)]
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SortOrder dataType)
        {
            return dataType switch
            {
                SortOrder.Asc => "mdi-arrow-up",
                SortOrder.Desc => "mdi-arrow-down",
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