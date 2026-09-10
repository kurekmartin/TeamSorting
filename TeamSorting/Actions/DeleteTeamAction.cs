using TeamSorting.Models;

namespace TeamSorting.Actions;

public sealed class DeleteTeamAction : IUndoableAction
{
    private readonly Teams _teams;
    private readonly Team _team;
    private readonly int _teamIndex;
    private readonly List<MemberState> _members;

    public DeleteTeamAction(Teams teams, Team team)
    {
        _teams = teams;
        _team = team;
        _teamIndex = teams.TeamList.IndexOf(team);
        _members = team.Members
                       .Select(member => new MemberState(member, member.AllowTeamChange))
                       .ToList();
    }

    public bool Execute()
    {
        return _teamIndex >= 0 && _teams.TeamList.Contains(_team) && _teams.RemoveTeam(_team);
    }

    public bool Undo()
    {
        if (!_teams.RestoreTeam(_team, _teamIndex))
        {
            return false;
        }

        foreach (MemberState memberState in _members)
        {
            if (memberState.Member.Team != _team)
            {
                memberState.Member.MoveToTeam(_team);
            }

            memberState.Member.AllowTeamChange = memberState.AllowTeamChange;
        }

        return true;
    }

    private readonly record struct MemberState(Member Member, bool AllowTeamChange);
}
