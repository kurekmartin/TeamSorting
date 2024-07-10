using System.Data;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using DynamicData;
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
        await ((MainWindowViewModel)DataContext).Data.LoadFromFile(streamReader);
        //AddDisciplinesToDataGrid();
        //CreateDataTable();
    }

    private void AddDisciplinesToDataGrid()
    {
        //TODO
        var context = (MainWindowViewModel)DataContext!;
        foreach (var discipline in context.Data.Disciplines)
        {
            var column = new DataGridTextColumn()
            {
                Header = discipline.Name,
                Binding = new Binding()
                {
                }
            };
            DataGrid.Columns.Add(column);
        }
    }

    private void CreateDataTable()
    {
        var context = (MainWindowViewModel)DataContext!;
        var members = context.Data.Members;

        var nameColumn = new StackPanel
        {
            Name = "Name",
            Orientation = Orientation.Vertical
        };
        InputGrid.Children.Add(nameColumn);

        nameColumn.Children.Add(new Label
        {
            Content = "Name",
            FontWeight = FontWeight.Bold
        });
        foreach (var member in members)
        {
            var text = new Label
            {
                Content = member.Name
            };
            nameColumn.Children.Add(text);
        }

        var withColumn = new StackPanel
        {
            Name = "With",
            Orientation = Orientation.Vertical
        };
        InputGrid.Children.Add(withColumn);

        withColumn.Children.Add(new Label
        {
            Content = "With",
            FontWeight = FontWeight.Bold
        });
        foreach (var member in members)
        {
            var text = new Label
            {
                Content = string.Join(", ", member.With)
            };
            withColumn.Children.Add(text);
        }

        var notWithColumn = new StackPanel
        {
            Name = "Not With",
            Orientation = Orientation.Vertical
        };
        InputGrid.Children.Add(notWithColumn);

        notWithColumn.Children.Add(new Label
        {
            Content = "Not With",
            FontWeight = FontWeight.Bold
        });
        foreach (var member in members)
        {
            var text = new Label
            {
                Content = string.Join(", ", member.NotWith)
            };
            notWithColumn.Children.Add(text);
        }

        foreach (var discipline in context.Data.Disciplines)
        {
            var column = new StackPanel
            {
                Name = discipline.Name,
                Orientation = Orientation.Vertical
            };
            InputGrid.Children.Add(column);

            var disciplineHeader = new Label
            {
                Content = discipline.Name,
                FontWeight = FontWeight.Bold
            };
            column.Children.Add(disciplineHeader);

            foreach (var member in members)
            {
                var text = new Label()
                {
                    Content = context.Data.GetMemberDisciplineRecord(member, discipline).RawValue
                };
                column.Children.Add(text);
            }
        }
    }
}