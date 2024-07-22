using TeamSorting.Sorting;

namespace TeamSorting.ViewModels;

public class InputViewModel(Data data, ISorter sorter) : ViewModelBase
{
    public ISorter Sorter { get; } = sorter;
    public Data Data { get; } = data;
}