using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using TeamSorting.Controls;
using TeamSorting.Enums;
using TeamSorting.Extensions;
using TeamSorting.Models;
using TeamSorting.ViewModels;

namespace TeamSorting.Views;

public partial class InputView : UserControl
{
    public InputView()
    {
        InitializeComponent();
        Members.ElementFactory = new InputTreeDataGridElementFactory(Cell_OnEditStarted);
        Members.AddHandler(KeyDownEvent, Members_OnKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    private void Cell_OnEditStarted(NavigableTreeDataGridTemplateCell cell)
    {
        if (!IsDisciplineColumn(cell.ColumnIndex))
        {
            return;
        }

        Dispatcher.UIThread.Post(() => FocusEditor(cell, focusLastTimePart: false), DispatcherPriority.Input);
    }

    private void Members_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!TryGetActiveDisciplineEditor(e, out NumericUpDown? editor, out NavigableTreeDataGridTemplateCell? currentCell))
        {
            return;
        }

        e.Handled = true;

        if (e.Key == Key.Enter)
        {
            NavigateToCell(currentCell, currentCell.ColumnIndex, currentCell.RowIndex + 1, focusLastTimePart: false);
            return;
        }

        int direction = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? -1 : 1;
        var timePicker = editor.FindVisualAncestorOrSelf<TimeSpanPicker>();
        if (timePicker is not null)
        {
            int partIndex = timePicker.GetPartIndex(editor);
            int targetPartIndex = partIndex + direction;
            if (targetPartIndex >= 0 && targetPartIndex < timePicker.PartCount)
            {
                timePicker.FocusPart(targetPartIndex);
                return;
            }
        }

        int targetColumn = FindDisciplineColumn(currentCell.ColumnIndex, direction);
        if (targetColumn >= 0)
        {
            NavigateToCell(
                currentCell,
                targetColumn,
                currentCell.RowIndex,
                focusLastTimePart: direction < 0);
        }
    }

    private bool TryGetActiveDisciplineEditor(
        KeyEventArgs e,
        [NotNullWhen(true)] out NumericUpDown? editor,
        [NotNullWhen(true)] out NavigableTreeDataGridTemplateCell? cell)
    {
        editor = (e.Source as Control).FindVisualAncestorOrSelf<NumericUpDown>();
        cell = null;

        if (e.Key is not (Key.Enter or Key.Tab) ||
            editor is null ||
            !Members.TryGetCell(editor, out TreeDataGridCell? gridCell) ||
            gridCell is not NavigableTreeDataGridTemplateCell { IsEditing: true } editableCell ||
            !IsDisciplineColumn(editableCell.ColumnIndex))
        {
            return false;
        }

        cell = editableCell;
        return true;
    }

    private void NavigateToCell(
        NavigableTreeDataGridTemplateCell currentCell,
        int columnIndex,
        int rowIndex,
        bool focusLastTimePart)
    {
        if (Members.Rows is null || rowIndex < 0 || rowIndex >= Members.Rows.Count)
        {
            return;
        }

        TreeDataGridRow? row = Members.TryGetRow(rowIndex) ?? Members.RowsPresenter?.BringIntoView(rowIndex) as TreeDataGridRow;
        row?.ApplyTemplate();
        row?.UpdateLayout();

        NavigableTreeDataGridTemplateCell? targetCell = row?.TryGetCell(columnIndex) as NavigableTreeDataGridTemplateCell ??
                                                        row?.CellsPresenter?.BringIntoView(columnIndex) as NavigableTreeDataGridTemplateCell;
        if (targetCell is null)
        {
            return;
        }

        currentCell.CommitEdit();
        targetCell.Focus();
        targetCell.StartEdit();

        Dispatcher.UIThread.Post(() => FocusEditor(targetCell, focusLastTimePart), DispatcherPriority.Input);
    }

    private static void FocusEditor(NavigableTreeDataGridTemplateCell cell, bool focusLastTimePart)
    {
        if (!cell.IsEditing)
        {
            return;
        }

        cell.ApplyTemplate();
        cell.UpdateLayout();

        TimeSpanPicker? timePicker = cell.GetVisualDescendants().OfType<TimeSpanPicker>().FirstOrDefault();
        if (timePicker is not null)
        {
            timePicker.ApplyTemplate();
            int partIndex = focusLastTimePart ? timePicker.PartCount - 1 : 0;
            timePicker.FocusPart(partIndex);
            return;
        }

        cell.GetVisualDescendants().OfType<NumericUpDown>().FirstOrDefault()?.FocusTextEditor();
    }

    private bool IsDisciplineColumn(int columnIndex)
    {
        IColumns? columns = Members.Columns;
        return columns is not null &&
               columnIndex >= 0 &&
               columnIndex < columns.Count &&
               columns[columnIndex].Tag is string tag &&
               tag.StartsWith(Constants.DisciplineColumnTagPrefix, StringComparison.Ordinal);
    }

