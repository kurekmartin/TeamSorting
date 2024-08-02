using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace TeamSorting.Controls;

public class IconRadioButton : RadioButton
{
    public static readonly StyledProperty<string> IconProperty =
        AvaloniaProperty.Register<IconRadioButton, string>(nameof(Icon));
    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<IconRadioButton, double>(nameof(Icon));
    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
}