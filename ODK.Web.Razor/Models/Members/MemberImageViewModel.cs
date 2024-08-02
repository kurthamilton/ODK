using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Members;

public class MemberImageViewModel : MemberAvatarViewModel
{
    public MemberImageViewModel(Chapter chapter, Member member)
        : base(chapter, member)
    {
    }

    public int ImageHeight { get; set; }
}
