using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using Serilog;
using TeamSorting.Controls;
using TeamSorting.Enums;
using TeamSorting.Models;
using TeamSorting.ViewModels;

namespace TeamSorting.Views;

public partial class TeamsView : UserControl
{
    private Point _ghostPosition = new(0, 0);
    private Point _mouseOffset = new(-5, -5);

    public TeamsView()
    {
        InitializeComponent();
        AddHandler(DragDrop.DragOverEvent, DragOver);
        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        if (DataContext is not TeamsViewModel teamsViewModel) return;
        object? data = e.Data.Get(TeamsViewModel.MemberFormat);
        if (data is not Member member) return;
        teamsViewModel.IsValidDestination(member, e.Source as Control);
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        Log.Debug("Drop");
        object? data = e.Data.Get(TeamsViewModel.MemberFormat);
        if (data is not Member member)
        {
            Log.Debug("No member dragged");
            return;
        }

        if (DataContext is not TeamsViewModel teamsViewModel) return;
        teamsViewModel.Drop(member, e.Source as Control);
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        Point currentPosition = e.GetPosition(TeamViewContainer);

        double offsetX = currentPosition.X - _ghostPosition.X;
        double offsetY = currentPosition.Y - _ghostPosition.Y;

        GhostCard.RenderTransform = new TranslateTransform(offsetX, offsetY);

        Log.Debug("DragOver {element}", e.Source?.GetType());
        e.DragEffects = DragDropEffects.Move;
        if (DataContext is not TeamsViewModel teamsViewModel) return;
        object? data = e.Data.Get(TeamsViewModel.MemberFormat);
        if (data is not Member member) return;
        if (!teamsViewModel.IsValidDestination(member, e.Source as Control))
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private async void MemberCard_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        PointerPoint point = e.GetCurrentPoint(control);
        if (point.Properties.IsRightButtonPressed)
        {
            return;
        }

        Log.Debug("MemberCard_OnPointerPressed");
        if (sender is not MemberCard memberCard) return;

        _mouseOffset = e.GetPosition(memberCard);
        memberCard.Copy(GhostCard);
        Point ghostPos = GhostCard.Bounds.Position;
        _ghostPosition = new Point(ghostPos.X + _mouseOffset.X, ghostPos.Y + _mouseOffset.Y);

        Point mousePos = e.GetPosition(TeamViewContainer);
        double offsetX = mousePos.X - ghostPos.X;
        double offsetY = mousePos.Y - ghostPos.Y + _mouseOffset.X;
        GhostCard.RenderTransform = new TranslateTransform(offsetX, offsetY);

        if (DataContext is not TeamsViewModel teamsViewModel) return;
        teamsViewModel.StartDrag(memberCard);

        GhostCard.IsVisible = true;

        var dragData = new DataObject();
        dragData.Set(TeamsViewModel.MemberFormat, memberCard.Member);
        DragDropEffects result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
        Log.Debug("DragDrop result: {result}", result);
        teamsViewModel.EndDrag();
        GhostCard.IsVisible = false;
    }

    protected override void OnInitialized()
    {
        if (DataContext is TeamsViewModel context)
        {
            context.NotificationManager = new WindowNotificationManager(TopLevel.GetTopLevel(this))
            {
                Position = NotificationPosition.BottomRight,
                Margin = new Thickness(0, 0, 0, 35)
            };

            var nameItem = new ComboBoxSortCriteria(Lang.Resources.InputView_DataGrid_ColumnHeader_Name, null);

            List<ComboBoxSortCriteria> items = [nameItem];
            items.AddRange(context.Data.Disciplines.Select(discipline =>
                new ComboBoxSortCriteria(discipline.Name, discipline)));

            SortCriteriaComboBox.ItemsSource = items.OrderBy(criteria => criteria.DisplayText).ToList();
            SortCriteriaComboBox.SelectedValue = nameItem;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        GhostCard.IsVisible = false;
        base.OnLoaded(e);
    }

    private async void Back_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this);
        if (window is MainWindow { DataContext: MainWindowViewModel mainWindowViewModel } mainWindow)
        {
            var dialog = new WarningDialog(
                message: Lang.Resources.TeamsView_Back_WarningDialog_Message,
                confirmButtonText: Lang.Resources.TeamsView_Back_WarningDialog_Delete,
                cancelButtonText: Lang.Resources.TeamsView_Back_WarningDialog_Cancel)
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
    }

    private async void ExportTeamsToCsv_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TeamsViewModel context) return;
        var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storageProvider is null)
        {
            return;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = Lang.Resources.TeamsView_ExportTeamsToCsv_FileDialogTitle,
            FileTypeChoices = new[]
            {
                new FilePickerFileType("csv") { Patterns = ["*.csv"] }
            }
        });

        if (file is null)
        {
            return;
        }

        var fileSaved = false;
        try
        {
            context.Data.WriteTeamsToCsv(file.Path.LocalPath);
            fileSaved = true;
        }
        catch (Exception exception)
        {
            string message = string.Format(Lang.Resources.TeamsView_CsvExport_Error, exception.Message);
            context.NotificationManager?.Show(message, NotificationType.Error);
        }

        if (fileSaved)
        {
            context.NotificationManager?.Show(Lang.Resources.TeamsView_CsvExport_Success, NotificationType.Success);
        }
    }

    private void MemberTeamMenu_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TeamsViewModel context) return;
        if (sender is not MenuItem menuItem) return;
        var memberCard = menuItem.FindLogicalAncestorOfType<MemberCard>();
        if (memberCard is { DataContext: Member member })
        {
            var team = context.Data.Teams.First(team =>
                string.Equals(team.Name, menuItem.Header as string, StringComparison.InvariantCultureIgnoreCase));
            member.MoveToTeam(team);
        }
    }

    private void ShowMemberDetailsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var cards = this.GetVisualDescendants().OfType<MemberCard>();
        foreach (var card in cards)
        {
            card.ShowDetail = true;
        }
    }

    private void HideMemberDetailsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var cards = this.GetVisualDescendants().OfType<MemberCard>();
        foreach (var card in cards)
        {
            card.ShowDetail = false;
        }
    }

    private void SortCriteriaComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is TeamsViewModel context && e.AddedItems.Count > 0 &&
            e.AddedItems[0] is ComboBoxSortCriteria item)
        {
            context.TeamsSortCriteria =
                new MemberSortCriteria((DisciplineInfo?)item.Value, context.TeamsSortCriteria.SortOrder);
        }
    }

    private void ToggleButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is IconRadioButton { IsChecked: true } && DataContext is TeamsViewModel context)
        {
            if (sender.Equals(SortAscRadioButton))
            {
                context.TeamsSortCriteria = new MemberSortCriteria(context.TeamsSortCriteria.Discipline, SortOrder.Asc);
            }
            else if (sender.Equals(SortDescRadioButton))
            {
                context.TeamsSortCriteria =
                    new MemberSortCriteria(context.TeamsSortCriteria.Discipline, SortOrder.Desc);
            }
        }
    }
}