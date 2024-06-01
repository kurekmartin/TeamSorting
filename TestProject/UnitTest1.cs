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
    public async Task LoadDataFromFile_Count()
    {
        using var file = new StreamReader(@"Data/input.csv");
        var members = new MembersData();

        await CsvUtil.LoadMembersData(members, file);

        members.TeamMembers.Count.Should().Be(3);
        members.DisciplinesInfo.Count.Should().Be(2);

        //  var member1 = members.TeamMembers.First(member => member.Name == "Jmeno1");
        // member1.Disciplines.Count.Should().Be(2);
        //  var mem1dis1 = member1.Disciplines.First(discipline => discipline.DisciplineInfo.Name == discipline1.Name);
        //  // mem1dis1.DisciplineInfo.Should().BeEquivalentTo(discipline1);
        //  mem1dis1.Value.Should().Be(new TimeSpan(hours: 0, minutes: 0, seconds: 10));
        //  var mem1dis2 = member1.Disciplines.First(discipline => discipline.DisciplineInfo.Name == discipline2.Name);
        //  //mem1dis2.DisciplineInfo.Should().BeEquivalentTo(discipline2);
        //  mem1dis2.Value.Should().Be(10);
        //
        //  var member2 = members.TeamMembers.First(member => member.Name == "Jmeno2");
        //  member2.Disciplines.Count.Should().Be(2);
        //  var mem2dis1 = member2.Disciplines.First(discipline => discipline.DisciplineInfo.Name == discipline1.Name);
        //  //mem2dis1.DisciplineInfo.Should().BeEquivalentTo(discipline1);
        //  mem2dis1.Value.Should().Be(new TimeSpan(hours: 0, minutes: 1, seconds: 15));
        //  var mem2dis2 = member2.Disciplines.First(discipline => discipline.DisciplineInfo.Name == discipline2.Name);
        //  //mem2dis2.DisciplineInfo.Should().BeEquivalentTo(discipline2);
        //  mem2dis2.Value.Should().Be(20);
        //
        //  var member3 = members.TeamMembers.First(member => member.Name == "Jmeno3");
        //  member3.Disciplines.Count.Should().Be(2);
        //  var mem3dis1 = member3.Disciplines.First(discipline => discipline.DisciplineInfo.Name == discipline1.Name);
        //  //mem3dis1.DisciplineInfo.Should().BeEquivalentTo(discipline1);
        //  mem3dis1.Value.Should().Be(new TimeSpan(hours: 0, minutes: 0, seconds: 0));
        //  var mem3dis2 = member3.Disciplines.First(discipline => discipline.DisciplineInfo.Name == discipline2.Name);
        //  //mem3dis2.DisciplineInfo.Should().BeEquivalentTo(discipline2);
        //  mem3dis2.Value.Should().Be(0);
    }

    [Test]
    public async Task LoadDataFromFile_MemberParsingWithoutDisciplines()
    {
        using var file = new StreamReader(@"Data/input.csv");
        var members = new MembersData();

        await CsvUtil.LoadMembersData(members, file);

        members.TeamMembers.Should().BeEquivalentTo(
        [
            new TeamMember() { Name = "Jmeno1", With = [], NotWith = [], Age = 10 },
            new TeamMember() { Name = "Jmeno2", With = [], NotWith = ["Jmeno1", "Jmeno2"], Age = 11 },
            new TeamMember() { Name = "Jmeno3", With = ["Jmeno1"], NotWith = [], Age = 12 }
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

    [Test]
    public void Discipline_ValueRange_Number()
    {
        var disciplineInfoAsc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Asc, DataType = DisciplineDataType.Number };
        var disciplineInfoDesc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Desc, DataType = DisciplineDataType.Number };


        _ = new DisciplineRecord(disciplineInfoAsc, "10");
        _ = new DisciplineRecord(disciplineInfoAsc, "5");
        _ = new DisciplineRecord(disciplineInfoAsc, "3");
        _ = new DisciplineRecord(disciplineInfoAsc, "50");


        _ = new DisciplineRecord(disciplineInfoDesc, "10");
        _ = new DisciplineRecord(disciplineInfoDesc, "5");
        _ = new DisciplineRecord(disciplineInfoDesc, "3");
        _ = new DisciplineRecord(disciplineInfoDesc, "50");


        disciplineInfoAsc.MaxValue.Should().Be(50);
        disciplineInfoAsc.MinValue.Should().Be(3);
        disciplineInfoDesc.MaxValue.Should().Be(50);
        disciplineInfoDesc.MinValue.Should().Be(3);
    }

    [Test]
    public void Discipline_ValueRange_Time()
    {
        var disciplineInfoAsc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Asc, DataType = DisciplineDataType.Time };
        var disciplineInfoDesc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Desc, DataType = DisciplineDataType.Time };


        _ = new DisciplineRecord(disciplineInfoAsc, "00:00:10");
        _ = new DisciplineRecord(disciplineInfoAsc, "00:15:00");
        _ = new DisciplineRecord(disciplineInfoAsc, "00:01:00");
        _ = new DisciplineRecord(disciplineInfoAsc, "00:00:05");


        _ = new DisciplineRecord(disciplineInfoDesc, "00:00:10");
        _ = new DisciplineRecord(disciplineInfoDesc, "00:15:00");
        _ = new DisciplineRecord(disciplineInfoDesc, "00:01:00");
        _ = new DisciplineRecord(disciplineInfoDesc, "00:00:05");


        disciplineInfoAsc.MaxValue.Should().Be((decimal)(new TimeSpan(hours: 0, minutes: 15, seconds: 0).TotalSeconds));
        disciplineInfoAsc.MinValue.Should().Be((decimal)(new TimeSpan(hours: 0, minutes: 0, seconds: 5).TotalSeconds));
        disciplineInfoDesc.MaxValue.Should()
            .Be((decimal)(new TimeSpan(hours: 0, minutes: 15, seconds: 0).TotalSeconds));
        disciplineInfoDesc.MinValue.Should().Be((decimal)(new TimeSpan(hours: 0, minutes: 0, seconds: 5).TotalSeconds));
    }

    [Test]
    public void Discipline_Score_Number()
    {
        var disciplineInfoAsc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Asc, DataType = DisciplineDataType.Number };
        var disciplineInfoDesc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Desc, DataType = DisciplineDataType.Number };

        List<DisciplineRecord> disciplinesAsc =
        [
            new DisciplineRecord(disciplineInfoAsc, "10"),
            new DisciplineRecord(disciplineInfoAsc, "0"),
            new DisciplineRecord(disciplineInfoAsc, "50")
        ];

        List<DisciplineRecord> disciplinesDesc =
        [
            new DisciplineRecord(disciplineInfoDesc, "10"),
            new DisciplineRecord(disciplineInfoDesc, "0"),
            new DisciplineRecord(disciplineInfoDesc, "50")
        ];

        disciplinesAsc[0].Score.Should().Be(20);
        disciplinesAsc[1].Score.Should().Be(0);
        disciplinesAsc[2].Score.Should().Be(100);

        disciplinesDesc[0].Score.Should().Be(80);
        disciplinesDesc[1].Score.Should().Be(100);
        disciplinesDesc[2].Score.Should().Be(0);
    }

    [Test]
    public void Discipline_Score_Time()
    {
        var disciplineInfoAsc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Asc, DataType = DisciplineDataType.Time };
        var disciplineInfoDesc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Desc, DataType = DisciplineDataType.Time };

        List<DisciplineRecord> disciplinesAsc =
        [
            new DisciplineRecord(disciplineInfoAsc, "00:01:00"),
            new DisciplineRecord(disciplineInfoAsc, "00:00:00"),
            new DisciplineRecord(disciplineInfoAsc, "00:05:00")
        ];

        List<DisciplineRecord> disciplinesDesc =
        [
            new DisciplineRecord(disciplineInfoDesc, "00:01:00"),
            new DisciplineRecord(disciplineInfoDesc, "00:00:00"),
            new DisciplineRecord(disciplineInfoDesc, "00:05:00")
        ];

        disciplinesAsc[0].Score.Should().Be(20);
        disciplinesAsc[1].Score.Should().Be(0);
        disciplinesAsc[2].Score.Should().Be(100);

        disciplinesDesc[0].Score.Should().Be(80);
        disciplinesDesc[1].Score.Should().Be(100);
        disciplinesDesc[2].Score.Should().Be(0);
    }

    [Test]
    public void Discipline_Score_SortTypeChanged()
    {
        var disciplineInfoAscToDesc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Asc, DataType = DisciplineDataType.Number };
        var disciplineInfoDescToAsc = new DisciplineInfo()
            { Name = "Discipline", SortType = DisciplineSortType.Desc, DataType = DisciplineDataType.Number };

        List<DisciplineRecord> disciplinesAscToDesc =
        [
            new DisciplineRecord(disciplineInfoAscToDesc, "10"),
            new DisciplineRecord(disciplineInfoAscToDesc, "0"),
            new DisciplineRecord(disciplineInfoAscToDesc, "50")
        ];

        List<DisciplineRecord> disciplinesDescToAsc =
        [
            new DisciplineRecord(disciplineInfoDescToAsc, "10"),
            new DisciplineRecord(disciplineInfoDescToAsc, "0"),
            new DisciplineRecord(disciplineInfoDescToAsc, "50")
        ];

        disciplineInfoAscToDesc.SortType = DisciplineSortType.Desc;
        disciplineInfoDescToAsc.SortType = DisciplineSortType.Asc;

        disciplinesDescToAsc[0].Score.Should().Be(20);
        disciplinesDescToAsc[1].Score.Should().Be(0);
        disciplinesDescToAsc[2].Score.Should().Be(100);

        disciplinesAscToDesc[0].Score.Should().Be(80);
        disciplinesAscToDesc[1].Score.Should().Be(100);
        disciplinesAscToDesc[2].Score.Should().Be(0);
    }
}