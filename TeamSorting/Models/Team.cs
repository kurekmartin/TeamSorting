using System.Collections.ObjectModel;

namespace TeamSorting.Models;

public class Team(string name)
{
    public string Name { get; set; } = name;
    public ObservableCollection<Member> Members { get; init; } = [];
    public bool IsValid => IsValidCheck(Members);

    public Dictionary<DisciplineInfo, double> TotalScores
    {
        get
        {
            return Members.SelectMany(member => member.Records.Values)
                .GroupBy(record => record.DisciplineInfo)
                .ToDictionary(g => g.Key, g => g.Sum(record => record.DoubleValue));
        }
    }

    public double GetTotalValueByDiscipline(DisciplineInfo discipline)
    {
        var records = Members.Select(member => member.GetRecord(discipline));
        return records.Sum(record => record.DoubleValue);
    }

    public static bool IsValidCheck(IEnumerable<Member> members)
    {
        var memberList = members.ToList();
        var memberNames = memberList.Select(member => member.Name).ToList(); 
        var with = memberList.SelectMany(member => member.With).ToList();
        var notWith = memberList.SelectMany(member => member.NotWith);

        return memberNames.Intersect(with).SequenceEqual(with)
               && !memberNames.Intersect(notWith).Any();

    }
}