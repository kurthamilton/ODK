using ODK.Core.Countries;

namespace ODK.Core.Subscriptions;

public class SiteSubscriptionPrice : IDatabaseEntity
{
    public decimal Amount { get; set; }

    public Currency Currency { get; set; } = null!;

    public Guid CurrencyId { get; set; }
    
    public string? ExternalId { get; set; }

    public SiteSubscriptionFrequency Frequency { get; set; }

    public Guid Id { get; set; }    

    public Guid SiteSubscriptionId { get; set; }    
}
