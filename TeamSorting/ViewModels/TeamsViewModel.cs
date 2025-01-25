using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Serilog;
using TeamSorting.Controls;
using TeamSorting.Models;

namespace TeamSorting.ViewModels;

public class TeamsViewModel(Data data) : ViewModelBase
{
    public const string MemberFormat = "member-card-format";
    public const string DragActiveClass = "drag-active";
    public WindowNotificationManager? NotificationManager { get; set; }
    private MemberSortCriteria _teamsSortCriteria;
    private MemberCard? _draggingMemberCard;
    private Timer? _timer;
    private Visual? _dragOverTeam;

    private static readonly Border TeamHighlight = new()
    {
        Background = Brushes.Aqua,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
        IsHitTestVisible = false,
        Opacity = 0.5,
        CornerRadius = new CornerRadius(5)
    };

    public MemberCard? DraggingMemberCard
    {
        get => _draggingMemberCard;
        set => this.RaiseAndSetIfChanged(ref _draggingMemberCard, value);
    }

    public Data Data { get; } = data;

    public MemberSortCriteria TeamsSortCriteria
    {
        get => _teamsSortCriteria;
        set
        {
            _teamsSortCriteria = value;
            Data.SortTeamsByCriteria(_teamsSortCriteria);
        }
    }

    public void StartDrag(MemberCard memberCard)
    {
        Log.Debug("StartDrag {member}", memberCard.Member.Name);
        DraggingMemberCard = memberCard;
        DraggingMemberCard.Classes.Add(DragActiveClass);
    }

    public void EndDrag()
    {
        DraggingMemberCard?.Classes.Remove(DragActiveClass);
        DraggingMemberCard = null;
        if (_dragOverTeam is not null)
        {
            AdornerLayer.SetAdorner(_dragOverTeam, null);
        }
    }

    public void Drop(Member member, Control? destination)
    {
        Log.Debug("Dropping member {Member}", member.Name);

        Visual? teamControl =
            destination?.GetVisualAncestors().FirstOrDefault(ancestor => ancestor.DataContext is Team);
        if (teamControl?.DataContext is not Team team) return;

        bool moved = member.MoveToTeam(team);

        if (!moved) return;
        Log.Debug("Moved member {name} to team {team}", member.Name, team.Name);
    }

    public bool IsValidDestination(Member member, Control? destination, bool isLeaving = false)
    {
        Log.Debug("Validating destination {dest}", destination?.GetType());
        Visual? teamControl;
        if (destination is Border && destination.DataContext is Team)
        {
            teamControl = destination;
        }
        else
        {
            teamControl = destination?
                .GetVisualAncestors()
                .OfType<Border>()
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
        AdornerLayer.SetAdorner(_dragOverTeam, TeamHighlight);

        return true;
    }

    private void RemoveTeamHighlight(Visual? control)
    {
        if (control is null) return;
        AdornerLayer.SetAdorner(control, null);
        _dragOverTeam = null;
    }

    private void ResetTimer()
    {
        _timer?.Change(100, 100);
    }

    private void CheckDrag()
    {
        Log.Debug("Checking drag");
        Dispatcher.UIThread.Post(() => RemoveTeamHighlight(_dragOverTeam));
        _timer?.Dispose();
        _timer = null;
    }
}