namespace TeamSorting.ViewModels;

public class InputViewModel(Data data) : ViewModelBase
{
    public InputViewModel() : this(new Data())
    {
    }

    public Data Data { get; } = data;
}