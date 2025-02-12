﻿using FluentAssertions;
using TeamSorting.Enums;
using TeamSorting.Models;
using TeamSorting.ViewModels;

namespace TestProject;

public class DisciplineRecordTests
{
    [Test]
    public void RemoveDiscipline_RemoveRecords()
    {
        var data = new Data();
        var discipline1 = new DisciplineInfo("Discipline1") { DataType = DisciplineDataType.Number };
        var discipline2 = new DisciplineInfo("Discipline2") { DataType = DisciplineDataType.Number };
        data.AddDiscipline(discipline1);
        data.AddDiscipline(discipline2);

        var member1 = new Member("Name1");
        var member2 = new Member("Name2");
        data.AddMember(member1);
        data.AddMember(member2);

        data.AddDisciplineRecord(member1, discipline1, "1");
        data.AddDisciplineRecord(member1, discipline2, "2");
        data.AddDisciplineRecord(member2, discipline1, "1");
        data.AddDisciplineRecord(member2, discipline2, "2");

        data.GetAllRecords().Count().Should().Be(4);

        data.RemoveDiscipline(discipline2);
        data.GetAllRecords().Count().Should().Be(2);
        data.GetDisciplineRecordsByDiscipline(discipline1).Count().Should().Be(2);
        data.GetDisciplineRecordsByDiscipline(discipline2).Count().Should().Be(0);
    }
    
    [Test]
    public void RemoveMember_RemoveRecords()
    {
        var data = new Data();
        var discipline1 = new DisciplineInfo("Discipline1") { DataType = DisciplineDataType.Number };
        var discipline2 = new DisciplineInfo("Discipline2") { DataType = DisciplineDataType.Number };
        data.AddDiscipline(discipline1);
        data.AddDiscipline(discipline2);

        var member1 = new Member("Name1");
        var member2 = new Member("Name2");
        data.AddMember(member1);
        data.AddMember(member2);

        data.AddDisciplineRecord(member1, discipline1, "1");
        data.AddDisciplineRecord(member1, discipline2, "2");
        data.AddDisciplineRecord(member2, discipline1, "1");
        data.AddDisciplineRecord(member2, discipline2, "2");

        data.GetAllRecords().Count().Should().Be(4);

        data.RemoveMember(member2);
        data.GetAllRecords().Count().Should().Be(2);
    }
}