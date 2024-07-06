using FluentAssertions;
using TeamSorting.Model.New;
using TeamSorting.ViewModel.New;

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
        var data = new Data();
        await data.LoadFromFile(file);

        data.Members.Count.Should().Be(3);
        data.Disciplines.Count.Should().Be(3);
        data.DisciplineRecords.Count.Should().Be(9);
    }

    [Test]
    public async Task LoadDataFromFile_MemberParsingWithoutDisciplines()
    {
        using var file = new StreamReader(@"Data/input.csv");
        var data = new Data();
        await data.LoadFromFile(file);

        data.Members.Should().BeEquivalentTo(
        [
            new Member(name: "Jmeno1") { With = [], NotWith = [] },
            new Member(name: "Jmeno2") { With = [], NotWith = ["Jmeno1", "Jmeno2"] },
            new Member(name: "Jmeno3") { With = ["Jmeno1"], NotWith = [] }
        ]);
    }

    [Test]
    public async Task LoadDataFromFile_DisciplineInfoParsing()
    {
        using var file = new StreamReader(@"Data/input.csv");
        var data = new Data();
        await data.LoadFromFile(file);

        var age = new DisciplineInfo("Age")
            { DataType = DisciplineDataType.Number, SortType = DisciplineSortType.Asc };
        var discipline1 = new DisciplineInfo("Discipline1")
            { DataType = DisciplineDataType.Time, SortType = DisciplineSortType.Asc };
        var discipline2 = new DisciplineInfo("Discipline2")
            { DataType = DisciplineDataType.Number, SortType = DisciplineSortType.Desc };
        
        data.Disciplines.Should().ContainEquivalentOf(age);
        data.Disciplines.Should().ContainEquivalentOf(discipline1);
        data.Disciplines.Should().ContainEquivalentOf(discipline2);
    }

    [Test]
    public async Task LoadDataFromFile_DisciplineParsing()
    {
        using var file = new StreamReader(@"Data/input.csv");
        var data = new Data();
        await data.LoadFromFile(file);
    
        var member = data.GetMemberByName("Jmeno1");
        Assert.That(member,Is.Not.Null);
        var disciplineRecords = data.GetMemberRecords(member).ToList();
        
        Assert.That(disciplineRecords, Is.Not.Null);

        disciplineRecords.Count.Should().Be(3);
        var age = data.GetDisciplineByName("Age");
        Assert.That(age,Is.Not.Null);
        data.GetMemberDisciplineValue(member, age).Should().Be(10);
        
        var discipline1 = data.GetDisciplineByName("Discipline1");
        Assert.That(discipline1,Is.Not.Null);
        data.GetMemberDisciplineValue(member, discipline1).Should().Be(new TimeSpan(hours: 0, minutes: 0, seconds: 30));
        
        var discipline2 = data.GetDisciplineByName("Discipline2");
        Assert.That(discipline2,Is.Not.Null);
        data.GetMemberDisciplineValue(member, discipline2).Should().Be(10);
    }
    
    [Test]
    public async Task LoadDataFromFile_DisciplineParsingNull()
    {
        using var file = new StreamReader(@"Data/input.csv");
        var data = new Data();
        await data.LoadFromFile(file);
    
        var member = data.GetMemberByName("Jmeno3");
        Assert.That(member,Is.Not.Null);

        data.GetMemberRecords(member).Count().Should().Be(3);
        var discipline1 = data.GetDisciplineByName("Discipline1");
        Assert.That(discipline1,Is.Not.Null);
        data.GetMemberDisciplineValue(member, discipline1).Should().Be(new TimeSpan(0));
        
        var discipline2 = data.GetDisciplineByName("Discipline2");
        Assert.That(discipline2,Is.Not.Null);
        data.GetMemberDisciplineValue(member, discipline2).Should().Be(0);
    }
}