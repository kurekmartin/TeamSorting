using TeamSorting.Model.New;
using Team = TeamSorting.Model.Team;

namespace TeamSorting.ViewModel.New;

public class Data
{
    public readonly List<DisciplineInfo> Disciplines = [];
    public readonly List<Member> Members = [];
    public readonly List<DisciplineRecord> DisciplineRecords = [];
    public readonly List<Team> Teams = [];

    #region Discipline

    public bool AddDiscipline(DisciplineInfo discipline)
    {
        if (Disciplines.Any(i => i.Name == discipline.Name))
        {
            return false;
        }

        Disciplines.Add(discipline);
        return true;
    }

    public void RemoveDiscipline(DisciplineInfo discipline)
    {
        Disciplines.Remove(discipline);
        DisciplineRecords.RemoveAll(record => record.DisciplineInfo == discipline);
    }

    private (double min, double max) GetDisciplineRange(DisciplineInfo discipline)
    {
        var records = DisciplineRecords.Where(record =>
            record.DisciplineInfo == discipline);
        var values = records.Select(record => record.DoubleValue).ToList();
        return (values.Min(), values.Max());
    }

    #endregion

    #region Member

    public bool AddMember(Member member)
    {
        if (Members.Any(m => m.Name == member.Name))
        {
            return false;
        }

        Members.Add(member);
        return true;
    }

    public void RemoveMember(Member member)
    {
        Members.Remove(member);
        DisciplineRecords.RemoveAll(record => record.Member == member);
    }

    public object? GetMemberDisciplineValue(Member member, DisciplineInfo discipline)
    {
        var record = DisciplineRecords.FirstOrDefault(record =>
            record.Member == member
            && record.DisciplineInfo == discipline);
        return record?.Value;
    }

    private double? GetMemberDisciplineDoubleValue(Member member, DisciplineInfo discipline)
    {
        var record = DisciplineRecords.FirstOrDefault(record =>
            record.Member == member
            && record.DisciplineInfo == discipline);
        return record?.DoubleValue;
    }

    public double GetMemberDisciplineScore(Member member, DisciplineInfo discipline)
    {
        var range = GetDisciplineRange(discipline);
        int min = discipline.SortType == DisciplineSortType.Asc ? 0 : 100;
        int max = discipline.SortType == DisciplineSortType.Asc ? 100 : 0;
        double value = GetMemberDisciplineDoubleValue(member, discipline)
                       ?? (discipline.SortType == DisciplineSortType.Asc ? range.min : range.max);
        return (((value - range.min) / (range.max - range.min)) * (max - min)) + min;
    }

    #endregion

    #region DisciplineRecord

    public bool AddDisciplineRecord(DisciplineRecord record)
    {
        if (!Members.Contains(record.Member) ||
            !Disciplines.Contains(record.DisciplineInfo))
        {
            return false;
        }

        DisciplineRecords.Add(record);
        return true;
    }

    public void RemoveDisciplineRecord(DisciplineRecord record)
    {
        DisciplineRecords.Remove(record);
    }

    #endregion

    #region Team

    public bool AddTeam(Team team)
    {
        if (Teams.Any(t => t.Name == team.Name))
        {
            return false;
        }

        Teams.Add(team);
        return true;
    }

    public void RemoveTeam(Team team)
    {
        Teams.Remove(team);
    }

    public void AddMemberToTeam(Member member, Team team)
    {
        //TODO
    }

    public void RemoveMemberFromTeam(Member member, Team team)
    {
        //TODO
    }

    #endregion
}