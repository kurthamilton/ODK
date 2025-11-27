using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Header;

public class ChapterHeaderViewModel : OdkComponentViewModel
{
    public ChapterHeaderViewModel(
        Chapter chapter, Member? member, OdkComponentContext context)
        : base(context)
    {
        Chapter = chapter;
        Member = member;
    }

    public Chapter Chapter { get; }

    public Member? Member { get; }
}
