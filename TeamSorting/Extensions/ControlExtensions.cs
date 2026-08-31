using Avalonia.Controls;
using Avalonia.VisualTree;

namespace TeamSorting.Extensions;

public static class ControlExtensions
{
    public static T? FindVisualAncestorOrSelf<T>(this Control? control) where T : Control
    {
        while (control is not null)
        {
            if (control is T result)
            {
                return result;
            }

            control = control.GetVisualParent() as Control;
        }

        return null;
    }
}
