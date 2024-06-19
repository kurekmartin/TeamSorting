using System.Collections.ObjectModel;
using FluentAssertions;
using TeamSorting.Model;
using TeamSorting.ViewModel;

namespace TestProject;

public class MembersDataTests
{
    [Test]
    public void SolutionMembersCombination_Valid()
    {
        var data = new MembersData()
        {
            TeamMembers =
            {
                new TeamMember("Name1", 0) { With = ["Name2"] },
                new TeamMember("Name2", 0) { With = ["Name1"] },
                new TeamMember("Name3", 0),
                new TeamMember("Name4", 0) { With = ["Name1"], NotWith = ["Name3"] }
            }
        };
        data.InvalidMembersCombinationList().Should().BeEmpty();
    }

    [Test]
    public void SolutionMembersCombination_InValid()
    {
        var data = new MembersData()
        {
            TeamMembers =
            {
                new TeamMember("Name1", 0) { With = ["Name2"] },
                new TeamMember("Name2", 0) { With = ["Name1"] },
                new TeamMember("Name3", 0),
                new TeamMember("Name4", 0) { With = ["Name1"], NotWith = ["Name2"] }
            }
        };
        data.InvalidMembersCombinationList().Should().BeEquivalentTo(["Name1", "Name2", "Name4"]);
    }
}