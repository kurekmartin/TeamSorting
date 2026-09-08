using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls.Primitives;
using TeamSorting.Models;

namespace TeamSorting.Controls;

public class MemberListControl : TemplatedControl
{
    public static readonly DirectProperty<MemberListControl, List<Member>> AllMembersProperty =
        AvaloniaProperty.RegisterDirect<MemberListControl, List<Member>>(
            nameof(AllMembers),
            o => o.AllMembers,
            (o, v) => o.AllMembers = v,
            []
        );

    public static readonly DirectProperty<MemberListControl, ObservableCollection<Member>> SelectedMembersProperty =
        AvaloniaProperty.RegisterDirect<MemberListControl, ObservableCollection<Member>>(
            nameof(SelectedMembers),
            o => o.SelectedMembers,
            (o, v) => o.SelectedMembers = v,
            []
        );

    public static readonly DirectProperty<MemberListControl, IReadOnlyList<string>> ValidationErrorsProperty =
        AvaloniaProperty.RegisterDirect<MemberListControl, IReadOnlyList<string>>(
            nameof(ValidationErrors),
            o => o.ValidationErrors,
            (o, v) => o.ValidationErrors = v,
            []);

    public static readonly DirectProperty<MemberListControl, IReadOnlyList<Member>> InvalidMembersProperty =
        AvaloniaProperty.RegisterDirect<MemberListControl, IReadOnlyList<Member>>(
            nameof(InvalidMembers),
            o => o.InvalidMembers,
            (o, v) => o.InvalidMembers = v,
            []);

    public static readonly DirectProperty<MemberListControl, bool> HasValidationErrorsProperty =
        AvaloniaProperty.RegisterDirect<MemberListControl, bool>(
            nameof(HasValidationErrors),
            o => o.HasValidationErrors);

    private static readonly DirectProperty<MemberListControl, List<Member>> SelectedMembersSortedProperty =
        AvaloniaProperty.RegisterDirect<MemberListControl, List<Member>>(
            nameof(SelectedMembersSorted),
            o => o.SelectedMembersSorted,
            null,
            []
        );

    private static readonly DirectProperty<MemberListControl, List<MemberListItem>> DisplayedMembersProperty =
        AvaloniaProperty.RegisterDirect<MemberListControl, List<MemberListItem>>(
            nameof(DisplayedMembers),
            o => o.DisplayedMembers,
            null,
            []);

    public static readonly DirectProperty<MemberListControl, Member> CurrentMemberProperty =
        AvaloniaProperty.RegisterDirect<MemberListControl, Member>(
            nameof(CurrentMember),
            o => o.CurrentMember,
            (o, v) => o.CurrentMember = v
        );


    private List<Member> _allMembers = [];
    private Member _currentMember = null!;
    private ObservableCollection<Member> _selectedMembers = [];
    private List<Member> _selectedMembersSorted = [];
    private IReadOnlyList<string> _validationErrors = [];
    private IReadOnlyList<Member> _invalidMembers = [];
    private List<MemberListItem> _displayedMembers = [];

    public List<Member> AllMembers
    {
        get => _allMembers;
        set => SetAndRaise(AllMembersProperty, ref _allMembers, value);
    }

    public ObservableCollection<Member> SelectedMembers
    {
        get => _selectedMembers;
        set
        {
            _selectedMembers.CollectionChanged -= SelectedMembersOnCollectionChanged;

            SetAndRaise(SelectedMembersProperty, ref _selectedMembers, value);

            if (VisualRoot is not null)
            {
                _selectedMembers.CollectionChanged += SelectedMembersOnCollectionChanged;
            }

            UpdateDisplayedMembers();
        }
    }

    public IReadOnlyList<string> ValidationErrors
    {
        get => _validationErrors;
        set
        {
            bool hadErrors = HasValidationErrors;
            if (SetAndRaise(ValidationErrorsProperty, ref _validationErrors, value))
            {
                RaisePropertyChanged(HasValidationErrorsProperty, hadErrors, HasValidationErrors);
            }
        }
    }

    public bool HasValidationErrors => ValidationErrors.Count > 0;

    public IReadOnlyList<Member> InvalidMembers
    {
        get => _invalidMembers;
        set
        {
            if (SetAndRaise(InvalidMembersProperty, ref _invalidMembers, value))
            {
                UpdateDisplayedMembers();
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _selectedMembers.CollectionChanged -= SelectedMembersOnCollectionChanged;
        _selectedMembers.CollectionChanged += SelectedMembersOnCollectionChanged;
        UpdateDisplayedMembers();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _selectedMembers.CollectionChanged -= SelectedMembersOnCollectionChanged;
    }

    private void SelectedMembersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Reset:
                UpdateDisplayedMembers();
                break;
        }
    }

    public Member CurrentMember
    {
        get => _currentMember;
        set => SetAndRaise(CurrentMemberProperty, ref _currentMember, value);
    }

    public List<Member> SelectedMembersSorted
    {
        get => _selectedMembersSorted;
        private set => SetAndRaise(SelectedMembersSortedProperty, ref _selectedMembersSorted, value);
    }

    public List<MemberListItem> DisplayedMembers
    {
        get => _displayedMembers;
        private set => SetAndRaise(DisplayedMembersProperty, ref _displayedMembers, value);
    }

    private void UpdateDisplayedMembers()
    {
        SelectedMembersSorted = [.. SelectedMembers.OrderBy(member => member.Name)];
        DisplayedMembers =
        [
            .. SelectedMembersSorted.Select(member => new MemberListItem(member, InvalidMembers.Contains(member)))
        ];
    }

    public void RemoveMember(object memberParam)
    {
        if (memberParam is not Member member)
        {
            return;
        }

        SelectedMembers.Remove(member);
    }
}

public sealed record MemberListItem(Member Member, bool IsInvalid);
