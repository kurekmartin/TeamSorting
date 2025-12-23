using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using TeamSorting.Enums;
using TeamSorting.Models;
using TeamSorting.ViewModels;

namespace TeamSorting.Views;

public partial class InputView : UserControl
{
    public InputView()
    {
        InitializeComponent();
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
                var dialog = new CsvErrorDialog
                {
                    DataContext = new CsvErrorViewModel(loadDataErrors),
                    MaxHeight = mainWindow.Height * 0.9,
                    MaxWidth = mainWindow.Width * 0.9
                };
                await dialog.ShowDialog(mainWindow);
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
        if (DataContext is not InputViewModel context || string.IsNullOrWhiteSpace(context.NewMemberName))
        {
            return;
        }

        var member = new Member(context.NewMemberName);
        context.Members.AddMember(member);
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
        var discipline = new DisciplineInfo(context.NewDisciplineName)
        {
            DataType = disciplineType,
            SortOrder = disciplineSortOrder
        };
        context.Disciplines.AddDiscipline(discipline);
        //TODO error handling when discipline with same name exists 
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
        if (DataContext is InputViewModel context)
        {
            var window = TopLevel.GetTopLevel(this);
            if (window is MainWindow { DataContext: MainWindowViewModel mainWindowViewModel } mainWindow)
            {
                var dialog = new WarningDialog(
                    message: Lang.Resources.InputView_DeleteData_WarningDialog_Message,
                    confirmButtonText: Lang.Resources.InputView_DeleteData_WarningDialog_Delete,
                    cancelButtonText: Lang.Resources.InputView_DeleteData_WarningDialog_Cancel)
                {
                    Position = mainWindow.Position //fix for WindowStartupLocation="CenterOwner" not working
                };
                var result = await dialog.ShowDialog<WarningDialogResult>(mainWindow);
                if (result == WarningDialogResult.Cancel)
                {
                    return;
                }

                mainWindowViewModel.SwitchToInputView();
            }

            context.ClearData();
        }
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