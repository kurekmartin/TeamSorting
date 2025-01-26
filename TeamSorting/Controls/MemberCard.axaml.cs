using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using TeamSorting.Models;

namespace TeamSorting.Controls;

public class MemberCard : TemplatedControl
{
    public static readonly DirectProperty<MemberCard, Member> MemberProperty =
        AvaloniaProperty.RegisterDirect<MemberCard, Member>(
            nameof(Member),
            o => o.Member,
            (o, v) => o.Member = v,
            null!,
            BindingMode.TwoWay);

    private Member _member = null!;

    public Member Member
    {
        get => _member;
        set => SetAndRaise(MemberProperty, ref _member, value);
    }

    public static readonly DirectProperty<MemberCard, bool> ShowDetailProperty =
        AvaloniaProperty.RegisterDirect<MemberCard, bool>(
            nameof(ShowDetail),
            o => o.ShowDetail,
            (o, v) => o.ShowDetail = v,
            true,
            BindingMode.TwoWay);

    private bool _showDetail = true;

    public bool ShowDetail
    {
        get => _showDetail;
        set => SetAndRaise(ShowDetailProperty, ref _showDetail, value);
    }

    public void Copy(MemberCard card)
    {
        card.ShowDetail = ShowDetail;
        card.Member = Member;
        card.Width = Bounds.Width;
        card.Height = Bounds.Height;
    }
}