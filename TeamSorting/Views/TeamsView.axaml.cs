using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using TeamSorting.Controls;
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
        if (DataContext is not TeamsViewModel context) return;
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

    private void AscDisciplineRadioButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: KeyValuePair<DisciplineInfo, double> disciplinePair })
        {
            var discipline = disciplinePair.Key;
            if (DataContext is TeamsViewModel context)
            {
                context.Data.SortTeamsByCriteria(discipline, SortOrder.Asc);
            }
        }
    }

    private void DescDisciplineRadioButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: KeyValuePair<DisciplineInfo, double> disciplinePair })
        {
            var discipline = disciplinePair.Key;
            if (DataContext is TeamsViewModel context)
            {
                context.Data.SortTeamsByCriteria(discipline, SortOrder.Desc);
            }
        }
    }

    private void AscNameRadioButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TeamsViewModel context)
        {
            context.Data.SortTeamsByCriteria(null, SortOrder.Asc);
        }
    }

    private void DescNameRadioButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TeamsViewModel context)
        {
            context.Data.SortTeamsByCriteria(null, SortOrder.Desc);
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
}