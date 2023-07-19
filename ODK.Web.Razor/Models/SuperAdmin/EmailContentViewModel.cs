using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Web.Razor.Models.SuperAdmin
{
    public class EmailContentViewModel
    {
        public EmailContentViewModel(Chapter chapter, Email email)
        {
            Chapter = chapter;
            Email = email;
        }

        public Chapter Chapter { get; }

        public Email Email { get; }
    }
}
