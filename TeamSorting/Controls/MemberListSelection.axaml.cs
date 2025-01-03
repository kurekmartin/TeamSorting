using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Serilog;
using TeamSorting.Models;
using ListBox = Avalonia.Controls.ListBox;

namespace TeamSorting.Controls;

public class MemberListSelection : TemplatedControl
{
    public static readonly DirectProperty<MemberListSelection, string> SearchTextProperty =
        AvaloniaProperty.RegisterDirect<MemberListSelection, string>(
            nameof(SearchText),
            o => o.SearchText,
            (o, v) => o.SearchText = v,
            string.Empty);

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
            new Member(string.Empty),
            BindingMode.OneTime);

    public static readonly DirectProperty<MemberListSelection, ObservableCollection<Member>> AllMembersProperty =
        AvaloniaProperty.RegisterDirect<MemberListSelection, ObservableCollection<Member>>(
            nameof(AllMembers),
            o => o.AllMembers,
            (o, v) => o.AllMembers = v,
            []);

    private ObservableCollection<Member> _allMembers = [];
    private Member _currentMember = null!;
    private ObservableCollection<Member> _selectedMembers = [];
    private string _searchText = string.Empty;
    private ListBox? _listBox;

    public ObservableCollection<FilterableMember> FilteredMembers { get; set; } = [];

    public ObservableCollection<Member> AllMembers
    {
        get => _allMembers;
        set => SetAndRaise(AllMembersProperty, ref _allMembers, value);
    }

    public Member CurrentMember
    {
        get => _currentMember;
        set => SetAndRaise(CurrentMemberProperty, ref _currentMember, value);
    }

    public ObservableCollection<Member> SelectedMembers
    {
        get => _selectedMembers;
        set => SetAndRaise(SelectedMembersProperty, ref _selectedMembers, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetAndRaise(SearchTextProperty, ref _searchText, value);
            UpdateFilteredMembers();
        }
    }


    private void InitializeFilteredMembers()
    {
        foreach (Member member in AllMembers)
        {
            if (FilteredMembers.FirstOrDefault(filterableMember => filterableMember.Member == member) is null)
            {
                FilteredMembers.Add(new FilterableMember(member));
            }
        }
    }

    private void UpdateSelectedMembers()
    {
        if (_listBox is null)
        {
            return;
        }

        _listBox.SelectedItems = FilteredMembers.Where(member => SelectedMembers.Contains(member.Member)).ToList();
    }

    private void UpdateFilteredMembers()
    {
        foreach (Member member in AllMembers)
        {
            FilterableMember? filterableMember =
                FilteredMembers.FirstOrDefault(filterableMember => filterableMember.Member == member);
            if (filterableMember is null)
            {
                continue;
            }

            filterableMember.IsVisible = member != CurrentMember &&
                                         member.Name.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        object? listboxObject = e.NameScope.Find("MemberSelectionListBox");
        if (listboxObject is not ListBox listBox)
        {
            return;
        }

        listBox.SelectionChanged += ListBoxOnSelectionChanged;
        _listBox = listBox;
        InitializeFilteredMembers();
        UpdateFilteredMembers();
        UpdateSelectedMembers();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        SearchText = string.Empty;
    }

    private void ListBoxOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        foreach (object item in e.AddedItems)
        {
            if (item is FilterableMember filterableMember && !SelectedMembers.Contains(filterableMember.Member))
            {
                SelectedMembers.Add(filterableMember.Member);
            }
        }

        foreach (object item in e.RemovedItems)
        {
            if (item is FilterableMember filterableMember)
            {
                SelectedMembers.Remove(filterableMember.Member);
            }
        }
    }
}