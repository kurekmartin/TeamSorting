using System.ComponentModel;
using Microsoft.Extensions.Logging;
using TeamSorting.Models;

namespace TeamSorting.ViewModels;

public class MainWindowViewModel(ILogger<MainWindowViewModel> logger, TeamsViewModel teamsViewModel, InputViewModel inputViewModel, Teams teams) : ViewModelBase
{
    private ViewModelBase _contentViewModel = inputViewModel;

    public Teams Teams { get; } = teams;

    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        private set => SetProperty(ref _contentViewModel, value);
    }

    [Localizable(false)]
    public string Version =>
        $"v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?.?.?"}";

    public void SwitchToTeamsView()
    {
        logger.LogInformation("Switching to teams view");
        ContentViewModel = teamsViewModel;
    }

    public void SwitchToInputView()
    {
        logger.LogInformation("Switching to input view");
        ContentViewModel = inputViewModel;
    }
}