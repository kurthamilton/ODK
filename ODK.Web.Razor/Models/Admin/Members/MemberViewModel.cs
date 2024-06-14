using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Admin.Members;

public class MemberViewModel
{
    public MemberViewModel(Chapter chapter, Member currentMember, Member member)
    {
        Chapter = chapter;
        CurrentMember = currentMember;
        Member = member;
    }

    public Chapter Chapter { get; }

    public Member CurrentMember { get; }

    public Member Member { get; }
}
