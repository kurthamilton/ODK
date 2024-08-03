using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Members;

public class MemberProfileViewModel
{
    public MemberProfileViewModel(Member member, Chapter chapter)
    {
        Chapter = chapter;
        Member = member;
    }

    public Chapter Chapter { get; }

    public Member Member { get; }
}
