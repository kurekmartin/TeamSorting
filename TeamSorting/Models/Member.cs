using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamSorting.Models;

public class Member : ObservableObject, INotifyDataErrorInfo
{
    public string Name
    {
        get => _name;
        set
        {
            ClearErrors(nameof(Name));
            if (string.IsNullOrWhiteSpace(value))
            {
                AddError(nameof(Name), Lang.Resources.InputView_Member_EmptyName_Error);
            }

            SetProperty(ref _name, value);
        }
    }

    public ObservableCollection<Member> With { get; } = [];
    public List<Member> SortedWith => With.OrderBy(member => member.Name).ToList();

    //TODO add list of warnings during loading
    public Dictionary<string, bool> WithValidation => ValidateWith();

    public ObservableCollection<Member> NotWith { get; } = [];
    public List<Member> SortedNotWith => NotWith.OrderBy(member => member.Name).ToList();

    //TODO add list of warnings during loading
    public Dictionary<string, bool> NotWithValidation => ValidateNotWith();
    public AvaloniaDictionary<Guid, DisciplineRecord> Records { get; } = [];

    private Team? _team;
    private string _name = string.Empty;

    public Member(string name)
    {
        Name = name;
        With.CollectionChanged += WithOnCollectionChanged;
        NotWith.CollectionChanged += NotWithOnCollectionChanged;
    }

