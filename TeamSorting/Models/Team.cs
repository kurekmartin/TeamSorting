using System.Collections.ObjectModel;
using Avalonia.Collections;

namespace TeamSorting.Models;

public class Team(string name)
{
    public string Name { get; set; } = name;
    public ObservableCollection<Member> Members { get; } = [];

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
}