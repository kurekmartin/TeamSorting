using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using TeamSorting.Lang;

namespace TeamSorting.Models;

public class Members(ILogger<Members> logger, Lazy<Disciplines> disciplines, Lazy<Teams> teams)
    : ObservableObject
{
    /// <summary>
    /// Do not modify this list directly.
    /// </summary>
    public ObservableCollection<Member> MemberList { get; } = [];
    public List<Member> SortedMembers => MemberList.OrderBy(m => m.Name).ToList();

    public bool AddMember(Member member)
    {
        logger.LogInformation("Adding member {memberId}", member.Id);
        foreach (DisciplineInfo discipline in disciplines.Value.DisciplineList)
        {
            disciplines.Value.AddDisciplineRecord(member, discipline, "");
        }

        member.PropertyChanged += MemberOnPropertyChanged;
        MemberList.Add(member);
        teams.Value.MembersWithoutTeam.AddMember(member);
        ValidateMemberDuplicates();
        OnPropertyChanged(nameof(SortedMembers));
        return true;
    }

    public bool RemoveMember(Member member)
    {
        logger.LogInformation("Removing member {memberId}", member.Id);
        bool result = MemberList.Remove(member);
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
        MemberList.Clear();
    }

    public Member? GetMemberByName(string name)
    {
        return MemberList.FirstOrDefault(member => member.Name == name);
    }

    public IEnumerable<Member> GetMembersByName(IEnumerable<string> names)
    {
        return MemberList.Where(member => names.Contains(member.Name));
    }

    public bool AddWithMember(Member member, string withMemberName)
    {
        Member? withMember = MemberList.FirstOrDefault(m => m.Name == withMemberName);
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
        Member? notWithMember = MemberList.FirstOrDefault(m => m.Name == notWithMemberName);
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
        List<IGrouping<string, Member>> memberGroups = MemberList
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