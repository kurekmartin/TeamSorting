using System.Globalization;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace TeamSorting.Controls;

public class TimeSpanPicker : TemplatedControl
{
    public static char Separator =>
        Convert.ToChar(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);

    public static readonly DirectProperty<TimeSpanPicker, TimeSpan> TimeSpanProperty =
        AvaloniaProperty.RegisterDirect<TimeSpanPicker, TimeSpan>(
            nameof(TimeSpan),
            o => o.TimeSpan,
            (o, v) => o.TimeSpan = v,
            TimeSpan.Zero,
            BindingMode.TwoWay);

    private TimeSpan _timeSpan;

    public TimeSpan TimeSpan
    {
        get => _timeSpan;
        set
        {
            if (_timeSpan == value)
            {
                return;
            }

            Hours = Math.Floor(value.TotalHours).ToString(CultureInfo.InvariantCulture);
            Minutes = value.Minutes.ToString();
            Seconds = value.Seconds.ToString();
            Milliseconds = (value.Milliseconds / 100).ToString();

            SetAndRaise(TimeSpanProperty, ref _timeSpan, value);
        }
    }

    private string? _hours;

    public string? Hours
    {
        get => _hours;
        set
        {
            if (_hours == value)
            {
                return;
            }

            _hours = value;
            UpdateValue();
        }
    }

    private string? _minutes;

    public string? Minutes
    {
        get => _minutes;
        set
        {
            if (_minutes == value)
            {
                return;
            }

            _minutes = value;
            UpdateValue();
        }
    }

    private string? _seconds;

    public string? Seconds
    {
        get => _seconds;
        set
        {
            if (_seconds == value)
            {
                return;
            }

            _seconds = value;
            UpdateValue();
        }
    }

    private string? _milliseconds;

    public string? Milliseconds
    {
        get => _milliseconds;
        set
        {
            if (_milliseconds == value)
            {
                return;
            }

            _milliseconds = value;
            UpdateValue();
        }
    }

    private void UpdateValue()
    {
        var newValue = new TimeSpan(
            days: 0,
            hours: int.Parse(Hours ?? "0"),
            minutes: int.Parse(Minutes ?? "0"),
            seconds: int.Parse(Seconds ?? "0"),
            milliseconds: int.Parse(Milliseconds ?? "0") * 100);
        TimeSpan = newValue;
    }
}