using TeamSorting.Lang;
using TeamSorting.Models;

namespace TeamSorting.Validators;

internal static class MemberConstraintValidator
{
    public static Dictionary<Member, MemberConstraintErrors> Validate(IEnumerable<Member> members)
    {
        List<Member> memberList = [.. members];
        HashSet<Member> knownMembers = [.. memberList];
        Dictionary<Member, MemberConstraintErrors> errors = memberList.ToDictionary(
            member => member,
            _ => new MemberConstraintErrors());
        HashSet<Member> visited = [];

        foreach (Member member in memberList)
        {
            if (visited.Contains(member))
            {
                continue;
            }

            HashSet<Member> component = GetWithComponent(member, knownMembers, visited);
            List<(Member First, Member Second)> conflicts = GetConflicts(component);

            foreach ((Member first, Member second) in conflicts)
            {
                List<Member> path = FindShortestWithPath(first, second, component);
                string message = string.Format(
                    Resources.InputView_MemberConstraintConflict_Error,
                    first.Name,
                    second.Name,
                    string.Join(" → ", path.Select(pathMember => $"**{pathMember.Name}**")));

                foreach (Member pathMember in path)
                {
                    errors[pathMember].With.Add(message);
                }

                for (var i = 0; i < path.Count - 1; i++)
                {
                    Member current = path[i];
                    Member next = path[i + 1];
                    errors[current].InvalidWithMembers.Add(next);
                    errors[next].InvalidWithMembers.Add(current);
                }

                errors[first].NotWith.Add(message);
                errors[second].NotWith.Add(message);
                errors[first].InvalidNotWithMembers.Add(second);
                errors[second].InvalidNotWithMembers.Add(first);
            }
        }

        return errors;
    }

    private static HashSet<Member> GetWithComponent(
        Member start,
        HashSet<Member> knownMembers,
        HashSet<Member> visited)
    {
        HashSet<Member> component = [];
        Stack<Member> pending = new();
        pending.Push(start);

        while (pending.TryPop(out Member? member))
        {
            if (!visited.Add(member))
            {
                continue;
            }

            component.Add(member);
            foreach (Member withMember in member.With.Where(knownMembers.Contains))
            {
                pending.Push(withMember);
            }
        }

        return component;
    }

    private static List<(Member First, Member Second)> GetConflicts(HashSet<Member> component)
    {
        List<(Member First, Member Second)> conflicts = [];
        HashSet<(Guid First, Guid Second)> seen = [];

        foreach (Member member in component)
        {
            foreach (Member notWithMember in member.NotWith.Where(component.Contains))
            {
                (Member first, Member second) = CompareMembers(member, notWithMember) <= 0
                    ? (member, notWithMember)
                    : (notWithMember, member);

                (Guid firstId, Guid secondId) = member.Id.CompareTo(notWithMember.Id) <= 0
                    ? (member.Id, notWithMember.Id)
                    : (notWithMember.Id, member.Id);

                if (seen.Add((firstId, secondId)))
                {
                    conflicts.Add((first, second));
                }
            }
        }

        return
        [
            .. conflicts.OrderBy(conflict => conflict.First.Name, StringComparer.CurrentCulture)
                        .ThenBy(conflict => conflict.Second.Name, StringComparer.CurrentCulture)
        ];
    }

    private static List<Member> FindShortestWithPath(Member start, Member destination, HashSet<Member> component)
    {
        if (start == destination)
        {
            return [start];
        }

        Queue<Member> pending = new();
        HashSet<Member> visited = [start];
        Dictionary<Member, Member> previous = [];
        pending.Enqueue(start);

        while (pending.TryDequeue(out Member? current))
        {
            IEnumerable<Member> neighbors = current.With
                                                   .Where(component.Contains)
                                                   .OrderBy(member => member.Name, StringComparer.CurrentCulture)
                                                   .ThenBy(member => member.Id);
            foreach (Member neighbor in neighbors)
            {
                if (!visited.Add(neighbor))
                {
                    continue;
                }

                previous[neighbor] = current;
                if (neighbor == destination)
                {
                    return BuildPath(start, destination, previous);
                }

                pending.Enqueue(neighbor);
            }
        }

        return [];
    }

    private static List<Member> BuildPath(Member start, Member destination, Dictionary<Member, Member> previous)
    {
        List<Member> path = [destination];
        Member current = destination;
        while (current != start)
        {
            current = previous[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }

    private static int CompareMembers(Member first, Member second)
    {
        int nameComparison = StringComparer.CurrentCulture.Compare(first.Name, second.Name);
        return nameComparison != 0 ? nameComparison : first.Id.CompareTo(second.Id);
    }
}