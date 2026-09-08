namespace TeamSorting.Models;

internal sealed class MemberConstraintErrors
{
    public HashSet<string> With { get; } = [];
    public HashSet<string> NotWith { get; } = [];
    public HashSet<Member> InvalidWithMembers { get; } = [];
    public HashSet<Member> InvalidNotWithMembers { get; } = [];
}