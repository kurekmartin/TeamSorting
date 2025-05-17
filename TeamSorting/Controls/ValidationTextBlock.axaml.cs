using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace TeamSorting.Controls;

public class ValidationTextBlock : TemplatedControl
{
    public static readonly StyledProperty<string?> ValidatedTextProperty =
        TextBlock.TextProperty.AddOwner<ValidationTextBlock>(
            new StyledPropertyMetadata<string?>(
                defaultBindingMode: BindingMode.OneWay,
                enableDataValidation: true));

    public string? ValidatedText
    {
        get => GetValue(ValidatedTextProperty);
        set => SetValue(ValidatedTextProperty, value);
    }

    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
    {
        if (property == ValidatedTextProperty)
        {
            DataValidationErrors.SetError(this, error);
        }

        base.UpdateDataValidation(property, state, error);
    }
}