using FluentAssertions;
using TeamSorting.Models;
using TeamSorting.ViewModels;

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
        data.GetAllRecords().Count().Should().Be(9);
    }

    //TODO fix test
    // [Test]
    // public async Task LoadDataFromFile_MemberParsingWithoutDisciplines()
    // {
    //     using var file = new StreamReader(@"Data/input.csv");
    //     var data = new Data();
    //     await data.LoadFromFile(file);
    //
    //     data.Members.Should().BeEquivalentTo(
    //     [
    //         new Member(name: "Jmeno1"),
    //         new Member(name: "Jmeno2") { With = [], NotWith = ["Jmeno1", "Jmeno2"] },
    //         new Member(name: "Jmeno3") { With = ["Jmeno1"], NotWith = [] }
    //     ],o=>o.Excluding(member => member.Records));
    // }

    [Test]
    public async Task LoadDataFromFile_DisciplineInfoParsing()
    {
        using var file = new StreamReader(@"Data/input.csv");
        var data = new Data();
        await data.LoadFromFile(file);

        var age = new DisciplineInfo("Age")
            { DataType = DisciplineDataType.Number, SortOrder = SortOrder.Asc };
        var discipline1 = new DisciplineInfo("Discipline1")
            { DataType = DisciplineDataType.Time, SortOrder = SortOrder.Asc };
        var discipline2 = new DisciplineInfo("Discipline2")
            { DataType = DisciplineDataType.Number, SortOrder = SortOrder.Desc };
        
        data.Disciplines.Should().ContainEquivalentOf(age,o=>o.Excluding(discipline => discipline.Id));
        data.Disciplines.Should().ContainEquivalentOf(discipline1,o=>o.Excluding(discipline => discipline.Id));
        data.Disciplines.Should().ContainEquivalentOf(discipline2,o=>o.Excluding(discipline => discipline.Id));
    }

    [Test]
    public async Task LoadDataFromFile_DisciplineParsing()
    {
        using var file = new StreamReader(@"Data/input.csv");
        var data = new Data();
        await data.LoadFromFile(file);
    
        var member = data.GetMemberByName("Jmeno1");
        Assert.That(member,Is.Not.Null);
        var disciplineRecords = member.GetRecordList();
        
        Assert.That(disciplineRecords, Is.Not.Null);

        disciplineRecords.Count.Should().Be(3);
        var age = data.GetDisciplineByName("Age");
        Assert.That(age,Is.Not.Null);
        member.GetRecord(age).Value.Should().Be(10);
        
        var discipline1 = data.GetDisciplineByName("Discipline1");
        Assert.That(discipline1,Is.Not.Null);
        member.GetRecord(discipline1).Value.Should().Be(new TimeSpan(hours: 0, minutes: 0, seconds: 30));
        
        var discipline2 = data.GetDisciplineByName("Discipline2");
        Assert.That(discipline2,Is.Not.Null);
        member.GetRecord(discipline2).Value.Should().Be(10);
    }
    
    [Test]
    public async Task LoadDataFromFile_DisciplineParsingNull()
    {
        using var file = new StreamReader(@"Data/input.csv");
        var data = new Data();
        await data.LoadFromFile(file);
    
        var member = data.GetMemberByName("Jmeno3");
        Assert.That(member,Is.Not.Null);

        member.GetRecordList().Count().Should().Be(3);
        var discipline1 = data.GetDisciplineByName("Discipline1");
        Assert.That(discipline1,Is.Not.Null);
        member.GetRecord(discipline1).Value.Should().Be(new TimeSpan(0));
        
        var discipline2 = data.GetDisciplineByName("Discipline2");
        Assert.That(discipline2,Is.Not.Null);
        member.GetRecord(discipline2).Value.Should().Be(0);
    }
}