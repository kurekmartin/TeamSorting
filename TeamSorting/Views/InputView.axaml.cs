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

    private async void SortToTeams_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not InputViewModel context || sender is not Button button) return;
        var window = TopLevel.GetTopLevel(this);
        if (window is not MainWindow { DataContext: MainWindowViewModel mainWindowViewModel } mainWindow)
        {
            return;
        }

        if (context.Teams.TeamList.Count > 0)
        {
            var dialog = new WarningDialog(
                message: Lang.Resources.InputView_Sort_WarningDialog_Message,
                confirmButtonText: Lang.Resources.InputView_Sort_WarningDialog_Delete,
                cancelButtonText: Lang.Resources.InputView_Sort_WarningDialog_Cancel);

            WarningDialogResult result = await mainWindow.ShowWarningDialog(dialog);

            if (result == WarningDialogResult.Cancel)
            {
                return;
            }
        }

        mainWindow.Cursor = new Cursor(StandardCursorType.Wait);

        button.IsEnabled = false;
        context.Teams.UnlockCurrentMembers();
        await context.Teams.SortToTeams((int)(NumberOfTeams.Value ?? 1));
        button.IsEnabled = true;

        mainWindowViewModel.SwitchToTeamsView();
        mainWindow.Cursor = Cursor.Default;
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
        var context = (InputViewModel)DataContext!;
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
        var context = (InputViewModel)DataContext!;
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
}