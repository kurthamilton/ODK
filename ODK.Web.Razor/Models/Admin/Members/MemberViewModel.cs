using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Admin.Members;

public class MemberViewModel
{
    public MemberViewModel(Chapter chapter, Member member)
    {
        Chapter = chapter;
        Member = member;
    }

    public Chapter Chapter { get; }

    public Member Member { get; }
}
