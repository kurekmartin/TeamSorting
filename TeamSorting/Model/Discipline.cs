namespace TeamSorting.Model;

public class Discipline
{
    public Discipline(DisciplineInfo disciplineInfo, string rawValue)
    {
        DisciplineInfo = disciplineInfo;
        RawValue = rawValue;
    }

    private readonly DisciplineInfo _disciplineInfo = null!;

    public DisciplineInfo DisciplineInfo
    {
        get => _disciplineInfo;
        init
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
            _valueIsValid = false;
        }
    }

    private object? _value;
    private bool _valueIsValid;

    public object Value
    {
        get
        {
            if (_value is not null && _valueIsValid)
            {
                return _value;
            }

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
            _valueIsValid = true;

            DisciplineInfo.UpdateValueRange(DoubleValue);

            return _value!;
        }
        private set
        {
            if (_value == value) return;
            _value = value;
            _scoreIsValid = false;
        }
    }

    private decimal DoubleValue =>
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
        var value = DoubleValue;
        var minValue = DisciplineInfo.MinValue;
        var maxValue = DisciplineInfo.MaxValue;
        const int max = 100;
        const int min = 0;
        return (((value - minValue) / (maxValue - minValue)) * (max - min)) + min;
    }

    private void DisciplineDataTypeInfoChanged(object? sender, EventArgs e)
    {
        _valueIsValid = false;
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