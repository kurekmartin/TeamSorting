using TeamSorting.Enums;
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
        set => SetProperty(ref _newMemberName, value);
    }
    
    private string _newDisciplineName = string.Empty;

    public string NewDisciplineName
    {
        get => _newDisciplineName;
        set => SetProperty(ref _newDisciplineName, value);
    }

    public static Array DisciplineDataTypes => Enum.GetValues(typeof(DisciplineDataType));
    public static Array SortOrder => Enum.GetValues(typeof(SortOrder));
}