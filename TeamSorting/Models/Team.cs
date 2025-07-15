using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeamSorting.Enums;

namespace TeamSorting.Models;

public class Team : ObservableObject
{
    private readonly ILogger? _logger = Ioc.Default.GetService<ILogger<Team>>();

    public Team([Localizable(false)] string name, TeamType teamType)
    {
        Name = name;
        TeamType = teamType;
        Members = new ReadOnlyObservableCollection<Member>(_members);
        _members.CollectionChanged += MembersOnCollectionChanged;
    }

    private void MembersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Reset:
                OnPropertyChanged(nameof(IsValid));
                OnPropertyChanged(nameof(AvgScores));
                SortMembers();
                break;
        }
    }

    public Guid Id { get; } = Guid.NewGuid();

    public string Name { get; set; }
    public TeamType TeamType { get; }
    public ReadOnlyObservableCollection<Member> Members { get; }
    private readonly ObservableCollection<Member> _members = [];

    private MemberSortCriteria _memberSortCriteria = new(null, SortOrder.Asc);

    public MemberSortCriteria SortCriteria
    {
        private get => _memberSortCriteria;
        set
        {
            SetProperty(ref _memberSortCriteria, value);
            SortMembers();
        }
    }

    private void SortMembers()
    {
        List<Member> sortedMembers;
        if (SortCriteria.SortOrder == SortOrder.Asc)
        {
            if (SortCriteria.Discipline is null)
            {
                sortedMembers = Members.OrderBy(member => member.Name).ToList();
                MoveMembersByList(sortedMembers);
                return;
            }

            sortedMembers = Members.OrderBy(member =>
                                       member.Records
                                             .First(record =>
                                                 record.Key == SortCriteria.Discipline.Id)
                                             .Value.DecimalValue)
                                   .ToList();
            MoveMembersByList(sortedMembers);
            return;
        }

        if (SortCriteria.Discipline is null)
        {
            sortedMembers = Members.OrderByDescending(member => member.Name).ToList();
            MoveMembersByList(sortedMembers);
            return;
        }

        sortedMembers = Members.OrderByDescending(member =>
                                   member.Records
                                         .First(record =>
                                             record.Key == SortCriteria.Discipline.Id)
                                         .Value.DecimalValue)
                               .ToList();
        MoveMembersByList(sortedMembers);
    }

    private void MoveMembersByList(List<Member> members)
    {
        foreach (Member member in members)
        {
            int oldIndex = _members.IndexOf(member);
            if (oldIndex == -1) continue;
            int newIndex = members.IndexOf(member);
            if (oldIndex == newIndex) continue;
            _members.Move(oldIndex, newIndex);
        }
    }

    private int GetMemberSortIndex(Member member)
    {
        var i = 0;
        for (; i < _members.Count; i++)
        {
            int compareResult;
            if (SortCriteria.Discipline is null)
            {
                compareResult = string.CompareOrdinal(member.Name, _members[i].Name);
            }
            else
            {
                compareResult = member.Records[SortCriteria.Discipline.Id].DecimalValue.CompareTo(_members[i].Records[SortCriteria.Discipline.Id].DecimalValue);
            }

            if (SortCriteria.SortOrder == SortOrder.Asc && compareResult < 0
                || SortCriteria.SortOrder == SortOrder.Desc && compareResult > 0)
            {
                return i;
            }
        }

        return i;
    }

    public bool DisableValidation => TeamType == TeamType.UnsortedTeam;
    public bool CanPinMember => TeamType == TeamType.SortTeam;

    public bool IsValid
    {
        get { return !DisableValidation && Members.All(member => member.IsValid); }
    }

    public void AddMembers(IEnumerable<Member> members)
    {
        foreach (Member member in members)
        {
            AddMember(member);
        }
    }

    public void AddMember(Member member)
    {
        _logger?.LogInformation("Adding member {memberId} to team {teamId}", member.Id, Id);
        member.Team?.RemoveMember(member);
        member.Team = this;
        if (TeamType == TeamType.UnsortedTeam)
        {
            member.AllowTeamChange = true;
        }

        int index = GetMemberSortIndex(member);
        _members.Insert(index, member);
    }


    public void RemoveMember(Member member)
    {
        _logger?.LogInformation("Removing member {memberId} from team {teamId}", member.Id, Id);
        member.Team = null;
        _members.Remove(member);
    }

    public void RemoveAllMembers()
    {
        _logger?.LogInformation("Removing all members from team {teamId}", Id);
        foreach (Member member in _members)
        {
            RemoveMember(member);
        }
    }

    public Dictionary<DisciplineInfo, object> AvgScores
    {
        get
        {
            Dictionary<DisciplineInfo, object> dictionary = [];
            var records = Members.SelectMany(member => member.Records.Values);
            var disciplines = records.Select(record => record.DisciplineInfo).Distinct();
            foreach (var discipline in disciplines)
            {
                dictionary[discipline] = GetAverageValueByDiscipline(discipline);
            }

            return dictionary;
        }
    }

    public object GetAverageValueByDiscipline(DisciplineInfo discipline)
    {
        IEnumerable<DisciplineRecord> records = Members.Select(member => member.GetRecord(discipline));
        return discipline.DataType switch
        {
            DisciplineDataType.Time when Members.Count == 0 => TimeSpan.Zero,
            DisciplineDataType.Time => new TimeSpan(records.Sum(record => ((TimeSpan)record.Value).Ticks) / Members.Count),
            DisciplineDataType.Number when Members.Count == 0 => 0,
            DisciplineDataType.Number => records.Sum(record => (decimal)record.Value) / Members.Count,
            _ => 0
        };
    }

    public static int InvalidMemberCount(IEnumerable<Member> members)
    {
        var invalidMembers = GetInvalidMembers(members);
        return invalidMembers.invalidWith.Count + invalidMembers.invalidNotWith.Count;
    }

    public static (List<string> invalidWith, List<string> invalidNotWith) GetInvalidMembers(IEnumerable<Member> members)
    {
        var memberList = members.ToList();
        var memberNames = memberList.Select(member => member.Name).ToList();
        var with = memberList.SelectMany(member => member.With.Select(m => m.Name)).ToList();
        var notWith = memberList.SelectMany(member => member.NotWith);

        return (with.Except(memberNames).ToList(), memberNames.Intersect(notWith.Select(m => m.Name)).ToList());
    }

    public List<Member> LockMembers()
    {
        List<Member> changedMembers = [];
        foreach (Member member in _members)
        {
            if (!member.AllowTeamChange) continue;
            member.AllowTeamChange = false;
            changedMembers.Add(member);
        }

        return changedMembers;
    }

    public List<Member> UnlockMembers()
    {
        List<Member> changedMembers = [];
        foreach (Member member in _members)
        {
            if (member.AllowTeamChange) continue;
            member.AllowTeamChange = true;
            changedMembers.Add(member);
        }

        return changedMembers;
    }
}