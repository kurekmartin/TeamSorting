using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Serilog;

namespace TeamSorting.Model;

public class Team
{
    public Team(string name)
    {
        Name = name;
        Members.CollectionChanged += MembersOnCollectionChanged;
    }

    public string Name { get; set; }

    public Dictionary<DisciplineInfo, double> Score
    {
        get
        {
            var dict = new Dictionary<DisciplineInfo, double>();
            var disciplines = Members[0].Disciplines.Select(record => record.DisciplineInfo);
            foreach (var discipline in disciplines)
            {
                var totalScore = Members.Sum(member =>
                    member.Disciplines.First(record => record.DisciplineInfo == discipline).Score);
                dict.Add(discipline, totalScore);
            }

            return dict;
        }
    }

    public ObservableCollection<TeamMember> Members { get; } = [];

    private void MembersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Log.Debug("Team {name} members changed", Name);
    }
}