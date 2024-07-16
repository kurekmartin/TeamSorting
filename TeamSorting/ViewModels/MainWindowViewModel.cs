using ReactiveUI;

namespace TeamSorting.ViewModels;

public class MainWindowViewModel(Data data, TeamsViewModel teamsViewModel, InputViewModel inputViewModel) : ViewModelBase
{
    private ViewModelBase _contentViewModel = new InputViewModel(data);
    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        private set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }

    public void SwitchToTeamsView()
    {
        ContentViewModel = teamsViewModel;
    }

    public void SwitchToInputView()
    {
        ContentViewModel = inputViewModel;
    }
}