using ReactiveUI;
using TeamSorting.Models;
using TeamSorting.Sorting;

namespace TeamSorting.ViewModels;

public class InputViewModel(Data data, ISorter sorter) : ViewModelBase
{
    public ISorter Sorter { get; } = sorter;
    public Data Data { get; } = data;
    public int NumberOfTeams { get; set; } = 2;
    private string _newMemberName = string.Empty;

    public string NewMemberName
    {
        get => _newMemberName;
        set => this.RaiseAndSetIfChanged(ref _newMemberName, value);
    }
}