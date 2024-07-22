namespace TeamSorting.ViewModels;

public class TeamsViewModel(Data data) : ViewModelBase
{
    public Data Data { get; } = data;
}