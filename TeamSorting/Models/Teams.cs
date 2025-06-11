using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using TeamSorting.Lang;
using TeamSorting.Sorting;
using TeamSorting.ViewModels;
using TeamSorting.Views;

namespace TeamSorting.Models;

public class Teams : ObservableObject
{
    private readonly ILogger<Teams> _logger;
    private readonly Lazy<Members> _members;
    private readonly ISorter _sorter;
    private int _teamNumber = 1;
    private bool _sortingInProgress;

    public Teams(ILogger<Teams> logger, Lazy<Members> members, ISorter sorter)
    {
        _logger = logger;
        _members = members;
        _sorter = sorter;
    }

    /// <summary>
    /// Do not modify this list directly.
    /// </summary>
    public ObservableCollection<Team> TeamList { get; } = [];
    public Team MembersWithoutTeam { get; } = new(Resources.Data_TeamName_Unsorted) { DisableValidation = true };

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
        if (TeamList.Any(t => t.Name == team.Name))
        {
            return false;
        }

        _logger.LogInformation("Adding team {teamId}", team.Id);
        TeamList.Add(team);
        _teamNumber++;

        return true;
    }

    public Team CreateAndAddTeam()
    {
        var team = new Team(string.Format(Resources.Data_TeamName_Template, _teamNumber));
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
        bool result = TeamList.Remove(team);
        return result;
    }

    public void RemoveAllTeams()
    {
        _logger.LogInformation("Removing all teams.");
        TeamList.Clear();
        _teamNumber = 1;
    }


    public void SortTeamsByCriteria(MemberSortCriteria sortCriteria)
    {
        foreach (var team in TeamList)
        {
            team.SortCriteria = sortCriteria;
        }

        MembersWithoutTeam.SortCriteria = sortCriteria;
    }

    //TODO: move sorting to separate class
    public async Task SortToTeams(UserControl visual, int? numberOfTeams = null)
    {
        var window = TopLevel.GetTopLevel(visual);
        if (window is not MainWindow { DataContext: MainWindowViewModel mainWindowViewModel } mainWindow)
        {
            return;
        }

        //Warn user about deletion of current teams
        if (TeamList.Count > 0)
        {
            var dialog = new WarningDialog(
                message: Resources.InputView_Sort_WarningDialog_Message,
                confirmButtonText: Resources.InputView_Sort_WarningDialog_Delete,
                cancelButtonText: Resources.InputView_Sort_WarningDialog_Cancel)
            {
                Position = mainWindow.Position //fix for WindowStartupLocation="CenterOwner" not working
            };
            var result = await dialog.ShowDialog<WarningDialogResult>(mainWindow);
            if (result == WarningDialogResult.Cancel)
            {
                return;
            }
        }

        mainWindow.Cursor = new Cursor(StandardCursorType.Wait);

        int teamsCount = numberOfTeams ?? TeamList.Count;
        SortingInProgress = true;
        (List<Team> teams, string? seed) sortResult = await Task.Run(() => _sorter.Sort(_members.Value.MemberList.ToList(), teamsCount, Progress, InputSeed));
        RemoveAllTeams();
        foreach (Team team in sortResult.teams)
        {
            AddTeam(team);
        }

        UsedSeed = sortResult.seed ?? string.Empty;
        SortingInProgress = false;
        mainWindowViewModel.SwitchToTeamsView();
        mainWindow.Cursor = Cursor.Default;
    }
}