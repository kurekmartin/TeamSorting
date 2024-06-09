using System.Collections.ObjectModel;

namespace TeamSorting.Model;

public class TeamMember(string name, int age)
{
    public string Name { get; set; } = name;
    public List<string> With { get; set; } = [];
    public List<string> NotWith { get; set; } = [];
    public int Age { get; set; } = age;
    public ObservableCollection<DisciplineRecord> Disciplines { get; } = [];
}