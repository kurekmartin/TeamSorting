using System.ComponentModel.DataAnnotations;

namespace TeamSorting.Extensions;

public static class EnumExtensions
{
    public static string GetLocalizedName(this Enum @enum)
    {
        var description = @enum.ToString();
        var fieldInfo = @enum.GetType().GetField(description);

        if (fieldInfo is null)
        {
            return description;
        }

        var attributes = (DisplayAttribute[])fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false);
        if (attributes.Length != 0)
        {
            description = attributes[0].GetName() ?? description;
        }

        return description;
    }
}