    private int FindDisciplineColumn(int currentColumn, int direction)
    {
        IColumns? columns = Members.Columns;
        if (columns is null)
        {
            return -1;
        }

        for (int column = currentColumn + direction;
             column >= 0 && column < columns.Count;
             column += direction)
        {
            if (IsDisciplineColumn(column))
            {
                return column;
            }
        }

        return -1;
    }

    [Localizable(false)]
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
            Title = Lang.Resources.InputView_LoadData_FileDialog,
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("csv") { Patterns = ["*.csv"] }
            ]
        });

        if (file.Count <= 0) return;
        await using var stream = await file[0].OpenReadAsync();
        using var streamReader = new StreamReader(stream);
        var loadDataErrors = context.CsvUtil.LoadFromFile(streamReader);
        if (loadDataErrors.Count != 0)
        {
            var window = TopLevel.GetTopLevel(this);
            if (window is MainWindow { DataContext: MainWindowViewModel } mainWindow)
            {
                await mainWindow.ShowCsvErrorDialogAsync(new CsvErrorViewModel(loadDataErrors));
            }
        }
    }

    private void ShowTeamsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this);
        if (window is MainWindow { DataContext: MainWindowViewModel mainWindowViewModel })
        {
            mainWindowViewModel.SwitchToTeamsView();
        }
    }

    private void NewMemberTextBox_OnClick(object? sender, RoutedEventArgs e)
    {
        AddMember();
    }

    private void NewMemberTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddMember();
        }
    }

    private void AddMember()
    {
        if (DataContext is not InputViewModel context ||
            string.IsNullOrWhiteSpace(context.NewMemberName) ||
            !context.CanAddMember)
        {
            return;
        }

        var member = new Member(context.NewMemberName);
        if (!context.Members.AddMember(member))
        {
            return;
        }

        context.NewMemberName = string.Empty;

        var members = this.FindControl<TreeDataGrid>("Members");
        if (members?.Rows is null)
        {
            return;
        }

        var index = 0;
        foreach (IRow row in members.Rows)
        {
            if (row.Model is Member rowMember && rowMember == member)
            {
                break;
            }

            index++;
        }

        members.RowsPresenter!.BringIntoView(index);
        members.TryGetRow(index)?.Focus();
    }

    private void RemoveMemberButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var gridRow = button.FindLogicalAncestorOfType<TreeDataGridRow>();
            if (gridRow?.DataContext is Member member)
            {
                var context = (InputViewModel)DataContext!;
                context.Members.RemoveMember(member);
            }
        }
    }

    private void AddDisciplineTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddDiscipline();
        }
    }

    private void AddDisciplineButton_OnClick(object? sender, RoutedEventArgs e)
    {
        AddDiscipline();
    }

    private void AddDiscipline()
    {
        if (DataContext is not InputViewModel context || string.IsNullOrWhiteSpace(context.NewDisciplineName))
        {
            return;
        }

        var disciplineType = (DisciplineDataType)(DisciplineTypeComboBox.SelectedItem ?? DisciplineDataType.Number);
        var disciplineSortOrder = (SortOrder)(DisciplineSortOrderComboBox.SelectionBoxItem ?? SortOrder.Asc);
        var discipline = new DisciplineInfo(context.NewDisciplineName.Trim())
        {
            DataType = disciplineType,
            SortOrder = disciplineSortOrder
        };
        if (!context.Disciplines.AddDiscipline(discipline))
        {
            return;
        }
        context.NewDisciplineName = string.Empty;
        if (context.Members.MemberList.Count > 0)
        {
            //TODO scroll even if members are empty
            // MemberGrid.ScrollIntoView(null, MemberGrid.Columns.Last());
        }
    }

    private void ComboBox_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is ComboBox comboBox)
        {
            comboBox.SelectedIndex = 0;
        }
    }

    private async void DeleteDataButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not InputViewModel context)
        {
            return;
        }

        var window = TopLevel.GetTopLevel(this);
        if (window is MainWindow { DataContext: MainWindowViewModel mainWindowViewModel } mainWindow)
        {
            WarningDialogResult result = await mainWindow.ShowWarningDialogAsync(
                message: Lang.Resources.InputView_DeleteData_WarningDialog_Message,
                confirmButtonText: Lang.Resources.InputView_DeleteData_WarningDialog_Delete,
                cancelButtonText: Lang.Resources.InputView_DeleteData_WarningDialog_Cancel);
            if (result == WarningDialogResult.Cancel)
            {
                return;
            }

            mainWindowViewModel.SwitchToInputView();
        }

        context.ClearData();
    }

    private void Members_OnSelectionChanging(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
    }

    private void AddMemberMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not InputViewModel context)
        {
            return;
        }

        context.NewMemberName = string.Empty;
        context.AddMode = AddMode.Member;
    }

    private void AddDisciplineMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not InputViewModel context)
        {
            return;
        }

        context.NewDisciplineName = string.Empty;
        context.AddMode = AddMode.Discipline;
    }

    private void AddToolbarCloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is InputViewModel context)
        {
            context.AddMode = AddMode.None;
        }
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && DataContext is InputViewModel context)
        {
            context.AddMode = AddMode.None;
        }
    }
}
