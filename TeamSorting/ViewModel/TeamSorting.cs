namespace TeamSorting.ViewModel;

public class TeamSorting
{
    public readonly MembersData MembersData = new();
    public readonly TeamsCollection TeamsCollection = new();

    public void SortMembersIntoTeams()
    {
        int teamCount = TeamsCollection.Teams.Count;
        foreach (var disciplineInfo in MembersData.DisciplinesInfo)
        {
            var sortedTeamMembers = disciplineInfo.GetSortedTeamMembers();
            var minTeam = TeamsCollection.Teams.MinBy(team => team.Score[disciplineInfo]);
            minTeam?.Members.Add(sortedTeamMembers.First());
            //TODO
        }
    }
}