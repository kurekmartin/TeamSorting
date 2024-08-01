﻿using Avalonia;
using Avalonia.Controls;
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
            new Member(string.Empty),
            BindingMode.TwoWay);
    
    private Member _member = null!;

    public Member Member
    {
        get => _member;
        set => SetAndRaise(MemberProperty, ref _member, value);
    }
}