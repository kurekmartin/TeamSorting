using FluentAssertions;
using TeamSorting.Models;
using TeamSorting.ViewModels;

namespace TestProject;

public class TeamSortingTests
{
    [Test]
    public void SortMembersToTeams()
    {
        var data = new Data();

        var member1 = new Member("Name1");
        var member2 = new Member("Name2");
        var member3 = new Member("Name3");
        var member4 = new Member("Name4") { With = ["Name1"] };
        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);
        data.AddMember(member4);

        var discipline = new DisciplineInfo("Discipline1")
            { DataType = DisciplineDataType.Number, SortType = DisciplineSortType.Asc };
        data.AddDiscipline(discipline);

        data.AddDisciplineRecord(member1, discipline, "0");
        data.AddDisciplineRecord(member2, discipline, "1");
        data.AddDisciplineRecord(member3, discipline, "2");
        data.AddDisciplineRecord(member4, discipline, "3");

        data.CreateTeams(3);

        TeamSorting.ViewModels.TeamSorting.SortMembersIntoTeams(data);

        data.Teams.Count.Should().Be(3);
        data.Teams.Should().AllSatisfy(team => team.Members.Count.Should().BeInRange(1, 2));
        data.Teams.SelectMany(team => team.Members).Should().BeEquivalentTo(data.Members);
    }
}