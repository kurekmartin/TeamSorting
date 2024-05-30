using FluentAssertions;
using TeamSorting;
using TeamSorting.Model;
using TeamSorting.ViewModel;

namespace TestProject;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task LoadDataFromFile()
    {
        using var file = new StreamReader(@"Data/input.csv");
        var members = new MembersData();

        await CsvUtil.LoadMembersData(members, file);

        members.DisciplinesInfo.Count.Should().Be(2);
        var discipline1 = new DisciplineInfo()
            { Name = "Discipline1", DataType = DisciplineDataType.Time, SortType = DisciplineSortType.Asc };
        var discipline2 = new DisciplineInfo()
            { Name = "Discipline2", DataType = DisciplineDataType.Number, SortType = DisciplineSortType.Desc };
        members.DisciplinesInfo.Should().ContainEquivalentOf(discipline1);
        members.DisciplinesInfo.Should().ContainEquivalentOf(discipline2);

        members.TeamMembers.Count.Should().Be(3);
        var member1 = members.TeamMembers.First(member => member.Name == "Jmeno1");
        member1.Should().BeEquivalentTo(new TeamMember()
                { Name = "Jmeno1", With = [], NotWith = [], Age = 10 },
            options => options.Excluding(member => member.Disciplines));
        
        var member2 = members.TeamMembers.First(member => member.Name == "Jmeno2");
        member2.Should().BeEquivalentTo(new TeamMember()
                { Name = "Jmeno2", With = [], NotWith = ["Jmeno1","Jmeno2"], Age = 11 },
            options => options.Excluding(member => member.Disciplines));
        
        var member3 = members.TeamMembers.First(member => member.Name == "Jmeno3");
        member3.Should().BeEquivalentTo(new TeamMember()
                { Name = "Jmeno3", With = ["Jmeno1"], NotWith = [], Age = 12 },
            options => options.Excluding(member => member.Disciplines));
    }
}