using ODK.Core.Chapters;
using ODK.Core.Payments;

namespace ODK.Data.Core.Chapters;

public class ChapterSubscriptionDto
{
    public required ChapterSubscription ChapterSubscription { get; init; }

    public required SitePaymentSettings? SitePaymentSettings { get; init; }
}