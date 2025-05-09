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

    private static readonly DirectProperty<MemberListControl, List<Member>> SelectedMembersSortedProperty =
        AvaloniaProperty.RegisterDirect<MemberListControl, List<Member>>(
            nameof(SelectedMembersSorted),
            o => o.SelectedMembersSorted,
            null,
            []
        );

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
            SelectedMembers.CollectionChanged -= SelectedMembersOnCollectionChanged;
            SetAndRaise(SelectedMembersProperty, ref _selectedMembers, value);
            SelectedMembers.CollectionChanged += SelectedMembersOnCollectionChanged;
            SelectedMembersSorted = SelectedMembers.OrderBy(member => member.Name).ToList();
        }
    }

    private void SelectedMembersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Reset:
                SelectedMembersSorted = SelectedMembers.OrderBy(member => member.Name).ToList();
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

    public void RemoveMember(object memberParam)
    {
        if (memberParam is not Member member)
        {
            return;
        }

        SelectedMembers.Remove(member);
    }
}