using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using ReactiveUI;
using Serilog;
using TeamSorting.Controls;
using TeamSorting.Models;

namespace TeamSorting.ViewModels;

public class TeamsViewModel(Data data) : ViewModelBase
{
    public const string MemberFormat = "member-card-format";
    public const string DragClass = "drag-active";
    public WindowNotificationManager? NotificationManager { get; set; }
    private MemberSortCriteria _teamsSortCriteria;
    private MemberCard? _draggingMemberCard;

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
        DraggingMemberCard.Classes.Add(DragClass);
    }

    public void Drop(Member member, Control? destination)
    {
        Log.Debug("Dropping member {Member}", member.Name);
        DraggingMemberCard?.Classes.Remove(DragClass);
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