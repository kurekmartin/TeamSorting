using System.Collections.ObjectModel;
using TeamSorting.Models;

namespace TeamSorting.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public Data Data { get; } = new();

    //TODO add discipline
    //TODO remove discipline
    //TODO add member
    //TODO remove member
}