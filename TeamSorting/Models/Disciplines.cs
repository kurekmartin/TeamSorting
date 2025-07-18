﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using TeamSorting.Enums;

namespace TeamSorting.Models;

public class Disciplines : ObservableObject
{
    private readonly ILogger<Disciplines> _logger;
    private readonly Members _members;
    private readonly Teams _teams;

    public ReadOnlyObservableCollection<DisciplineInfo> DisciplineList { get; }
    private readonly ObservableCollection<DisciplineInfo> _disciplineList = [];


    public Disciplines(ILogger<Disciplines> logger, Members members, Teams teams)
    {
        _logger = logger;
        _members = members;
        _teams = teams;
        DisciplineList = new ReadOnlyObservableCollection<DisciplineInfo>(_disciplineList);
        ((INotifyCollectionChanged)_members.MemberList).CollectionChanged += MembersOnCollectionChanged;
        ((INotifyCollectionChanged)_teams.TeamList).CollectionChanged += TeamListOnCollectionChanged;
    }

    private void TeamListOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (Team team in e.NewItems!)
                {
                    team.PropertyChanged += TeamOnPropertyChanged;
                }

                OnPropertyChanged(nameof(DisciplineDelta));
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (Team team in e.OldItems!)
                {
                    team.PropertyChanged -= TeamOnPropertyChanged;
                }

                OnPropertyChanged(nameof(DisciplineDelta));
                break;
            case NotifyCollectionChangedAction.Replace:
                foreach (Team team in e.NewItems!)
                {
                    team.PropertyChanged += TeamOnPropertyChanged;
                }

                foreach (Team team in e.OldItems!)
                {
                    team.PropertyChanged -= TeamOnPropertyChanged;
                }

                OnPropertyChanged(nameof(DisciplineDelta));
                break;
            case NotifyCollectionChangedAction.Reset:
                OnPropertyChanged(nameof(DisciplineDelta));
                break;
        }
    }

    private void TeamOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender?.GetType() == typeof(Team) &&
            e.PropertyName == nameof(Team.AvgScores))
        {
            OnPropertyChanged(nameof(DisciplineDelta));
        }
    }

    private void MembersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (Member member in e.NewItems!)
                {
                    member.DisciplineRecordChanged += MemberOnDisciplineRecordChanged;
                    foreach (DisciplineInfo discipline in DisciplineList)
                    {
                        AddDisciplineRecord(member, discipline, "");
                    }
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (Member member in e.OldItems!)
                {
                    member.DisciplineRecordChanged -= MemberOnDisciplineRecordChanged;
                }

                break;
            case NotifyCollectionChangedAction.Replace:
                foreach (Member member in e.NewItems!)
                {
                    member.DisciplineRecordChanged += MemberOnDisciplineRecordChanged;
                    foreach (DisciplineInfo discipline in DisciplineList)
                    {
                        AddDisciplineRecord(member, discipline, "");
                    }
                }

                foreach (Member member in e.OldItems!)
                {
                    member.DisciplineRecordChanged -= MemberOnDisciplineRecordChanged;
                }

                break;
        }
    }

    private void MemberOnDisciplineRecordChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(DisciplineAverage));
    }

    public Dictionary<DisciplineInfo, object> DisciplineDelta
    {
        get
        {
            var dict = new Dictionary<DisciplineInfo, object>();
            foreach (DisciplineInfo discipline in DisciplineList)
            {
                if (_teams.TeamList.All(team => team.Members.Count == 0))
                {
                    dict.Add(discipline, 0);
                    continue;
                }

                List<object> teamScores = _teams.TeamList.Where(team => team.Members.Count > 0)
                                                .Select(t => t.GetAverageValueByDiscipline(discipline)).ToList();

                switch (discipline.DataType)
                {
                    case DisciplineDataType.Time:
                    {
                        TimeSpan minTimeSpan = teamScores.Select(o => (TimeSpan)o).Min();
                        TimeSpan maxTimeSpan = teamScores.Select(o => (TimeSpan)o).Max();
                        TimeSpan diffTimeSpan = maxTimeSpan - minTimeSpan;
                        dict.Add(discipline, diffTimeSpan);
                        break;
                    }
                    case DisciplineDataType.Number:
                    {
                        decimal min = teamScores.Select(o => (decimal)o).Min();
                        decimal max = teamScores.Select(o => (decimal)o).Max();
                        decimal diff = max - min;
                        dict.Add(discipline, Math.Round(diff, 2));
                        break;
                    }
                    default:
                        dict.Add(discipline, 0);
                        break;
                }
            }

            return dict;
        }
    }

    public Dictionary<DisciplineInfo, object> DisciplineAverage
    {
        get
        {
            var dict = new Dictionary<DisciplineInfo, object>();
            foreach (DisciplineInfo discipline in DisciplineList)
            {
                IEnumerable<DisciplineRecord> records = _members.MemberList.Select(member => member.GetRecord(discipline));
                object average = discipline.DataType switch
                {
                    DisciplineDataType.Time when _members.MemberList.Count == 0 => TimeSpan.Zero,
                    DisciplineDataType.Time => new TimeSpan(records.Sum(record => ((TimeSpan)record.Value).Ticks) / _members.MemberList.Count),
                    DisciplineDataType.Number when _members.MemberList.Count == 0 => 0,
                    DisciplineDataType.Number => records.Sum(record => (decimal)record.Value) / _members.MemberList.Count,
                    _ => 0
                };
                dict.Add(discipline, average);
            }

            return dict;
        }
    }

    public bool AddDiscipline(DisciplineInfo discipline)
    {
        if (DisciplineList.Any(i => i.Name == discipline.Name))
        {
            return false;
        }

        _logger.LogInformation("Adding discipline {disciplineId}", discipline.Id);
        _disciplineList.Add(discipline);
        foreach (var member in _members.MemberList)
        {
            AddDisciplineRecord(member, discipline, "");
        }

        return true;
    }

    public bool RemoveDiscipline(DisciplineInfo discipline)
    {
        _logger.LogInformation("Removing discipline {disciplineId}", discipline.Id);
        bool result = _disciplineList.Remove(discipline);
        if (!result) return result;
        foreach (var member in _members.MemberList)
        {
            member.RemoveDisciplineRecord(discipline.Id);
        }

        return result;
    }

    public void RemoveAllDisciplines()
    {
        _disciplineList.Clear();
    }

    public DisciplineInfo? GetDisciplineById(Guid id)
    {
        return DisciplineList.FirstOrDefault(discipline => discipline.Id == id);
    }

    public IEnumerable<DisciplineRecord> GetAllRecords()
    {
        return _members.MemberList.SelectMany(member => member.Records.Values);
    }

    public DisciplineRecord? AddDisciplineRecord(Member member, DisciplineInfo discipline, string value)
    {
        if (!DisciplineList.Contains(discipline)) return null;
        var record = member.AddDisciplineRecord(discipline, value);
        return record;
    }


    public IEnumerable<DisciplineRecord> GetDisciplineRecordsByDiscipline(DisciplineInfo discipline)
    {
        return GetAllRecords().Where(record => record.DisciplineInfo == discipline);
    }
}