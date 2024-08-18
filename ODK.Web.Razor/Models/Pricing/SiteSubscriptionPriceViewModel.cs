using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;

namespace ODK.Web.Razor.Models.Pricing;

public class SiteSubscriptionPriceViewModel
{
    public required decimal Amount { get; init; }

    public Chapter? Chapter { get; init; }

    public required Currency Currency { get; init; }

    public required string ExternalId { get; init; }

    public required SiteSubscriptionFrequency Frequency { get; init; }    

    public required PaymentProviderType Provider { get; init; }

    public required SiteSubscription SiteSubscription { get; init; }

    public required Guid SiteSubscriptionPriceId { get; init; }
}
