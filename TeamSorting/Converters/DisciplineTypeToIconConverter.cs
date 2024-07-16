﻿using System.Globalization;
using Avalonia.Data.Converters;
using TeamSorting.Models;

namespace TeamSorting.Converters;

public class DisciplineTypeToIconConverter:IValueConverter
{
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