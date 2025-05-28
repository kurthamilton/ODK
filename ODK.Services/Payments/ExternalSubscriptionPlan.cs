using ODK.Core.Subscriptions;

namespace ODK.Services.Payments;

public class ExternalSubscriptionPlan
{
    public required decimal Amount { get; init; }
    public required string CurrencyCode { get; init; }
    public required string ExternalId { get; init; }
    public required string ExternalProductId { get; init; }
    public required SiteSubscriptionFrequency Frequency { get; init; }
    public required string Name { get; init; }
    public int NumberOfMonths { get; init; }    
    public required bool Recurring { get; init; }
}
