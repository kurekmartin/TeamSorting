using DynamicData.Binding;

namespace TeamSorting.Models;

public class FilterableMember(Member member) : AbstractNotifyPropertyChanged
{
    private Member _member = member;
    private bool _isVisible = true;

    public Member Member
    {
        get => _member;
        set => SetAndRaise(ref _member, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetAndRaise(ref _isVisible, value);
    }
}