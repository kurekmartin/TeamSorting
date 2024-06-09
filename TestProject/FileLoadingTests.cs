using FluentAssertions;
using TeamSorting;
using TeamSorting.Model;
using TeamSorting.ViewModel;

namespace TestProject;

public class FileLoadingTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task LoadDataFromFile_Count()
    {
        using var file = new StreamReader(@"Data/input.csv");
        var members = new MembersData();

        await CsvUtil.LoadMembersData(members, file);

        members.TeamMembers.Count.Should().Be(3);
        members.DisciplinesInfo.Count.Should().Be(2);
        members.DisciplinesInfo.Should().AllSatisfy(info => info.TeamMembers.Count.Should().Be(3));

        members.DisciplinesInfo.Should()
            .AllSatisfy(info => info.TeamMembers.Should().BeEquivalentTo(members.TeamMembers));
        members.TeamMembers.Should().AllSatisfy(member =>
            member.Disciplines.Select(
                discipline => discipline.DisciplineInfo).Should().BeEquivalentTo(members.DisciplinesInfo));
    }

    [Test]
    public async Task LoadDataFromFile_MemberParsingWithoutDisciplines()
    {
        using var file = new StreamReader(@"Data/input.csv");
        var members = new MembersData();

        await CsvUtil.LoadMembersData(members, file);

        members.TeamMembers.Should().BeEquivalentTo(
        [
            new TeamMember(name: "Jmeno1", age: 10) { With = [], NotWith = [] },
            new TeamMember(name: "Jmeno2", age: 11) { With = [], NotWith = ["Jmeno1", "Jmeno2"] },
            new TeamMember(name: "Jmeno3", age: 12) { With = ["Jmeno1"], NotWith = [] }
        ], options => options.Excluding(member => member.Disciplines));
    }

    [Test]
    public async Task LoadDataFromFile_DisciplineInfoParsing()
    {
        using var file = new StreamReader(@"Data/input.csv");
        var members = new MembersData();

        await CsvUtil.LoadMembersData(members, file);

        var discipline1 = new DisciplineInfo()
            { Name = "Discipline1", DataType = DisciplineDataType.Time, SortType = DisciplineSortType.Asc };
        var discipline2 = new DisciplineInfo()
            { Name = "Discipline2", DataType = DisciplineDataType.Number, SortType = DisciplineSortType.Desc };
        members.DisciplinesInfo.Should().ContainEquivalentOf(discipline1, options => options
            .Including(info => info.Name)
            .Including(info => info.DataType)
            .Including(info => info.SortType));
        members.DisciplinesInfo.Should().ContainEquivalentOf(discipline2, options => options
            .Including(info => info.Name)
            .Including(info => info.DataType)
            .Including(info => info.SortType));
    }

    [Test]
    public async Task LoadDataFromFile_DisciplineParsing()
    {
        using var file = new StreamReader(@"Data/input.csv");
        var members = new MembersData();

        await CsvUtil.LoadMembersData(members, file);

        var member = members.TeamMembers.First(member => member.Name == "Jmeno1");
        member.Disciplines.Count.Should().Be(2);
        var discipline1 = member.Disciplines.First(discipline => discipline.DisciplineInfo.Name == "Discipline1");
        discipline1.Value.Should().Be(new TimeSpan(hours: 0, minutes: 0, seconds: 30));
        var discipline2 = member.Disciplines.First(discipline => discipline.DisciplineInfo.Name == "Discipline2");
        discipline2.Value.Should().Be(10);
    }

    [Test]
    public async Task LoadDataFromFile_DisciplineParsingNull()
    {
        using var file = new StreamReader(@"Data/input.csv");
        var members = new MembersData();

        await CsvUtil.LoadMembersData(members, file);

        var member = members.TeamMembers.First(member => member.Name == "Jmeno3");
        member.Disciplines.Count.Should().Be(2);
        var discipline1 = member.Disciplines.First(discipline => discipline.DisciplineInfo.Name == "Discipline1");
        discipline1.Value.Should().Be(new TimeSpan(0));
        var discipline2 = member.Disciplines.First(discipline => discipline.DisciplineInfo.Name == "Discipline2");
        discipline2.Value.Should().Be(0);
    }
}