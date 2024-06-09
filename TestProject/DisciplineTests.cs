using FluentAssertions;
using TeamSorting.Model;

namespace TestProject;

public class DisciplineTests
{
    [Test]
    public void Discipline_MemberAdded_ValueRange_Number()
    {
        var disciplineInfoAsc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Asc, DataType = DisciplineDataType.Number };
        var disciplineInfoDesc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Desc, DataType = DisciplineDataType.Number };

        var member1 = new TeamMember(name: "Jmeno1", age: 10);
        var member2 = new TeamMember(name: "Jmeno2", age: 10);
        var member3 = new TeamMember(name: "Jmeno3", age: 10);
        var member4 = new TeamMember(name: "Jmeno4", age: 10);

        member1.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "10"));
        member2.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "5"));
        member3.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "3"));
        member4.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "50"));

        member1.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "10"));
        member2.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "5"));
        member3.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "3"));
        member4.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "50"));

        disciplineInfoAsc.TeamMembers.Add(member1);
        disciplineInfoAsc.TeamMembers.Add(member2);
        disciplineInfoAsc.TeamMembers.Add(member3);
        disciplineInfoAsc.TeamMembers.Add(member4);

        disciplineInfoDesc.TeamMembers.Add(member1);
        disciplineInfoDesc.TeamMembers.Add(member2);
        disciplineInfoDesc.TeamMembers.Add(member3);
        disciplineInfoDesc.TeamMembers.Add(member4);

        disciplineInfoAsc.MaxValue.Should().Be(50);
        disciplineInfoAsc.MinValue.Should().Be(3);
        disciplineInfoDesc.MaxValue.Should().Be(50);
        disciplineInfoDesc.MinValue.Should().Be(3);
    }

    [Test]
    public void Discipline_MemberRemoved_ValueRange_Number()
    {
        var disciplineInfoAsc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Asc, DataType = DisciplineDataType.Number };
        var disciplineInfoDesc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Desc, DataType = DisciplineDataType.Number };

        var member1 = new TeamMember(name: "Jmeno1", age: 10);
        var member2 = new TeamMember(name: "Jmeno2", age: 10);
        var member3 = new TeamMember(name: "Jmeno3", age: 10);
        var member4 = new TeamMember(name: "Jmeno4", age: 10);

        member1.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "10"));
        member2.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "5"));
        member3.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "3"));
        member4.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "50"));

        member1.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "10"));
        member2.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "5"));
        member3.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "3"));
        member4.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "50"));

        disciplineInfoAsc.TeamMembers.Add(member1);
        disciplineInfoAsc.TeamMembers.Add(member2);
        disciplineInfoAsc.TeamMembers.Add(member3);
        disciplineInfoAsc.TeamMembers.Add(member4);
        disciplineInfoAsc.TeamMembers.Remove(member4);
        disciplineInfoAsc.TeamMembers.Remove(member3);

        disciplineInfoDesc.TeamMembers.Add(member1);
        disciplineInfoDesc.TeamMembers.Add(member2);
        disciplineInfoDesc.TeamMembers.Add(member3);
        disciplineInfoDesc.TeamMembers.Add(member4);
        disciplineInfoDesc.TeamMembers.Remove(member4);
        disciplineInfoDesc.TeamMembers.Remove(member3);

        disciplineInfoAsc.MaxValue.Should().Be(10);
        disciplineInfoAsc.MinValue.Should().Be(5);
        disciplineInfoDesc.MaxValue.Should().Be(10);
        disciplineInfoDesc.MinValue.Should().Be(5);
    }

    [Test]
    public void Discipline_MemberAdded_ValueRange_Time()
    {
        var disciplineInfoAsc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Asc, DataType = DisciplineDataType.Time };
        var disciplineInfoDesc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Desc, DataType = DisciplineDataType.Time };

        var member1 = new TeamMember(name: "Jmeno1", age: 10);
        var member2 = new TeamMember(name: "Jmeno2", age: 10);
        var member3 = new TeamMember(name: "Jmeno3", age: 10);
        var member4 = new TeamMember(name: "Jmeno4", age: 10);

        member1.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "00:00:10"));
        member2.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "00:15:00"));
        member3.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "00:01:00"));
        member4.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "00:00:05"));

        member1.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "00:00:10"));
        member2.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "00:15:00"));
        member3.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "00:01:00"));
        member4.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "00:00:05"));

        disciplineInfoAsc.TeamMembers.Add(member1);
        disciplineInfoAsc.TeamMembers.Add(member2);
        disciplineInfoAsc.TeamMembers.Add(member3);
        disciplineInfoAsc.TeamMembers.Add(member4);

        disciplineInfoDesc.TeamMembers.Add(member1);
        disciplineInfoDesc.TeamMembers.Add(member2);
        disciplineInfoDesc.TeamMembers.Add(member3);
        disciplineInfoDesc.TeamMembers.Add(member4);

        disciplineInfoAsc.MaxValue.Should().Be(new TimeSpan(hours: 0, minutes: 15, seconds: 0).TotalSeconds);
        disciplineInfoAsc.MinValue.Should().Be(new TimeSpan(hours: 0, minutes: 0, seconds: 5).TotalSeconds);
        disciplineInfoDesc.MaxValue.Should()
            .Be(new TimeSpan(hours: 0, minutes: 15, seconds: 0).TotalSeconds);
        disciplineInfoDesc.MinValue.Should().Be(new TimeSpan(hours: 0, minutes: 0, seconds: 5).TotalSeconds);
    }

    [Test]
    public void Discipline_MemberRemoved_ValueRange_Time()
    {
        var disciplineInfoAsc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Asc, DataType = DisciplineDataType.Time };
        var disciplineInfoDesc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Desc, DataType = DisciplineDataType.Time };

        var member1 = new TeamMember(name: "Jmeno1", age: 10);
        var member2 = new TeamMember(name: "Jmeno2", age: 10);
        var member3 = new TeamMember(name: "Jmeno3", age: 10);
        var member4 = new TeamMember(name: "Jmeno4", age: 10);

        member1.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "00:00:10"));
        member2.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "00:15:00"));
        member3.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "00:01:00"));
        member4.Disciplines.Add(new DisciplineRecord(disciplineInfoAsc, "00:00:05"));

        member1.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "00:00:10"));
        member2.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "00:15:00"));
        member3.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "00:01:00"));
        member4.Disciplines.Add(new DisciplineRecord(disciplineInfoDesc, "00:00:05"));

        disciplineInfoAsc.TeamMembers.Add(member1);
        disciplineInfoAsc.TeamMembers.Add(member2);
        disciplineInfoAsc.TeamMembers.Add(member3);
        disciplineInfoAsc.TeamMembers.Add(member4);
        disciplineInfoAsc.TeamMembers.Remove(member4);
        disciplineInfoAsc.TeamMembers.Remove(member2);

        disciplineInfoDesc.TeamMembers.Add(member1);
        disciplineInfoDesc.TeamMembers.Add(member2);
        disciplineInfoDesc.TeamMembers.Add(member3);
        disciplineInfoDesc.TeamMembers.Add(member4);
        disciplineInfoDesc.TeamMembers.Remove(member4);
        disciplineInfoDesc.TeamMembers.Remove(member2);

        disciplineInfoAsc.MaxValue.Should().Be(new TimeSpan(hours: 0, minutes: 1, seconds: 0).TotalSeconds);
        disciplineInfoAsc.MinValue.Should().Be(new TimeSpan(hours: 0, minutes: 0, seconds: 10).TotalSeconds);
        disciplineInfoDesc.MaxValue.Should()
            .Be(new TimeSpan(hours: 0, minutes: 1, seconds: 0).TotalSeconds);
        disciplineInfoDesc.MinValue.Should()
            .Be(new TimeSpan(hours: 0, minutes: 0, seconds: 10).TotalSeconds);
    }

    [Test]
    public void DisciplineInfoAsc_Score_Number()
    {
        var disciplineInfoAsc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Asc, DataType = DisciplineDataType.Number };

        var member1 = new TeamMember(name: "Jmeno1", age: 10);
        var member2 = new TeamMember(name: "Jmeno2", age: 10);
        var member3 = new TeamMember(name: "Jmeno3", age: 10);

        var disciplineAscRecord1 = new DisciplineRecord(disciplineInfoAsc, "10");
        var disciplineAscRecord2 = new DisciplineRecord(disciplineInfoAsc, "0");
        var disciplineAscRecord3 = new DisciplineRecord(disciplineInfoAsc, "50");

        member1.Disciplines.Add(disciplineAscRecord1);
        member2.Disciplines.Add(disciplineAscRecord2);
        member3.Disciplines.Add(disciplineAscRecord3);

        disciplineInfoAsc.TeamMembers.Add(member1);
        disciplineInfoAsc.TeamMembers.Add(member2);
        disciplineInfoAsc.TeamMembers.Add(member3);

        disciplineAscRecord1.Score.Should().Be(20);
        disciplineAscRecord2.Score.Should().Be(0);
        disciplineAscRecord3.Score.Should().Be(100);
    }

    [Test]
    public void DisciplineInfoDesc_Score_Number()
    {
        var disciplineInfoDesc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Desc, DataType = DisciplineDataType.Number };

        var member1 = new TeamMember(name: "Jmeno1", age: 10);
        var member2 = new TeamMember(name: "Jmeno2", age: 10);
        var member3 = new TeamMember(name: "Jmeno3", age: 10);

        var disciplineDescRecord1 = new DisciplineRecord(disciplineInfoDesc, "10");
        var disciplineDescRecord2 = new DisciplineRecord(disciplineInfoDesc, "0");
        var disciplineDescRecord3 = new DisciplineRecord(disciplineInfoDesc, "50");

        member1.Disciplines.Add(disciplineDescRecord1);
        member2.Disciplines.Add(disciplineDescRecord2);
        member3.Disciplines.Add(disciplineDescRecord3);

        disciplineInfoDesc.TeamMembers.Add(member1);
        disciplineInfoDesc.TeamMembers.Add(member2);
        disciplineInfoDesc.TeamMembers.Add(member3);

        disciplineDescRecord1.Score.Should().Be(80);
        disciplineDescRecord2.Score.Should().Be(100);
        disciplineDescRecord3.Score.Should().Be(0);
    }

    [Test]
    public void DisciplineInfoAsc_Score_Time()
    {
        var disciplineInfoAsc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Asc, DataType = DisciplineDataType.Time };

        var member1 = new TeamMember(name: "Jmeno1", age: 10);
        var member2 = new TeamMember(name: "Jmeno2", age: 10);
        var member3 = new TeamMember(name: "Jmeno3", age: 10);

        var disciplineAscRecord1 = new DisciplineRecord(disciplineInfoAsc, "00:01:00");
        var disciplineAscRecord2 = new DisciplineRecord(disciplineInfoAsc, "00:00:00");
        var disciplineAscRecord3 = new DisciplineRecord(disciplineInfoAsc, "00:05:00");

        member1.Disciplines.Add(disciplineAscRecord1);
        member2.Disciplines.Add(disciplineAscRecord2);
        member3.Disciplines.Add(disciplineAscRecord3);

        disciplineInfoAsc.TeamMembers.Add(member1);
        disciplineInfoAsc.TeamMembers.Add(member2);
        disciplineInfoAsc.TeamMembers.Add(member3);

        disciplineAscRecord1.Score.Should().Be(20);
        disciplineAscRecord2.Score.Should().Be(0);
        disciplineAscRecord3.Score.Should().Be(100);
    }

    [Test]
    public void DisciplineInfoDesc_Score_Time()
    {
        var disciplineInfoDesc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Desc, DataType = DisciplineDataType.Time };

        var member1 = new TeamMember(name: "Jmeno1", age: 10);
        var member2 = new TeamMember(name: "Jmeno2", age: 10);
        var member3 = new TeamMember(name: "Jmeno3", age: 10);

        var disciplineDescRecord1 = new DisciplineRecord(disciplineInfoDesc, "00:01:00");
        var disciplineDescRecord2 = new DisciplineRecord(disciplineInfoDesc, "00:00:00");
        var disciplineDescRecord3 = new DisciplineRecord(disciplineInfoDesc, "00:05:00");

        member1.Disciplines.Add(disciplineDescRecord1);
        member2.Disciplines.Add(disciplineDescRecord2);
        member3.Disciplines.Add(disciplineDescRecord3);

        disciplineInfoDesc.TeamMembers.Add(member1);
        disciplineInfoDesc.TeamMembers.Add(member2);
        disciplineInfoDesc.TeamMembers.Add(member3);

        disciplineDescRecord1.Score.Should().Be(80);
        disciplineDescRecord2.Score.Should().Be(100);
        disciplineDescRecord3.Score.Should().Be(0);
    }

    [Test]
    public void DisciplineInfoAsc_Score_SortTypeChanged()
    {
        var disciplineInfoAsc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Asc, DataType = DisciplineDataType.Number };

        var member1 = new TeamMember(name: "Jmeno1", age: 10);
        var member2 = new TeamMember(name: "Jmeno2", age: 10);
        var member3 = new TeamMember(name: "Jmeno3", age: 10);

        var disciplineAscRecord1 = new DisciplineRecord(disciplineInfoAsc, "10");
        var disciplineAscRecord2 = new DisciplineRecord(disciplineInfoAsc, "0");
        var disciplineAscRecord3 = new DisciplineRecord(disciplineInfoAsc, "50");

        member1.Disciplines.Add(disciplineAscRecord1);
        member2.Disciplines.Add(disciplineAscRecord2);
        member3.Disciplines.Add(disciplineAscRecord3);

        disciplineInfoAsc.TeamMembers.Add(member1);
        disciplineInfoAsc.TeamMembers.Add(member2);
        disciplineInfoAsc.TeamMembers.Add(member3);

        disciplineInfoAsc.SortType = DisciplineSortType.Desc;

        disciplineAscRecord1.Score.Should().Be(80);
        disciplineAscRecord2.Score.Should().Be(100);
        disciplineAscRecord3.Score.Should().Be(0);
    }

    [Test]
    public void DisciplineInfoDesc_Score_SortTypeChanged()
    {
        var disciplineInfoDesc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Desc, DataType = DisciplineDataType.Number };

        var member1 = new TeamMember(name: "Jmeno1", age: 10);
        var member2 = new TeamMember(name: "Jmeno2", age: 10);
        var member3 = new TeamMember(name: "Jmeno3", age: 10);

        var disciplineDescRecord1 = new DisciplineRecord(disciplineInfoDesc, "10");
        var disciplineDescRecord2 = new DisciplineRecord(disciplineInfoDesc, "0");
        var disciplineDescRecord3 = new DisciplineRecord(disciplineInfoDesc, "50");

        member1.Disciplines.Add(disciplineDescRecord1);
        member2.Disciplines.Add(disciplineDescRecord2);
        member3.Disciplines.Add(disciplineDescRecord3);

        disciplineInfoDesc.TeamMembers.Add(member1);
        disciplineInfoDesc.TeamMembers.Add(member2);
        disciplineInfoDesc.TeamMembers.Add(member3);

        disciplineInfoDesc.SortType = DisciplineSortType.Asc;

        disciplineDescRecord1.Score.Should().Be(20);
        disciplineDescRecord2.Score.Should().Be(0);
        disciplineDescRecord3.Score.Should().Be(100);
    }
}