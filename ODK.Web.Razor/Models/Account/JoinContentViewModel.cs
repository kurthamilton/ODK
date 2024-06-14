using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Account;

public class JoinContentViewModel
{
    public JoinContentViewModel(Chapter chapter)
    {
        Chapter = chapter;
    }

    public Chapter Chapter { get; }
}
