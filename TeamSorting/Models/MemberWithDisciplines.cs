using System.Collections.ObjectModel;

namespace TeamSorting.Models;

public class MemberWithDisciplines(Member member, List<DisciplineRecord> records)
{
    public Member Member { get; set; } = member;
    public ObservableCollection<DisciplineRecord> Records { get; set; } = new(records);
}