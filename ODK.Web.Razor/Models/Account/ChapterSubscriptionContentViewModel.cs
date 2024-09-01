using ODK.Core.Chapters;
using ODK.Core.Payments;

namespace ODK.Web.Razor.Models.Account;

public class ChapterSubscriptionContentViewModel
{
    public required Chapter Chapter { get; init; }

    public required IPaymentSettings? ChapterPaymentSettings { get; init; }
}
