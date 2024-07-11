using System.Collections.ObjectModel;
using TeamSorting.Models;

namespace TeamSorting.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public Data Data { get; } = new();
    public ObservableCollection<MemberWithDisciplines> GridData { get; set; } = [];

    public void LoadGridData()
    {
        foreach (var member in Data.Members)
        {
            List<DisciplineRecord> records = [];
            //add all disciplines for member
            records.AddRange(Data.Disciplines.Select(discipline =>
                Data.GetMemberDisciplineRecord(member, discipline)));
            var memberWithDisciplines = new MemberWithDisciplines(member, records);
            GridData.Add(memberWithDisciplines);
        }
    }
    //TODO add discipline
    //TODO remove discipline
    //TODO add member
    //TODO remove member
}