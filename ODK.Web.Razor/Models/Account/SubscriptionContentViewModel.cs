using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Account;

public class SubscriptionContentViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterPaymentSettings? ChapterPaymentSettings { get; init; }
}
