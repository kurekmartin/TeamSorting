using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeamSorting.Models;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using ListBox = Avalonia.Controls.ListBox;

namespace TeamSorting.Controls;

public class MemberListSelection : TemplatedControl
{
    private readonly ILogger? _logger = Ioc.Default.GetService<ILogger<MemberListSelection>>();

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
            null!,
            BindingMode.OneTime);

    public static readonly DirectProperty<MemberListSelection, List<Member>> AllMembersProperty =
        AvaloniaProperty.RegisterDirect<MemberListSelection, List<Member>>(
            nameof(AllMembers),
            o => o.AllMembers,
            (o, v) => o.AllMembers = v,
            []);

    private List<Member> _allMembers = [];
    private Member _currentMember = null!;
    private ObservableCollection<Member> _selectedMembers = [];
    private string _searchText = string.Empty;
    private ListBox? _listBox;

    public ObservableCollection<FilterableMember> FilteredMembers { get; set; } = [];

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
        foreach (FilterableMember filterableMember in FilteredMembers.Where(filterableMember =>
                     AllMembers.FirstOrDefault(member => member == filterableMember.Member) is null).ToList())
        {
            FilteredMembers.Remove(filterableMember);
            _logger?.LogDebug("FilteredMembers removed member {name}", filterableMember.Member.Name);
        }

        foreach (Member member in AllMembers)
        {
            if (FilteredMembers.FirstOrDefault(filterableMember => filterableMember.Member == member) is not null)
            {
                continue;
            }
            
            var newMember = new FilterableMember(member);

            // Find the correct insertion point
            var insertIndex = 0;
            while (insertIndex < FilteredMembers.Count &&
                   string.Compare(FilteredMembers[insertIndex].Member.Name, member.Name, StringComparison.OrdinalIgnoreCase) < 0)
            {
                insertIndex++;
            }

            FilteredMembers.Insert(insertIndex, newMember);

            _logger?.LogDebug("FilteredMembers added member {name}", member.Name);
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
        _logger?.LogDebug("ApplyTemplate");
        object? listboxObject = e.NameScope.Find("MemberSelectionListBox");
        if (listboxObject is not ListBox listBox)
        {
            return;
        }

        listBox.SelectionChanged += ListBoxOnSelectionChanged;
        _listBox = listBox;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        _logger?.LogDebug("Loaded");
        InitializeFilteredMembers();
        UpdateFilteredMembers();
        UpdateSelectedMembers();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _logger?.LogDebug("Unloaded");
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