﻿using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using TeamSorting.Enums;
using TeamSorting.Lang;
using TeamSorting.Sorting;

namespace TeamSorting.Models;

public class Teams : ObservableObject
{
    private readonly ILogger<Teams> _logger;
    private readonly Members _members;
    private readonly ISorter _sorter;
    private int _teamNumber = 1;
    private bool _sortingInProgress;

    public Teams(ILogger<Teams> logger, Members members, ISorter sorter)
    {
        _logger = logger;
        _members = members;
        _sorter = sorter;
        ((INotifyCollectionChanged)_members.MemberList).CollectionChanged += MemberListOnCollectionChanged;
        TeamList = new ReadOnlyObservableCollection<Team>(_teamList);
    }

    private void MemberListOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                HandleAddedMembers(e.NewItems!);
                break;
            case NotifyCollectionChangedAction.Remove:
                HandleRemovedMembers(e.OldItems!);
                break;
            case NotifyCollectionChangedAction.Replace:
                HandleRemovedMembers(e.OldItems!);
                HandleAddedMembers(e.NewItems!);
                break;
            case NotifyCollectionChangedAction.Reset:
                HandleMembersReset();
                break;
        }
    }

    private void HandleMembersReset()
    {
        MembersWithoutTeam.RemoveAllMembers();
        MembersWithoutTeam.AddMembers(_members.MemberList);
    }

    private void HandleAddedMembers(IList addedMembers)
    {
        foreach (Member member in addedMembers)
        {
            if (member.Team is not null)
            {
                continue;
            }

            MembersWithoutTeam.AddMember(member);
        }
    }

    private static void HandleRemovedMembers(IList removedMembers)
    {
        foreach (Member member in removedMembers)
        {
            member.Team?.RemoveMember(member);
        }
    }

    private readonly ObservableCollection<Team> _teamList = [];

    public ReadOnlyObservableCollection<Team> TeamList { get; }

    public Team MembersWithoutTeam { get; } = new(Resources.Data_TeamName_Unsorted, TeamType.UnsortedTeam);

    //TODO: move progress indication to separate class
    public bool SortingInProgress
    {
        get => _sortingInProgress;
        set => SetProperty(ref _sortingInProgress, value);
    }

    public ProgressValues Progress { get; } = new();
    public string InputSeed { get; set; } = string.Empty;
    public string UsedSeed { get; set; } = string.Empty;

    public bool AddTeam(Team team)
    {
        if (_teamList.Any(t => t.Name == team.Name))
        {
            return false;
        }

        _logger.LogInformation("Adding team {teamId}", team.Id);
        _teamList.Add(team);
        _teamNumber++;

        return true;
    }

    public Team CreateAndAddTeam()
    {
        var team = new Team(string.Format(Resources.Data_TeamName_Template, _teamNumber), TeamType.SortTeam);
        AddTeam(team);
        return team;
    }

    public bool RemoveTeam(Team team)
    {
        foreach (Member member in team.Members.ToList())
        {
            member.MoveToTeam(MembersWithoutTeam);
        }

        _logger.LogInformation("Removing team {teamId}", team.Id);
        bool result = _teamList.Remove(team);
        return result;
    }

    public void RemoveAllTeams()
    {
        _logger.LogInformation("Removing all teams.");
        _teamList.Clear();
        _teamNumber = 1;
    }


    public void SortTeamsByCriteria(MemberSortCriteria sortCriteria)
    {
        foreach (var team in _teamList)
        {
            team.SortCriteria = sortCriteria;
        }

        MembersWithoutTeam.SortCriteria = sortCriteria;
    }

    public List<Member> LockCurrentMembers()
    {
        List<Member> changedMembers = [];
        foreach (Team team in _teamList)
        {
            changedMembers.AddRange(team.LockMembers());
        }

        return changedMembers;
    }

    public List<Member> UnlockCurrentMembers()
    {
        List<Member> changedMembers = [];
        foreach (Team team in _teamList)
        {
            changedMembers.AddRange(team.UnlockMembers());
        }

        return changedMembers;
    }

    //TODO: move sorting to separate class
    public async Task SortToTeams(int? numberOfTeams = null)
    {
        SortingInProgress = true;
        var teamsToCreate = 0;
        if (numberOfTeams is not null && _teamList.Count < numberOfTeams)
        {
            teamsToCreate = (int)(numberOfTeams - _teamList.Count);
        }

        if (teamsToCreate > 0)
        {
            for (var i = 0; i < teamsToCreate; i++)
            {
                CreateAndAddTeam();
            }
        }

        string? seed = await Task.Run(() => _sorter.Sort(_members.MemberList.ToList(), _teamList.ToList(), Progress, InputSeed));

        UsedSeed = seed ?? string.Empty;
        SortingInProgress = false;
    }
}