using Avalonia.Collections;

namespace TeamSorting.Models;

public class Member(string name)
{
    public string Name { get; set; } = name;
    public List<string> With { get; set; } = [];
    public List<string> NotWith { get; set; } = [];
    public AvaloniaDictionary<Guid, DisciplineRecord> Records { get; } = [];

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
}