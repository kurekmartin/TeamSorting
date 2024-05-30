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

    private decimal _minValue = decimal.MaxValue;
    private decimal _maxValue = decimal.MinValue;

    public decimal MinValue
    {
        get => _minValue;
        private set
        {
            if (_minValue == value) return;
            _minValue = value;
            OnValueRangeChanged();
        }
    }

    public decimal MaxValue
    {
        get => _maxValue;
        private set
        {
            if (_maxValue == value) return;
            _maxValue = value;
            OnValueRangeChanged();
        }
    }

    public void UpdateValueRange(decimal value)
    {
        if (value < MinValue)
        {
            MinValue = value;
        }
        else if (value > MaxValue)
        {
            MaxValue = value;
        }
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