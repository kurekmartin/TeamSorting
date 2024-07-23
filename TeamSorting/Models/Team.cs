using System.Collections.ObjectModel;

namespace TeamSorting.Models;

public class Team(string name)
{
    public string Name { get; set; } = name;
    public ObservableCollection<Member> Members { get; } = [];

    public bool IsValid
    {
        get { return Members.All(member => member.IsValid); }
    }

    public void AddMembers(IEnumerable<Member> members)
    {
        foreach (var member in members)
        {
            AddMember(member);
        }
    }

    public void AddMember(Member member)
    {
        member.Team = this;
        Members.Add(member);
        ValidateTeamMembers();
    }

    private void ValidateTeamMembers()
    {
        foreach (var member in Members)
        {
            member.Validate();
        }
    }

    public void RemoveMember(Member member)
    {
        member.Team = null;
        Members.Remove(member);
    }

    public Dictionary<DisciplineInfo, double> TotalScores
    {
        get
        {
            return Members.SelectMany(member => member.Records.Values)
                .GroupBy(record => record.DisciplineInfo)
                .ToDictionary(g => g.Key, g => Math.Round(g.Sum(record => record.DoubleValue), 2));
        }
    }

    public double GetTotalValueByDiscipline(DisciplineInfo discipline)
    {
        var records = Members.Select(member => member.GetRecord(discipline));
        return records.Sum(record => record.DoubleValue);
    }

    public static bool ValidateMemberList(IEnumerable<Member> members)
    {
        var invalidMembers = GetInvalidMembers(members);
        return invalidMembers.invalidWith.Count == 0 && invalidMembers.invalidNotWith.Count == 0;
    }

    public static (List<string> invalidWith, List<string> invalidNotWith) GetInvalidMembers(IEnumerable<Member> members)
    {
        var memberList = members.ToList();
        var memberNames = memberList.Select(member => member.Name).ToList();
        var with = memberList.SelectMany(member => member.With.Keys).ToList();
        var notWith = memberList.SelectMany(member => member.NotWith.Keys);

        return (with.Except(memberNames).ToList(), memberNames.Intersect(notWith).ToList());
    }
}