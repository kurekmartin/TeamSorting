using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TeamSorting.Views;

public partial class WarningDialog : UserControl
{
    public event Action<WarningDialogResult>? CloseRequested;

    public WarningDialog() : this(string.Empty)
    {
    }

    public WarningDialog(string message, string? confirmButtonText = null, string? cancelButtonText = null)
    {
        InitializeComponent();
        Message.Text = message;
        BtnCancel.Content = cancelButtonText ?? Lang.Resources.WarningDialog_Cancel_Button;
        BtnConfirm.Content = confirmButtonText ?? Lang.Resources.WarningDialog_Confirm_Button;
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Cancel();
    }

    private void Confirm_OnClick(object? sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(WarningDialogResult.Confirm);
    }

    internal void Cancel()
    {
        CloseRequested?.Invoke(WarningDialogResult.Cancel);
    }

    internal void FocusInitialControl()
    {
        BtnCancel.Focus();
    }
}

public enum WarningDialogResult
{
    Cancel,
    Confirm
}
