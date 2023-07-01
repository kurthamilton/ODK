using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Chapters
{
    public class ChapterContentViewModel
    {
        public ChapterContentViewModel(Chapter chapter, Member? member)
        {
            Chapter = chapter;
            Member = member;
        }

        public Chapter Chapter { get; set; }

        public Member? Member { get; set; }
    }
}
