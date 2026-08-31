using Avalonia.Controls;
using Avalonia.VisualTree;

namespace TeamSorting.Controls;

internal static class NumericUpDownExtensions
{
    public static bool FocusTextEditor(this NumericUpDown editor)
    {
        editor.ApplyTemplate();

        TextBox? textBox = editor.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
        if (textBox is null)
        {
            return editor.Focus();
        }

        bool focused = textBox.Focus();
        textBox.SelectAll();
        return focused;
    }
}
