using ODK.Web.Razor.Models.Contact;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterAdminStartConversationFormViewModel : ConversationFormViewModel
{
    public Guid MemberId { get; set; }
}
