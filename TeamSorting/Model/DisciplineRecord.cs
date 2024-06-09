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
                    DisciplineDataType.Number => 0d,
                    _ => throw new FormatException()
                };
            }

            return _value;
        }

        private set => _value = value;
    }

    private void UpdateValue()
    {
        Value = DisciplineInfo.DataType switch
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

    public double DoubleValue =>
        DisciplineInfo.DataType switch
        {
            DisciplineDataType.Time => ((TimeSpan)Value).TotalSeconds,
            DisciplineDataType.Number => (double)Value,
            _ => throw new FormatException()
        };

    public double Score => NormalizedValue();


    private double NormalizedValue()
    {
        var value = DoubleValue;
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
}