using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Account;

public class SubscriptionContentViewModel
{
    public SubscriptionContentViewModel(Chapter chapter)
    {
        Chapter = chapter;
    }

    public Chapter Chapter { get; }
}
