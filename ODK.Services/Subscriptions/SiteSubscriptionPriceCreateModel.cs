using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionPriceCreateModel
{
    public int Amount { get; set; }

    public Guid CurrencyId { get; set; }

    public SiteSubscriptionFrequency Frequency { get; set; }
}
