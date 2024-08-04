using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TeamSorting.Views;

public partial class WarningDialog : Window
{
    public WarningDialog(string message = null!)
    {
        InitializeComponent();
        Message.Text = message;
    }
    
    private void Cancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(WarningDialogResult.Cancel);
    }

    private void Continue_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(WarningDialogResult.Continue);
    }
}

public enum WarningDialogResult
{
    Cancel,
    Continue
}