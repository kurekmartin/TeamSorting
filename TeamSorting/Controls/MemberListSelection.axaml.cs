using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Primitives;
using TeamSorting.Models;

namespace TeamSorting.Controls;

public class MemberListSelection : TemplatedControl
{
    public static readonly DirectProperty<MemberListSelection, ObservableCollection<Member>> SelectedMembersProperty =
        AvaloniaProperty.RegisterDirect<MemberListSelection, ObservableCollection<Member>>(
            nameof(SelectedMembers),
            o => o.SelectedMembers,
            (o, v) => o.SelectedMembers = v,
            []);

    public static readonly DirectProperty<MemberListSelection, Member> CurrentMemberProperty =
        AvaloniaProperty.RegisterDirect<MemberListSelection, Member>(
            nameof(CurrentMember),
            o => o.CurrentMember,
            (o, v) => o.CurrentMember = v,
            new Member(string.Empty));

    public static readonly DirectProperty<MemberListSelection, List<Member>> AllMembersProperty =
        AvaloniaProperty.RegisterDirect<MemberListSelection, List<Member>>(
            nameof(AllMembers),
            o => o.AllMembers,
            (o, v) => o.AllMembers = v,
            []);

    private List<Member> _allMembers = [];
    private Member _currentMember = null!;
    private ObservableCollection<Member> _selectedMembers = [];

    public List<Member> AllMembers
    {
        get => _allMembers;
        set => SetAndRaise(AllMembersProperty, ref _allMembers, value);
    }

    public Member CurrentMember
    {
        get => _currentMember;
        set => SetAndRaise(CurrentMemberProperty, ref _currentMember, value);
    }

    public IEnumerable<Member> Members => AllMembers.Except([CurrentMember]);

    public ObservableCollection<Member> SelectedMembers
    {
        get => _selectedMembers;
        set => SetAndRaise(SelectedMembersProperty, ref _selectedMembers, value);
    }
}