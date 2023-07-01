using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Members
{
    public class ListMemberViewModel
    {
        public ListMemberViewModel(Chapter chapter, Member member)
        {
            Chapter = chapter;
            Member = member;
        }

        public Chapter Chapter { get; }

        public bool HideName => Size is "sm" or "xs";
        
        public int MaxWidth { get; set; }

        public Member Member { get; }

        public string? Size { get; set; }
    }
}
