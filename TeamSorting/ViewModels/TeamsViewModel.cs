using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using TeamSorting.Controls;
using TeamSorting.Lang;
using TeamSorting.Models;
using TeamSorting.Utils;

namespace TeamSorting.ViewModels;

public class TeamsViewModel(Data data, Teams teams) : ViewModelBase
{
    public const string MemberFormat = "member-card-format";
    public const string DragActiveClass = "drag-active";
    public WindowNotificationManager? NotificationManager { get; set; }
    private MemberSortCriteria _teamsSortCriteria;
    private MemberCard? _draggingMemberCard;
    private Timer? _timer;
    private Visual? _dragOverTeam;

    public MemberCard? DraggingMemberCard
    {
        get => _draggingMemberCard;
        set => SetProperty(ref _draggingMemberCard, value);
    }

    public Data Data { get; } = data;
    public Teams Teams { get; } = teams;

    public MemberSortCriteria TeamsSortCriteria
    {
        get => _teamsSortCriteria;
        set
        {
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
        Visual? teamControl =
            destination?.GetVisualAncestors().FirstOrDefault(ancestor => ancestor.DataContext is Team);
        if (teamControl?.DataContext is not Team team) return;

        Team? oldTeam = member.Team;
        bool moved = member.MoveToTeam(team);

        if (!moved) return;

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
        Visual? teamControl;
        if (destination is Grid && destination is { DataContext: Team, Name: "Team" })
        {
            teamControl = destination;
        }
        else
        {
            teamControl = destination?
                .GetVisualAncestors()
                .OfType<Grid>()
                .FirstOrDefault(ancestor => ancestor is { Name: "Team", DataContext: Team });
        }

        if (teamControl is not null)
        {
            _timer ??= new Timer(_ => CheckDrag());
            ResetTimer();
        }

        if (teamControl == _dragOverTeam) return true;

        if (teamControl is null)
        {
            RemoveTeamHighlight(_dragOverTeam);
            return false;
        }

        if (_dragOverTeam is not null)
        {
            RemoveTeamHighlight(_dragOverTeam);
        }

        if (teamControl.DataContext is Team team && team == member.Team)
        {
            _dragOverTeam = teamControl;
            return true;
        }

        _dragOverTeam = teamControl;

        AddTeamHighlight(_dragOverTeam);

        return true;
    }

    private static void AddTeamHighlight(Visual? control)
    {
        Border? highlight = control?.GetLogicalChildren().OfType<Border>()
            .FirstOrDefault(child => child.Name == "TeamHighlight");
        if (highlight != null)
        {
            highlight.IsVisible = true;
        }
    }

    private void RemoveTeamHighlight(Visual? control)
    {
        Border? highlight = control?.GetLogicalChildren().OfType<Border>()
            .FirstOrDefault(child => child.Name == "TeamHighlight");
        if (highlight != null)
        {
            highlight.IsVisible = false;
        }

        _dragOverTeam = null;
    }

    private void ResetTimer()
    {
        _timer?.Change(100, 100);
    }

    private void CheckDrag()
    {
        Dispatcher.UIThread.Post(() => RemoveTeamHighlight(_dragOverTeam));
        _timer?.Dispose();
        _timer = null;
    }
}