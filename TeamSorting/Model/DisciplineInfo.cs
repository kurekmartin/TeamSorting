using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace TeamSorting.Model;

public class DisciplineInfo
{
    public DisciplineInfo()
    {
        TeamMembers.CollectionChanged += TeamMembersOnCollectionChanged;
    }

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

    private double _minValue = double.MaxValue;
    private double _maxValue = double.MinValue;

    public double MinValue
    {
        get => _minValue;
        private set
        {
            if (Math.Abs(_minValue - value) < 0.0001) return;
            _minValue = value;
            OnValueRangeChanged();
        }
    }

    public double MaxValue
    {
        get => _maxValue;
        private set
        {
            if (Math.Abs(_maxValue - value) < 0.0001) return;
            _maxValue = value;
            OnValueRangeChanged();
        }
    }

    public readonly ObservableCollection<TeamMember> TeamMembers = [];

    private void TeamMembersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                TeamMembersAdded(e.NewItems!);
                break;
            case NotifyCollectionChangedAction.Remove:
                TeamMembersRemoved(e.OldItems!);
                break;
        }
    }

    private void TeamMembersAdded(IList teamMembersAdded)
    {
        foreach (TeamMember teamMember in teamMembersAdded)
        {
            var disciplineValue = teamMember.Disciplines.FirstOrDefault(record => record.DisciplineInfo == this)
                ?.DoubleValue;
            if (disciplineValue < MinValue)
            {
                MinValue = (double)disciplineValue;
            }
            else if (disciplineValue > MaxValue)
            {
                MaxValue = (double)disciplineValue;
            }
        }
    }

    private void TeamMembersRemoved(IList teamMembersRemoved)
    {
        foreach (TeamMember teamMember in teamMembersRemoved)
        {
            var disciplineValue = teamMember.Disciplines.FirstOrDefault(record => record.DisciplineInfo == this)?.DoubleValue;
            if (disciplineValue is not null && Math.Abs((double)(disciplineValue - MinValue)) < 0.0001)
            {
                MinValue = GetDisciplineValues().Min();
            }
            else if (disciplineValue is not null && Math.Abs((double)(disciplineValue - MaxValue)) < 0.0001)
            {
                MaxValue = GetDisciplineValues().Max();
            }
        }
    }

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