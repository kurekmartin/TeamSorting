using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using ReactiveUI;
using Serilog;
using TeamSorting.Models;

namespace TeamSorting.ViewModels;

public class TeamsViewModel(Data data) : ViewModelBase
{
    public const string MemberFormat = "member-card-format";
    public WindowNotificationManager? NotificationManager { get; set; }
    private MemberSortCriteria _teamsSortCriteria;
    private Member? _draggingMember;

    public Member? DraggingMember
    {
        get => _draggingMember;
        set => this.RaiseAndSetIfChanged(ref _draggingMember, value);
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

    public void StartDrag(Member member)
    {
        Log.Debug("StartDrag {member}", member.Name);
        DraggingMember = member;
    }

    public void Drop(Member member, Control? destination)
    {
        Log.Debug("Dropping member {Member}", member.Name);
        Visual? teamControl =
            destination?.GetVisualAncestors().FirstOrDefault(ancestor => ancestor.DataContext is Team);
        if (teamControl?.DataContext is not Team team) return;
        member.MoveToTeam(team);
        Log.Debug("Moved member {name} to team {team}", member.Name, team.Name);
    }

    public static bool IsValidDestination(Member member, Control? destination)
    {
        Log.Debug("Validating destination {dest}", destination?.GetType());
        Visual? teamControl =
            destination?.GetVisualAncestors().FirstOrDefault(ancestor => ancestor.DataContext is Team);
        return teamControl is not null;
    }
}