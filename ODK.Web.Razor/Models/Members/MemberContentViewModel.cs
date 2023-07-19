using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Members
{
    public class MemberContentViewModel
    {
        public MemberContentViewModel(Chapter chapter, Member member, Member currentMember)
        {
            CurrentMember = currentMember;
            Chapter = chapter;
            Member = member;
        }

        public Chapter Chapter { get; }

        public Member CurrentMember { get; }

        public Member Member { get; }
    }
}
