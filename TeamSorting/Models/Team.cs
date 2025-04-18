using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using ReactiveUI;
using Serilog;
using TeamSorting.Enums;

namespace TeamSorting.Models;

public class Team : ReactiveObject
{
    public Team([Localizable(false)] string name)
    {
        Name = name;
        Members.CollectionChanged += MembersOnCollectionChanged;
    }

    private void MembersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Reset:
                this.RaisePropertyChanged(nameof(IsValid));
                this.RaisePropertyChanged(nameof(AvgScores));
                this.RaisePropertyChanged(nameof(SortedMembers));
                break;
        }
    }

    public string Name { get; set; }
    public ObservableCollection<Member> Members { get; } = [];

    private MemberSortCriteria _memberSortCriteria = new(null, SortOrder.Asc);

    public MemberSortCriteria SortCriteria
    {
        private get => _memberSortCriteria;
        set
        {
            this.RaiseAndSetIfChanged(ref _memberSortCriteria, value);
            this.RaisePropertyChanged(nameof(SortedMembers));
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
            Log.Debug($"Sorted members for {Name} by discipline {SortCriteria.Discipline.Name}");
        }
        else
        {
            Log.Debug($"Sorted members for {Name} by name");
        }

        foreach (var member in sortedMembers)
        {
            if (SortCriteria.Discipline is not null)
            {
                var value = member.Records.First(record => record.Key == SortCriteria.Discipline.Id);
                Log.Debug($"{member.Name}: {value.Value.Value}");
            }
            else
            {
                Log.Debug($"{member.Name}");
            }
        }
#endif
    }

    public bool DisableValidation { get; init; }

    public bool IsValid
    {
        get { return !DisableValidation && Members.All(member => member.IsValid); }
    }

    public void AddMembers(IEnumerable<Member> members)
    {
        foreach (var member in members)
        {
            AddMember(member);
        }
    }

    public void AddMember(Member member)
    {
        member.Team?.RemoveMember(member);
        member.Team = this;
        Members.Add(member);
    }


    public void RemoveMember(Member member)
    {
        member.Team = null;
        Members.Remove(member);
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
}