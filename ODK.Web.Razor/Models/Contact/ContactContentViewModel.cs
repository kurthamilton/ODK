using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Contact;

public class ContactContentViewModel
{
    public ContactContentViewModel(Chapter chapter)
    {
        Chapter = chapter;
    }

    public Chapter Chapter { get; }

    public bool Sent { get; set; }
}
