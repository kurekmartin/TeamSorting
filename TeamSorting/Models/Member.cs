﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Collections;
using ReactiveUI;

namespace TeamSorting.Models;

public class Member : ReactiveObject
{
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
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
    private string _name;

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

                this.RaisePropertyChanged(nameof(SortedNotWith));
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is not null)
                {
                    foreach (Member member in e.OldItems.OfType<Member>())
                    {
                        member.RemoveNotWithMember(this);
                    }
                }

                this.RaisePropertyChanged(nameof(SortedNotWith));
                break;
            case NotifyCollectionChangedAction.Replace:
                this.RaisePropertyChanged(nameof(SortedNotWith));
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

                this.RaisePropertyChanged(nameof(SortedWith));
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is not null)
                {
                    foreach (Member member in e.OldItems.OfType<Member>())
                    {
                        member.RemoveWithMember(this);
                    }
                }

                this.RaisePropertyChanged(nameof(SortedWith));
                break;
            case NotifyCollectionChangedAction.Replace:
                this.RaisePropertyChanged(nameof(SortedWith));
                break;
        }
    }

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
            this.RaisePropertyChanged(nameof(IsValid));
        }
    }

    private void MembersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(WithValidation));
        this.RaisePropertyChanged(nameof(NotWithValidation));
        this.RaisePropertyChanged(nameof(IsValid));
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
        }

        record = new DisciplineRecord(this, discipline, value);
        Records.Add(discipline.Id, record);
        return record;
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