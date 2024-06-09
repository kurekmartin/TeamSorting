namespace TeamSorting.ViewModel;

public class TeamSorting
{
    public MembersData MembersData = new();
    public TeamsCollection TeamsCollection = new();

    public void SortMembersIntoTeams()
    {
        var teamCount = TeamsCollection.Teams.Count;
        //TODO
    }
}