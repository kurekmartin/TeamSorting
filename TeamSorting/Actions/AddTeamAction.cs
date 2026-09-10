using TeamSorting.Models;

namespace TeamSorting.Actions;

public sealed class AddTeamAction(Teams teams) : IUndoableAction
{
    private int _teamIndex;

    public Team? Team { get; private set; }

    public bool Execute()
    {
        if (Team is not null)
        {
            return teams.RestoreTeam(Team, _teamIndex);
        }

        if (!teams.CreateAndAddTeam())
        {
            return false;
        }

        Team = teams.TeamList[^1];
        _teamIndex = teams.TeamList.Count - 1;
        return true;
    }

    public bool Undo()
    {
        return Team is not null && teams.RemoveTeam(Team);
    }
}
