using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace TeamSorting.ViewModels;

public class MainWindowViewModel(ILogger<MainWindowViewModel> logger, Data data, TeamsViewModel teamsViewModel, InputViewModel inputViewModel) : ViewModelBase
{
    private ViewModelBase _contentViewModel = inputViewModel;

    public Data Data { get; } = data;

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