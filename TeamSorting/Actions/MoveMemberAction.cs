using TeamSorting.Models;

namespace TeamSorting.Actions;

public sealed class MoveMemberAction(Member member, Team sourceTeam, Team destinationTeam) : IUndoableAction
{
    public bool Execute()
    {
        return member.Team == sourceTeam && member.MoveToTeam(destinationTeam);
    }

    public bool Undo()
    {
        return member.Team == destinationTeam && member.MoveToTeam(sourceTeam);
    }
}
