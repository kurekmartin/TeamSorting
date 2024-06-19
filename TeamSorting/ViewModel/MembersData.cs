using System.Collections;
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
                TeamMemberAdded(e.NewItems!);
                break;
            case NotifyCollectionChangedAction.Remove:
                TeamMemberRemoved(e.OldItems!);
                break;
        }
    }

    private void DisciplinesInfoOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                DisciplinesAdded(e.NewItems!);
                break;
            case NotifyCollectionChangedAction.Remove:
                DisciplinesRemoved(e.OldItems!);
                break;
            default:
                return;
        }
    }

    public ObservableCollection<DisciplineInfo> DisciplinesInfo { get; } = [];
    public ObservableCollection<TeamMember> TeamMembers { get; } = [];

    private void DisciplinesAdded(IList disciplinesAdded)
    {
        foreach (DisciplineInfo disciplineInfo in disciplinesAdded)
        {
            foreach (var teamMember in TeamMembers)
            {
                if (teamMember.Disciplines.Any(discipline => discipline.DisciplineInfo.Name == disciplineInfo.Name))
                    continue;
                var newDisciplineRecord = new DisciplineRecord(disciplineInfo, "");
                teamMember.Disciplines.Add(newDisciplineRecord);
            }
        }
    }

    private void DisciplinesRemoved(IList disciplinesRemoved)
    {
        foreach (DisciplineInfo disciplineInfo in disciplinesRemoved)
        {
            foreach (var teamMember in TeamMembers)
            {
                var disciplines = teamMember.Disciplines.Where(record => record.DisciplineInfo == disciplineInfo);
                foreach (var discipline in disciplines)
                {
                    teamMember.Disciplines.Remove(discipline);
                }
            }
        }
    }

    private void TeamMemberAdded(IList teamMembersAdded)
    {
        foreach (TeamMember teamMember in teamMembersAdded)
        {
            foreach (var disciplineInfo in DisciplinesInfo)
            {
                if (disciplineInfo.TeamMembers.Any(member => member.Name == teamMember.Name)) continue;
                disciplineInfo.TeamMembers.Add(teamMember);
            }
        }
    }

    private void TeamMemberRemoved(IList teamMembersRemoved)
    {
        foreach (TeamMember teamMember in teamMembersRemoved)
        {
            foreach (var disciplineInfo in DisciplinesInfo)
            {
                var members = disciplineInfo.TeamMembers.Where(member => member.Name == teamMember.Name);
                foreach (var member in members)
                {
                    disciplineInfo.TeamMembers.Remove(member);
                }
            }
        }
    }

    public List<string> InvalidMembersCombinationList()
    {
        List<string> notValidMembers = [];
        foreach (var teamMember in TeamMembers)
        {
            List<TeamMember> group = [teamMember];
            var i = 0;
            bool newMembersAdded;
            do
            {
                newMembersAdded = false;
                var newMembers = group[i..];
                var withMembers = newMembers.SelectMany(member => member.With).Distinct();
                foreach (string withMember in withMembers)
                {
                    var member = TeamMembers.First(member => member.Name == withMember);
                    if (group.Contains(member)) continue;
                    newMembersAdded = true;
                    group.Add(member);
                    i++;
                }
            } while (newMembersAdded);

            var membersNames = group.Select(member => member.Name).Distinct().ToList();
            var notWithMembers = group.SelectMany(member => member.NotWith).Distinct().ToList();
            bool intersectExists = membersNames.Intersect(notWithMembers).Any();
            if (intersectExists)
            {
                notValidMembers.AddRange(membersNames);
            }
            
        }

        return notValidMembers.Distinct().ToList();
    }

    public List<TeamMember> GetWithMembers(TeamMember teamMember)
    {
        List<TeamMember> group = [teamMember];
        var i = 0;
        bool newMembersAdded;
        do
        {
            newMembersAdded = false;
            var newMembers = group[i..];
            var withMembers = newMembers.SelectMany(member => member.With).Distinct();
            foreach (string withMember in withMembers)
            {
                var member = TeamMembers.First(member => member.Name == withMember);
                if (group.Contains(member)) continue;
                newMembersAdded = true;
                group.Add(member);
                i++;
            }
        } while (newMembersAdded);

        group.Remove(teamMember);
        return group;
    }

    public List<TeamMember> GetNotWithMembers(TeamMember teamMember)
    {
        List<TeamMember> group = [];
        var notWithMembers = teamMember.NotWith;
        foreach (string notWithMember in notWithMembers)
        {
            var member = TeamMembers.First(member => member.Name == notWithMember);
            if (group.Contains(member)) continue;
            group.Add(member);
        }

        return group;
    }
}