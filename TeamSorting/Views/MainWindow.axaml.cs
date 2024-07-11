using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using TeamSorting.Models;
using TeamSorting.ViewModels;

namespace TeamSorting.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void LoadData_OnClick(object? sender, RoutedEventArgs e)
    {
        var context = (MainWindowViewModel)DataContext!;
        var file = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Choose file to load",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("csv") { Patterns = ["*.csv"] }
            }
        });

        if (file.Count <= 0) return;
        await using var stream = await file[0].OpenReadAsync();
        using var streamReader = new StreamReader(stream);
        await context.Data.LoadFromFile(streamReader);
        context.LoadGridData();
        AddDisciplinesToDataGrid();
    }

    private void AddDisciplinesToDataGrid()
    {
        var context = (MainWindowViewModel)DataContext!;

        var i = 0;
        foreach (var discipline in context.Data.Disciplines)
        {
            var column = new DataGridTextColumn()
            {
                Header = discipline.Name,
                Binding = new Binding()
                {
                    Path = $"{nameof(MemberWithDisciplines.Records)}[{i}].{nameof(DisciplineRecord.RawValue)}"
                }
            };
            DataGrid.Columns.Add(column);
            i++;
        }
    }
}