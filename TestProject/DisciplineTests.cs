using FluentAssertions;
using TeamSorting.Models;
using TeamSorting.ViewModels;

namespace TestProject;

public class DisciplineTests
{
    [Test]
    public void AddDiscipline()
    {
        var data = new Data();
        var discipline1 = new DisciplineInfo("Discipline1");
        var discipline2 = new DisciplineInfo("Discipline2");

        data.AddDiscipline(discipline1);
        data.AddDiscipline(discipline2);

        data.Disciplines.Count.Should().Be(2);
        data.Disciplines.Should().Contain(discipline1);
        data.Disciplines.Should().Contain(discipline2);
    }

    [Test]
    public void AddDuplicateDiscipline()
    {
        var data = new Data();
        var discipline1 = new DisciplineInfo("Discipline1");
        var discipline2 = new DisciplineInfo("Discipline1");

        data.AddDiscipline(discipline1);
        data.AddDiscipline(discipline2);

        data.Disciplines.Count.Should().Be(1);
        data.Disciplines.Should().Contain(discipline1);
    }

    [Test]
    public void RemoveDiscipline()
    {
        var data = new Data();
        var discipline1 = new DisciplineInfo("Discipline1");
        var discipline2 = new DisciplineInfo("Discipline2");

        data.AddDiscipline(discipline1);
        data.AddDiscipline(discipline2);

        data.Disciplines.Count.Should().Be(2);
        data.Disciplines.Should().Contain(discipline1);
        data.Disciplines.Should().Contain(discipline2);

        data.RemoveDiscipline(discipline2);

        data.Disciplines.Count.Should().Be(1);
        data.Disciplines.Should().Contain(discipline1);
    }

    [Test]
    public void Discipline_MemberAdded_ValueRange_Number()
    {
        var data = new Data();
        var disciplineInfoAsc = new DisciplineInfo("Discipline1")
            { SortOrder = SortOrder.Asc, DataType = DisciplineDataType.Number };
        var disciplineInfoDesc = new DisciplineInfo("Discipline2")
            { SortOrder = SortOrder.Desc, DataType = DisciplineDataType.Number };
        data.AddDiscipline(disciplineInfoAsc);
        data.AddDiscipline(disciplineInfoDesc);

        var member1 = new Member(name: "Jmeno1");
        var member2 = new Member(name: "Jmeno2");
        var member3 = new Member(name: "Jmeno3");
        var member4 = new Member(name: "Jmeno4");
        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);
        data.AddMember(member4);

        data.AddDisciplineRecord(member1, disciplineInfoAsc, "10");
        data.AddDisciplineRecord(member2, disciplineInfoAsc, "5");
        data.AddDisciplineRecord(member3, disciplineInfoAsc, "3");
        data.AddDisciplineRecord(member4, disciplineInfoAsc, "50");

        data.AddDisciplineRecord(member1, disciplineInfoDesc, "10");
        data.AddDisciplineRecord(member2, disciplineInfoDesc, "5");
        data.AddDisciplineRecord(member3, disciplineInfoDesc, "3");
        data.AddDisciplineRecord(member4, disciplineInfoDesc, "50");

