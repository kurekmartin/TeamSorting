using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Octokit;
using TeamSorting.Models;

namespace TeamSorting.ViewModels;

public class MainWindowViewModel(ILogger<MainWindowViewModel> logger, TeamsViewModel teamsViewModel, InputViewModel inputViewModel, Teams teams) : ViewModelBase
{
    private ViewModelBase _contentViewModel = inputViewModel;
    private bool _newVersionAvailable;

    public Teams Teams { get; } = teams;
    public TeamsViewModel TeamsViewModel => teamsViewModel;

    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        private set => SetProperty(ref _contentViewModel, value);
    }

    [Localizable(false)]
    public string Version =>
        $"v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?.?.?"}";

    public bool NewVersionAvailable
    {
        get => _newVersionAvailable;
        private set => SetProperty(ref _newVersionAvailable, value);
    }

    public string ReleaseUrl { get; private set; } = string.Empty;

    public void SwitchToTeamsView()
    {
        logger.LogInformation("Switching to teams view");
        ContentViewModel = teamsViewModel;
    }

    public void SwitchToInputView()
    {
        logger.LogInformation("Switching to input view");
        ContentViewModel = inputViewModel;
    }

    public async void CheckForUpdates()
    {
        logger.LogInformation("Checking for updates");
        Version currentVersion = System.Version.Parse(Version.Replace("v", ""));
        logger.LogInformation("Current version: {CurrentVersion}", currentVersion);
        try
        {
            var github = new GitHubClient(new ProductHeaderValue("TeamSorting"));
            Release? release = await github.Repository.Release.GetLatest("kurekmartin", "TeamSorting");
            Version latestVersion = System.Version.Parse(release.TagName.Replace("v", ""));
            logger.LogInformation("Latest version: {LatestVersion}", latestVersion);

            if (latestVersion > currentVersion)
            {
                NewVersionAvailable = true;
                ReleaseUrl = release.HtmlUrl;
                return;
            }

            logger.LogInformation("No new version available");
        }
        catch (Exception exception)
        {
            logger.LogError("Error checking for updates. {error}", exception.Message);
        }
    }
}