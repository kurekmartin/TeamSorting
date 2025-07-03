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
                OnPropertyChanged(nameof(SortedMembers));
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
            OnPropertyChanged(nameof(SortedMembers));
        }
    }

    public List<Member> SortedMembers
    {
        get
        {
            List<Member> sortedMembers;
            if (SortCriteria.SortOrder == SortOrder.Asc)
            {
                if (SortCriteria.Discipline is null)
                {
                    sortedMembers = Members.OrderBy(member => member.Name).ToList();
                    LogMembersOrder(sortedMembers);
                    return sortedMembers;
                }

                sortedMembers = Members.OrderBy(member =>
                                           member.Records
                                                 .First(record =>
                                                     record.Key == SortCriteria.Discipline.Id)
                                                 .Value.DecimalValue)
                                       .ToList();
                LogMembersOrder(sortedMembers);
                return sortedMembers;
            }

            if (SortCriteria.Discipline is null)
            {
                sortedMembers = Members.OrderByDescending(member => member.Name).ToList();
                LogMembersOrder(sortedMembers);
                return sortedMembers;
            }

            sortedMembers = Members.OrderByDescending(member =>
                                       member.Records
                                             .First(record =>
                                                 record.Key == SortCriteria.Discipline.Id)
                                             .Value.DecimalValue)
                                   .ToList();
            LogMembersOrder(sortedMembers);
            return sortedMembers;
        }
    }

    private void LogMembersOrder(List<Member> sortedMembers)
    {
#if DEBUG
        if (SortCriteria.Discipline is not null)
        {
            _logger?.LogDebug("Sorted members for {TeamName} by discipline {DisciplineName}", Name, SortCriteria.Discipline.Name);
        }
        else
        {
            _logger?.LogDebug("Sorted members for {TeamName} by name", Name);
        }

        foreach (var member in sortedMembers)
        {
            if (SortCriteria.Discipline is not null)
            {
                var value = member.Records.First(record => record.Key == SortCriteria.Discipline.Id);
                _logger?.LogDebug("{MemberName}: {ValueValue}", member.Name, value.Value.Value);
            }
            else
            {
                _logger?.LogDebug("{MemberName}", member.Name);
            }
        }
#endif
    }

    public bool DisableValidation => TeamType == TeamType.UnsortedTeam;
    public bool CanPinMember => TeamType == TeamType.SortTeam;

    public bool IsValid
    {
        get { return !DisableValidation && Members.All(member => member.IsValid); }
    }

    public void AddMembers(IEnumerable<Member> members)
    {
        //TODO pause collection changed events while adding
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

        _members.Add(member);
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