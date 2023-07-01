using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Members
{
    public class MemberImageViewModel
    {
        public MemberImageViewModel(Chapter chapter, Member member)
        {
            Chapter = chapter;
            Member = member;
        }

        public Chapter Chapter { get; }

        public bool IsTop { get; set; }

        public int MaxWidth { get; set; }

        public Member Member { get; }
    }
}
