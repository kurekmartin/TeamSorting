using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using DynamicData;
using DynamicData.Binding;
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
    private readonly SourceCache<Member, string> _memberCache = new(x => x.Name);
    private ReadOnlyObservableCollection<Member> _filteredMembers = null!;
    public ReadOnlyObservableCollection<Member> FilteredMembers => _filteredMembers;

    public ObservableCollection<Member> AllMembers
    {
        get => _allMembers;
        set
        {
            SetAndRaise(AllMembersProperty, ref _allMembers, value);
            InitializeFilter();
        }
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

    public string SearchText
    {
        get => _searchText;
        set => SetAndRaise(SearchTextProperty, ref _searchText, value);
    }

    private static Func<Member, bool> CreateFilter(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return _ => true;
        }

        return member => member.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase);
    }

    private void InitializeFilter()
    {
        IObservable<Func<Member, bool>> filter = this.WhenValueChanged(@this => @this.SearchText)
            .Select(CreateFilter);

        foreach (Member member in AllMembers)
        {
            _memberCache.AddOrUpdate(member);
        }

        _memberCache.Connect()
            .RefCount()
            .Filter(filter)
            .SortBy(member => member.Name)
            .Bind(out _filteredMembers)
            .Subscribe();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var listboxObject = e.NameScope.Find("MemberSelectionListBox");
        if (listboxObject is ListBox listBox)
        {
            listBox.SelectionChanged += ListBoxOnSelectionChanged;
        }

        base.OnApplyTemplate(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        SearchText = string.Empty;
    }

    private void ListBoxOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        throw new NotImplementedException();
    }
}