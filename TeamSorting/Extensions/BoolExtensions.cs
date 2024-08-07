using TeamSorting.Lang;

namespace TeamSorting.Extensions;

public static class BoolExtensions
{
    public static string GetLocalizedName(this bool @bool)
    {
        return @bool ? Resources.Bool_True : Resources.Bool_False;
    }
}