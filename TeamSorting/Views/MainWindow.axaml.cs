using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeamSorting.ViewModels;

namespace TeamSorting.Views;

public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow>? _logger = Ioc.Default.GetService<ILogger<MainWindow>>();

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        _logger?.LogInformation("Window loaded");
        base.OnLoaded(e);
        
        if (DataContext is MainWindowViewModel context)
        {
            context.CheckForUpdates();
        }
    }

    public async Task<WarningDialogResult> ShowWarningDialog(WarningDialog warningDialog)
    {
        warningDialog.Position = Position; //fix for WindowStartupLocation="CenterOwner" not working
        var result = await warningDialog.ShowDialog<WarningDialogResult>(this);
        return result;
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
}