        var disciplineAscRange = data.GetDisciplineRange(disciplineInfoAsc);
        disciplineAscRange.max.Should().Be(50);
        disciplineAscRange.min.Should().Be(3);
        var disciplineDescRange = data.GetDisciplineRange(disciplineInfoDesc);
        disciplineDescRange.max.Should().Be(50);
        disciplineDescRange.min.Should().Be(3);
    }

    [Test]
    public void Discipline_MemberRemoved_ValueRange_Number()
    {
        var data = new Data();
        var disciplineInfoAsc = new DisciplineInfo("Discipline1")
            { SortOrder = SortOrder.Asc, DataType = DisciplineDataType.Number };
        var disciplineInfoDesc = new DisciplineInfo("Discipline2")
            { SortOrder = SortOrder.Desc, DataType = DisciplineDataType.Number };
        data.AddDiscipline(disciplineInfoAsc);
        data.AddDiscipline(disciplineInfoDesc);

        var member1 = new Member(name: "Jmeno1");
        var member2 = new Member(name: "Jmeno2");
        var member3 = new Member(name: "Jmeno3");
        var member4 = new Member(name: "Jmeno4");
        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);
        data.AddMember(member4);

        data.AddDisciplineRecord(member1, disciplineInfoAsc, "10");
        data.AddDisciplineRecord(member2, disciplineInfoAsc, "5");
        data.AddDisciplineRecord(member3, disciplineInfoAsc, "3");
        data.AddDisciplineRecord(member4, disciplineInfoAsc, "50");

        data.AddDisciplineRecord(member1, disciplineInfoDesc, "10");
        data.AddDisciplineRecord(member2, disciplineInfoDesc, "5");
        data.AddDisciplineRecord(member3, disciplineInfoDesc, "3");
        data.AddDisciplineRecord(member4, disciplineInfoDesc, "50");

        data.RemoveMember(member4);
        data.RemoveMember(member3);

        var disciplineAscRange = data.GetDisciplineRange(disciplineInfoAsc);
        disciplineAscRange.max.Should().Be(10);
        disciplineAscRange.min.Should().Be(5);
        var disciplineDescRange = data.GetDisciplineRange(disciplineInfoDesc);
        disciplineDescRange.max.Should().Be(10);
        disciplineDescRange.min.Should().Be(5);
    }

    [Test]
    public void Discipline_MemberAdded_ValueRange_Time()
    {
        var data = new Data();
        var disciplineInfoAsc = new DisciplineInfo("Discipline1")
            { SortOrder = SortOrder.Asc, DataType = DisciplineDataType.Time };
        var disciplineInfoDesc = new DisciplineInfo("Discipline2")
            { SortOrder = SortOrder.Desc, DataType = DisciplineDataType.Time };
        data.AddDiscipline(disciplineInfoAsc);
        data.AddDiscipline(disciplineInfoDesc);

        var member1 = new Member(name: "Jmeno1");
        var member2 = new Member(name: "Jmeno2");
        var member3 = new Member(name: "Jmeno3");
        var member4 = new Member(name: "Jmeno4");
        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);
        data.AddMember(member4);

        data.AddDisciplineRecord(member1, disciplineInfoAsc, "00:00:10");
        data.AddDisciplineRecord(member2, disciplineInfoAsc, "00:15:00");
        data.AddDisciplineRecord(member3, disciplineInfoAsc, "00:01:00");
        data.AddDisciplineRecord(member4, disciplineInfoAsc, "00:00:05");

        data.AddDisciplineRecord(member1, disciplineInfoDesc, "00:00:10");
        data.AddDisciplineRecord(member2, disciplineInfoDesc, "00:15:00");
        data.AddDisciplineRecord(member3, disciplineInfoDesc, "00:01:00");
        data.AddDisciplineRecord(member4, disciplineInfoDesc, "00:00:05");

        var disciplineAscRange = data.GetDisciplineRange(disciplineInfoAsc);
        disciplineAscRange.max.Should().Be(new TimeSpan(hours: 0, minutes: 15, seconds: 0).TotalSeconds);
        disciplineAscRange.min.Should().Be(new TimeSpan(hours: 0, minutes: 0, seconds: 5).TotalSeconds);
        var disciplineDescRange = data.GetDisciplineRange(disciplineInfoDesc);
        disciplineDescRange.max.Should().Be(new TimeSpan(hours: 0, minutes: 15, seconds: 0).TotalSeconds);
        disciplineDescRange.min.Should().Be(new TimeSpan(hours: 0, minutes: 0, seconds: 5).TotalSeconds);
    }

    [Test]
    public void Discipline_MemberRemoved_ValueRange_Time()
    {
        var data = new Data();
        var disciplineInfoAsc = new DisciplineInfo("Discipline1")
            { SortOrder = SortOrder.Asc, DataType = DisciplineDataType.Time };
        var disciplineInfoDesc = new DisciplineInfo("Discipline2")
            { SortOrder = SortOrder.Desc, DataType = DisciplineDataType.Time };
        data.AddDiscipline(disciplineInfoAsc);
        data.AddDiscipline(disciplineInfoDesc);

        var member1 = new Member(name: "Jmeno1");
        var member2 = new Member(name: "Jmeno2");
        var member3 = new Member(name: "Jmeno3");
        var member4 = new Member(name: "Jmeno4");
        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);
        data.AddMember(member4);

        data.AddDisciplineRecord(member1, disciplineInfoAsc, "00:00:10");
        data.AddDisciplineRecord(member2, disciplineInfoAsc, "00:15:00");
        data.AddDisciplineRecord(member3, disciplineInfoAsc, "00:01:00");
        data.AddDisciplineRecord(member4, disciplineInfoAsc, "00:00:05");

        data.AddDisciplineRecord(member1, disciplineInfoDesc, "00:00:10");
        data.AddDisciplineRecord(member2, disciplineInfoDesc, "00:15:00");
        data.AddDisciplineRecord(member3, disciplineInfoDesc, "00:01:00");
        data.AddDisciplineRecord(member4, disciplineInfoDesc, "00:00:05");

        data.RemoveMember(member4);
        data.RemoveMember(member2);

        var disciplineAscRange = data.GetDisciplineRange(disciplineInfoAsc);
        disciplineAscRange.max.Should().Be(new TimeSpan(hours: 0, minutes: 1, seconds: 0).TotalSeconds);
        disciplineAscRange.min.Should().Be(new TimeSpan(hours: 0, minutes: 0, seconds: 10).TotalSeconds);
        var disciplineDescRange = data.GetDisciplineRange(disciplineInfoDesc);
        disciplineDescRange.max.Should().Be(new TimeSpan(hours: 0, minutes: 1, seconds: 0).TotalSeconds);
        disciplineDescRange.min.Should().Be(new TimeSpan(hours: 0, minutes: 0, seconds: 10).TotalSeconds);
    }

    [Test]
    public void DisciplineInfoAsc_Score_Number()
    {
        var data = new Data();
        var disciplineInfoAsc = new DisciplineInfo("Discipline")
            { SortOrder = SortOrder.Asc, DataType = DisciplineDataType.Number };
        data.AddDiscipline(disciplineInfoAsc);

        var member1 = new Member(name: "Jmeno1");
        var member2 = new Member(name: "Jmeno2");
        var member3 = new Member(name: "Jmeno3");
        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);

        data.AddDisciplineRecord(member1, disciplineInfoAsc, "10");
        data.AddDisciplineRecord(member2, disciplineInfoAsc, "0");
        data.AddDisciplineRecord(member3, disciplineInfoAsc, "50");

        data.GetMemberDisciplineScore(member1, disciplineInfoAsc).Should().Be(20);
        data.GetMemberDisciplineScore(member2, disciplineInfoAsc).Should().Be(0);
        data.GetMemberDisciplineScore(member3, disciplineInfoAsc).Should().Be(100);
    }

    [Test]
    public void DisciplineInfoDesc_Score_Number()
    {
        var data = new Data();
        var disciplineInfoDesc = new DisciplineInfo("Discipline")
            { SortOrder = SortOrder.Desc, DataType = DisciplineDataType.Number };
        data.AddDiscipline(disciplineInfoDesc);

        var member1 = new Member(name: "Jmeno1");
        var member2 = new Member(name: "Jmeno2");
        var member3 = new Member(name: "Jmeno3");
        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);

        data.AddDisciplineRecord(member1, disciplineInfoDesc, "10");
        data.AddDisciplineRecord(member2, disciplineInfoDesc, "0");
        data.AddDisciplineRecord(member3, disciplineInfoDesc, "50");

        data.GetMemberDisciplineScore(member1, disciplineInfoDesc).Should().Be(80);
        data.GetMemberDisciplineScore(member2, disciplineInfoDesc).Should().Be(100);
        data.GetMemberDisciplineScore(member3, disciplineInfoDesc).Should().Be(0);
    }

    [Test]
    public void DisciplineInfoAsc_Score_Time()
    {
        var data = new Data();
        var disciplineInfoAsc = new DisciplineInfo("Discipline")
            { SortOrder = SortOrder.Asc, DataType = DisciplineDataType.Time };
        data.AddDiscipline(disciplineInfoAsc);

        var member1 = new Member(name: "Jmeno1");
        var member2 = new Member(name: "Jmeno2");
        var member3 = new Member(name: "Jmeno3");
        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);

        data.AddDisciplineRecord(member1, disciplineInfoAsc, "00:01:00");
        data.AddDisciplineRecord(member2, disciplineInfoAsc, "00:00:00");
        data.AddDisciplineRecord(member3, disciplineInfoAsc, "00:05:00");

        data.GetMemberDisciplineScore(member1, disciplineInfoAsc).Should().Be(20);
        data.GetMemberDisciplineScore(member2, disciplineInfoAsc).Should().Be(0);
        data.GetMemberDisciplineScore(member3, disciplineInfoAsc).Should().Be(100);
    }

    [Test]
    public void DisciplineInfoDesc_Score_Time()
    {
        var data = new Data();
        var disciplineInfoDesc = new DisciplineInfo("Discipline")
            { SortOrder = SortOrder.Desc, DataType = DisciplineDataType.Time };
        data.AddDiscipline(disciplineInfoDesc);

        var member1 = new Member(name: "Jmeno1");
        var member2 = new Member(name: "Jmeno2");
        var member3 = new Member(name: "Jmeno3");
        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);

        data.AddDisciplineRecord(member1, disciplineInfoDesc, "00:01:00");
        data.AddDisciplineRecord(member2, disciplineInfoDesc, "00:00:00");
        data.AddDisciplineRecord(member3, disciplineInfoDesc, "00:05:00");

        data.GetMemberDisciplineScore(member1, disciplineInfoDesc).Should().Be(80);
        data.GetMemberDisciplineScore(member2, disciplineInfoDesc).Should().Be(100);
        data.GetMemberDisciplineScore(member3, disciplineInfoDesc).Should().Be(0);
    }

    [Test]
    public void DisciplineInfoAsc_Score_SortTypeChanged()
    {
        var data = new Data();
        var disciplineInfoAsc = new DisciplineInfo("Discipline")
            { SortOrder = SortOrder.Asc, DataType = DisciplineDataType.Number };
        data.AddDiscipline(disciplineInfoAsc);

        var member1 = new Member(name: "Jmeno1");
        var member2 = new Member(name: "Jmeno2");
        var member3 = new Member(name: "Jmeno3");
        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);

        data.AddDisciplineRecord(member1, disciplineInfoAsc, "10");
        data.AddDisciplineRecord(member2, disciplineInfoAsc, "0");
        data.AddDisciplineRecord(member3, disciplineInfoAsc, "50");

        disciplineInfoAsc.SortOrder = SortOrder.Desc;

        data.GetMemberDisciplineScore(member1, disciplineInfoAsc).Should().Be(80);
        data.GetMemberDisciplineScore(member2, disciplineInfoAsc).Should().Be(100);
        data.GetMemberDisciplineScore(member3, disciplineInfoAsc).Should().Be(0);
    }

    [Test]
    public void DisciplineInfoDesc_Score_SortTypeChanged()
    {
        var data = new Data();
        var disciplineInfoDesc = new DisciplineInfo("Discipline")
            { SortOrder = SortOrder.Desc, DataType = DisciplineDataType.Number };
        data.AddDiscipline(disciplineInfoDesc);

        var member1 = new Member(name: "Jmeno1");
        var member2 = new Member(name: "Jmeno2");
        var member3 = new Member(name: "Jmeno3");
        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);

        data.AddDisciplineRecord(member1, disciplineInfoDesc, "10");
        data.AddDisciplineRecord(member2, disciplineInfoDesc, "0");
        data.AddDisciplineRecord(member3, disciplineInfoDesc, "50");

        disciplineInfoDesc.SortOrder = SortOrder.Asc;

        data.GetMemberDisciplineScore(member1, disciplineInfoDesc).Should().Be(20);
        data.GetMemberDisciplineScore(member2, disciplineInfoDesc).Should().Be(0);
        data.GetMemberDisciplineScore(member3, disciplineInfoDesc).Should().Be(100);
    }

    [Test]
    public void DisciplineRecordChanged_Score_Number()
    {
        var data = new Data();
        var disciplineInfo = new DisciplineInfo("Discipline")
            { SortOrder = SortOrder.Asc, DataType = DisciplineDataType.Number };
        data.AddDiscipline(disciplineInfo);

        var member1 = new Member(name: "Jmeno1");
        var member2 = new Member(name: "Jmeno2");
        var member3 = new Member(name: "Jmeno3");
        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);

        data.AddDisciplineRecord(member1, disciplineInfo, "10");
        data.AddDisciplineRecord(member2, disciplineInfo, "0");
        data.AddDisciplineRecord(member3, disciplineInfo, "100");

        data.GetMemberDisciplineScore(member1, disciplineInfo).Should().Be(10);
        data.GetMemberDisciplineScore(member2, disciplineInfo).Should().Be(0);
        data.GetMemberDisciplineScore(member3, disciplineInfo).Should().Be(100);

        var record = member3.GetRecord(disciplineInfo);
        Assert.That(record, Is.Not.Null);
        record.RawValue = "50";

        data.GetMemberDisciplineScore(member1, disciplineInfo).Should().Be(20);
        data.GetMemberDisciplineScore(member2, disciplineInfo).Should().Be(0);
        data.GetMemberDisciplineScore(member3, disciplineInfo).Should().Be(100);
    }
}