    private void NotWithOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems is not null)
                {
                    foreach (Member member in e.NewItems.OfType<Member>())
                    {
                        member.AddNotWithMember(this);
                    }
                }

                OnPropertyChanged(nameof(SortedNotWith));
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is not null)
                {
                    foreach (Member member in e.OldItems.OfType<Member>())
                    {
                        member.RemoveNotWithMember(this);
                    }
                }

                OnPropertyChanged(nameof(SortedNotWith));
                break;
            case NotifyCollectionChangedAction.Replace:
                OnPropertyChanged(nameof(SortedNotWith));
                break;
        }
    }

    private void WithOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems is not null)
                {
                    foreach (Member member in e.NewItems.OfType<Member>())
                    {
                        member.AddWithMember(this);
                    }
                }

                OnPropertyChanged(nameof(SortedWith));
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is not null)
                {
                    foreach (Member member in e.OldItems.OfType<Member>())
                    {
                        member.RemoveWithMember(this);
                    }
                }

                OnPropertyChanged(nameof(SortedWith));
                break;
            case NotifyCollectionChangedAction.Replace:
                OnPropertyChanged(nameof(SortedWith));
                break;
        }
    }

    public Team? Team
    {
        get => _team;
        set
        {
            if (_team == value) return;
            _team = value;
            if (_team is null) return;
            _team.Members.CollectionChanged += MembersOnCollectionChanged;
            OnPropertyChanged(nameof(WithValidation));
            OnPropertyChanged(nameof(NotWithValidation));
            OnPropertyChanged(nameof(IsValid));
        }
    }

    private void MembersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(WithValidation));
        OnPropertyChanged(nameof(NotWithValidation));
        OnPropertyChanged(nameof(IsValid));
    }

    public bool IsValid
    {
        get
        {
            return WithValidation.Values.All(val => val)
                   && NotWithValidation.Values.All(val => val);
        }
    }

    public void AddWithMember(Member member)
    {
        if (With.Contains(member))
        {
            return;
        }

        With.Add(member);
        member.AddWithMember(this);
    }

    public void AddWithMembers(IEnumerable<Member> members)
    {
        foreach (var member in members)
        {
            AddWithMember(member);
        }
    }

    public void RemoveWithMember(Member member)
    {
        if (!With.Contains(member))
        {
            return;
        }

        With.Remove(member);
        member.RemoveWithMember(this);
    }

    public void ClearWithMembers()
    {
        foreach (Member member in With.ToList())
        {
            RemoveWithMember(member);
        }
    }

    public void AddNotWithMember(Member member)
    {
        if (NotWith.Contains(member))
        {
            return;
        }

        NotWith.Add(member);
        member.AddNotWithMember(this);
    }

    public void AddNotWithMembers(IEnumerable<Member> members)
    {
        foreach (Member member in members)
        {
            AddNotWithMember(member);
        }
    }

    public void RemoveNotWithMember(Member member)
    {
        if (!NotWith.Contains(member))
        {
            return;
        }

        NotWith.Remove(member);
        member.RemoveNotWithMember(this);
    }

    public void ClearNotWithMembers()
    {
        foreach (Member member in NotWith.ToList())
        {
            RemoveNotWithMember(member);
        }
    }

    private Dictionary<string, bool> ValidateWith()
    {
        if (Team is null || Team.DisableValidation)
        {
            return SortedWith.ToDictionary(member => member.Name, _ => true);
        }

        Dictionary<string, bool> dict = [];
        foreach (string withMember in SortedWith.Select(m => m.Name))
        {
            bool value = Team?.Members.Any(member => member.Name == withMember) ?? false;
            dict.Add(withMember, value);
        }

        return dict;
    }

    private Dictionary<string, bool> ValidateNotWith()
    {
        if (Team is null || Team.DisableValidation)
        {
            return SortedNotWith.ToDictionary(member => member.Name, _ => true);
        }

        Dictionary<string, bool> dict = [];
        foreach (string notWithMember in SortedNotWith.Select(m => m.Name))
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

    public DisciplineRecord AddDisciplineRecord(DisciplineInfo discipline, string value)
    {
        if (Records.TryGetValue(discipline.Id, out var record))
        {
            record.RawValue = value;
            return record;
        }

        record = new DisciplineRecord(this, discipline, value);
        Records.Add(discipline.Id, record);
        return record;
    }

    public void RemoveDisciplineRecord(Guid recordId)
    {
        Records.Remove(recordId);
    }

    public bool MoveToTeam(Team team)
    {
        if (Team == team)
        {
            return false;
        }

        team.AddMember(this);
        return true;
    }

    public static int CompareDisciplinesDescending(Member? m1, Member? m2, DisciplineInfo discipline)
    {
        return CompareDisciplineValues(m2, m1, discipline);
    }

    public static int CompareDisciplinesAscending(Member? m1, Member? m2, DisciplineInfo discipline)
    {
        return CompareDisciplineValues(m1, m2, discipline);
    }

    private static int CompareDisciplineValues(Member? m1, Member? m2, DisciplineInfo discipline)
    {
        if (m1 is null
            || !m1.Records.TryGetValue(discipline.Id, out DisciplineRecord? member1Record)
            || m2 is null
            || !m2.Records.TryGetValue(discipline.Id, out DisciplineRecord? member2Record))
        {
            return 0;
        }

        return member1Record.DecimalValue.CompareTo(member2Record.DecimalValue);
    }

    #region Errors

    private readonly Dictionary<string, List<string>> _validationErrors = [];

    public void AddError(string propertyName, string errorMessage)
    {
        if (_validationErrors.TryGetValue(propertyName, out List<string>? errors))
        {
            if (errors.Contains(errorMessage))
            {
                return;
            }

            errors.Add(errorMessage);
        }
        else
        {
            _validationErrors.Add(propertyName, [errorMessage]);
        }

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    public void RemoveError(string propertyName, string errorMessage)
    {
        if (_validationErrors.TryGetValue(propertyName, out List<string>? errors))
        {
            errors.Remove(errorMessage);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    private void ClearErrors(string propertyName)
    {
        _validationErrors.Remove(propertyName);
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName is null)
        {
            return _validationErrors.SelectMany(pair => pair.Value);
        }

        _validationErrors.TryGetValue(propertyName, out List<string>? errors);
        return errors ?? [];
    }

    public bool HasErrors => _validationErrors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    #endregion
}