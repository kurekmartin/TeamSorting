using CommunityToolkit.Mvvm.ComponentModel;

namespace TeamSorting.Models;

public class FilterableMember(Member member) : ObservableObject
{
    private Member _member = member;
    private bool _isVisible = true;

    public Member Member
    {
        get => _member;
        set => SetProperty(ref _member, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }
}