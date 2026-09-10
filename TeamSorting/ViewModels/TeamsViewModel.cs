using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using TeamSorting.Actions;
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
    public IRelayCommand UndoCommand { get; }
    public IRelayCommand RedoCommand { get; }
    private readonly UndoRedoHistory _history;
    private MemberSortCriteria _teamsSortCriteria;
    private MemberCard? _draggingMemberCard;
    private Visual? _dragOverTeam;
    private object? _moveNotificationContent;
    private int _numberOfTeams = 2;
    private bool _showUnsortedMembers = true;

    public TeamsViewModel(
        Teams teams,
        Disciplines disciplines,
        CsvUtil csvUtil,
        Members members,
        UndoRedoHistory history)
    {
        Teams = teams;
        Disciplines = disciplines;
        CsvUtil = csvUtil;
        _history = history;
        UndoCommand = new RelayCommand(Undo, CanUndo);
        RedoCommand = new RelayCommand(Redo, CanRedo);

        ((INotifyCollectionChanged)teams.TeamList).CollectionChanged += TeamsOnCollectionChanged;
        ((INotifyCollectionChanged)members.MemberList).CollectionChanged += MembersOnCollectionChanged;
        teams.PropertyChanged += TeamsOnPropertyChanged;
        history.StateChanged += HistoryOnStateChanged;
    }

    private void TeamsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Remove
            or NotifyCollectionChangedAction.Replace
            or NotifyCollectionChangedAction.Reset)
        {
            ClearHistory();
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Reset:
                NumberOfTeams = Teams.TeamList.Count;
                break;
        }
    }

    private void MembersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Remove
            or NotifyCollectionChangedAction.Replace
            or NotifyCollectionChangedAction.Reset)
        {
            ClearHistory();
        }
    }

    private void TeamsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Teams.SortingInProgress))
        {
            NotifyCommandsCanExecuteChanged();
        }
    }

    private void HistoryOnStateChanged(object? sender, EventArgs e)
    {
        CloseMoveNotification();
        NotifyCommandsCanExecuteChanged();
    }

    private bool CanUndo()
    {
        return !Teams.SortingInProgress && _history.CanUndo;
    }

    private bool CanRedo()
    {
        return !Teams.SortingInProgress && _history.CanRedo;
    }

    private void NotifyCommandsCanExecuteChanged()
    {
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }

    private void Undo()
    {
        _history.Undo();
    }

    private void Redo()
    {
        _history.Redo();
    }

    public void ClearHistory()
    {
        _history.Clear();
        CloseMoveNotification();
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
        if (teamControl?.Team is not { } newTeam)
        {
            return;
        }

        Team? oldTeam = member.Team;
        if (oldTeam is null)
        {
            return;
        }

        bool moved = _history.Execute(new MoveMemberAction(member, oldTeam, newTeam));

        if (!moved)
        {
            return;
        }

        newTeam = member.Team;
        string message = string.Format(Resources.TeamsView_MemberMoved_Message, member.Name, oldTeam?.Name,
            newTeam?.Name);
        var textbox = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Inlines = TextParser.Parse(message)
        };
        var undoButton = new Button
        {
            Content = Resources.TeamsView_Undo_Button,
            Command = UndoCommand,
            HorizontalAlignment = HorizontalAlignment.Right,
            Padding = new Thickness(0)
        };
        var content = new StackPanel
        {
            Margin = new Thickness(10),
            Spacing = 8,
            Children =
            {
                textbox,
                undoButton
            }
        };

        _moveNotificationContent = content;
        NotificationManager?.Show(content, NotificationType.Success);
    }

    private void CloseMoveNotification()
    {
        if (_moveNotificationContent is null)
        {
            return;
        }

        NotificationManager?.Close(_moveNotificationContent);
        _moveNotificationContent = null;
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