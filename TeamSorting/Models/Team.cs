using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using ReactiveUI;

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
                this.RaisePropertyChanged(nameof(TotalScores));
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

    public IEnumerable<Member> SortedMembers
    {
        get
        {
            if (SortCriteria.SortOrder == SortOrder.Asc)
            {
                if (SortCriteria.Discipline is null)
                {
                    return Members.OrderBy(member => member.Name);
                }

                return Members.OrderBy(member =>
                    member.Records
                        .First(record =>
                            record.Key == SortCriteria.Discipline.Id)
                        .Value.DoubleValue);
            }

            if (SortCriteria.Discipline is null)
            {
                return Members.OrderByDescending(member => member.Name);
            }

            return Members.OrderByDescending(member =>
                member.Records
                    .First(record =>
                        record.Key == SortCriteria.Discipline.Id)
                    .Value.DoubleValue);
        }
    }


    public bool IsValid
    {
        get { return Members.All(member => member.IsValid); }
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
        member.Team = this;
        Members.Add(member);
    }


    public void RemoveMember(Member member)
    {
        member.Team = null;
        Members.Remove(member);
    }

    public Dictionary<DisciplineInfo, double> TotalScores
    {
        get
        {
            return Members.SelectMany(member => member.Records.Values)
                .GroupBy(record => record.DisciplineInfo)
                .ToDictionary(g => g.Key, g => Math.Round(g.Sum(record => record.DoubleValue), 2));
        }
    }

    public double GetTotalValueByDiscipline(DisciplineInfo discipline)
    {
        var records = Members.Select(member => member.GetRecord(discipline));
        return records.Sum(record => record.DoubleValue);
    }

    public static bool ValidateMemberList(IEnumerable<Member> members)
    {
        var invalidMembers = GetInvalidMembers(members);
        return invalidMembers.invalidWith.Count == 0 && invalidMembers.invalidNotWith.Count == 0;
    }

    public static (List<string> invalidWith, List<string> invalidNotWith) GetInvalidMembers(IEnumerable<Member> members)
    {
        var memberList = members.ToList();
        var memberNames = memberList.Select(member => member.Name).ToList();
        var with = memberList.SelectMany(member => member.With).ToList();
        var notWith = memberList.SelectMany(member => member.NotWith);

        return (with.Except(memberNames).ToList(), memberNames.Intersect(notWith).ToList());
    }
}