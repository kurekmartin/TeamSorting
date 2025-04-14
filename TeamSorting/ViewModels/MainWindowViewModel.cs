﻿using System.ComponentModel;

namespace TeamSorting.ViewModels;

public class MainWindowViewModel(TeamsViewModel teamsViewModel, InputViewModel inputViewModel) : ViewModelBase
{
    private ViewModelBase _contentViewModel = inputViewModel;

    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        private set => SetProperty(ref _contentViewModel, value);
    }

    [Localizable(false)]
    public string Version =>
        $"v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?.?.?"}";

    public void SwitchToTeamsView()
    {
        ContentViewModel = teamsViewModel;
    }

    public void SwitchToInputView()
    {
        ContentViewModel = inputViewModel;
    }
}