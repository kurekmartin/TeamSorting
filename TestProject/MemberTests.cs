using FluentAssertions;
using TeamSorting.Models;
using TeamSorting.ViewModels;

namespace TestProject;

public class MemberTests
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

        var members = data.GetMembersByName(["Name1", "Name2"]).ToList();
        members.Count.Should().Be(2);
        members.Should().Contain(member1);
        members.Should().Contain(member2);
    }

    [Test]
    public void MemberGetWithList()
    {
        var data = new Data();

        var member1 = new Member("Name1");
        member1.AddWithMembers(["Name2"]);
        var member2 = new Member("Name2");
        member2.AddWithMembers(["Name1", "Name3"]);
        var member3 = new Member("Name3");
        var member4 = new Member("Name4");

        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);
        data.AddMember(member4);

        data.GetWithMembers(member1).Should().BeEquivalentTo([member2, member3]);
    }

    [Test]
    public void MemberGetNotWithList()
    {
        var data = new Data();

        var member1 = new Member("Name1");
        member1.AddNotWithMembers(["Name2", "Name3"]);
        var member2 = new Member("Name2");
        member2.AddWithMembers(["Name4"]);
        var member3 = new Member("Name3");
        var member4 = new Member("Name4");
        member4.AddWithMembers(["Name5"]);
        var member5 = new Member("Name5");
        var member6 = new Member("Name6");

        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);
        data.AddMember(member4);
        data.AddMember(member5);
        data.AddMember(member6);

        data.GetNotWithMembers(member1).Should().BeEquivalentTo([member2, member3, member4, member5]);
    }

    [Test]
    public void SolutionMembersCombination_Valid()
    {
        var data = new Data();

        var member1 = new Member("Name1");
        member1.AddWithMembers(["Name2"]);
        var member2 = new Member("Name2");
        member2.AddWithMembers(["Name1"]);
        var member3 = new Member("Name3");
        var member4 = new Member("Name4");
        member4.AddWithMembers(["Name1"]);
        member4.AddNotWithMembers(["Name3"]);
        
        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);
        data.AddMember(member4);

        data.InvalidMembersCombination().Should().BeEmpty();
    }

    [Test]
    public void SolutionMembersCombination_Invalid()
    {
        var data = new Data();

        var member1 = new Member("Name1");
        member1.AddWithMembers(["Name2"]);
        var member2 = new Member("Name2");
        member2.AddWithMembers(["Name1"]);
        var member3 = new Member("Name3");
        var member4 = new Member("Name4");
        member4.AddWithMembers(["Name1"]);
        member4.AddNotWithMembers(["Name2"]);

        data.AddMember(member1);
        data.AddMember(member2);
        data.AddMember(member3);
        data.AddMember(member4);

        data.InvalidMembersCombination().Should().BeEquivalentTo([member1, member2, member4]);
    }
}