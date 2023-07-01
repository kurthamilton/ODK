using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Account
{
    public class ChangePasswordContentViewModel
    {
        public ChangePasswordContentViewModel(Chapter chapter, Member currentMember)
        {
            Chapter = chapter;
            CurrentMember = currentMember;
        }

        public Chapter Chapter { get; set; }

        public Member CurrentMember { get; }
    }
}
