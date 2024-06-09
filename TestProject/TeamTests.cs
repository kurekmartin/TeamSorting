using FluentAssertions;
using TeamSorting.Model;
using TeamSorting.ViewModel;

namespace TestProject;

public class TeamTests
{
    [Test]
    public void Team_TotalScore()
    {
        var disciplineInfo1 = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Asc, DataType = DisciplineDataType.Number };
        var disciplineInfo2 = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Desc, DataType = DisciplineDataType.Number };

        var member1 = new TeamMember(name: "Jmeno1", age: 10);
        var member2 = new TeamMember(name: "Jmeno2", age: 10);
        var member3 = new TeamMember(name: "Jmeno3", age: 10);
        var member4 = new TeamMember(name: "Jmeno4", age: 10);
        var member5 = new TeamMember(name: "Jmeno5", age: 10);
        var member6 = new TeamMember(name: "Jmeno6", age: 10);

        member1.Disciplines.Add(new DisciplineRecord(disciplineInfo1, "0")); //score 0
        member2.Disciplines.Add(new DisciplineRecord(disciplineInfo1, "10")); //score 20
        member3.Disciplines.Add(new DisciplineRecord(disciplineInfo1, "50")); //score 100
        member4.Disciplines.Add(new DisciplineRecord(disciplineInfo1, "0")); //score 0
        member5.Disciplines.Add(new DisciplineRecord(disciplineInfo1, "20")); //score 40
        member6.Disciplines.Add(new DisciplineRecord(disciplineInfo1, "50")); //score 100

        member1.Disciplines.Add(new DisciplineRecord(disciplineInfo2, "0")); //score 100
        member2.Disciplines.Add(new DisciplineRecord(disciplineInfo2, "10")); //score 80
        member3.Disciplines.Add(new DisciplineRecord(disciplineInfo2, "50")); //score 0
        member4.Disciplines.Add(new DisciplineRecord(disciplineInfo2, "0")); //score 100
        member5.Disciplines.Add(new DisciplineRecord(disciplineInfo2, "20")); //score 60
        member6.Disciplines.Add(new DisciplineRecord(disciplineInfo2, "50")); //score 0

        disciplineInfo1.TeamMembers.Add(member1);
        disciplineInfo1.TeamMembers.Add(member2);
        disciplineInfo1.TeamMembers.Add(member3);

        disciplineInfo2.TeamMembers.Add(member4);
        disciplineInfo2.TeamMembers.Add(member5);
        disciplineInfo2.TeamMembers.Add(member6);

        var team1 = new Team(name: "Team1")
        {
            Members = { member1, member2, member3 } //discipline 1 total = 120, discipline2 total = 180
        };
        var team2 = new Team(name: "Team2")
        {
            Members = { member4, member5, member6 } //discipline 1 total = 140, discipline2 total = 160
        };
        var teams = new TeamsCollection()
        {
            Teams = { team1, team2 }
        };

        var disciplines = teams.NormalizedDisciplines;
        disciplines.Count.Should().Be(2);
        disciplines[team1].Should().BeEquivalentTo(new Dictionary<DisciplineInfo, double>()
        {
            { disciplineInfo1, 0 },
            { disciplineInfo2, 100 }
        });
        disciplines[team2].Should().BeEquivalentTo(new Dictionary<DisciplineInfo, double>()
        {
            { disciplineInfo1, 100 },
            { disciplineInfo2, 0 }
        });
    }
}