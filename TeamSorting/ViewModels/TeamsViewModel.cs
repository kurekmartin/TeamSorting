namespace TeamSorting.ViewModels;

public class TeamsViewModel(Data data) : ViewModelBase
{
    public TeamsViewModel() : this(new Data())
    {
    }

    public Data Data { get; } = data;
}