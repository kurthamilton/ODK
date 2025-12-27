using ODK.Core.Payments;

namespace ODK.Core.Chapters.Dtos;

public class ChapterSubscriptionAdminDto
{
    public required ChapterSubscription ChapterSubscription { get; init; }

    public required SitePaymentSettings? SitePaymentSettings { get; init; }

    public required bool Used { get; init; }
}
