using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Account;

public class ChangePasswordContentViewModel
{
    public ChangePasswordContentViewModel(Chapter chapter)
    {
        Chapter = chapter;
    }

    public Chapter Chapter { get; set; }
}
