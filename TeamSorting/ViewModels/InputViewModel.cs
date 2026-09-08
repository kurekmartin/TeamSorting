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
using Microsoft.Extensions.Logging;
using Projektanker.Icons.Avalonia;
using TeamSorting.Controls;
using TeamSorting.Converters;
using TeamSorting.Lang;
using TeamSorting.Utils;

namespace TeamSorting.ViewModels;

public class InputViewModel : ViewModelBase, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = new();
    public bool HasErrors => _errors.Count != 0;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return _errors.Values.SelectMany(errorList => errorList);
        }

        return _errors.TryGetValue(propertyName, out List<string>? errors) ? errors : Enumerable.Empty<string>();
    }

    private readonly ILogger<InputViewModel> _logger;
    public Disciplines Disciplines { get; }
    public Members Members { get; }
    public Teams Teams { get; }
    public CsvUtil CsvUtil { get; }
    public FlatTreeDataGridSource<Member> TreeDataGridSource { get; }
    private string _newMemberName = string.Empty;

    public AddMode AddMode
    {
        get => _addMode;
        set
        {
            if (!SetProperty(ref _addMode, value))
            {
                return;
            }

            OnPropertyChanged(nameof(ShowAddMemberToolbar));
            OnPropertyChanged(nameof(ShowAddDisciplineToolbar));
        }
    }

    public bool MembersEmpty => Members.MemberList.Count == 0;
    public bool CanShowTeams => !Members.HasConstraintErrors;

    public bool ShowAddMemberToolbar => AddMode == AddMode.Member;
    public bool ShowAddDisciplineToolbar => AddMode == AddMode.Discipline;

    public string NewMemberName
    {
        get => _newMemberName;
        set
        {
            if (SetProperty(ref _newMemberName, value))
            {
                ValidateNewMemberName();
            }
        }
    }

    public bool IsNewMemberNameDuplicate => Members.MemberList.Any(member =>
        string.Equals(member.Name.Trim(), NewMemberName.Trim(), StringComparison.Ordinal) && NewMemberName.Trim().Length > 0);

    private void ValidateNewMemberName()
    {
        const string property = nameof(NewMemberName);
        _errors.Remove(property);
        if (IsNewMemberNameDuplicate)
        {
            _errors[property] = [Resources.InputView_Member_DuplicateName_Error];
        }

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(property));
        OnPropertyChanged(nameof(HasErrors));
        OnPropertyChanged(nameof(IsNewMemberNameDuplicate));
        OnPropertyChanged(nameof(CanAddMember));
    }

    public bool CanAddMember => !IsNewMemberNameDuplicate;

    private string _newDisciplineName = string.Empty;
    private AddMode _addMode = AddMode.None;

    public string NewDisciplineName
    {
        get => _newDisciplineName;
        set
        {
            if (SetProperty(ref _newDisciplineName, value))
            {
                ValidateNewDisciplineName();
            }
        }
    }

    public bool IsNewDisciplineNameDuplicate => Disciplines.DisciplineList.Any(d =>
        string.Equals(d.Name.Trim(), NewDisciplineName.Trim(), StringComparison.Ordinal) && NewDisciplineName.Trim().Length > 0);

    private void ValidateNewDisciplineName()
    {
        const string property = nameof(NewDisciplineName);
        _errors.Remove(property);
        if (IsNewDisciplineNameDuplicate)
        {
            _errors[property] = [Resources.InputView_DuplicateDiscipline_Error];
        }

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(property));
        OnPropertyChanged(nameof(HasErrors));
        OnPropertyChanged(nameof(IsNewDisciplineNameDuplicate));
        OnPropertyChanged(nameof(CanAddDiscipline));
    }

    public bool CanAddDiscipline => !IsNewDisciplineNameDuplicate;

    public static Array DisciplineDataTypes => Enum.GetValues(typeof(DisciplineDataType));
    public static Array SortOrder => Enum.GetValues(typeof(SortOrder));

    public InputViewModel(ILogger<InputViewModel> logger, Disciplines disciplines, Members members, Teams teams, CsvUtil csvUtil)
    {
        _logger = logger;
        Disciplines = disciplines;
        Members = members;
        Teams = teams;
        CsvUtil = csvUtil;
        ((INotifyCollectionChanged)Disciplines.DisciplineList).CollectionChanged += DisciplinesOnCollectionChanged;
        ((INotifyCollectionChanged)Members.MemberList).CollectionChanged += MembersOnCollectionChanged;
        Members.PropertyChanged += MembersOnPropertyChanged;
        TreeDataGridSource = new FlatTreeDataGridSource<Member>(Members.MemberList)
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
                        BeginEditGestures = BeginEditGestures.Tap,
                        CompareAscending = (member, member1) => string.Compare(member?.Name, member1?.Name, StringComparison.Ordinal),
                        CompareDescending = (member, member1) => string.Compare(member1?.Name, member?.Name, StringComparison.Ordinal),
                        CanUserSortColumn = true
                    }),
                new TemplateColumn<Member>(Resources.InputView_DataGrid_ColumnHeader_With, "WithCell", null, GridLength.Auto),
                new TemplateColumn<Member>(Resources.InputView_DataGrid_ColumnHeader_NotWith, "NotWithCell", null, GridLength.Auto)
            }
        };
    }

    private void MembersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ValidateNewMemberName();
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Reset:
                OnPropertyChanged(nameof(MembersEmpty));
                break;
        }
    }

    private void MembersOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Members.HasConstraintErrors):
                OnPropertyChanged(nameof(CanShowTeams));
                break;
            case nameof(Members.SortedMembers):
                ValidateNewMemberName();
                break;
        }
    }

    private void DisciplinesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ValidateNewDisciplineName();
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
                AddDisciplineColumns(Disciplines.DisciplineList);
                break;
        }
    }

    private void RemoveDisciplineColumns()
    {
        _logger.LogInformation("Removing all discipline columns");
        int columnCount = TreeDataGridSource.Columns.Count;
        var columns = new IColumn<Member>[columnCount];
        TreeDataGridSource.Columns.CopyTo(columns, 0);
        foreach (IColumn<Member> column in columns)
        {
            if (column.Tag is string tag &&
                tag.StartsWith(Constants.DisciplineColumnTagPrefix, StringComparison.Ordinal))
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

        _logger.LogInformation("Adding discipline column {ColumnTag}", column.Tag);
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
        return $"{Constants.DisciplineColumnTagPrefix}{discipline.Id}";
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

        _logger.LogInformation("Removing discipline column {ColumnTag}", disciplineColumn.Tag);
        TreeDataGridSource.Columns.Remove(disciplineColumn);
    }

    private StackPanel CreateDisciplineColumnHeader(DisciplineInfo discipline)
    {
        var panel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Tag = discipline.Id
        };


        var removeButton = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            [Attached.IconProperty] = "mdi-close",
            [ToolTip.TipProperty] = Resources.InputView_RemoveDiscipline_Button
        };
        removeButton.Click += RemoveDiscipline_Button_OnClick;
        panel.Children.Add(removeButton);


        var text = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 5, 0)
        };
        var textBinding = new Binding
        {
            Source = discipline,
            Path = nameof(DisciplineInfo.Name)
        };
        text.Bind(TextBlock.TextProperty, textBinding);
        panel.Children.Add(text);

        var iconPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 2
        };
        panel.Children.Add(iconPanel);

        var iconType = new Icon
        {
            FontSize = 20,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        var iconTypeBinding = new Binding
        {
            Source = discipline,
            Path = nameof(DisciplineInfo.DataType),
            Converter = new DisciplineTypeToIconConverter()
        };
        iconType.Bind(Icon.ValueProperty, iconTypeBinding);
        iconPanel.Children.Add(iconType);

        var iconSort = new Icon
        {
            FontSize = 20,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        var iconSortBinding = new Binding
        {
            Source = discipline,
            Path = nameof(DisciplineInfo.SortOrder),
            Converter = new DisciplineSortToIconConverter()
        };
        iconSort.Bind(Icon.ValueProperty, iconSortBinding);
        iconPanel.Children.Add(iconSort);

        var iconPriority = new Icon
        {
            FontSize = 20,
            Value = "mdi-priority-high",
            Margin = new Thickness(5, 0, 0, 0),
        };
        var priorityField = new NumericUpDown
        {
            AllowSpin = true,
            ShowButtonSpinner = false,
            Minimum = DisciplineInfo.PriorityMin,
            Maximum = DisciplineInfo.PriorityMax,
            FormatString = "0",
            MinWidth = 40,
            [ToolTip.TipProperty] = Resources.DisciplineInfo_Priority,
            InnerLeftContent = iconPriority
        };
        var priorityBinding = new Binding
        {
            Source = discipline,
            Path = nameof(DisciplineInfo.Priority)
        };
        priorityField.Bind(NumericUpDown.ValueProperty, priorityBinding);
        panel.Children.Add(priorityField);

        return panel;
    }

    private void RemoveDiscipline_Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        var panel = button.FindLogicalAncestorOfType<StackPanel>();
        if (panel is not { DataContext: InputViewModel, Tag: Guid disciplineId })
        {
            return;
        }

        DisciplineInfo? discipline = Disciplines.GetDisciplineById(disciplineId);
        if (discipline is not null)
        {
            Disciplines.RemoveDiscipline(discipline);
        }
    }

    public void ClearData()
    {
        Members.RemoveAllMembers();
        Disciplines.RemoveAllDisciplines();
        Teams.RemoveAllTeams();
    }
}
