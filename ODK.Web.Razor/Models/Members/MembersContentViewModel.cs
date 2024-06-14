using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Members;

public class MembersContentViewModel
{
    public MembersContentViewModel(Chapter chapter, Member member)
    {
        Chapter = chapter;
        Member = member;
    }

    public Chapter Chapter { get; }

    public Member Member { get; }
}
