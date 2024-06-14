using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Account;

public class DeleteAccountContentViewModel
{
    public DeleteAccountContentViewModel(Chapter chapter)
    {
        Chapter = chapter;
    }

    public Chapter Chapter { get; }
}
