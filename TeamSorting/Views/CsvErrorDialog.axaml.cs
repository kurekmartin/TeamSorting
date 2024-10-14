using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TeamSorting.Views;

public partial class CsvErrorDialog : Window
{
    public CsvErrorDialog()
    {
        InitializeComponent();
    }

    private void OkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}