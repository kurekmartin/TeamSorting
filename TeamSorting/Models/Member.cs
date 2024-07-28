using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Collections;
using ReactiveUI;

namespace TeamSorting.Models;

public class Member(string name) : ReactiveObject
{
    public string Name { get; set; } = name;
    public ObservableCollection<string> With { get; } = [];
    public Dictionary<string, bool> WithValidation => ValidateWith();
    public ObservableCollection<string> NotWith { get; } = [];
    public Dictionary<string, bool> NotWithValidation => ValidateNotWith();
    public AvaloniaDictionary<Guid, DisciplineRecord> Records { get; } = [];

    private Team? _team;

    public Team? Team
    {
        private get => _team;
        set
        {
            if (_team == value) return;
            _team = value;
            if (_team is null) return;
            _team.Members.CollectionChanged += MembersOnCollectionChanged;
            this.RaisePropertyChanged(nameof(WithValidation));
            this.RaisePropertyChanged(nameof(NotWithValidation));
        }
    }

    private void MembersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(WithValidation));
        this.RaisePropertyChanged(nameof(NotWithValidation));
    }

    public bool IsValid
    {
        get
        {
            return WithValidation.Values.All(val => val)
                   && NotWithValidation.Values.All(val => val);
        }
    }

    public void AddWithMember(string member)
    {
        if (With.Contains(member))
        {
            return;
        }

        With.Add(member);
    }

    public void AddWithMembers(IEnumerable<string> members)
    {
        foreach (string member in members)
        {
            AddWithMember(member);
        }
    }

    public void RemoveWithMember(string member)
    {
        With.Remove(member);
    }

    public void AddNotWithMember(string member)
    {
        if (NotWith.Contains(member))
        {
            return;
        }

        NotWith.Add(member);
    }

    public void AddNotWithMembers(IEnumerable<string> members)
    {
        foreach (string member in members)
        {
            AddNotWithMember(member);
        }
    }

    public void RemoveNotWithMember(string member)
    {
        NotWith.Remove(member);
    }

    private Dictionary<string, bool> ValidateWith()
    {
        Dictionary<string, bool> dict = [];
        foreach (string withMember in With)
        {
            bool value = Team?.Members.Any(member => member.Name == withMember) ?? false;
            dict.Add(withMember, value);
        }

        return dict;
    }

    private Dictionary<string, bool> ValidateNotWith()
    {
        Dictionary<string, bool> dict = [];
        foreach (string notWithMember in NotWith)
        {
            bool value = Team?.Members.All(member => member.Name != notWithMember) ?? false;
            dict.Add(notWithMember, value);
        }

        return dict;
    }

    public DisciplineRecord GetRecord(DisciplineInfo discipline)
    {
        return Records[discipline.Id];
    }

    public List<DisciplineRecord> GetRecordList()
    {
        return Records.Values.ToList();
    }

    public void AddDisciplineRecord(DisciplineInfo discipline, string value)
    {
        if (Records.TryGetValue(discipline.Id, out var record))
        {
            record.RawValue = value;
        }

        record = new DisciplineRecord(this, discipline, value);
        Records.Add(discipline.Id, record);
    }

    public void RemoveDisciplineRecord(Guid recordId)
    {
        Records.Remove(recordId);
    }

    public void MoveToTeam(Team team)
    {
        if (Team == team)
        {
            return;
        }

        Team?.RemoveMember(this);
        team.AddMember(this);
    }
}