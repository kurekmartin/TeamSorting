using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Projektanker.Icons.Avalonia;
using TeamSorting.Models;
using TeamSorting.Sorting;
using TeamSorting.ViewModels;

namespace TeamSorting.Views;

public partial class InputView : UserControl
{
    public InputView()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        if (DataContext is not null)
        {
            AddDisciplinesToDataGrid();
        }

        base.OnInitialized();
    }

    private async void LoadData_OnClick(object? sender, RoutedEventArgs e)
    {
        var context = (InputViewModel)DataContext!;
        var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storageProvider is null)
        {
            return;
        }

        var file = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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
        AddDisciplinesToDataGrid();
    }

    private void AddDisciplinesToDataGrid()
    {
        var context = (InputViewModel)DataContext!;

        foreach (var discipline in context.Data.Disciplines)
        {
            if (DataGridContainsDisciplineColumn(DataGrid, discipline))
            {
                continue;
            }

            var column = new DataGridTextColumn
            {
                Tag = discipline.Id,
                Header = CreateDisciplineColumnHeader(discipline),
                Binding = new Binding($"{nameof(Member.Records)}[{discipline.Id}].{nameof(DisciplineRecord.RawValue)}"),
                IsReadOnly = false
            };
            DataGrid.Columns.Add(column);
        }
    }

    private bool DataGridContainsDisciplineColumn(DataGrid dataGrid, DisciplineInfo discipline)
    {
        var column = dataGrid.Columns.FirstOrDefault(column => column.Tag is Guid id && id == discipline.Id);
        return column is not null;
    }

    private static object CreateDisciplineColumnHeader(DisciplineInfo discipline)
    {
        var panel = new DockPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        var iconSort = new Icon
        {
            FontSize = 20,
            HorizontalAlignment = HorizontalAlignment.Right,
            [DockPanel.DockProperty] = Dock.Right
        };
        var iconSortBinding = new Binding
        {
            Source = discipline,
            Path = nameof(DisciplineInfo.SortType),
            Converter = new Converters.DisciplineSortToIconConverter()
        };
        iconSort.Bind(Icon.ValueProperty, iconSortBinding);
        panel.Children.Add(iconSort);

        var iconType = new Icon
        {
            FontSize = 20,
            HorizontalAlignment = HorizontalAlignment.Right,
            [DockPanel.DockProperty] = Dock.Right
        };
        var iconTypeBinding = new Binding
        {
            Source = discipline,
            Path = nameof(DisciplineInfo.DataType),
            Converter = new Converters.DisciplineTypeToIconConverter()
        };
        iconType.Bind(Icon.ValueProperty, iconTypeBinding);
        panel.Children.Add(iconType);

        var text = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(0, 0, 10, 0),
            [DockPanel.DockProperty] = Dock.Left
        };
        var textBinding = new Binding
        {
            Source = discipline,
            Path = nameof(DisciplineInfo.Name)
        };
        text.Bind(TextBlock.TextProperty, textBinding);
        panel.Children.Add(text);

        return panel;
    }

    private void SortToTeams_OnClick(object? sender, RoutedEventArgs e)
    {
        var context = (InputViewModel)DataContext!;
        var numberOfTeams = (int)(NumberOfTeams.Value ?? 1);
        context.Data.Teams = new ObservableCollection<Team>(
            EvolutionSorter.Sort(context.Data.Members.ToList(), numberOfTeams));
        //context.Data.CreateTeams(numberOfTeams);
        //ViewModels.TeamSorting.SortMembersIntoTeams(context.Data);

        var window = TopLevel.GetTopLevel(this);
        if (window is MainWindow { DataContext: MainWindowViewModel mainWindowViewModel })
        {
            mainWindowViewModel.SwitchToTeamsView();
        }
    }
}