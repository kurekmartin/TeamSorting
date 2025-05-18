using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using TeamSorting.Enums;
using TeamSorting.Models;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Projektanker.Icons.Avalonia;
using TeamSorting.Controls;
using TeamSorting.Converters;
using TeamSorting.Lang;

namespace TeamSorting.ViewModels;

public class InputViewModel : ViewModelBase
{
    public Data Data { get; }
    public FlatTreeDataGridSource<Member> TreeDataGridSource { get; }
    public int NumberOfTeams { get; set; } = 2;
    private string _newMemberName = string.Empty;

    public string NewMemberName
    {
        get => _newMemberName;
        set => SetProperty(ref _newMemberName, value);
    }

    private string _newDisciplineName = string.Empty;

    public string NewDisciplineName
    {
        get => _newDisciplineName;
        set => SetProperty(ref _newDisciplineName, value);
    }

    public static Array DisciplineDataTypes => Enum.GetValues(typeof(DisciplineDataType));
    public static Array SortOrder => Enum.GetValues(typeof(SortOrder));

    public InputViewModel(Data data)
    {
        Data = data;
        data.Disciplines.CollectionChanged += DisciplinesOnCollectionChanged;
        TreeDataGridSource = new FlatTreeDataGridSource<Member>(data.Members)
        {
            Columns =
            {
                new TemplateColumn<Member>(null,
                    "RemoveMemberCell",
                    null,
                    GridLength.Auto,
                    new TemplateColumnOptions<Member>
                    {
                        CanUserResizeColumn = false
                    }),
                new TemplateColumn<Member>(Resources.InputView_DataGrid_ColumnHeader_Name,
                    "MemberNameCell",
                    "MemberNameCellEdit",
                    GridLength.Auto,
                    new TemplateColumnOptions<Member>
                    {
                        BeginEditGestures = BeginEditGestures.Tap
                    }),
                new TemplateColumn<Member>(Resources.InputView_DataGrid_ColumnHeader_With, "WithCell", null, GridLength.Auto),
                new TemplateColumn<Member>(Resources.InputView_DataGrid_ColumnHeader_NotWith, "NotWithCell", null, GridLength.Auto)
            }
        };
    }

