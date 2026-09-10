using Avalonia;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeamSorting.Controls;
using TeamSorting.Enums;
using TeamSorting.Models;
using TeamSorting.ViewModels;

namespace TeamSorting.Views;

public partial class TeamsView : UserControl
{
    private Point _ghostPosition = new(0, 0);
    private Point _mouseOffset;
    private TopLevel? _keyboardShortcutHost;
    private readonly ILogger<TeamsView>? _logger = Ioc.Default.GetService<ILogger<TeamsView>>();

    public TeamsView()
    {
        InitializeComponent();
        AddHandler(DragDrop.DragOverEvent, DragOver);
        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
    }

    private void TeamsView_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled || DataContext is not TeamsViewModel context || IsTextEditingSource(e.Source))
        {
            return;
        }

        KeyModifiers primaryModifier = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
        bool undo = e.Key == Key.Z && e.KeyModifiers == primaryModifier;
        bool redo;
        if (OperatingSystem.IsMacOS())
        {
            redo = e.Key == Key.Z && e.KeyModifiers == (primaryModifier | KeyModifiers.Shift);
        }
        else
        {
            redo = e.Key == Key.Y && e.KeyModifiers == primaryModifier
                   || e.Key == Key.Z && e.KeyModifiers == (primaryModifier | KeyModifiers.Shift);
        }

        if (undo && context.UndoCommand.CanExecute(null))
        {
            context.UndoCommand.Execute(null);
            e.Handled = true;
        }
        else if (redo && context.RedoCommand.CanExecute(null))
        {
            context.RedoCommand.Execute(null);
            e.Handled = true;
        }
    }

    private static bool IsTextEditingSource(object? source)
    {
        return source is TextBox
               || (source as Visual)?.GetVisualAncestors().OfType<TextBox>().Any() == true;
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        if (DataContext is not TeamsViewModel teamsViewModel)
        {
            return;
        }

        object? data = e.Data.Get(TeamsViewModel.MemberFormat);
        if (data is not Member member)
        {
            return;
        }

        teamsViewModel.IsValidDestination(member, e.Source as Control);
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        object? data = e.Data.Get(TeamsViewModel.MemberFormat);
        if (data is not Member member)
        {
            return;
        }

        if (DataContext is not TeamsViewModel teamsViewModel)
        {
            return;
        }

        teamsViewModel.Drop(member, e.Source as Control);
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        Point currentPosition = e.GetPosition(TeamViewContainer);
        UpdateGhostPosition(currentPosition);

        e.DragEffects = DragDropEffects.Move;
        if (DataContext is not TeamsViewModel teamsViewModel)
        {
            return;
        }

        object? data = e.Data.Get(TeamsViewModel.MemberFormat);
        if (data is not Member member)
        {
            return;
        }

        if (!teamsViewModel.IsValidDestination(member, e.Source as Control))
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private async void MemberCard_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not TeamsViewModel teamsViewModel
            || teamsViewModel.Teams.SortingInProgress)
        {
            return;
        }

        MemberCard? memberCard = e.Source as MemberCard
                                 ?? (e.Source as Visual)?.GetVisualAncestors().OfType<MemberCard>().FirstOrDefault();
        if (memberCard is null)
        {
            return;
        }

        PointerPoint point = e.GetCurrentPoint(memberCard);
        if (!point.Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (!memberCard.Member.AllowTeamChange)
        {
            return;
        }

        memberCard.Copy(GhostCard);
        _mouseOffset = e.GetPosition(memberCard);

        GhostCard.IsVisible = true;
        GhostCard.UpdateLayout(); //position is not updated when hidden

        Point ghostPos = GhostCard.Bounds.Position;
        _ghostPosition = new Point(ghostPos.X + _mouseOffset.X, ghostPos.Y + _mouseOffset.Y);

        Point mousePos = e.GetPosition(TeamViewContainer);
        UpdateGhostPosition(mousePos);

        teamsViewModel.StartDrag(memberCard);

        var dragData = new DataObject();
        dragData.Set(TeamsViewModel.MemberFormat, memberCard.Member);
        await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
        teamsViewModel.EndDrag();
        GhostCard.IsVisible = false;
    }

    private void UpdateGhostPosition(Point pointerPosition)
    {
        double offsetX = pointerPosition.X - _ghostPosition.X;
        double offsetY = pointerPosition.Y - _ghostPosition.Y;
        GhostCard.RenderTransform = new TranslateTransform(offsetX, offsetY);
    }

    protected override void OnInitialized()
    {
        if (DataContext is not TeamsViewModel context)
        {
            return;
        }

        if (context.NotificationManager is null)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is not null)
            {
                context.NotificationManager = new WindowNotificationManager(topLevel)
                {
                    Position = NotificationPosition.BottomRight,
                    Margin = new Thickness(0, 0, 0, 35)
                };
            }
        }

        ((INotifyCollectionChanged)context.Disciplines.DisciplineList).CollectionChanged += DisciplineListOnCollectionChanged;
        UpdateSortCriteria();
    }

    private void DisciplineListOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateSortCriteria();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        GhostCard.IsVisible = false;
        _keyboardShortcutHost = TopLevel.GetTopLevel(this);
        _keyboardShortcutHost?.AddHandler(KeyDownEvent, TeamsView_OnKeyDown, RoutingStrategies.Tunnel);
        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _keyboardShortcutHost?.RemoveHandler(KeyDownEvent, TeamsView_OnKeyDown);
        _keyboardShortcutHost = null;
        base.OnUnloaded(e);
    }

    private void UpdateSortCriteria()
    {
        if (DataContext is not TeamsViewModel context)
        {
            return;
        }

        var selected = SortCriteriaComboBox.SelectedValue as ComboBoxSortCriteria;
        var nameItem = new ComboBoxSortCriteria(Lang.Resources.InputView_DataGrid_ColumnHeader_Name, null);

        List<ComboBoxSortCriteria> items = [nameItem];
        items.AddRange(context.Disciplines.DisciplineList.Select(discipline =>
            new ComboBoxSortCriteria(discipline.Name, discipline)));

        List<ComboBoxSortCriteria> orderedItems = items.OrderBy(criteria => criteria.DisplayText).ToList();
        SortCriteriaComboBox.ItemsSource = orderedItems;
        if (selected is not null && orderedItems.FirstOrDefault(i => Equals(i.Value, selected.Value)) is { } existing)
        {
            SortCriteriaComboBox.SelectedValue = existing;
        }
        else if (context.TeamsSortCriteria.Discipline is not null && orderedItems.FirstOrDefault(i => Equals(i.Value, context.TeamsSortCriteria.Discipline)) is { } matching)
        {
            SortCriteriaComboBox.SelectedValue = matching;
        }
        else
        {
            SortCriteriaComboBox.SelectedValue = nameItem;
        }
    }

    private void Back_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this);
        if (window is MainWindow { DataContext: MainWindowViewModel mainWindowViewModel } mainWindow)
        {
            mainWindowViewModel.SwitchToInputView();
        }
    }

    private async void ExportTeamsToCsv_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TeamsViewModel context)
        {
            return;
        }

        IStorageProvider? storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storageProvider is null)
        {
            return;
        }

        IStorageFile? file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Lang.Resources.TeamsView_ExportTeamsToCsv_FileDialogTitle,
            FileTypeChoices =
            [
                new FilePickerFileType("csv") { Patterns = ["*.csv"] }
            ]
        });

        if (file is null)
        {
            return;
        }

        var fileSaved = false;
        try
        {
            context.CsvUtil.WriteTeamsToCsv(file.Path.LocalPath);
            fileSaved = true;
        }
        catch (Exception exception)
        {
            _logger?.LogError("Error writing teams to csv: {message}", exception.Message);
            string message = string.Format(Lang.Resources.TeamsView_CsvExport_Error, exception.Message);
            context.NotificationManager?.Show(message, NotificationType.Error);
        }

        if (fileSaved)
        {
            context.NotificationManager?.Show(Lang.Resources.TeamsView_CsvExport_Success, NotificationType.Success);
        }
    }

    private void ShowMemberDetailsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        IEnumerable<MemberCard> cards = this.GetVisualDescendants().OfType<MemberCard>();
        foreach (MemberCard card in cards)
        {
            card.ShowDetail = true;
        }
    }

    private void HideMemberDetailsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        IEnumerable<MemberCard> cards = this.GetVisualDescendants().OfType<MemberCard>();
        foreach (MemberCard card in cards)
        {
            card.ShowDetail = false;
        }
    }

    private void SortCriteriaComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is TeamsViewModel context && e.AddedItems.Count > 0 &&
            e.AddedItems[0] is ComboBoxSortCriteria item)
        {
            var newCriteria = new MemberSortCriteria((DisciplineInfo?)item.Value, context.TeamsSortCriteria.SortOrder);
            if (context.TeamsSortCriteria != newCriteria)
            {
                context.TeamsSortCriteria = newCriteria;
            }
        }
    }

    private void ToggleButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not IconRadioButton { IsChecked: true } || DataContext is not TeamsViewModel context)
        {
            return;
        }

        if (sender.Equals(SortAscRadioButton))
        {
            var newCriteria = new MemberSortCriteria(context.TeamsSortCriteria.Discipline, SortOrder.Asc);
            if (context.TeamsSortCriteria != newCriteria)
            {
                context.TeamsSortCriteria = newCriteria;
            }
        }
        else if (sender.Equals(SortDescRadioButton))
        {
            var newCriteria = new MemberSortCriteria(context.TeamsSortCriteria.Discipline, SortOrder.Desc);
            if (context.TeamsSortCriteria != newCriteria)
            {
                context.TeamsSortCriteria = newCriteria;
            }
        }
    }

    private void AddTeamButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TeamsViewModel context)
        {
            context.AddTeam();
        }
    }

    private void DeleteTeamButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not TeamControl { Team: { } team } || DataContext is not TeamsViewModel context)
        {
            return;
        }

        context.DeleteTeam(team);
    }

    private async void NewCombinationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TeamsViewModel context || sender is not Button button)
        {
            return;
        }

        var window = TopLevel.GetTopLevel(this);
        if (window is not MainWindow { DataContext: MainWindowViewModel } mainWindow)
        {
            return;
        }

        int currentNumberOfTeams = context.Teams.TeamList.Count;
        int targetNumberOfTeams = context.NumberOfTeams;
        if (currentNumberOfTeams > targetNumberOfTeams)
        {
            List<Team> teamsToDelete = context.Teams.TeamList.Skip(targetNumberOfTeams).ToList();
            string teamNamesToDelete = string.Join(", ", teamsToDelete.Select(team => team.Name));

            string message = string.Format(Lang.Resources.TeamsView_Sort_WarningDialog_Message, teamNamesToDelete);
            var dialog = new WarningDialog(
                message: message,
                confirmButtonText: Lang.Resources.TeamsView_Sort_WarningDialog_Delete,
                cancelButtonText: Lang.Resources.TeamsView_Sort_WarningDialog_Cancel);

            WarningDialogResult result = await mainWindow.ShowWarningDialog(dialog);

            if (result == WarningDialogResult.Cancel)
            {
                return;
            }

            context.ClearHistory();
            teamsToDelete.ForEach(team => context.Teams.RemoveTeam(team));
        }
        else
        {
            context.ClearHistory();
        }

        context.ShowUnsortedMembers = false;
        Cursor = new Cursor(StandardCursorType.Wait);
        button.IsEnabled = false;
        await context.Teams.SortToTeams(targetNumberOfTeams);
        button.IsEnabled = true;
        context.Teams.InputSeed = string.Empty;
        Cursor = Cursor.Default;
    }

    private void PinMembersButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TeamsViewModel context)
        {
            return;
        }

        context.Teams.LockCurrentMembers();
    }

    private void UnpinMembersButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TeamsViewModel context)
        {
            return;
        }

        context.Teams.UnlockCurrentMembers();
    }

    private void TextBox_TeamNameOnTextChanged(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TeamsViewModel teamsViewModel)
        {
            return;
        }

        teamsViewModel.Teams.ValidateTeamNames();
    }

    private async void DeleteAllTeamsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TeamsViewModel teamsViewModel)
        {
            return;
        }

        if (teamsViewModel.Teams.TeamList.Count == 0)
        {
            return;
        }

        var window = TopLevel.GetTopLevel(this);
        if (window is not MainWindow { DataContext: MainWindowViewModel } mainWindow)
        {
            return;
        }

        var dialog = new WarningDialog(
            message: Lang.Resources.TeamsView_DeleteAllTeams_WarningDialog_Message,
            confirmButtonText: Lang.Resources.TeamsView_Sort_WarningDialog_Delete,
            cancelButtonText: Lang.Resources.TeamsView_Sort_WarningDialog_Cancel);

        WarningDialogResult result = await mainWindow.ShowWarningDialog(dialog);

        if (result == WarningDialogResult.Cancel)
        {
            return;
        }

        teamsViewModel.ClearHistory();
        teamsViewModel.Teams.RemoveAllTeams();
    }

    private void HideUnsortedMembersButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TeamsViewModel teamsViewModel)
        {
            return;
        }

        teamsViewModel.ShowUnsortedMembers = false;
    }

    private void ShowUnsortedMembersButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TeamsViewModel teamsViewModel)
        {
            return;
        }

        teamsViewModel.ShowUnsortedMembers = true;
    }
}