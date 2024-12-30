using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TeamSorting.Views;

public partial class WarningDialog : Window
{
    public WarningDialog(string message = null!, string? confirmButtonText = null, string? cancelButtonText = null)
    {
        InitializeComponent();
        Message.Text = message;
        BtnCancel.Content = cancelButtonText ?? Lang.Resources.WarningDialog_Cancel_Button;
        BtnConfirm.Content = confirmButtonText ?? Lang.Resources.WarningDialog_Confirm_Button;
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(WarningDialogResult.Cancel);
    }

    private void Confirm_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(WarningDialogResult.Confirm);
    }
}

public enum WarningDialogResult
{
    Cancel,
    Confirm
}