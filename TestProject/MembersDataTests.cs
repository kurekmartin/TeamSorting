using FluentAssertions;
using TeamSorting.Model.New;
using TeamSorting.ViewModel.New;

namespace TestProject;

public class MembersTests
{
    [Test]
    public void AddMember()
    {
        var data = new Data();

        var member1 = new Member("Name1");
        var member2 = new Member("Name2");

        data.AddMember(member1);
        data.AddMember(member2);

        data.Members.Count.Should().Be(2);
        data.Members.Should().Contain(member1);
        data.Members.Should().Contain(member2);
    }
    
    [Test]
    public void RemoveMember()
    {
        var data = new Data();

        var member1 = new Member("Name1");
        var member2 = new Member("Name2");

        data.AddMember(member1);
        data.AddMember(member2);

        data.RemoveMember(member2);
        
        data.Members.Count.Should().Be(1);
        data.Members.Should().Contain(member1);
    }
    
    [Test]
    public void GetMemberByName()
    {
        var data = new Data();

        var member1 = new Member("Name1");
        var member2 = new Member("Name2");

        data.AddMember(member1);
        data.AddMember(member2);

        data.GetMemberByName("Name1").Should().Be(member1);
    }
    
    [Test]
    public void GetMembersByName()
    {
        var data = new Data();

        var member1 = new Member("Name1");
        var member2 = new Member("Name2");

        data.AddMember(member1);
        data.AddMember(member2);

        var members = data.GetMembersByName(["Name1","Name2"]).ToList();
        members.Count.Should().Be(2);
        members.Should().Contain(member1);
        members.Should().Contain(member2);
    }
    
    [Test]
    public void SolutionMembersCombination_Valid()
    {
        var data = new Data();

        data.AddMember(new Member("Name1") { With = ["Name2"] });
        data.AddMember(new Member("Name2") { With = ["Name1"] });
        data.AddMember(new Member("Name3"));
        data.AddMember(new Member("Name4") { With = ["Name1"], NotWith = ["Name3"] });

        data.InvalidMembersCombination().Should().BeEmpty();
    }

    [Test]
    public void SolutionMembersCombination_Invalid()
    {
        var data = new Data();

        var member1 = new Member("Name1") { With = ["Name2"] };
        var member2 = new Member("Name2") { With = ["Name1"] };
        var member3 = new Member("Name3");
        var member4 = new Member("Name4") { With = ["Name1"], NotWith = ["Name2"] };
        
        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);
        data.AddMember(member4);

        data.InvalidMembersCombination().Should().BeEquivalentTo([member1, member2, member4]);
    }
}