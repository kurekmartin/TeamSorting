using TeamSorting.Model.New;

namespace TeamSorting.ViewModel.New;

public static class TeamSorting
{
    public static void SortMembersIntoTeams(Data data)
    {
        //TODO exclude without members
        int membersCount = data.Members.Count;
        var sortedMembers = new List<Member>();
        var sortedDisciplines = data.GetSortedDisciplines();

        while (sortedMembers.Count < membersCount)
        {
            foreach (var sortedDiscipline in sortedDisciplines)
            {
                var records = sortedDiscipline.Value.ExceptBy(sortedMembers, record => record.Member).ToList();
                if (records.Count == 0)
                {
                    continue;
                }

                var sortedTeams = data.GetSortedTeamsByValueByDiscipline(sortedDiscipline.Key);
                foreach (var team in sortedTeams)
                {
                    var member = records.Last().Member;
                    //TODO get and add with members as group
                    data.AddMemberToTeam(member, team.Key);
                    sortedMembers.Add(member);
                    records.RemoveAt(records.Count - 1);
                }
            }
        }
    }
}