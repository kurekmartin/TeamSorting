using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Logging;

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
    }
}