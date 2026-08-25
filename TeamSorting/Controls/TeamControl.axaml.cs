using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TeamSorting.Enums;
using TeamSorting.Models;

namespace TeamSorting.Controls;

public partial class TeamControl : UserControl
{
    public static readonly StyledProperty<Team> TeamProperty =
        AvaloniaProperty.Register<TeamControl, Team>(nameof(Team));

    public static readonly StyledProperty<bool> IsCompactProperty =
        AvaloniaProperty.Register<TeamControl, bool>(nameof(IsCompact));

    public static readonly DirectProperty<TeamControl, bool> IsUnsortedProperty =
        AvaloniaProperty.RegisterDirect<TeamControl, bool>(nameof(IsUnsorted), control => control.IsUnsorted);

    public static readonly RoutedEvent<RoutedEventArgs> DeleteRequestedEvent =
        RoutedEvent.Register<TeamControl, RoutedEventArgs>(nameof(DeleteRequested), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> TeamNameChangedEvent =
        RoutedEvent.Register<TeamControl, RoutedEventArgs>(nameof(TeamNameChanged), RoutingStrategies.Bubble);

    public TeamControl()
    {
        InitializeComponent();
    }

    public Team Team
    {
        get => GetValue(TeamProperty);
        set => SetValue(TeamProperty, value);
    }

    public bool IsCompact
    {
        get => GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    private bool _isUnsorted;

    public bool IsUnsorted
    {
        get => _isUnsorted;
        private set => SetAndRaise(IsUnsortedProperty, ref _isUnsorted, value);
    }

    public event EventHandler<RoutedEventArgs> DeleteRequested
    {
        add => AddHandler(DeleteRequestedEvent, value);
        remove => RemoveHandler(DeleteRequestedEvent, value);
    }

    public event EventHandler<RoutedEventArgs> TeamNameChanged
    {
        add => AddHandler(TeamNameChangedEvent, value);
        remove => RemoveHandler(TeamNameChangedEvent, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TeamProperty)
        {
            IsUnsorted = Team?.TeamType == TeamType.UnsortedTeam;
        }
    }

    private void DeleteTeam_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Team?.TeamType == TeamType.UnsortedTeam) return;
        RaiseEvent(new RoutedEventArgs(DeleteRequestedEvent));
    }

    private void PinMembers_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Team?.TeamType == TeamType.SortTeam) Team.PinMembers();
    }

    private void UnpinMembers_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Team?.TeamType == TeamType.SortTeam) Team.UnpinMembers();
    }

    private void TeamName_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (Team?.TeamType == TeamType.UnsortedTeam) return;
        RaiseEvent(new RoutedEventArgs(TeamNameChangedEvent));
    }
}
