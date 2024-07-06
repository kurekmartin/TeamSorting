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
            //TODO prevent infinite loop
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
                    var newMembers = data.GetWithMembers(member).ToList();
                    newMembers.Add(member);
                    foreach (var newMember in newMembers)
                    {
                        data.AddMemberToTeam(newMember, team.Key);
                    }

                    sortedMembers.AddRange(newMembers);
                    records.RemoveAll(record => newMembers.Any(m => m.Name == record.Member.Name));
                }
            }
        }
    }
}