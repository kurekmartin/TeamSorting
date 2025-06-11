using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using TeamSorting.Lang;

namespace TeamSorting.Models;

public class Members : ObservableObject
{
    private readonly ObservableCollection<Member> _memberList = [];

    public ReadOnlyObservableCollection<Member> MemberList { get; }
    private readonly ILogger<Members> _logger;

    public Members(ILogger<Members> logger)
    {
        _logger = logger;
        MemberList = new ReadOnlyObservableCollection<Member>(_memberList);
    }

    public List<Member> SortedMembers => _memberList.OrderBy(m => m.Name).ToList();

    public bool AddMember(Member member)
    {
        _logger.LogInformation("Adding member {memberId}", member.Id);
        member.PropertyChanged += MemberOnPropertyChanged;
        _memberList.Add(member);
        ValidateMemberDuplicates();
        OnPropertyChanged(nameof(SortedMembers));
        return true;
    }

    public bool RemoveMember(Member member)
    {
        _logger.LogInformation("Removing member {memberId}", member.Id);
        bool result = _memberList.Remove(member);
        if (result)
        {
            member.Team?.RemoveMember(member);
            member.ClearWithMembers();
            member.ClearNotWithMembers();
            ValidateMemberDuplicates();
        }

        OnPropertyChanged(nameof(SortedMembers));
        return result;
    }

    public void RemoveAllMembers()
    {
        _memberList.Clear();
    }

    public Member? GetMemberByName(string name)
    {
        return _memberList.FirstOrDefault(member => member.Name == name);
    }

    public IEnumerable<Member> GetMembersByName(IEnumerable<string> names)
    {
        return _memberList.Where(member => names.Contains(member.Name));
    }

    public bool AddWithMember(Member member, string withMemberName)
    {
        Member? withMember = _memberList.FirstOrDefault(m => m.Name == withMemberName);
        if (withMember is null)
        {
            return false;
        }

        member.AddWithMember(withMember);
        return true;
    }

    public List<(string Name, bool Added)> AddWithMembers(Member member, List<string> withMemberNames)
    {
        List<(string Name, bool Added)> results = [];
        foreach (string withMemberName in withMemberNames)
        {
            bool result = AddWithMember(member, withMemberName);
            results.Add((withMemberName, result));
        }

        return results;
    }

    public bool AddNotWithMember(Member member, string notWithMemberName)
    {
        Member? notWithMember = _memberList.FirstOrDefault(m => m.Name == notWithMemberName);
        if (notWithMember is null)
        {
            return false;
        }

        member.AddNotWithMember(notWithMember);
        return true;
    }

    public List<(string Name, bool Added)> AddNotWithMembers(Member member, List<string> notWithMemberNames)
    {
        List<(string Name, bool Added)> results = [];
        foreach (string notWithMemberName in notWithMemberNames)
        {
            bool result = AddNotWithMember(member, notWithMemberName);
            results.Add((notWithMemberName, result));
        }

        return results;
    }

    private void MemberOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Member)
        {
            return;
        }

        if (e.PropertyName == nameof(Member.Name))
        {
            ValidateMemberDuplicates();
        }
    }

    private void ValidateMemberDuplicates()
    {
        List<IGrouping<string, Member>> memberGroups = _memberList
                                                       .GroupBy(member => member.Name).ToList();
        foreach (IGrouping<string, Member> members in memberGroups)
        {
            if (members.Count() > 1)
            {
                foreach (Member member in members)
                {
                    member.AddError(nameof(Member.Name), Resources.InputView_Member_DuplicateName_Error);
                }
            }
            else
            {
                foreach (Member member in members)
                {
                    member.RemoveError(nameof(Member.Name), Resources.InputView_Member_DuplicateName_Error);
                }
            }
        }
    }
}