    private void DisciplinesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                AddDisciplineColumns(e.NewItems);
                RemoveDisciplineColumns(e.OldItems);
                break;
            case NotifyCollectionChangedAction.Reset:
                RemoveDisciplineColumns();
                AddDisciplineColumns(Data.Disciplines);
                break;
        }
    }

    private void RemoveDisciplineColumns()
    {
        int columnCount = TreeDataGridSource.Columns.Count;
        var columns = new IColumn<Member>[columnCount];
        TreeDataGridSource.Columns.CopyTo(columns,0);
        foreach (IColumn<Member> column in columns)
        {
            if (column.Tag is string tag && tag.StartsWith("Discipline-"))
            {
                TreeDataGridSource.Columns.Remove(column);
            }
        }
    }

    private void AddDisciplineColumns(IList? disciplines)
    {
        if (disciplines is null)
        {
            return;
        }

        foreach (object disciplineObject in disciplines)
        {
            if (disciplineObject is DisciplineInfo discipline)
            {
                AddDisciplineColumn(discipline);
            }
        }
    }

    private void AddDisciplineColumn(DisciplineInfo discipline)
    {
        var column = new TemplateColumn<Member>(
            CreateDisciplineColumnHeader(discipline),
            GetDisciplineCellTemplate(discipline),
            GetDisciplineEditTemplate(discipline),
            GridLength.Auto,
            new TemplateColumnOptions<Member>
            {
                CanUserSortColumn = true,
                BeginEditGestures = BeginEditGestures.Tap,
                CompareAscending = (member, member1) => Member.CompareDisciplinesAscending(member, member1, discipline),
                CompareDescending = (member, member1) => Member.CompareDisciplinesDescending(member, member1, discipline)
            })
        {
            Tag = CreateDisciplineColumnTag(discipline)
        };

        TreeDataGridSource.Columns.Add(column);
    }

    private static FuncDataTemplate<Member?> GetDisciplineCellTemplate(DisciplineInfo discipline)
    {
        return new FuncDataTemplate<Member?>((member, _) =>
            new TextBlock
            {
                Text = (string)(new DisciplineRecordValueConverter().Convert(member?.Records.GetValueOrDefault(discipline.Id)?.Value, typeof(string), null, CultureInfo.CurrentCulture) ?? string.Empty),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(4, 2),
                TextAlignment = TextAlignment.Right,
            }
        );
    }

    private static FuncDataTemplate<Member> GetDisciplineEditTemplate(DisciplineInfo discipline)
    {
        FuncDataTemplate<Member> template = discipline.DataType switch
        {
            DisciplineDataType.Number => new FuncDataTemplate<Member>((_, _) => new NumericUpDown
            {
                [!NumericUpDown.ValueProperty] = new Binding($"{nameof(Member.Records)}[{discipline.Id}].{nameof(DisciplineRecord.Value)}"),
                FormatString = "0.0",
                Increment = 1,
                BorderBrush = Brushes.Transparent,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                ShowButtonSpinner = false
            }),
            DisciplineDataType.Time => new FuncDataTemplate<Member>((_, _) => new TimeSpanPicker
            {
                [!TimeSpanPicker.TimeSpanProperty] = new Binding($"{nameof(Member.Records)}[{discipline.Id}].{nameof(DisciplineRecord.Value)}")
            }),
            _ => throw new FormatException($"Invalid data type {discipline.DataType}")
        };

        return template;
    }

    private static string CreateDisciplineColumnTag(DisciplineInfo discipline)
    {
        return $"Discipline-{discipline.Id}";
    }

    private void RemoveDisciplineColumns(IList? disciplines)
    {
        if (disciplines is null)
        {
            return;
        }

        foreach (object disciplineObject in disciplines)
        {
            if (disciplineObject is DisciplineInfo discipline)
            {
                RemoveDisciplineColumn(discipline);
            }
        }
    }

    private void RemoveDisciplineColumn(DisciplineInfo discipline)
    {
        string columnTag = CreateDisciplineColumnTag(discipline);
        IColumn<Member>? disciplineColumn = null;
        foreach (IColumn<Member> column in TreeDataGridSource.Columns)
        {
            if (column.Tag is string tag && tag == columnTag)
            {
                disciplineColumn = column;
                break;
            }
        }

        if (disciplineColumn is null)
        {
            return;
        }

        TreeDataGridSource.Columns.Remove(disciplineColumn);
    }

    private static DockPanel CreateDisciplineColumnHeader(DisciplineInfo discipline)
    {
        var panel = new DockPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Tag = discipline.Id
        };

        var removeButton = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            [DockPanel.DockProperty] = Dock.Left,
            [Attached.IconProperty] = "mdi-close",
            Margin = new Thickness(0, 0, 5, 0),
            [ToolTip.TipProperty] = Lang.Resources.InputView_RemoveDiscipline_Button
        };
        removeButton.Click += RemoveDiscipline_Button_OnClick;
        panel.Children.Add(removeButton);

        var priorityField = new NumericUpDown
        {
            AllowSpin = true,
            ShowButtonSpinner = false,
            [DockPanel.DockProperty] = Dock.Right,
            Minimum = DisciplineInfo.PriorityMin,
            Maximum = DisciplineInfo.PriorityMax,
            FormatString = "0",
            HorizontalContentAlignment = HorizontalAlignment.Right,
            MinWidth = 40,
            [ToolTip.TipProperty] = Resources.DisciplineInfo_Priority
        };
        var priorityBinding = new Binding
        {
            Source = discipline,
            Path = nameof(DisciplineInfo.Priority)
        };
        priorityField.Bind(NumericUpDown.ValueProperty, priorityBinding);
        panel.Children.Add(priorityField);

        var iconSort = new Icon
        {
            FontSize = 20,
            HorizontalAlignment = HorizontalAlignment.Right,
            [DockPanel.DockProperty] = Dock.Right
        };
        var iconSortBinding = new Binding
        {
            Source = discipline,
            Path = nameof(DisciplineInfo.SortOrder),
            Converter = new DisciplineSortToIconConverter()
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

    private static void RemoveDiscipline_Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        var panel = button.FindLogicalAncestorOfType<DockPanel>();
        if (panel is not { DataContext: InputViewModel context, Tag: Guid disciplineId })
        {
            return;
        }

        DisciplineInfo? discipline = context.Data.GetDisciplineById(disciplineId);
        if (discipline is not null)
        {
            context.Data.RemoveDiscipline(discipline);
        }
    }
}