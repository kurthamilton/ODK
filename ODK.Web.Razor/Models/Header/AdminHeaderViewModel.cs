using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Header
{
    public class AdminHeaderViewModel
    {
        public AdminHeaderViewModel(Chapter chapter, Member currentMember)
        {
            Chapter = chapter;
            CurrentMember = currentMember;
        }

        public Chapter Chapter { get; }

        public Member CurrentMember { get; }
    }
}
