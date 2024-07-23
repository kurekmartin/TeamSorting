using Avalonia.Collections;

namespace TeamSorting.Models;

public class Member(string name)
{
    public string Name { get; set; } = name;
    public Dictionary<string, bool> With { get; } = [];
    public Dictionary<string, bool> NotWith { get; } = [];
    public AvaloniaDictionary<Guid, DisciplineRecord> Records { get; } = [];

    private Team? _team;

    public Team? Team
    {
        private get => _team;
        set
        {
            if (_team == value) return;
            _team = value;
            Validate();
        }
    }

    public bool IsValid
    {
        get
        {
            return With.Values.All(val => val == true)
                   && NotWith.Values.All(val => val == true);
        }
    }

    public void AddWithMember(string member)
    {
        With.Add(member, false);
    }

    public void AddWithMembers(IEnumerable<string> members)
    {
        foreach (string member in members)
        {
            AddWithMember(member);
        }
    }

    public void AddNotWithMember(string member)
    {
        NotWith.Add(member, false);
    }

    public void AddNotWithMembers(IEnumerable<string> members)
    {
        foreach (string member in members)
        {
            AddNotWithMember(member);
        }
    }

    public void Validate()
    {
        ValidateWith();
        ValidateNotWith();
    }

    private void ValidateWith()
    {
        foreach (string withMember in With.Keys)
        {
            With[withMember] = Team?.Members.Any(member => member.Name == withMember) ?? false;
        }
    }

    private void ValidateNotWith()
    {
        foreach (string notWithMember in NotWith.Keys)
        {
            NotWith[notWithMember] = Team?.Members.All(member => member.Name != notWithMember) ?? false;
        }
    }

    public DisciplineRecord GetRecord(DisciplineInfo discipline)
    {
        return Records[discipline.Id];
    }

    public List<DisciplineRecord> GetRecordList()
    {
        return Records.Values.ToList();
    }

    public void AddDisciplineRecord(DisciplineInfo discipline, string value)
    {
        if (Records.TryGetValue(discipline.Id, out var record))
        {
            record.RawValue = value;
        }

        record = new DisciplineRecord(this, discipline, value);
        Records.Add(discipline.Id, record);
    }

    public void RemoveDisciplineRecord(Guid recordId)
    {
        Records.Remove(recordId);
    }

    public void MoveToTeam(Team team)
    {
        if (Team == team)
        {
            return;
        }

        Team?.RemoveMember(this);
        team.AddMember(this);
    }
}