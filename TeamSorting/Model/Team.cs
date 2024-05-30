using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Serilog;

namespace TeamSorting.Model;

public class Team
{
    public Team()
    {
        Members.CollectionChanged += MembersOnCollectionChanged;
    }

    public string Name { get; set; } = string.Empty;
    public ObservableCollection<TeamMember> Members { get; } = [];

    private void MembersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Log.Debug("Team {name} members changed", Name);
    }
}