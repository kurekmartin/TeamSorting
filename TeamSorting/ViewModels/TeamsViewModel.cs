using TeamSorting.Models;

namespace TeamSorting.ViewModels;

public class TeamsViewModel(Data data) : ViewModelBase
{
    private MemberSortCriteria _teamsSortCriteria;
    public Data Data { get; } = data;

    public MemberSortCriteria TeamsSortCriteria
    {
        get => _teamsSortCriteria;
        set
        {
            _teamsSortCriteria = value;
            Data.SortTeamsByCriteria(_teamsSortCriteria);
        }
    }
}