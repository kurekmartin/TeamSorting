namespace TeamSorting.Model;

public class DisciplineRecord
{
    public DisciplineRecord(DisciplineInfo disciplineInfo, string rawValue)
    {
        DisciplineInfo = disciplineInfo;
        RawValue = rawValue;
    }

    private readonly DisciplineInfo _disciplineInfo = null!;

    public DisciplineInfo DisciplineInfo
    {
        get => _disciplineInfo;
        private init
        {
            if (value == _disciplineInfo) return;
            _disciplineInfo = value;
            _disciplineInfo.DisciplineDataTypeChanged += DisciplineDataTypeInfoChanged;
            _disciplineInfo.DisciplineSortTypeChanged += DisciplineSortTypeChanged;
            _disciplineInfo.ValueRangeChanged += OnValueRangeChanged;
        }
    }

    private string _rawValue = string.Empty;

    public string RawValue
    {
        get => _rawValue;
        set
        {
            if (value == _rawValue) return;
            _rawValue = value;
            UpdateValue();
        }
    }

    private object? _value;

    public object Value
    {
        get
        {
            if (_value == null)
            {
                return DisciplineInfo.DataType switch
                {
                    DisciplineDataType.Time => TimeSpan.Zero,
                    DisciplineDataType.Number => decimal.Zero,
                    _ => throw new FormatException()
                };
            }

            return _value;
        }

        private set
        {
            if (_value == value) return;
            _value = value;
            _scoreIsValid = false;
        }
    }

    private void UpdateValue()
    {
        Value = DisciplineInfo.DataType switch
        {
            DisciplineDataType.Time => string.IsNullOrWhiteSpace(RawValue)
                ? TimeSpan.Zero
                : TimeSpan.Parse(RawValue),
            DisciplineDataType.Number => string.IsNullOrWhiteSpace(RawValue)
                ? decimal.Zero
                : decimal.Parse(RawValue),
            _ => throw new FormatException()
        };
    }

    public decimal DecimalValue =>
        DisciplineInfo.DataType switch
        {
            DisciplineDataType.Time => (decimal)((TimeSpan)Value).TotalSeconds,
            DisciplineDataType.Number => ((decimal)Value),
            _ => throw new FormatException()
        };

    private decimal _score;
    private bool _scoreIsValid;

    public decimal Score
    {
        get
        {
            if (_scoreIsValid) return _score;

            var normalizedScore = NormalizedValue();
            _score = normalizedScore;
            _scoreIsValid = true;

            return _score;
        }
        set => _score = value;
    }

    private decimal NormalizedValue()
    {
        var value = DecimalValue;
        var minValue = DisciplineInfo.MinValue;
        var maxValue = DisciplineInfo.MaxValue;
        var max = DisciplineInfo.SortType == DisciplineSortType.Asc ? 100 : 0;
        var min = DisciplineInfo.SortType == DisciplineSortType.Asc ? 0 : 100;
        return (((value - minValue) / (maxValue - minValue)) * (max - min)) + min;
    }

    private void DisciplineDataTypeInfoChanged(object? sender, EventArgs e)
    {
        UpdateValue();
    }

    private void DisciplineSortTypeChanged(object? sender, EventArgs e)
    {
        _scoreIsValid = false;
    }

    private void OnValueRangeChanged(object? sender, EventArgs e)
    {
        _scoreIsValid = false;
    }
}