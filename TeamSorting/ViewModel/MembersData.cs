using System.Collections.ObjectModel;
using System.Collections.Specialized;
using TeamSorting.Model;

namespace TeamSorting.ViewModel;

public class MembersData
{
    public MembersData()
    {
        DisciplinesInfo.CollectionChanged += DisciplinesInfoOnCollectionChanged;
        TeamMembers.CollectionChanged += TeamMembersOnCollectionChanged;
    }

    private void TeamMembersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                //TODO TeamMember added
                break;
            case NotifyCollectionChangedAction.Remove:
                //TODO TeamMember removed
                break;
            default:
                return;
        }
    }

    private void DisciplinesInfoOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                DisciplinesAdded((List<DisciplineInfo>)e.NewItems!);
                break;
            case NotifyCollectionChangedAction.Remove:
                DisciplinesRemoved((List<DisciplineInfo>)e.NewItems!);
                break;
            default:
                return;
        }
    }

    public ObservableCollection<DisciplineInfo> DisciplinesInfo { get; set; } = [];
    public ObservableCollection<TeamMember> TeamMembers { get; set; } = [];

    private void DisciplinesAdded(List<DisciplineInfo> disciplinesAdded)
    {
        foreach (var disciplineInfo in disciplinesAdded)
        {
            if (DisciplinesInfo.Any(discipline => discipline.Name == disciplineInfo.Name)) return;
            foreach (var teamMember in TeamMembers)
            {
                var newDisciplineRecord = new DisciplineRecord(disciplineInfo, "");
                teamMember.Disciplines.Add(newDisciplineRecord);
            }
        }
    }

    private void DisciplinesRemoved(List<DisciplineInfo> disciplinesRemoved)
    {
        foreach (var disciplineInfo in disciplinesRemoved)
        {
            DisciplinesInfo.Remove(disciplineInfo);
            foreach (var teamMember in TeamMembers)
            {
                teamMember.Disciplines.RemoveAll(record => record.DisciplineInfo == disciplineInfo);
            }
        }
    }

    public void TeamMemberAdded(TeamMember teamMember)
    {
        if (TeamMembers.Any(member => member.Name == teamMember.Name)) return;
        foreach (var disciplineInfo in DisciplinesInfo)
        {
            disciplineInfo.TeamMembers.Add(teamMember);
        }

        TeamMembers.Add(teamMember);
    }
}