using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TeamSorting.Views;

public partial class CsvErrorDialog : UserControl
{
    public event Action? CloseRequested;

    public CsvErrorDialog()
    {
        InitializeComponent();
    }

    private void OkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        CloseDialog();
    }

    internal void CloseDialog()
    {
        CloseRequested?.Invoke();
    }

    internal void FocusInitialControl()
    {
        OkButton.Focus();
    }
}
