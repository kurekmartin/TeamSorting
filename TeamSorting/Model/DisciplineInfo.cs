namespace TeamSorting.Model;

public class DisciplineInfo
{
    public string Name { get; set; } = string.Empty;

    private DisciplineDataType _dataType;

    public DisciplineDataType DataType
    {
        get => _dataType;
        set
        {
            if (value == _dataType) return;
            _dataType = value;
            OnDisciplineDataTypeChanged();
        }
    }

    private DisciplineSortType _sortType;

    public DisciplineSortType SortType
    {
        get => _sortType;
        set
        {
            if (_sortType == value) return;
            _sortType = value;
            OnDisciplineSortTypeChanged();
        }
    }

    public double MinValue => GetDisciplineValues().Min();

    public double MaxValue => GetDisciplineValues().Max();

    public readonly List<TeamMember> TeamMembers = [];

    private IEnumerable<double> GetDisciplineValues()
    {
        return TeamMembers.Select(member =>
            member.Disciplines.First(record => record.DisciplineInfo == this).DoubleValue);
    }

    public event EventHandler? DisciplineDataTypeChanged;

    protected virtual void OnDisciplineDataTypeChanged()
    {
        DisciplineDataTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? DisciplineSortTypeChanged;

    protected virtual void OnDisciplineSortTypeChanged()
    {
        DisciplineSortTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? ValueRangeChanged;

    protected virtual void OnValueRangeChanged()
    {
        ValueRangeChanged?.Invoke(this, EventArgs.Empty);
    }
}

public enum DisciplineDataType
{
    Time,
    Number
}

public enum DisciplineSortType
{
    Asc,
    Desc
}