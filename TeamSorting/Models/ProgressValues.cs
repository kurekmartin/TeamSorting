using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamSorting.Models;

public class ProgressValues : ObservableObject
{
    private bool _isIndeterminate = true;
    private double _value;
    private double _maximum;
    private double _minimum;
    private bool _showText = true;
    private string _text = string.Empty;

    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        set => SetProperty(ref _isIndeterminate, value);
    }

    public double Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public double Maximum
    {
        get => _maximum;
        set => SetProperty(ref _maximum, value);
    }

    public double Minimum
    {
        get => _minimum;
        set => SetProperty(ref _minimum, value);
    }

    public bool ShowText
    {
        get => _showText;
        set => SetProperty(ref _showText, value);
    }

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }
}