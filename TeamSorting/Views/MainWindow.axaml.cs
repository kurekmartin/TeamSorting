using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeamSorting.ViewModels;

namespace TeamSorting.Views;

public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow>? _logger = Ioc.Default.GetService<ILogger<MainWindow>>();
    private IInputElement? _previouslyFocusedElement;

    internal bool IsDialogOpen => DialogOverlay.IsVisible;

    public MainWindow()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, DialogOverlay_OnKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);
#if DEBUG
        this.AttachDevTools();
#endif
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        _logger?.LogInformation("Window loaded");
        base.OnLoaded(e);

        if (DataContext is not MainWindowViewModel context)
        {
            return;
        }

        context.TeamsViewModel.NotificationManager ??= new WindowNotificationManager(this)
        {
            Position = NotificationPosition.BottomRight,
            Margin = new Thickness(0, 0, 0, 35)
        };

        context.CheckForUpdates();
    }

    public Task<WarningDialogResult> ShowWarningDialogAsync(
        string message,
        string? confirmButtonText = null,
        string? cancelButtonText = null)
    {
        var dialog = new WarningDialog(message, confirmButtonText, cancelButtonText);
        var completion = new TaskCompletionSource<WarningDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        dialog.CloseRequested += CloseDialog;
        ShowOverlay(dialog);
        Dispatcher.UIThread.Post(dialog.FocusInitialControl, DispatcherPriority.Input);
        return completion.Task;

        void CloseDialog(WarningDialogResult result)
        {
            dialog.CloseRequested -= CloseDialog;
            HideDialog();
            completion.TrySetResult(result);
        }
    }

    public Task ShowCsvErrorDialogAsync(CsvErrorViewModel viewModel)
    {
        var dialog = new CsvErrorDialog
        {
            DataContext = viewModel
        };
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        dialog.CloseRequested += CloseDialog;
        ShowOverlay(dialog);
        Dispatcher.UIThread.Post(dialog.FocusInitialControl, DispatcherPriority.Input);
        return completion.Task;

        void CloseDialog()
        {
            dialog.CloseRequested -= CloseDialog;
            HideDialog();
            completion.TrySetResult();
        }
    }

    private void ShowOverlay(Control dialog)
    {
        if (DialogContent.Content is not null)
        {
            throw new InvalidOperationException("Only one overlay dialog can be shown at a time.");
        }

        if (DataContext is MainWindowViewModel context)
        {
            context.TeamsViewModel.SuppressNotifications();
        }

        _previouslyFocusedElement = FocusManager?.GetFocusedElement();
        DialogContent.Content = dialog;
        ApplicationContent.IsEnabled = false;
        DialogOverlay.IsVisible = true;
    }

    private void HideDialog()
    {
        DialogOverlay.IsVisible = false;
        DialogContent.Content = null;
        ApplicationContent.IsEnabled = true;

        if (DataContext is MainWindowViewModel context)
        {
            context.TeamsViewModel.RestoreNotifications();
        }

        IInputElement? elementToFocus = _previouslyFocusedElement;
        _previouslyFocusedElement = null;
        if (elementToFocus is not null)
        {
            Dispatcher.UIThread.Post(() => elementToFocus.Focus(), DispatcherPriority.Input);
        }
    }

    private void DialogOverlay_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!DialogOverlay.IsVisible || e.Key != Key.Escape)
        {
            return;
        }

        switch (DialogContent.Content)
        {
            case WarningDialog warningDialog:
                warningDialog.Cancel();
                break;
            case CsvErrorDialog csvErrorDialog:
                csvErrorDialog.CloseDialog();
                break;
        }

        e.Handled = true;
    }

    private void NewVersionPageButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel context || string.IsNullOrEmpty(context.ReleaseUrl))
        {
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(context.ReleaseUrl)
            {
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", context.ReleaseUrl);
        }
    }

    private void SwitchThemeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not { } application)
        {
            return;
        }

        if (application.ActualThemeVariant == ThemeVariant.Dark)
        {
            application.RequestedThemeVariant = ThemeVariant.Light;
        }
        else
        {
            application.RequestedThemeVariant = ThemeVariant.Dark;
        }
    }
}
