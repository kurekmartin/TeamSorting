using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamSorting.Models;

public class FilterableMember(Member member) : ObservableObject
{
    private bool _isVisible = true;

    public Member Member { get; } = member;

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }
}