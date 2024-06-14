using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Header;

public class ChapterHeaderViewModel
{
    public ChapterHeaderViewModel(Chapter chapter, Member? member)
    {
        Chapter = chapter;
        Member = member;
    }

    public Chapter Chapter { get; }

    public Member? Member { get; }
}
