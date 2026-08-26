using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.VisualTree;
using TeamSorting.Controls;
using TeamSorting.Lang;
using TeamSorting.Models;
using TeamSorting.Utils;

namespace TeamSorting.ViewModels;

public class TeamsViewModel : ViewModelBase
{
    public const string MemberFormat = "member-card-format";
    public const string DragActiveClass = "drag-active";
    public Teams Teams { get; }
    public Disciplines Disciplines { get; }
    public CsvUtil CsvUtil { get; }
    public WindowNotificationManager? NotificationManager { get; set; }
    private MemberSortCriteria _teamsSortCriteria;
    private MemberCard? _draggingMemberCard;
    private Visual? _dragOverTeam;
    private int _numberOfTeams = 2;
    private bool _showUnsortedMembers = true;

    public TeamsViewModel(Teams teams, Disciplines disciplines, CsvUtil csvUtil)
    {
        Teams = teams;
        Disciplines = disciplines;
        CsvUtil = csvUtil;

        ((INotifyCollectionChanged)teams.TeamList).CollectionChanged += TeamsOnCollectionChanged;
    }

    private void TeamsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Reset:
                NumberOfTeams = Teams.TeamList.Count;
                break;
        }
    }

    public int NumberOfTeams
    {
        get => _numberOfTeams;
        set => SetProperty(ref _numberOfTeams, value);
    }

    public bool ShowUnsortedMembers
    {
        get => _showUnsortedMembers;
        set => SetProperty(ref _showUnsortedMembers, value);
    }

    public MemberCard? DraggingMemberCard
    {
        get => _draggingMemberCard;
        set => SetProperty(ref _draggingMemberCard, value);
    }

    public MemberSortCriteria TeamsSortCriteria
    {
        get => _teamsSortCriteria;
        set
        {
            if (_teamsSortCriteria == value)
            {
                return;
            }

            _teamsSortCriteria = value;
            Teams.SortTeamsByCriteria(_teamsSortCriteria);
        }
    }

    public void StartDrag(MemberCard memberCard)
    {
        DraggingMemberCard = memberCard;
        DraggingMemberCard.Classes.Add(DragActiveClass);
    }

    public void EndDrag()
    {
        DraggingMemberCard?.Classes.Remove(DragActiveClass);
        DraggingMemberCard = null;
        if (_dragOverTeam is not null)
        {
            RemoveTeamHighlight(_dragOverTeam);
        }
    }

    public void Drop(Member member, Control? destination)
    {
        TeamControl? teamControl = FindTeamControl(destination);
        if (teamControl?.Team is not { } team)
        {
            return;
        }

        Team? oldTeam = member.Team;
        bool moved = member.MoveToTeam(team);

        if (!moved)
        {
            return;
        }

        Team? newTeam = member.Team;
        string message = string.Format(Resources.TeamsView_MemberMoved_Message, member.Name, oldTeam?.Name,
            newTeam?.Name);
        var textbox = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(10),
            Inlines = TextParser.Parse(message)
        };
        NotificationManager?.Show(textbox, NotificationType.Success);
    }

    public bool IsValidDestination(Member member, Control? destination)
    {
        TeamControl? teamControl = FindTeamControl(destination);

        if (teamControl == _dragOverTeam)
        {
            return true;
        }

        if (teamControl is null)
        {
            RemoveTeamHighlight(_dragOverTeam);
            return false;
        }

        if (_dragOverTeam is not null)
        {
            RemoveTeamHighlight(_dragOverTeam);
        }

        if (teamControl.Team == member.Team)
        {
            _dragOverTeam = teamControl;
            return true;
        }

        _dragOverTeam = teamControl;

        AddTeamHighlight(_dragOverTeam);

        return true;
    }

    private static TeamControl? FindTeamControl(Control? destination)
    {
        TeamControl? teamControl = destination as TeamControl
                                   ?? destination?.GetVisualAncestors().OfType<TeamControl>().FirstOrDefault();
        return teamControl is { IsCompact: false } ? teamControl : null;
    }

    private static void AddTeamHighlight(Visual? control)
    {
        control?.Classes.Add("Highlight");
    }

    private void RemoveTeamHighlight(Visual? control)
    {
        control?.Classes.Remove("Highlight");

        _dragOverTeam = null;
    }
}
