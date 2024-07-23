using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using TeamSorting.Models;
using TeamSorting.ViewModels;

namespace TeamSorting.Views;

public partial class TeamsView : UserControl
{
    public TeamsView()
    {
        InitializeComponent();
    }

    private void Back_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this);
        if (window is MainWindow { DataContext: MainWindowViewModel mainWindowViewModel })
        {
            mainWindowViewModel.SwitchToInputView();
        }
    }

    private async void ExportTeamsToCsv_OnClick(object? sender, RoutedEventArgs e)
    {
        var context = (TeamsViewModel)DataContext!;
        var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storageProvider is null)
        {
            return;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Export Teams to CSV",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("csv") { Patterns = ["*.csv"] }
            }
        });

        if (file is null)
        {
            return;
        }

        context.Data.WriteTeamsToCsv(file.Path.LocalPath);
    }

    private void MemberTeamMenu_OnClick(object? sender, RoutedEventArgs e)
    {
        var context = (TeamsViewModel)DataContext!;
        if (sender is not MenuItem menuItem) return;
        var memberCard = menuItem.FindLogicalAncestorOfType<Border>();
        if (memberCard is { Name: "Member", DataContext: Member })
        {
            var member = (Member)memberCard.DataContext;
            var team = context.Data.Teams.First(team =>
                string.Equals(team.Name, menuItem.Header as string, StringComparison.InvariantCultureIgnoreCase));
            member.MoveToTeam(team);
        }
        
    }
}