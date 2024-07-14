namespace TeamSorting.Models;

public class DisciplineRecord(Member member, DisciplineInfo disciplineInfo, string rawValue)
{
    public Member Member { get; } = member;
    public DisciplineInfo DisciplineInfo { get; } = disciplineInfo;
    public string RawValue { get; set; } = rawValue;

    public object Value
    {
        get
        {
            return DisciplineInfo.DataType switch
            {
                DisciplineDataType.Time => string.IsNullOrWhiteSpace(RawValue)
                    ? TimeSpan.Zero
                    : TimeSpan.Parse(RawValue),
                DisciplineDataType.Number => string.IsNullOrWhiteSpace(RawValue)
                    ? 0d
                    : double.Parse(RawValue),
                _ => throw new FormatException()
            };
        }
    }

    public double DoubleValue =>
        DisciplineInfo.DataType switch
        {
            DisciplineDataType.Time => ((TimeSpan)Value).TotalSeconds,
            DisciplineDataType.Number => (double)Value,
            _ => throw new FormatException()
        };
}