﻿namespace TeamSorting.Models;

public class Team(string name)
{
    public string Name { get; set; } = name;
    public List<Member> Members { get; } = [];

    public double GetTotalValueByDiscipline(DisciplineInfo discipline)
    {
        var records = Members.Select(member => member.GetRecord(discipline));
        return records.Sum(record => record.DoubleValue);
    }
}